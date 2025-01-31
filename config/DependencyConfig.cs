using expense_tracker.Dto.Request.UserRequest;
using expense_tracker.Repo;
using expense_tracker.Repo.IRepo;
using expense_tracker.Service;
using expense_tracker.Service.IService;
using expense_tracker.Util;
using expense_tracker.Validation;
using FluentValidation;

namespace expense_tracker.config;

public static class DependencyConfig
{
    public static IServiceCollection CustomDependencyConfig(this IServiceCollection service)
    {
        service.AddScoped<IUserRepo, UserRepo>();
        service.AddScoped<IUserService, UserService>();

        service.AddScoped<RedisCaching>();
        service.AddTransient<OtpGenerator>();
        service.AddSingleton<RazorPageRenderer>();
        service.AddTransient<PasswordHasher>();
        service.AddScoped<JwtGenerator>();
        service.AddSingleton<PaginationMaker>();
        return service;
    }

    public static IServiceCollection FluentValidationConfig(this IServiceCollection service)
    {
        service.AddTransient<IValidator<SignUpByUapRequest>, SignupUapRequestValidation>();
        service.AddTransient<IValidator<NameReuqest>, NameRequestValidation>();
        service.AddTransient<IValidator<UserLoginRequest>, UserLoginRequestValidation>();

        return service;
    }
}