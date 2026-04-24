using EmailSenderApi.Jobs;
using EmailSenderApi.Models;
using Hangfire;

namespace EmailSenderApi.Services;

public class EmailQueueService : IEmailQueueService
{
    private readonly IBackgroundJobClient _jobClient;

    public EmailQueueService(IBackgroundJobClient jobClient)
    {
        _jobClient = jobClient;
    }

    public string Enqueue(EmailJobPayload payload) =>
        _jobClient.Enqueue<EmailSendJob>(job => job.ExecuteAsync(payload));

    public string Schedule(EmailJobPayload payload, DateTime scheduledAtUtc)
    {
        var delay = scheduledAtUtc - DateTime.UtcNow;
        if (delay <= TimeSpan.Zero)
            return Enqueue(payload);

        return _jobClient.Schedule<EmailSendJob>(
            job => job.ExecuteAsync(payload),
            delay);
    }
}
