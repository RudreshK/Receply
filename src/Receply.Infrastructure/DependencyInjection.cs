using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Receply.Application.AiEngine;
using Receply.Application.Channels;
using Receply.Application.Common;
using Receply.Infrastructure.AiEngine;
using Receply.Infrastructure.Channels.WhatsApp;
using Receply.Infrastructure.Multitenancy;
using Receply.Infrastructure.Persistence;
using StackExchange.Redis;

namespace Receply.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ReceplyDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Postgres")));
        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ReceplyDbContext>());

        services.AddSingleton<IConnectionMultiplexer>(_ =>
            ConnectionMultiplexer.Connect(configuration.GetConnectionString("Redis") ?? "localhost:6379"));

        services.AddScoped<ITenantContext, TenantContext>();

        services.Configure<WhatsAppOptions>(configuration.GetSection(WhatsAppOptions.SectionName));
        services.AddHttpClient<IChannelProvider, WhatsAppCloudApiProvider>();

        services.Configure<ClaudeOptions>(configuration.GetSection(ClaudeOptions.SectionName));
        services.AddSingleton<IAiConversationAgent, ClaudeConversationAgent>();

        services.AddQuartz();
        services.AddQuartzHostedService(opts => opts.WaitForJobsToComplete = true);

        return services;
    }
}
