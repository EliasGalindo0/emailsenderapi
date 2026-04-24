namespace EmailSenderApi.Models;

public class QueueResponse
{
    public string JobId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime? ScheduledAt { get; set; }
    public int TotalRecipients { get; set; }
}
