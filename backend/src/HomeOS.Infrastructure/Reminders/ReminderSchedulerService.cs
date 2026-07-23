using HomeOS.Application.Reminders.Commands;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HomeOS.Infrastructure.Reminders;

/// <summary>
/// Polls for due reminders once a minute. A simple timer loop rather than a
/// job-queue library (Hangfire, etc.) — at this scale a second moving part
/// with its own storage schema wasn't worth it.
/// </summary>
public class ReminderSchedulerService(IServiceScopeFactory scopeFactory, ILogger<ReminderSchedulerService> logger) : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromMinutes(1);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(Interval);
        do
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var sender = scope.ServiceProvider.GetRequiredService<ISender>();
                var fired = await sender.Send(new ProcessDueRemindersCommand(), stoppingToken);
                if (fired > 0)
                {
                    logger.LogInformation("Fired {Count} due reminder(s)", fired);
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Reminder scan failed");
            }
        } while (await timer.WaitForNextTickAsync(stoppingToken));
    }
}
