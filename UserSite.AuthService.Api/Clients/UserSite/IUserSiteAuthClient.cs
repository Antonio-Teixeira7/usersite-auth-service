using UserSite.AuthService.Api.Dtos;

namespace UserSite.AuthService.Api.Clients.UserSite;

public interface IUserSiteAuthClient
{
    Task<AuthenticatedUserDto?> ValidateCredentialsAsync(string email, string password, CancellationToken cancellationToken);
}
