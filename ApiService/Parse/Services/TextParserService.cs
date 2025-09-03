using System.Text.Json;
using System.Text.RegularExpressions;
using ApiService.Parse.Models;

namespace ApiService.Parse.Services;

public class TextParserService
{
    private readonly GroqClient _groqClient;
    private readonly ILogger<TextParserService> _logger;
    private readonly string _model;

    private static readonly Regex PhoneRegex = new(@"(\+?\d{1,3}[-.\s]?)?\(?([0-9]{3})\)?[-.\s]?([0-9]{3})[-.\s]?([0-9]{4})");
    private static readonly Regex ZipRegex = new(@"\b\d{5}(?:-\d{4})?\b");

    public TextParserService(GroqClient groqClient, IConfiguration configuration, ILogger<TextParserService> logger)
    {
        _groqClient = groqClient;
        _logger = logger;
        _model = configuration["TextParserModel"] ?? "llama-3.3-70b-versatile";
    }

    public async Task<Person> ParseTextAsync(string inputText)
    {
        try
        {
            // Lightweight regex pre-pass
            var phoneMatch = PhoneRegex.Match(inputText);
            var zipMatch = ZipRegex.Match(inputText);

            var prompt = $@"Extract personal information from this text. Return ONLY valid JSON with these exact fields (use empty string for missing fields, never null):

{{
  ""name"": """",
  ""street"": """",
  ""city"": """",
  ""state"": """",
  ""country"": """",
  ""zip_code"": """",
  ""phone_number"": """"
}}

Text to parse: {inputText}

IMPORTANT: 
- Never fabricate values
- Preserve exact spellings and numbers from input
- Use empty string for missing fields
- Return only valid JSON";

            var response = await _groqClient.ChatCompletionAsync(_model, prompt);
            
            var person = JsonSerializer.Deserialize<Person>(response, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (person == null)
            {
                _logger.LogWarning("Failed to deserialize person data from GROQ response");
                return new Person();
            }

            // Apply regex results if GROQ missed them
            if (string.IsNullOrEmpty(person.PhoneNumber) && phoneMatch.Success)
            {
                person.PhoneNumber = phoneMatch.Value.Trim();
            }

            if (string.IsNullOrEmpty(person.ZipCode) && zipMatch.Success)
            {
                person.ZipCode = zipMatch.Value.Trim();
            }

            return person;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse JSON response from GROQ");
            throw new InvalidOperationException("Failed to parse response from AI model", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing text input");
            throw;
        }
    }
}