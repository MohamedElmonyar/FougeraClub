using Application.Mapping;
using Application.Services;
using Application.Validation.PurchaseOrders;
using FluentValidation;
using Infrastructure.Data;
using Infrastructure.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Threading.RateLimiting;

public static class ServiceExtensions
{
    // =========================================================
    // 1. Infrastructure Layer (Database & Repositories)
    // =========================================================
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        // 1. Database Context
        services.AddDbContext<Context>(options =>
            options.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(10),
                    errorNumbersToAdd: null);
            }));

        // 2. Scrutor for Repositories & Services (تعديل هنا) 🔥
        // خلينا الـ Scan يشوف الـ Repository وكمان الـ Service اللي في الانفرا (زي NotificationService)
        services.Scan(scan => scan
            .FromAssemblies(typeof(PurchaseOrderRepository).Assembly)
            .AddClasses(classes => classes.Where(type =>
                type.Name.EndsWith("Repository") ||
                type.Name.EndsWith("Service"))) // <--- ضفنا الشرط ده
            .AsImplementedInterfaces()
            .WithScopedLifetime());

        return services;
    }

    // =========================================================
    // 3. Rate Limiting Configuration (API Protection)
    // =========================================================
    public static IServiceCollection AddRateLimitingConfig(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
            {
                return RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    factory: partition => new FixedWindowRateLimiterOptions
                    {
                        AutoReplenishment = true,
                        PermitLimit = 100,
                        Window = TimeSpan.FromMinutes(1),
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 2
                    });
            });

            options.OnRejected = async (context, token) =>
            {
                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                await context.HttpContext.Response.WriteAsync("Too many requests. Please try again later.", token);
            };
        });

        return services;
    }

    // =========================================================
    // 4. Application Layer (Services, Mapper, Validators)
    // =========================================================
    public static IServiceCollection AddApplicationLayer(this IServiceCollection services, IConfiguration configuration)
    {
        // 1. AutoMapper
        // استخدام Overload آمن لتجنب مشاكل الإصدارات
        services.AddAutoMapper(cfg => { }, typeof(PurchaseOrderProfile).Assembly);

        // 2. FluentValidation
        services.AddValidatorsFromAssembly(typeof(SavePurchaseOrderValidator).Assembly);

        // 3. Scrutor for Services (Auto Register)
        // يسجل أي خدمة تنتهي بـ "Service"
        services.Scan(scan => scan
            .FromAssemblies(typeof(PurchaseOrderService).Assembly)
            .AddClasses(classes => classes.Where(type => type.Name.EndsWith("Service")))
            .AsImplementedInterfaces()
            .WithScopedLifetime());

        return services;
    }

    // =========================================================
    // 5. Presentation Layer & Localization
    // =========================================================
    public static IServiceCollection AddPresentationLayer(this IServiceCollection services)
    {
        // 1. تفعيل الترجمة العامة (Resources Path)

        // 2. تفعيل الـ MVC مع دعم الترجمة للـ Views وللـ Validation
        // ده السطر اللي كان ناقص عشان الـ Localizer يشتغل في الـ HTML
        services.AddControllersWithViews()
                .AddViewLocalization(Microsoft.AspNetCore.Mvc.Razor.LanguageViewLocationExpanderFormat.Suffix)
                .AddDataAnnotationsLocalization();

        return services;
    }

}