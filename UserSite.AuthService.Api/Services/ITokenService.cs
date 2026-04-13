using UserSite.AuthService.Api.Dtos;

namespace UserSite.AuthService.Api.Services;

public interface ITokenService
{
    LoginResponseDto GenerateToken(AuthenticatedUserDto user);
}
