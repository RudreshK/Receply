using System.Reflection;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Receply.Application.AiEngine;
using Receply.Application.Common.Behaviors;
using Receply.Application.Conversations;

namespace Receply.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));
        services.AddValidatorsFromAssembly(assembly);
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        services.AddScoped<IAiToolExecutor, AiToolExecutor>();
        services.AddSingleton<IConversationNotifier, NullConversationNotifier>();

        return services;
    }
}
