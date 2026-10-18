using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Taskedin.Service.DTOs;

namespace Taskedin.Service.Interfaces;

public interface IUserService
{
    /// <summary>
    /// Returns the user with department name and roles, or null when not found.
    /// </summary>
    Task<UserResponse?> GetByIdAsync(int id);

    /// <summary>
    /// Updates user details. Returns null when the user does not exist.
    /// Throws InvalidOperationException when the email is taken by another
    /// user or the department does not exist.
    /// </summary>
    Task<UserResponse?> UpdateAsync(int id, UpdateUserRequest request);

    /// <summary>
    /// Activates or deactivates a user. Returns null when the user does not exist.
    /// </summary>
    Task<UserResponse?> SetActiveAsync(int id, bool isActive);
}
