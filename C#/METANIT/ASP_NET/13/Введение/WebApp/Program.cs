using Microsoft.AspNetCore.Authorization;

namespace WebApp;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        // добавление сервисов аутентификации
        builder.Services.AddAuthentication("Bearer")    // схема аутентификации - с помощью jwt-токенов
            .AddJwtBearer();    // подключение аутентификации с помощью jwt-токенов
        
        builder.Services.AddAuthorization();    // добавление сервисов авторизации
        
        var app = builder.Build();
        app.UseAuthentication();    // добавление middleware аутентификации
        app.UseAuthorization();   // добавление middleware авторизации 
        
        app.Map("/hello", [Authorize]() => "Hello World!");
        app.Map("/", () => "Home Page");

        app.Run();
    }
}