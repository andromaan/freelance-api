using System.Globalization;
using System.Reflection;
using System.Text;
using BLL.Common.Behaviours;
using BLL.Hubs;
using BLL.Models;
using BLL.Services.ImageService;
using BLL.Services.JwtService;
using BLL.Services.Notifications;
using BLL.Services.PasswordHasher;
using BLL.Services.StripeService;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Stripe;

namespace BLL;

public static class ConfigureBusinessLogic
{
    public static void AddBusinessLogic(this IServiceCollection services, WebApplicationBuilder builder)
    {
        services.AddMediatrConfig();
        services.AddRegistrations();

        services.AddServices();

        services.AddJwtTokenAuth(builder);
        services.AddSwaggerAuth();
        
        // AutoMapper: scans all assemblies for profiles
        services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

        // Authorization: policy-based with role checks, policies defined in Settings.Roles
        services.AddAuthorization(options =>
        {
            options.AddPolicy(Settings.Roles.AnyAuthenticated,
                policy => policy.RequireRole(Settings.Roles.AdminRole, Settings.Roles.EmployerRole,
                    Settings.Roles.FreelancerRole, Settings.Roles.ModeratorRole));

            options.AddPolicy(Settings.Roles.AdminOrModerator,
                policy => policy.RequireRole(Settings.Roles.AdminRole, Settings.Roles.ModeratorRole));

            options.AddPolicy(Settings.Roles.AdminOrEmployer,
                policy => policy.RequireRole(Settings.Roles.AdminRole, Settings.Roles.EmployerRole));

            options.AddPolicy(Settings.Roles.AdminOrFreelancer,
                policy => policy.RequireRole(Settings.Roles.AdminRole, Settings.Roles.FreelancerRole));
        });

        // SignalR: використовуємо кастомний провайдер userId (читає claim "id")
        builder.Services.AddSingleton<IUserIdProvider, NotificationUserIdProvider>();
        builder.Services.AddSingleton<ChatPresenceTracker>();

        // Stripe configuration: 
        services.AddStripeConfiguration(builder);
        
        // Culture: use Ukrainian currency for now
        var culture = new CultureInfo("uk-UA");
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;
    }
    
    private static void AddStripeConfiguration(this IServiceCollection services, WebApplicationBuilder builder)
    {
        services.Configure<StripeModel>(builder.Configuration.GetSection("Stripe"));
        services.AddScoped<TokenService>();
        services.AddScoped<CustomerService>();
        services.AddScoped<ChargeService>();
    }

    private static void AddMediatrConfig(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));
    }

    private static void AddServices(this IServiceCollection services)
    {
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IImageService, ImageService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IStripeService, StripeService>();
    }

    private static void AddJwtTokenAuth(this IServiceCollection services, WebApplicationBuilder builder)
    {
        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                // options.TokenValidationParameters = new TokenValidationParameters
                // {
                //     RequireExpirationTime = true,
                //     ValidateLifetime = true,
                //     ClockSkew = TimeSpan.Zero,
                //     ValidateIssuer = true,
                //     ValidateAudience = true,
                //     ValidateIssuerSigningKey = true,
                //     IssuerSigningKey =
                //         new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder!.Configuration["AuthSettings:key"]!)),
                //     ValidIssuer = builder.Configuration["AuthSettings:issuer"],
                //     ValidAudience = builder.Configuration["AuthSettings:audience"]
                // };
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    RequireExpirationTime = false,
                    ValidateLifetime = false,
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey =
                        new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["AuthSettings:key"]!)),
                    ValidIssuer = builder.Configuration["AuthSettings:issuer"],
                    ValidAudience = builder.Configuration["AuthSettings:audience"]
                };

                // SignalR передає токен через query string (WebSocket не підтримує заголовки)
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];
                        var path = context.HttpContext.Request.Path;
                        if (!string.IsNullOrEmpty(accessToken) &&
                            (path.StartsWithSegments("/notifications") || path.StartsWithSegments("/chat")))
                        {
                            context.Token = accessToken;
                        }

                        return Task.CompletedTask;
                    }
                };
            });
    }

    private static void AddSwaggerAuth(this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo { Title = "Freelance Marketplace API", Version = "v1" });

            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Введіть JWT токен"
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
    }
}

public class BLLClassForScanning;