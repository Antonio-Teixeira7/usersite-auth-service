using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using UserSite.AuthService.Api.Dtos;
using UserSite.AuthService.Api.Options;

namespace UserSite.AuthService.Api.Clients.UserSite;

public class UserSiteAuthClient : IUserSiteAuthClient
{
    private readonly HttpClient _httpClient;
    private readonly UserSiteOptions _options;

    public UserSiteAuthClient(HttpClient httpClient, IOptions<UserSiteOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public async Task<AuthenticatedUserDto?> ValidateCredentialsAsync(string email, string password, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, _options.ValidateCredentialsPath)
        {
            Content = JsonContent.Create(new
            {
                Email = email,
                Password = password
            })
        };

        if (!string.IsNullOrWhiteSpace(_options.InternalApiKey))
        {
            request.Headers.Add("X-Internal-Api-Key", _options.InternalApiKey);
        }

        using var response = await _httpClient.SendAsync(request, cancellationToken);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            return null;
        }

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"Falha ao validar credenciais no UserSite. Status code: {(int)response.StatusCode}.");
        }

        var authenticatedUser = await response.Content.ReadFromJsonAsync<AuthenticatedUserDto>(cancellationToken: cancellationToken);

        if (authenticatedUser is null)
        {
            throw new InvalidOperationException("O UserSite retornou uma resposta inválida para validação de credenciais.");
        }

        return authenticatedUser;
    }
}
