namespace EmailSenderApi.Models;

public class TemplateEmailRequest
{
    public string TemplateId { get; set; } = string.Empty;

    /// <summary>
    /// Data/hora agendada (UTC). Null = enfileira para envio imediato.
    /// </summary>
    public DateTime? ScheduledAt { get; set; }

    public List<RecipientWithVariables> Recipients { get; set; } = new();
}
