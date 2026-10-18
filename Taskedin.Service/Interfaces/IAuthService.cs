using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Taskedin.Service.DTOs;

namespace Taskedin.Service.Interfaces;

public interface IAuthService
{
    /// <summary>
    /// Registers a new user with the default "User" role and returns a token.
    /// Throws InvalidOperationException when the email is already taken
    /// or the department does not exist.
    /// </summary>
    Task<TokenResponse> RegisterAsync(RegisterRequest request);

    /// <summary>
    /// Verifies credentials and returns a token, or null when the
    /// email/password combination is invalid.
    /// Throws InvalidOperationException when the account is deactivated.
    /// </summary>
    Task<TokenResponse?> LoginAsync(LoginRequest request);
}
