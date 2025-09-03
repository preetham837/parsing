using Microsoft.AspNetCore.Mvc;
using ApiService.Parse.Models;
using ApiService.Parse.Services;
using NSwag.Annotations;

namespace ApiService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ParseController : ControllerBase
{
    private readonly TextParserService _textParserService;
    private readonly LookupService _lookupService;
    private readonly ILogger<ParseController> _logger;

    public ParseController(
        TextParserService textParserService, 
        LookupService lookupService,
        ILogger<ParseController> logger)
    {
        _textParserService = textParserService;
        _lookupService = lookupService;
        _logger = logger;
    }

    [HttpPost(Name = nameof(Parse))]
    [OpenApiOperation("Parse personal information from text", @"Parse personal information from a sentence using AI, or lookup stored user data by ID.

**How it works:**
1. If you provide an `id` → returns stored person data (like 'jim-croce')
2. If you provide `input_text` → AI extracts: Name, Street, City, State, Country, Phone Number, Zip Code

**Example text input:** 
'My name is Jim Croce, I live in 2944 Monaco dr, Manchester, Colorado, USA, 92223. My phone number is 893-366-8888.'

**Available IDs:** jim-croce, person-1, person-2, person-3")]
    [ProducesResponseType(typeof(ParseResponse), StatusCodes.Status200OK, "application/json")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ParseResponse>> Parse([FromBody] ParseRequest request)
    {
        try
        {
            if (!string.IsNullOrEmpty(request.Id))
            {
                var storedPerson = _lookupService.GetPersonById(request.Id);
                if (storedPerson != null)
                {
                    _logger.LogInformation("Returning stored person data for ID: {Id}", request.Id);
                    return Ok(new ParseResponse
                    {
                        Source = "id_lookup",
                        Data = storedPerson
                    });
                }
                
                _logger.LogWarning("Person with ID {Id} not found, falling back to text parsing", request.Id);
            }

            if (string.IsNullOrWhiteSpace(request.InputText))
            {
                return BadRequest("Either 'id' for lookup or 'input_text' for AI parsing is required");
            }

            _logger.LogInformation("Parsing text input");
            var person = await _textParserService.ParseTextAsync(request.InputText);
            
            return Ok(new ParseResponse
            {
                Source = "text",
                Data = person
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing parse request");
            return Problem("An error occurred while processing the request");
        }
    }
}