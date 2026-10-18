using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Taskedin.Domain.Entities;
using Taskedin.Service.DTOs;

namespace Taskedin.Service.Interfaces;

public interface ITokenService
{
    /// <summary>
    /// Creates a signed access token for the user and returns it with its expiry time.
    /// </summary>
    (string AccessToken, DateTime ExpiresAt) CreateToken(
        User user, IEnumerable<string> roles);
}
