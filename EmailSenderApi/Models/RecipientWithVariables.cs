namespace EmailSenderApi.Models;

public class RecipientWithVariables : Recipient
{
    /// <summary>
    /// Variáveis para substituição no template.
    /// Exemplo: { "{{Nome}}": "João", "{{Empresa}}": "ACME" }
    /// </summary>
    public Dictionary<string, string> Variables { get; set; } = new();
}
