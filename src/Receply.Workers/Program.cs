using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Quartz;
using Receply.Application;
using Receply.Infrastructure;
using Receply.Workers.Jobs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddQuartz(q =>
{
    var jobKey = new JobKey(nameof(AppointmentReminderJob));
    q.AddJob<AppointmentReminderJob>(opts => opts.WithIdentity(jobKey));
    q.AddTrigger(opts => opts
        .ForJob(jobKey)
        .WithIdentity($"{nameof(AppointmentReminderJob)}-trigger")
        .WithSimpleSchedule(s => s.WithIntervalInHours(1).RepeatForever()));
});

var app = builder.Build();

// Azure App Service for Containers pings this to confirm the container started - the worker
// itself does its actual work via the Quartz-hosted background service registered above.
app.MapGet("/healthz", () => Results.Ok());

app.Run();
