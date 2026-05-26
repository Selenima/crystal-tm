using System.ComponentModel.DataAnnotations;

namespace Crystal.Web.Models;

public class UserSessionToken
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    [Required]
    [StringLength(128)]
    public string AccessTokenHash { get; set; } = string.Empty;

    [Required]
    [StringLength(128)]
    public string RefreshTokenHash { get; set; } = string.Empty;

    public DateTime AccessTokenExpiresAt { get; set; }
    public DateTime RefreshTokenExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? RevokedAt { get; set; }

    public bool IsRevoked => RevokedAt.HasValue;
}
