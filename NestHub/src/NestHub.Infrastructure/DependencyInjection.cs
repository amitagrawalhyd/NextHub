using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NestHub.Application.Common.Interfaces;
using NestHub.Domain.Repositories;
using NestHub.Infrastructure.Notifications;
using NestHub.Infrastructure.Persistence;
using NestHub.Infrastructure.Persistence.Repositories;
using NestHub.Infrastructure.Services;
using NestHub.Infrastructure.Services.Ai;

namespace NestHub.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<NestHubDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("NestHubDatabase")));

        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));

        services.AddHttpContextAccessor();
        services.AddSignalR();

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();

        services.AddScoped<ISocietyRepository, SocietyRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IResidentRepository, ResidentRepository>();
        services.AddScoped<IVendorRepository, VendorRepository>();
        services.AddScoped<IReviewRepository, ReviewRepository>();
        services.AddScoped<ISosRequestRepository, SosRequestRepository>();
        services.AddScoped<IAnalyticsLogRepository, AnalyticsLogRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();

        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
        services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddSingleton<IAiService, LocalAiService>();
        services.AddScoped<INotificationService, SosNotificationService>();

        services.AddSingleton<IFileStorageService>(_ =>
            new LocalFileStorageService(Path.Combine(AppContext.BaseDirectory, "wwwroot", "uploads")));

        return services;
    }
}
