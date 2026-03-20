using System.IdentityModel.Tokens.Jwt;

using System.Security.Claims;
using System.Text;
using Backend.Api.Data;
using Dapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Data.Sqlite;
using Microsoft.IdentityModel.Tokens;

namespace Backend.Api;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        var dbPath = Path.Combine(builder.Environment.ContentRootPath, "miniportal.db");
        var connectionString = $"Data Source={dbPath}";

        var dbInitializer = new DbInitializer(connectionString);
        dbInitializer.Initialize();

        var jwt = builder.Configuration.GetSection("Jwt");
        var jwtSecret = jwt["Secret"];
        var jwtIssuer = jwt["Issuer"];
        var jwtAudience = jwt["Audience"];

        if (string.IsNullOrWhiteSpace(jwtSecret) || jwtSecret.Length < 32)
        {
            throw new InvalidOperationException("JWT secret must be configured with at least 32 characters.");
        }

        jwtIssuer ??= "MiniPortal";
        jwtAudience ??= "MiniPortal";

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));

        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwtIssuer,
                    ValidateAudience = true,
                    ValidAudience = jwtAudience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = signingKey,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(1)
                };
            });

        builder.Services.AddAuthorization();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler(errorApp =>
            {
                errorApp.Run(async context =>
                {
                    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    context.Response.ContentType = "text/plain";
                    await context.Response.WriteAsync("An error occurred.");
                });
            });
        }

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapPost("/api/auth/login", async (LoginRequest request) =>
            {
                if (request is null || string.IsNullOrWhiteSpace(request.username) || string.IsNullOrWhiteSpace(request.password))
                {
                    return Results.BadRequest();
                }

                await using var connection = new SqliteConnection(connectionString);

                var user = await connection.QueryFirstOrDefaultAsync<UserRow>(
                    "SELECT Id, PasswordHash FROM Users WHERE Username = @u",
                    new { u = request.username }
                );

                if (user is null)
                {
                    return Results.Unauthorized();
                }

                var passwordHash = DbInitializer.HashPassword(request.password);
                if (!string.Equals(user.PasswordHash, passwordHash, StringComparison.Ordinal))
                {
                    return Results.Unauthorized();
                }

                var token = CreateJwtToken(
                    userId: user.Id,
                    username: request.username,
                    signingKey: signingKey,
                    issuer: jwtIssuer,
                    audience: jwtAudience
                );

                return Results.Ok(new TokenResponse(token));
            })
            .AllowAnonymous();

        app.MapGet("/api/data", async () =>
            {
                await using var connection = new SqliteConnection(connectionString);

                var items = await connection.QueryAsync<ItemRow>(
                    "SELECT Id as id, Name as name FROM Items ORDER BY Id"
                );

                var dto = new ItemsResponse(items.Select(i => new ItemDto(i.id, i.name)).ToList());
                return Results.Ok(dto);
            })
            .RequireAuthorization();

        app.Run();
    }

    private static string CreateJwtToken(
        long userId,
        string username,
        SymmetricSecurityKey signingKey,
        string issuer,
        string audience)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new("username", username)
        };

        var creds = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(60),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private sealed class LoginRequest
    {
        public string? username { get; set; }
        public string? password { get; set; }
    }

    private sealed class TokenResponse
    {
        public string token { get; set; }

        public TokenResponse(string token)
        {
            this.token = token;
        }
    }

    private sealed class ItemsResponse
    {
        public List<ItemDto> items { get; set; }

        public ItemsResponse(List<ItemDto> items)
        {
            this.items = items;
        }
    }

    private sealed class ItemDto(long id, string name)
    {
        public long id { get; set; } = id;
        public string name { get; set; } = name;
    }

    private sealed class UserRow
    {
        public long Id { get; set; }
        public string PasswordHash { get; set; } = string.Empty;
    }

    private sealed class ItemRow
    {
        public long id { get; set; }
        public string name { get; set; } = string.Empty;
    }
}