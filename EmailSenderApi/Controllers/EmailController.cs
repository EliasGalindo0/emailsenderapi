using EmailSenderApi.Models;
using EmailSenderApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace EmailSenderApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmailController : ControllerBase
{
    private readonly IEmailService _emailService;

    public EmailController(IEmailService emailService)
    {
        _emailService = emailService;
    }

    /// <summary>
    /// Envia e-mail para uma lista de destinatários.
    /// </summary>
    [HttpPost("send")]
    [ProducesResponseType(typeof(EmailResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Send([FromBody] EmailRequest request, CancellationToken cancellationToken)
    {
        if (!request.Recipients.Any())
            return BadRequest("A lista de destinatários não pode estar vazia.");

        if (string.IsNullOrWhiteSpace(request.Subject))
            return BadRequest("O assunto do e-mail é obrigatório.");

        if (string.IsNullOrWhiteSpace(request.Body))
            return BadRequest("O corpo do e-mail é obrigatório.");

        var result = await _emailService.SendAsync(request, cancellationToken);
        return Ok(result);
    }
}
