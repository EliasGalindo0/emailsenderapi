namespace EmailSenderApi.Models;

public class EmailTemplate
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Assunto com suporte a variáveis: ex. "Olá, {{Nome}}!"
    /// </summary>
    public string Subject { get; set; } = string.Empty;

    /// <summary>
    /// Corpo com suporte a variáveis: ex. "Bem-vindo, {{Nome}}, da empresa {{Empresa}}."
    /// </summary>
    public string Body { get; set; } = string.Empty;

    public bool IsHtml { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
