using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Receply.Application.AiEngine;
using Receply.Application.Auth;
using Receply.Application.Channels;
using Receply.Application.Common;
using Receply.Application.Translation;
using Receply.Infrastructure.AiEngine;
using Receply.Infrastructure.Auth;
using Receply.Infrastructure.Channels.WhatsApp;
using Receply.Infrastructure.Email;
using Receply.Infrastructure.Multitenancy;
using Receply.Infrastructure.Persistence;
using Receply.Infrastructure.Security;
using Receply.Infrastructure.Translation;
using StackExchange.Redis;

namespace Receply.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<AuditSaveChangesInterceptor>();
        services.AddDbContext<ReceplyDbContext>((sp, options) =>
            options.UseNpgsql(configuration.GetConnectionString("Postgres"))
                .AddInterceptors(sp.GetRequiredService<AuditSaveChangesInterceptor>()));
        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ReceplyDbContext>());

        services.AddSingleton<IConnectionMultiplexer>(_ =>
            ConnectionMultiplexer.Connect(configuration.GetConnectionString("Redis") ?? "localhost:6379"));

        services.AddScoped<ITenantContext, TenantContext>();
        services.AddScoped<ICurrentUserContext, CurrentUserContext>();

        services.Configure<WhatsAppOptions>(configuration.GetSection(WhatsAppOptions.SectionName));
        services.AddHttpClient<IChannelProvider, WhatsAppCloudApiProvider>();

        services.Configure<EncryptionOptions>(configuration.GetSection(EncryptionOptions.SectionName));
        services.AddSingleton<IChannelCredentialProtector, AesGcmChannelCredentialProtector>();

        services.Configure<OpenAiOptions>(configuration.GetSection(OpenAiOptions.SectionName));
        services.AddSingleton<IAiConversationAgent, OpenAiConversationAgent>();
        services.AddSingleton<ITranslationService, OpenAiTranslationService>();

        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<IOtpSender, LoggingOtpSender>();

        services.Configure<SmtpOptions>(configuration.GetSection(SmtpOptions.SectionName));
        services.AddScoped<IEmailSender, SmtpEmailSender>();

        services.AddQuartz();
        services.AddQuartzHostedService(opts => opts.WaitForJobsToComplete = true);

        return services;
    }
}
