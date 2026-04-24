using System.Collections.Concurrent;
using EmailSenderApi.Models;

namespace EmailSenderApi.Services;

public class TemplateService : ITemplateService
{
    private readonly ConcurrentDictionary<string, EmailTemplate> _store = new();

    public EmailTemplate Create(EmailTemplate template)
    {
        template.Id = Guid.NewGuid().ToString("N")[..8];
        template.CreatedAt = DateTime.UtcNow;
        _store[template.Id] = template;
        return template;
    }

    public EmailTemplate? GetById(string id) =>
        _store.TryGetValue(id, out var template) ? template : null;

    public IEnumerable<EmailTemplate> GetAll() =>
        _store.Values.OrderByDescending(t => t.CreatedAt);

    public bool Delete(string id) => _store.TryRemove(id, out _);

    public string Render(string text, Dictionary<string, string> variables)
    {
        foreach (var (key, value) in variables)
            text = text.Replace(key, value, StringComparison.OrdinalIgnoreCase);

        return text;
    }
}
