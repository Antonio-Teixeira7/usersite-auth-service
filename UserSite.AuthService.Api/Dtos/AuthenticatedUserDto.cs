namespace UserSite.AuthService.Api.Dtos;

public class AuthenticatedUserDto
{
    public int Id { get; init; }

    public string Email { get; init; } = string.Empty;

    public bool IsActive { get; init; }
}
