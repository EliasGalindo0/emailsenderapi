using EmailSenderApi.Models;
using EmailSenderApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace EmailSenderApi.Controllers;

[ApiController]
[Route("api/email")]
public class EmailScheduleController : ControllerBase
{
    private readonly ITemplateService _templateService;
    private readonly IEmailQueueService _queueService;

    public EmailScheduleController(ITemplateService templateService, IEmailQueueService queueService)
    {
        _templateService = templateService;
        _queueService = queueService;
    }

    /// <summary>
    /// Enfileira ou agenda o envio usando um template com variáveis individuais por destinatário.
    /// O corpo e assunto são renderizados separadamente para cada destinatário antes de enfileirar.
    /// Se ScheduledAt (UTC) for informado, o envio ocorre naquela data/hora.
    /// </summary>
    [HttpPost("queue")]
    [ProducesResponseType(typeof(QueueResponse), StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Queue([FromBody] TemplateEmailRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.TemplateId))
            return BadRequest("O ID do template é obrigatório.");

        if (!request.Recipients.Any())
            return BadRequest("A lista de destinatários não pode estar vazia.");

        var template = _templateService.GetById(request.TemplateId);
        if (template is null)
            return NotFound($"Template '{request.TemplateId}' não encontrado.");

        // Renderiza o conteúdo individualmente para cada destinatário
        var renderedEmails = request.Recipients.Select(r => new RenderedRecipientEmail
        {
            Subject = _templateService.Render(template.Subject, r.Variables),
            Body = _templateService.Render(template.Body, r.Variables),
            Recipient = new Recipient { Name = r.Name, Email = r.Email }
        }).ToList();

        var payload = new EmailJobPayload
        {
            IsHtml = template.IsHtml,
            Emails = renderedEmails
        };

        string jobId;
        string status;

        if (request.ScheduledAt.HasValue && request.ScheduledAt.Value > DateTime.UtcNow)
        {
            jobId = _queueService.Schedule(payload, request.ScheduledAt.Value);
            status = "scheduled";
        }
        else
        {
            jobId = _queueService.Enqueue(payload);
            status = "enqueued";
        }

        return Accepted(new QueueResponse
        {
            JobId = jobId,
            Status = status,
            ScheduledAt = request.ScheduledAt,
            TotalRecipients = request.Recipients.Count
        });
    }
}
