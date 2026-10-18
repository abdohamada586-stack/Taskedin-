using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Taskedin.Domain.Common;
using Taskedin.Service.DTOs;
using Taskedin.Service.Interfaces;

namespace Taskedin.Controllers;

// Public authentication endpoints: account registration and login.
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    // Returns 409 Conflict when registration is rejected (email taken or unknown department).
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        try
        {
            var result = await _authService.RegisterAsync(request);

            return Ok(new ApiResponse<TokenResponse>
            {
                Success = true,
                Message = "Registration successful",
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

    // Returns 401 for bad credentials and 403 when the account is deactivated.
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        try
        {
            var result = await _authService.LoginAsync(request);

            if (result == null)
            {
                return Unauthorized(new ApiResponse<string>
                {
                    Success = false,
                    Message = "Invalid email or password"
                });
            }

            return Ok(new ApiResponse<TokenResponse>
            {
                Success = true,
                Message = "Login successful",
                Data = result
            });
        }
        catch (InvalidOperationException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new ApiResponse<string>
            {
                Success = false,
                Message = ex.Message
            });
        }
    }
}
