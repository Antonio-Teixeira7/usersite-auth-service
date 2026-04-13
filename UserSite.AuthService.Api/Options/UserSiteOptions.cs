namespace UserSite.AuthService.Api.Options;

public class UserSiteOptions
{
    public string BaseUrl { get; set; } = string.Empty;

    public string ValidateCredentialsPath { get; set; } = "api/internal/auth/validate-credentials";

    public string InternalApiKey { get; set; } = string.Empty;
}
