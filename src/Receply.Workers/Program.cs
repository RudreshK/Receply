using Quartz;
using Receply.Application;
using Receply.Infrastructure;
using Receply.Workers.Jobs;

var builder = Host.CreateApplicationBuilder(args);

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

var host = builder.Build();
host.Run();
