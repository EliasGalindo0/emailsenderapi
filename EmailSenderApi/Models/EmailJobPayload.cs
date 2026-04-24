namespace EmailSenderApi.Models;

/// <summary>
/// Payload serializado pelo Hangfire. Cada entrada já tem o conteúdo renderizado
/// individualmente para o respectivo destinatário.
/// </summary>
public class EmailJobPayload
{
    public bool IsHtml { get; set; }
    public List<RenderedRecipientEmail> Emails { get; set; } = new();
}

public class RenderedRecipientEmail
{
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public Recipient Recipient { get; set; } = new();
}
