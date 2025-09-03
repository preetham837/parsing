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
    private readonly ImageParserService _imageParserService;
    private readonly LookupService _lookupService;
    private readonly ILogger<ParseController> _logger;

    public ParseController(
        TextParserService textParserService,
        ImageParserService imageParserService,
        LookupService lookupService,
        ILogger<ParseController> logger)
    {
        _textParserService = textParserService;
        _imageParserService = imageParserService;
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

    [HttpPost("id", Name = nameof(ParseId))]
    [OpenApiOperation("Parse ID document information", @"Parse driver's license information from image, URL, or text fallback, or lookup stored ID data by ID.

**How it works:**
1. If you provide an `id` → returns stored ID data
2. If you provide an image file → AI extracts driver's license information using GROQ vision (meta-llama/llama-4-scout-17b-16e-instruct)
3. If you provide `image_url` → Downloads and processes the image 
4. If you provide `input_text` → Falls back to text parsing

**Example usage:** Upload a driver's license image to extract structured information including name, address, document numbers, dates, and physical characteristics.")]
    [ProducesResponseType(typeof(ParseIdResponse), StatusCodes.Status200OK, "application/json")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ParseIdResponse>> ParseId([FromForm] string? id = null, [FromForm] string? inputText = null, [FromForm] string? imageUrl = null, IFormFile? image = null)
    {
        try
        {
            // ID lookup first
            if (!string.IsNullOrEmpty(id))
            {
                var storedIdData = _lookupService.GetIdParsedById(id);
                if (storedIdData != null)
                {
                    _logger.LogInformation("Returning stored ID data for ID: {Id}", id);
                    return Ok(new ParseIdResponse
                    {
                        Source = "id_lookup",
                        Data = storedIdData
                    });
                }
                
                _logger.LogWarning("ID data with ID {Id} not found, falling back to image parsing", id);
            }

            // Image parsing (file upload)
            if (image != null)
            {
                _logger.LogInformation("Parsing uploaded image file: {FileName}, Size: {Size} bytes", 
                    image.FileName, image.Length);
                
                // Convert IFormFile to base64 for GROQ ImageParserService
                using var memoryStream = new MemoryStream();
                await image.CopyToAsync(memoryStream);
                var imageBytes = memoryStream.ToArray();
                var base64Image = Convert.ToBase64String(imageBytes);
                
                var idParsed = await _imageParserService.ParseImageAsync(base64Image);
                
                return Ok(new ParseIdResponse
                {
                    Source = "image",
                    Data = idParsed
                });
            }

            // Image parsing (URL)
            if (!string.IsNullOrWhiteSpace(imageUrl))
            {
                _logger.LogInformation("Parsing image from URL");
                var idParsed = await _imageParserService.ParseImageFromUrlAsync(imageUrl);
                
                return Ok(new ParseIdResponse
                {
                    Source = "image",
                    Data = idParsed
                });
            }

            // Text fallback
            if (!string.IsNullOrWhiteSpace(inputText))
            {
                _logger.LogInformation("Falling back to text parsing");
                var person = await _textParserService.ParseTextAsync(inputText);
                
                // Convert Person to IdParsed for consistency
                var idParsed = new IdParsed
                {
                    FullName = person.Name,
                    Address = new IdAddress
                    {
                        Street = person.Street,
                        City = person.City,
                        State = person.State,
                        Country = person.Country,
                        ZipCode = person.ZipCode
                    }
                };
                
                return Ok(new ParseIdResponse
                {
                    Source = "text",
                    Data = idParsed
                });
            }

            return BadRequest("Either 'id' for lookup, image file/URL for parsing, or 'inputText' for fallback is required");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing parse ID request. Exception Type: {ExceptionType}, Message: {Message}", 
                ex.GetType().Name, ex.Message);
            
            // Log inner exceptions for better debugging
            var innerEx = ex.InnerException;
            while (innerEx != null)
            {
                _logger.LogError("Inner Exception - Type: {ExceptionType}, Message: {Message}", 
                    innerEx.GetType().Name, innerEx.Message);
                innerEx = innerEx.InnerException;
            }
            
            // In development, return detailed error information
            var isDevelopment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";
            if (isDevelopment)
            {
                return Problem(
                    detail: $"Exception: {ex.GetType().Name} - {ex.Message}\nStackTrace: {ex.StackTrace}",
                    title: "An error occurred while processing your request.");
            }
            
            return Problem("An error occurred while processing the request");
        }
    }
}