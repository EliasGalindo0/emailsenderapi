namespace EmailSenderApi.Models;

public class EmailRequest
{
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public bool IsHtml { get; set; } = false;
    public List<Recipient> Recipients { get; set; } = new();
}
