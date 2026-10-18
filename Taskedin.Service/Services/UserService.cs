using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Taskedin.Domain.Entities;
using Taskedin.Persistence.Context;
using Taskedin.Service.DTOs;
using Taskedin.Service.Interfaces;

namespace Taskedin.Service.Services;

// User lookup and update operations, resolving department name and roles on read.
public class UserService : IUserService
{
    private readonly TaskedinDbContext _db;

    public UserService(TaskedinDbContext db)
    {
        _db = db;
    }

    public async Task<UserResponse?> GetByIdAsync(int id)
    {
        var user = await _db.Users.FindAsync(id);

        if (user == null)
            return null;

        return await ToUserResponseAsync(user);
    }

    public async Task<UserResponse?> UpdateAsync(int id, UpdateUserRequest request)
    {
        var user = await _db.Users.FindAsync(id);

        if (user == null)
            return null;

        var email = request.Email.Trim().ToLower();

        var emailTaken = await _db.Users
            .AnyAsync(u => u.Id != id && u.Email == email);
        if (emailTaken)
            throw new InvalidOperationException("Email is already registered.");

        var departmentExists = await _db.Departments
            .AnyAsync(d => d.Id == request.DepartmentId);
        if (!departmentExists)
            throw new InvalidOperationException("Department does not exist.");

        user.FullName = $"{request.FirstName.Trim()} {request.LastName.Trim()}";
        user.Email = email;
        user.DepartmentId = request.DepartmentId;

        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            // Covers a duplicate email that slipped past the check above and
            // hits the unique index instead.
            if (await _db.Users.AnyAsync(u => u.Id != id && u.Email == email))
                throw new InvalidOperationException("Email is already registered.");

            throw;
        }

        return await ToUserResponseAsync(user);
    }

    public async Task<UserResponse?> SetActiveAsync(int id, bool isActive)
    {
        var user = await _db.Users.FindAsync(id);

        if (user == null)
            return null;

        user.IsActive = isActive;
        await _db.SaveChangesAsync();

        return await ToUserResponseAsync(user);
    }

    // Projects a user entity onto the API response shape (department name and roles included).
    private async Task<UserResponse> ToUserResponseAsync(User user)
    {
        var departmentName = await _db.Departments
            .Where(d => d.Id == user.DepartmentId)
            .Select(d => d.Name)
            .FirstOrDefaultAsync() ?? string.Empty;

        var roles = await (
                from ur in _db.UserRoles
                join r in _db.Roles on ur.RoleId equals r.Id
                where ur.UserId == user.Id
                select r.Name)
            .ToListAsync();

        return new UserResponse
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            DepartmentId = user.DepartmentId,
            DepartmentName = departmentName,
            IsActive = user.IsActive,
            Roles = roles
        };
    }
}
