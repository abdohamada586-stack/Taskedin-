using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Taskedin.Domain.Common;
using Taskedin.Service.DTOs;
using Taskedin.Service.Interfaces;
using Taskedin.Service.Security;

namespace Taskedin.Controllers;

// User profile endpoints; activation and deactivation require the Admin role.
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        if (!IsSelfOrAdmin(id))
            return Forbid();

        var result = await _userService.GetByIdAsync(id);

        if (result == null)
            return NotFound(new ApiResponse<string>
            {
                Success = false,
                Message = "User not found"
            });

        return Ok(new ApiResponse<UserResponse>
        {
            Success = true,
            Message = "User retrieved successfully",
            Data = result
        });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateUserRequest request)
    {
        if (!IsSelfOrAdmin(id))
            return Forbid();

        try
        {
            var result = await _userService.UpdateAsync(id, request);

            if (result == null)
                return NotFound(new ApiResponse<string>
                {
                    Success = false,
                    Message = "User not found"
                });

            return Ok(new ApiResponse<UserResponse>
            {
                Success = true,
                Message = "User updated successfully",
                Data = result
            });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new ApiResponse<string>
            {
                Success = false,
                Message = ex.Message
            });
        }
    }

    [HttpPatch("{id:int}/activate")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Activate(int id)
    {
        return await SetActiveAsync(id, true, "User activated successfully");
    }

    [HttpPatch("{id:int}/deactivate")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Deactivate(int id)
    {
        return await SetActiveAsync(id, false, "User deactivated successfully");
    }

    private async Task<IActionResult> SetActiveAsync(
        int id, bool isActive, string successMessage)
    {
        var result = await _userService.SetActiveAsync(id, isActive);

        if (result == null)
            return NotFound(new ApiResponse<string>
            {
                Success = false,
                Message = "User not found"
            });

        return Ok(new ApiResponse<UserResponse>
        {
            Success = true,
            Message = successMessage,
            Data = result
        });
    }

    // Admins may access any user; all other callers only their own record.
    private bool IsSelfOrAdmin(int userId)
    {
        if (User.IsInRole("Admin"))
            return true;

        var claimValue = User.FindFirstValue(JwtClaimTypes.Subject);

        return int.TryParse(claimValue, out var currentUserId)
            && currentUserId == userId;
    }
}
