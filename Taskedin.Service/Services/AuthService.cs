using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Taskedin.Domain.Entities;
using Taskedin.Persistence.Context;
using Taskedin.Service.DTOs;
using Taskedin.Service.Interfaces;

namespace Taskedin.Service.Services;

// Handles registration and login: password hashing, default role assignment and token issuance.
public class AuthService : IAuthService
{
    private const string DefaultRoleName = "User";

    private readonly TaskedinDbContext _db;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly ITokenService _tokenService;

    public AuthService(
        TaskedinDbContext db,
        IPasswordHasher<User> passwordHasher,
        ITokenService tokenService)
    {
        _db = db;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
    }

    public async Task<TokenResponse> RegisterAsync(RegisterRequest request)
    {
        var email = request.Email.Trim().ToLower();

        if (await _db.Users.AnyAsync(u => u.Email == email))
            throw new InvalidOperationException("Email is already registered.");

        var departmentExists = await _db.Departments
            .AnyAsync(d => d.Id == request.DepartmentId);
        if (!departmentExists)
            throw new InvalidOperationException("Department does not exist.");

        var fullName = $"{request.FirstName.Trim()} {request.LastName.Trim()}";

        var user = new User
        {
            Email = email,
            FullName = fullName,
            DepartmentId = request.DepartmentId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

        var defaultRole = await EnsureRoleExistsAsync(DefaultRoleName);

        // User creation and the default role assignment are persisted atomically:
        // if either insert fails, the transaction rolls both back so a user can
        // never be left registered without a role.
        await using var transaction = await _db.Database.BeginTransactionAsync();

        _db.Users.Add(user);

        try
        {
            // Save the user first so its generated Id can be used for the role link.
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            // Covers a duplicate email that slipped past the check above
            // (concurrent registration) and hits the unique index instead.
            if (await _db.Users.AnyAsync(u => u.Email == email))
                throw new InvalidOperationException("Email is already registered.");

            throw;
        }

        _db.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = defaultRole.Id });

        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            throw new InvalidOperationException(
                "Registration could not be completed. Please try again.");
        }

        await transaction.CommitAsync();

        return await BuildTokenResponseAsync(user);
    }

    public async Task<TokenResponse?> LoginAsync(LoginRequest request)
    {
        var email = request.Email.Trim().ToLower();

        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.Email == email);

        if (user == null)
            return null;

        var verificationResult = _passwordHasher.VerifyHashedPassword(
            user, user.PasswordHash, request.Password);

        if (verificationResult == PasswordVerificationResult.Failed)
            return null;

        if (!user.IsActive)
            throw new InvalidOperationException(
                "Account is deactivated. Contact an administrator.");

        return await BuildTokenResponseAsync(user);
    }

    // Returns the named role, creating it on first use and tolerating concurrent creation.
    private async Task<Role> EnsureRoleExistsAsync(string roleName)
    {
        var role = await _db.Roles.FirstOrDefaultAsync(r => r.Name == roleName);

        if (role != null)
            return role;

        var newRole = new Role { Name = roleName };
        _db.Roles.Add(newRole);

        try
        {
            await _db.SaveChangesAsync();
            return newRole;
        }
        catch (DbUpdateException)
        {
            // Another request created the same role concurrently; use that row.
            _db.Entry(newRole).State = EntityState.Detached;

            role = await _db.Roles.FirstOrDefaultAsync(r => r.Name == roleName);

            if (role == null)
                throw;

            return role;
        }
    }

    // Combines the issued access token with the user profile for login/registration responses.
    private async Task<TokenResponse> BuildTokenResponseAsync(User user)
    {
        var roles = await GetUserRoleNamesAsync(user.Id);
        var (accessToken, expiresAt) = _tokenService.CreateToken(user, roles);

        return new TokenResponse
        {
            AccessToken = accessToken,
            ExpiresAt = expiresAt,
            User = await ToUserResponseAsync(user)
        };
    }

    private async Task<List<string>> GetUserRoleNamesAsync(int userId)
    {
        return await (
                from ur in _db.UserRoles
                join r in _db.Roles on ur.RoleId equals r.Id
                where ur.UserId == userId
                select r.Name)
            .ToListAsync();
    }

    private async Task<UserResponse> ToUserResponseAsync(User user)
    {
        var departmentName = await _db.Departments
            .Where(d => d.Id == user.DepartmentId)
            .Select(d => d.Name)
            .FirstOrDefaultAsync() ?? string.Empty;

        return new UserResponse
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            DepartmentId = user.DepartmentId,
            DepartmentName = departmentName,
            IsActive = user.IsActive,
            Roles = await GetUserRoleNamesAsync(user.Id)
        };
    }
}
