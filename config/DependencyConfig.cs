using Personal_finance_tracker.utils;

namespace expense_tracker.config;

public static class DependencyConfig
{
    public static IServiceCollection CustomDependencyConfig(this IServiceCollection service)
    {



        service.AddSingleton<RedisCaching>();
        service.AddSingleton<OtpGenerator>();
        service.AddSingleton<RazorPageRenderer>();
        service.AddSingleton<PasswordHasher>();
        service.AddSingleton<JwtGenerator>();
        service.AddSingleton<PaginationMaker>();
        return service;
    }

    public static IServiceCollection FluentValidationConfig(this IServiceCollection service)
    {
        return service;
    }
}