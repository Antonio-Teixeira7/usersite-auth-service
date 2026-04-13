namespace UserSite.AuthService.Api.Dtos;

public class LoginResponseDto
{
    public string AccessToken { get; init; } = string.Empty;

    public DateTime ExpiresAtUtc { get; init; }

    public int UserId { get; init; }

    public string Email { get; init; } = string.Empty;
}
