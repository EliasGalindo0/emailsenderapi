using EmailSenderApi.Models;

namespace EmailSenderApi.Services;

public interface IEmailQueueService
{
    /// <summary>
    /// Enfileira o envio para execução imediata em background.
    /// </summary>
    string Enqueue(EmailJobPayload payload);

    /// <summary>
    /// Agenda o envio para uma data/hora específica (UTC).
    /// </summary>
    string Schedule(EmailJobPayload payload, DateTime scheduledAtUtc);
}
