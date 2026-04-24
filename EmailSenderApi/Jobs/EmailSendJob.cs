using EmailSenderApi.Models;
using EmailSenderApi.Services;
using Hangfire;

namespace EmailSenderApi.Jobs;

public class EmailSendJob
{
    private readonly IEmailService _emailService;
    private readonly ILogger<EmailSendJob> _logger;

    public EmailSendJob(IEmailService emailService, ILogger<EmailSendJob> logger)
    {
        _emailService = emailService;
        _logger = logger;
    }

    /// <summary>
    /// Retenta até 3 vezes em caso de falha total: após 5min, 15min e 30min.
    /// </summary>
    [AutomaticRetry(Attempts = 3, DelaysInSeconds = new[] { 300, 900, 1800 })]
    public async Task ExecuteAsync(EmailJobPayload payload)
    {
        _logger.LogInformation(
            "Iniciando job de envio. Total de destinatários: {Count}", payload.Emails.Count);

        int sent = 0, failed = 0;

        foreach (var entry in payload.Emails)
        {
            var request = new EmailRequest
            {
                Subject = entry.Subject,
                Body = entry.Body,
                IsHtml = payload.IsHtml,
                Recipients = new List<Recipient> { entry.Recipient }
            };

            var result = await _emailService.SendAsync(request);

            if (result.TotalSent > 0)
                sent++;
            else
            {
                failed++;
                _logger.LogWarning(
                    "Falha ao enviar para {Email}: {Error}",
                    entry.Recipient.Email, result.Details.FirstOrDefault()?.Error);
            }
        }

        _logger.LogInformation("Job concluído. Enviados: {Sent} | Falhas: {Failed}", sent, failed);

        if (sent == 0 && failed > 0)
            throw new InvalidOperationException(
                $"Todos os {failed} envio(s) falharam. Acionando retry.");
    }
}
