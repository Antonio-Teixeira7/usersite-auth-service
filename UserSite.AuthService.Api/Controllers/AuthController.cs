using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UserSite.AuthService.Api.Clients.UserSite;
using UserSite.AuthService.Api.Dtos;
using UserSite.AuthService.Api.Services;

namespace UserSite.AuthService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUserSiteAuthClient _userSiteAuthClient;
    private readonly ITokenService _tokenService;

    public AuthController(IUserSiteAuthClient userSiteAuthClient, ITokenService tokenService)
    {
        _userSiteAuthClient = userSiteAuthClient;
        _tokenService = tokenService;
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponseDto>> Login(LoginRequestDto request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim();

        try
        {
            var user = await _userSiteAuthClient.ValidateCredentialsAsync(email, request.Password, cancellationToken);

            if (user is null || !user.IsActive)
            {
                return Unauthorized();
            }

            var response = _tokenService.GenerateToken(user);
            return Ok(response);
        }
        catch (InvalidOperationException exception)
        {
            return Problem(
                title: "Falha na integração com o serviço de usuários.",
                detail: exception.Message,
                statusCode: StatusCodes.Status503ServiceUnavailable);
        }
    }
}
