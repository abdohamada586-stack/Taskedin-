namespace Taskedin.Service.Security;

/// <summary>
/// Short JWT claim names used when issuing and validating tokens.
/// Kept in one place so the token issuer (TokenService) and the token
/// validator (JwtBearer configuration) cannot drift apart.
/// </summary>
public static class JwtClaimTypes
{
    public const string Subject = "sub";
    public const string Name = "name";
    public const string Email = "email";
    public const string Role = "role";
    public const string Jti = "jti";
}
