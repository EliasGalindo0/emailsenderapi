namespace EmailSenderApi.Models;

public class EmailResult
{
    public bool Success { get; set; }
    public int TotalSent { get; set; }
    public int TotalFailed { get; set; }
    public List<EmailDeliveryDetail> Details { get; set; } = new();
}

public class EmailDeliveryDetail
{
    public string Email { get; set; } = string.Empty;
    public bool Sent { get; set; }
    public string? Error { get; set; }
}
