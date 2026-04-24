using EmailSenderApi.Models;
using EmailSenderApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace EmailSenderApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TemplateController : ControllerBase
{
    private readonly ITemplateService _templateService;

    public TemplateController(ITemplateService templateService)
    {
        _templateService = templateService;
    }

    /// <summary>
    /// Lista todos os templates cadastrados.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<EmailTemplate>), StatusCodes.Status200OK)]
    public IActionResult GetAll() => Ok(_templateService.GetAll());

    /// <summary>
    /// Retorna um template pelo ID.
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(EmailTemplate), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetById(string id)
    {
        var template = _templateService.GetById(id);
        return template is null ? NotFound() : Ok(template);
    }

    /// <summary>
    /// Cria um novo template de e-mail com suporte a variáveis (ex: {{Nome}}, {{Empresa}}).
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(EmailTemplate), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult Create([FromBody] EmailTemplate template)
    {
        if (string.IsNullOrWhiteSpace(template.Name))
            return BadRequest("O nome do template é obrigatório.");

        if (string.IsNullOrWhiteSpace(template.Subject))
            return BadRequest("O assunto do template é obrigatório.");

        if (string.IsNullOrWhiteSpace(template.Body))
            return BadRequest("O corpo do template é obrigatório.");

        var created = _templateService.Create(template);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>
    /// Remove um template pelo ID.
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Delete(string id)
    {
        return _templateService.Delete(id) ? NoContent() : NotFound();
    }
}
