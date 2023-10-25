global using Microsoft.EntityFrameworkCore;
global using snake_game.Data;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<DataContext>(options => options.UseNpgsql(connectionString));

builder.Services.AddValidatorsFromAssemblyContaining<Program>();

//builder.Services.AddTransient<TokenManagerMiddleware>();
//builder.Services.AddTransient<ITokenManager, TokenManager>();
//builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
//builder.Services.AddMemoryCache();
//
//builder.Services
//    .AddAuthentication(options =>
//    {
//        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
//        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
//        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
//    })
//    .AddJwtBearer(options =>
//    {
//        options.TokenValidationParameters = new TokenValidationParameters
//        {
//            ValidIssuer = builder.Configuration["Jwt:Issuer"],
//            ValidAudience = builder.Configuration["Jwt:Audience"],
//            IssuerSigningKey = new SymmetricSecurityKey(
//                Encoding.UTF8.GetBytes(builder.Configuration.GetSection("Jwt:Key").Value!)
//            ),
//            ValidateIssuer = true,
//            ValidateAudience = true,
//            ValidateLifetime = true,
//            ValidateIssuerSigningKey = true,
//        };
//
//        options.Events = new JwtBearerEvents
//        {
//            OnMessageReceived = context =>
//            {
//                var accessToken = context.Request.Query["access_token"];
//
//                var path = context.HttpContext.Request.Path;
//                if (!string.IsNullOrEmpty(accessToken) && (path.StartsWithSegments("/gamehub")))
//                {
//                    context.Token = accessToken;
//                }
//                return Task.CompletedTask;
//            }
//        };
//    });

// Add services to the container.

//builder.Services
//    .AddControllers(options =>
//    {
//        options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
//    })
//    .ConfigureApiBehaviorOptions(options =>
//    {
//        options.SuppressModelStateInvalidFilter = true;
//    });

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle

// Minimal APIs
//builder.Services.AddEndpointsApiExplorer();

//builder.Services.AddSwaggerGen();
//
//builder.Services.AddSignalR();
//builder.Services.AddSingleton<GameManager>();

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
                    FormatException => 422,
                    InvalidOperationException => 422,
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

var rootGroup = app.MapGroup("/api");
rootGroup.MapMapsApi();
rootGroup.MapUsersApi();
rootGroup.MapTournamentsApi();

//// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}
//
//using (var scope = app.Services.CreateScope())
//{
//    var services = scope.ServiceProvider;
//
//    var context = services.GetRequiredService<DataContext>();
//    context.Database.EnsureCreated();
//    new DbInitializer(context);
//}
//
//app.UseHttpsRedirection();
//
//app.UseAuthentication();
//app.UseAuthorization();
//
//app.UseMiddleware<TokenManagerMiddleware>();
//
//app.MapControllers();
//
//app.MapHub<GameHub>("/gamehub");

app.Run();
