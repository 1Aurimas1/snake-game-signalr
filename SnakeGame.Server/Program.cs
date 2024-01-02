global using Microsoft.EntityFrameworkCore;
global using SnakeGame.Server.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using SnakeGame.Server.Auth.Services.TokenServices;
using SnakeGame.Server.Models;
using SnakeGame.Server.Services.DataServices;
using SnakeGame.Server.Services.GameService;

JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

var builder = WebApplication.CreateBuilder(args);

var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<DataContext>(options => options.UseNpgsql(connectionString));
builder.Services.AddTransient<IUserService, UserService>();
builder.Services.AddTransient<IMapService, MapService>();
builder.Services.AddTransient<IGameService, GameService>();
builder.Services.AddTransient<ITournamentService, TournamentService>();

builder.Services.AddValidatorsFromAssemblyContaining<Program>();

builder.Services.AddTransient<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<DbSeeder>();

builder
    .Services
    .AddIdentity<User, IdentityRole<int>>()
    .AddEntityFrameworkStores<DataContext>()
    .AddDefaultTokenProviders();

builder
    .Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidIssuer = builder.Configuration["Jwt:ValidIssuer"],
            ValidAudience = builder.Configuration["Jwt:ValidAudience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)
            ),
            ClockSkew = TimeSpan.Zero
        };
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && (path.StartsWithSegments("/gamehub")))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddSignalR();
builder.Services.AddSingleton<GameManager>();

var app = builder.Build();

app.UseExceptionHandler(
    x =>
        x.Run(async context =>
        {
            var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;
            if (exception is not null)
            {
                context.Response.StatusCode = exception.GetBaseException() switch
                {
                    FormatException or InvalidOperationException => 422,
                    _ => 400
                };

                CustomError response = exception.InnerException switch
                {
                    not null
                        => JsonResponseGenerator.GenerateExceptionResponse(
                            exception.InnerException.GetType().ToString(),
                            exception.InnerException.Message
                        ),
                    null
                        => JsonResponseGenerator.GenerateExceptionResponse(
                            exception.GetType().ToString(),
                            exception.Message
                        )
                };

                await context.Response.WriteAsJsonAsync(response);
            }
        })
);

var rootGroup = app.MapGroup("/api/v1");

//.AddEndpointFilter(async (context, next) =>
//    {
//        var result = await next(context);
//
//        System.Console.WriteLine(result.GetType());
//        if (result is NotFound<string> notFoundResult)
//        {
//        context.HttpContext.Response.StatusCode = StatusCodes.Status404NotFound;
//            result = JsonResponseGenerator.GenerateNotFoundResponse(notFoundResult.Value);
//        }
//        else if (result is ForbidHttpResult)
//        {
//            result = JsonResponseGenerator.GenerateForbidResponse();
//        }
//        else if (result is UnprocessableEntity)
//        {
//        System.Console.WriteLine("unprocs");
//            //result = JsonResponseGenerator.GenerateUnprocessableEntityResponse();
//        }
//
//        return result;
//    });
rootGroup.MapAuthApi();
rootGroup.MapUsersApi();
rootGroup.MapTournamentsApi();
rootGroup.MapMapsApi();
rootGroup.MapGamesApi();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var dbSeeder = services.GetRequiredService<DbSeeder>();
    await dbSeeder.SeedAsync();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapHub<GameHub>("/gamehub");

app.Run();

public partial class Program { }
