using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using UserSite.AuthService.Api.Clients.UserSite;
using UserSite.AuthService.Api.Options;
using UserSite.AuthService.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "UserSite.AuthService.Api",
        Version = "v1"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Informe o token JWT no formato Bearer {token}."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var jwtSection = builder.Configuration.GetSection("Jwt");
builder.Services.Configure<JwtOptions>(jwtSection);
var userSiteSection = builder.Configuration.GetSection("UserSite");
builder.Services.Configure<UserSiteOptions>(userSiteSection);

var jwtOptions = jwtSection.Get<JwtOptions>()
    ?? throw new InvalidOperationException("A seção 'Jwt' não foi configurada.");

var userSiteOptions = userSiteSection.Get<UserSiteOptions>()
    ?? throw new InvalidOperationException("A seção 'UserSite' não foi configurada.");

if (string.IsNullOrWhiteSpace(jwtOptions.Issuer) ||
    string.IsNullOrWhiteSpace(jwtOptions.Audience) ||
    string.IsNullOrWhiteSpace(jwtOptions.SecretKey) ||
    jwtOptions.ExpirationMinutes <= 0)
{
    throw new InvalidOperationException("A configuração de JWT está inválida.");
}

if (!Uri.TryCreate(userSiteOptions.BaseUrl, UriKind.Absolute, out var userSiteBaseUri))
{
    throw new InvalidOperationException("A configuração de UserSite.BaseUrl está inválida.");
}

if (string.IsNullOrWhiteSpace(userSiteOptions.ValidateCredentialsPath))
{
    throw new InvalidOperationException("A configuração de UserSite.ValidateCredentialsPath está inválida.");
}

var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SecretKey));

builder.Services.AddHttpClient<IUserSiteAuthClient, UserSiteAuthClient>(client =>
{
    client.BaseAddress = userSiteBaseUri;
    client.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddScoped<ITokenService, TokenService>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtOptions.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = signingKey,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
