using System.Text;
using expense_tracker.Data;
using FluentEmail.MailKitSmtp;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Personal_finance_tracker.config;
using Serilog;

namespace expense_tracker.config;

public static class AppServiceConfig
{
    public static IServiceCollection AddDbConfig(this IServiceCollection service, IConfiguration configuration)
    {
        string? dbConStr = configuration["DB_CONNECTION_STR"];
        return service.AddDbContext<AppDbContext>(opt => opt.UseSqlServer(dbConStr));
    }

    public static IServiceCollection AddRedisConfig(this IServiceCollection service, IConfiguration configuration)
    {
        string? redisConStr = configuration["REDIS_CONNECTION"];
        return service.AddStackExchangeRedisCache(opt =>
        {
            opt.Configuration = redisConStr;
            opt.InstanceName = "_FT";
            if (opt.Configuration != null)
                opt.ConfigurationOptions = new StackExchange.Redis.ConfigurationOptions()
                {
                    AbortOnConnectFail = true,
                    DefaultDatabase = 0,
                    EndPoints = { opt.Configuration }
                };
        });
    }

    public static IServiceCollection MapperConfig(this IServiceCollection service) =>
        service.AddAutoMapper(typeof(AppMapper));

    public static IServiceCollection AddGlobalExceptionHandler(this IServiceCollection services) =>
        services.AddExceptionHandler<GlobalExceptionHandler>();

    public static IServiceCollection LoggingConfig(this IServiceCollection services)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.Console()
            .WriteTo.File("log/loggingInfo", rollingInterval: RollingInterval.Day)
            .CreateLogger();
        services.AddSingleton(Log.Logger);
        return services;
    }

    public static IServiceCollection AddSwaggerConfig(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer(); 
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Personal Finance Tracker API",
                Version = "v1",
                Description = "API documentation for Personal Finance Tracker",
            });

            // Uncomment to include XML comments
            // var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            // var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            // options.IncludeXmlComments(xmlPath);
        });

        return services;
    }

    public static IServiceCollection AddEmailConfig(this IServiceCollection service, IConfiguration config)
    {
        string? username = config["DEFAULT_SMTP_USERNAME"];
        string? user = config["SMTP_EMAIL"];
        string? password = config["SMTP_PASSWORD"];
        Console.WriteLine(user);
        Console.WriteLine(password);
        SmtpClientOptions smtpClientOptions = new SmtpClientOptions
        {
            Server = config["SMTP_PROVIDER"],
            Port = Convert.ToInt32(config["SMTP_PORT"]),
            User = config["SMTP_EMAIL"],
            Password = config["SMTP_PASSWORD"],
            //UseSsl = true,
            RequiresAuthentication = true
        };

        service.AddFluentEmail(smtpClientOptions.User, username)
            .AddMailKitSender(smtpClientOptions);
        return service;
    }





    public static IServiceCollection AddAuthenticationConfig(this IServiceCollection service, IConfiguration config)
    {
        service.AddAuthentication(opt =>
            {
                opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidAlgorithms = [SecurityAlgorithms.HmacSha256Signature],
                    IssuerSigningKey =
                        new SymmetricSecurityKey(Encoding.ASCII.GetBytes(config["S_KEY"] ?? string.Empty)),
                    ValidateIssuerSigningKey = true,
                    ValidateAudience = true,
                    ValidateIssuer = true,
                    ValidateLifetime = true,
                    ValidIssuer = "roa.io",
                    ValidAudience = "personal-finance-app",
                    ClockSkew = TimeSpan.FromMinutes(5)
                };
            });
        return service;
    }

    public static IServiceCollection AddAuthorizationConfig(this IServiceCollection service) =>
        service.AddAuthorization(opt =>
        {
            opt.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
            opt.AddPolicy("CanViewResource",
                policy => policy.RequireClaim("Permission", "CanViewResource"));
        });
}