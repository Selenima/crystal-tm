using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Crystal.Web.Data;
using Crystal.Web.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;

namespace Crystal.Web.Services;

public interface ISessionTokenService
{
    Task<SessionTokenPair> CreateSessionAsync(User user);
    Task RevokeCurrentSessionAsync(ClaimsPrincipal principal);
    Task<ClaimsPrincipal?> ValidateOrRefreshAsync(ClaimsPrincipal principal, HttpContext httpContext);
    void AppendRefreshTokenCookie(HttpResponse response, string refreshToken, DateTime expiresAt);
    void DeleteRefreshTokenCookie(HttpResponse response);
}

public sealed record SessionTokenPair(
    int SessionId,
    string AccessToken,
    string RefreshToken,
    DateTime AccessTokenExpiresAt,
    DateTime RefreshTokenExpiresAt);

public class SessionTokenService(ApplicationDbContext context) : ISessionTokenService
{
    public const string SessionIdClaimType = "crystal_session_id";
    public const string AccessTokenClaimType = "crystal_access_token";
    public const string RefreshTokenCookieName = "crystal_refresh_token";

    private static readonly TimeSpan AccessTokenLifetime = TimeSpan.FromMinutes(15);
    private static readonly TimeSpan RefreshTokenLifetime = TimeSpan.FromDays(7);

    public async Task<SessionTokenPair> CreateSessionAsync(User user)
    {
        var tokens = CreateTokenPair(DateTime.UtcNow);
        var session = new UserSessionToken
        {
            UserId = user.Id,
            AccessTokenHash = HashToken(tokens.AccessToken),
            RefreshTokenHash = HashToken(tokens.RefreshToken),
            AccessTokenExpiresAt = tokens.AccessTokenExpiresAt,
            RefreshTokenExpiresAt = tokens.RefreshTokenExpiresAt,
            CreatedAt = DateTime.UtcNow
        };

        context.UserSessionTokens.Add(session);
        await context.SaveChangesAsync();

        return tokens with { SessionId = session.Id };
    }

    public async Task RevokeCurrentSessionAsync(ClaimsPrincipal principal)
    {
        if (!TryGetSessionId(principal, out var sessionId))
        {
            return;
        }

        var session = await context.UserSessionTokens.FindAsync(sessionId);
        if (session == null || session.IsRevoked)
        {
            return;
        }

        session.RevokedAt = DateTime.UtcNow;
        await context.SaveChangesAsync();
    }

    public async Task<ClaimsPrincipal?> ValidateOrRefreshAsync(ClaimsPrincipal principal, HttpContext httpContext)
    {
        if (!TryGetSessionId(principal, out var sessionId))
        {
            return null;
        }

        var accessToken = principal.FindFirstValue(AccessTokenClaimType);
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return null;
        }

        var session = await context.UserSessionTokens
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.Id == sessionId);

        if (session == null || session.IsRevoked || session.RefreshTokenExpiresAt <= DateTime.UtcNow)
        {
            return null;
        }

        if (session.AccessTokenHash == HashToken(accessToken) && session.AccessTokenExpiresAt > DateTime.UtcNow)
        {
            return principal;
        }

        var refreshToken = httpContext.Request.Cookies[RefreshTokenCookieName];
        if (string.IsNullOrWhiteSpace(refreshToken) || session.RefreshTokenHash != HashToken(refreshToken))
        {
            return null;
        }

        var tokens = CreateTokenPair(DateTime.UtcNow);
        session.AccessTokenHash = HashToken(tokens.AccessToken);
        session.RefreshTokenHash = HashToken(tokens.RefreshToken);
        session.AccessTokenExpiresAt = tokens.AccessTokenExpiresAt;
        session.RefreshTokenExpiresAt = tokens.RefreshTokenExpiresAt;
        await context.SaveChangesAsync();

        AppendRefreshTokenCookie(httpContext.Response, tokens.RefreshToken, tokens.RefreshTokenExpiresAt);
        return BuildPrincipal(session.User, session.Id, tokens.AccessToken);
    }

    public void AppendRefreshTokenCookie(HttpResponse response, string refreshToken, DateTime expiresAt)
    {
        response.Cookies.Append(RefreshTokenCookieName, refreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = response.HttpContext.Request.IsHttps,
            SameSite = SameSiteMode.Strict,
            Expires = expiresAt
        });
    }

    public void DeleteRefreshTokenCookie(HttpResponse response)
    {
        response.Cookies.Delete(RefreshTokenCookieName, new CookieOptions
        {
            HttpOnly = true,
            Secure = response.HttpContext.Request.IsHttps,
            SameSite = SameSiteMode.Strict
        });
    }

    public static ClaimsPrincipal BuildPrincipal(User user, int sessionId, string accessToken)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.FullName),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.IsAdmin ? "Admin" : "User"),
            new(SessionIdClaimType, sessionId.ToString()),
            new(AccessTokenClaimType, accessToken)
        };

        return new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme));
    }

    private static SessionTokenPair CreateTokenPair(DateTime issuedAt)
    {
        return new SessionTokenPair(
            0,
            CreateToken(),
            CreateToken(),
            issuedAt.Add(AccessTokenLifetime),
            issuedAt.Add(RefreshTokenLifetime));
    }

    private static string CreateToken()
    {
        return WebEncoders.Base64UrlEncode(RandomNumberGenerator.GetBytes(64));
    }

    private static string HashToken(string token)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(hash);
    }

    private static bool TryGetSessionId(ClaimsPrincipal principal, out int sessionId)
    {
        return int.TryParse(principal.FindFirstValue(SessionIdClaimType), out sessionId);
    }
}
