using EmailSenderApi.Models;

namespace EmailSenderApi.Services;

public interface IEmailService
{
    Task<EmailResult> SendAsync(EmailRequest request, CancellationToken cancellationToken = default);
}
