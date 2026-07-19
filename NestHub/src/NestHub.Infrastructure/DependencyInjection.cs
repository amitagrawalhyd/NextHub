using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NestHub.Application.Common.Interfaces;
using NestHub.Domain.Repositories;
using NestHub.Domain.Vendors;
using NestHub.Infrastructure.Notifications;
using NestHub.Infrastructure.Persistence;
using NestHub.Infrastructure.Persistence.EventHandlers;
using NestHub.Infrastructure.Persistence.Repositories;
using NestHub.Infrastructure.Services;
using NestHub.Infrastructure.Services.Ai;

namespace NestHub.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<NestHubDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("NestHubDatabase"), sql => sql.UseNetTopologySuite()));

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
        services.AddScoped<IVendorSocietyCoverageRepository, VendorSocietyCoverageRepository>();
        services.AddScoped<IAnnouncementRepository, AnnouncementRepository>();
        services.AddScoped<IEmergencyContactRepository, EmergencyContactRepository>();
        services.AddScoped<IVendorFavoriteRepository, VendorFavoriteRepository>();
        services.AddScoped<IVendorBroadcastRepository, VendorBroadcastRepository>();
        services.AddScoped<IVendorMuteRepository, VendorMuteRepository>();

        // First domain-event handler in the codebase: MediatR only assembly-scans
        // NestHub.Application (see Application.DependencyInjection), and this handler must live
        // here because DomainEventNotification<T> is an Infrastructure-only type — so it's wired
        // explicitly rather than picked up by the scan.
        services.AddTransient<INotificationHandler<DomainEventNotification<VendorLocationChangedDomainEvent>>, VendorLocationChangedDomainEventHandler>();

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
