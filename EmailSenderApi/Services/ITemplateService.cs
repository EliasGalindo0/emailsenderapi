using EmailSenderApi.Models;

namespace EmailSenderApi.Services;

public interface ITemplateService
{
    EmailTemplate Create(EmailTemplate template);
    EmailTemplate? GetById(string id);
    IEnumerable<EmailTemplate> GetAll();
    bool Delete(string id);

    /// <summary>
    /// Substitui as variáveis do template pelo valor informado para o destinatário.
    /// </summary>
    string Render(string text, Dictionary<string, string> variables);
}
