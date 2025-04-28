using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace WebApp;

public class Program
{
    public static void Main(string[] args)
    {
        
        var people = new List<Person>
        {
            new Person("tom@gmail.com", "12345"),
            new Person("bob@gmail.com", "55555")
        };
        
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddAuthorization();
        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = AuthOptions.ISSUER,
                    ValidateAudience = true,
                    ValidAudience = AuthOptions.AUDIENCE,
                    ValidateLifetime = true,
                    IssuerSigningKey = AuthOptions.GetSymmetricSecurityKey(),
                    ValidateIssuerSigningKey = true,
                };
            });
            
        
        
        var app = builder.Build();

        app.UseDefaultFiles();
        app.UseStaticFiles();
        
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapPost("/login", (Person loginData) =>
        {
            Person? person = people.FirstOrDefault(p => p.Email == loginData.Email && p.Password == loginData.Password);
            if (person is null) return Results.Unauthorized();
            
            var claims = new List<Claim> { new Claim(ClaimTypes.Name, person.Email) };
            var jwt = new JwtSecurityToken(
                issuer: AuthOptions.ISSUER,
                audience: AuthOptions.AUDIENCE,
                claims: claims,
                expires: DateTime.UtcNow.Add(TimeSpan.FromMinutes(2)),
                signingCredentials: new SigningCredentials(AuthOptions.GetSymmetricSecurityKey(),
                    SecurityAlgorithms.HmacSha256));
            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

            var response = new
            {
                access_token = encodedJwt,
                username = person.Email
            };
            
            return Results.Json(response);

        });
        
        app.Map("/data", [Authorize] () => new { message = "Hello World!" });
        
        app.Run();
    }
    
    public class AuthOptions
    {
        public const string ISSUER = "MyAuthServer"; // издатель токена
        public const string AUDIENCE = "MyAuthClient"; // потребитель токена
        const string KEY = "mysupersecret_secretsecretsecretkey!123";   // ключ для шифрации
        public static SymmetricSecurityKey GetSymmetricSecurityKey() => 
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(KEY));
    }
    
    record class Person(string Email, string Password);
}