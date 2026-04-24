using EmailSenderApi.Models;
using EmailSenderApi.Settings;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace EmailSenderApi.Services;

public class EmailService : IEmailService
{
    private readonly SmtpSettings _settings;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IOptions<SmtpSettings> settings, ILogger<EmailService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<EmailResult> SendAsync(EmailRequest request, CancellationToken cancellationToken = default)
    {
        var result = new EmailResult();

        using var client = new SmtpClient();

        var socketOptions = _settings.UseSsl
            ? SecureSocketOptions.StartTls
            : SecureSocketOptions.None;

        await client.ConnectAsync(_settings.Host, _settings.Port, socketOptions, cancellationToken);

        if (!string.IsNullOrEmpty(_settings.Username))
            await client.AuthenticateAsync(_settings.Username, _settings.Password, cancellationToken);

        foreach (var recipient in request.Recipients)
        {
            var detail = new EmailDeliveryDetail { Email = recipient.Email };

            try
            {
                var message = BuildMessage(request, recipient);
                await client.SendAsync(message, cancellationToken);

                detail.Sent = true;
                result.TotalSent++;
                _logger.LogInformation("E-mail enviado para {Email}", recipient.Email);
            }
            catch (Exception ex)
            {
                detail.Sent = false;
                detail.Error = ex.Message;
                result.TotalFailed++;
                _logger.LogError(ex, "Falha ao enviar e-mail para {Email}", recipient.Email);
            }

            result.Details.Add(detail);
        }

        await client.DisconnectAsync(true, cancellationToken);

        result.Success = result.TotalFailed == 0;
        return result;
    }

    private MimeMessage BuildMessage(EmailRequest request, Recipient recipient)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_settings.SenderName, _settings.SenderEmail));
        message.To.Add(new MailboxAddress(recipient.Name, recipient.Email));
        message.Subject = request.Subject;

        var bodyBuilder = new BodyBuilder();
        if (request.IsHtml)
            bodyBuilder.HtmlBody = request.Body;
        else
            bodyBuilder.TextBody = request.Body;

        message.Body = bodyBuilder.ToMessageBody();
        return message;
    }
}
