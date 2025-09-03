using System.Text.Json;
using ApiService.Parse.Models;

namespace ApiService.Parse.Services;

public class OpenAiImageParserService
{
    private readonly OpenAiClient _openAiClient;
    private readonly ILogger<OpenAiImageParserService> _logger;

    public OpenAiImageParserService(OpenAiClient openAiClient, ILogger<OpenAiImageParserService> logger)
    {
        _openAiClient = openAiClient;
        _logger = logger;
    }

    public async Task<IdParsed> ParseImageAsync(IFormFile imageFile)
    {
        try
        {
            _logger.LogInformation("Starting image parsing for file: {FileName}, Size: {Size} bytes", 
                imageFile.FileName, imageFile.Length);
            
            return await ParseImageInternalAsync(null, imageFile);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in ParseImageAsync for file: {FileName}", imageFile.FileName);
            throw;
        }
    }

    public async Task<IdParsed> ParseImageFromUrlAsync(string imageUrl)
    {
        return await ParseImageInternalAsync(imageUrl, null);
    }

    private async Task<IdParsed> ParseImageInternalAsync(string? imageUrl, IFormFile? imageFile)
    {
        try
        {
            var prompt = @"Extract driver's license information from this image. Return ONLY valid JSON with these exact fields (use empty string for missing fields):

{
  ""fullName"": """",
  ""dateOfBirth"": """",
  ""address"": {
    ""street"": """",
    ""city"": """",
    ""state"": """",
    ""country"": """",
    ""zipCode"": """"
  },
  ""documentNumber"": """",
  ""expirationDate"": """",
  ""issueDate"": """",
  ""licenseClass"": """",
  ""endorsements"": """",
  ""restrictions"": """",
  ""sex"": """",
  ""eyeColor"": """",
  ""height"": """",
  ""detectedCountry"": """",
  ""detectedState"": """",
  ""barcodePresent"": false,
  ""warnings"": [],
  ""confidences"": {},
  ""boxes"": {}
}

IMPORTANT:
- Return only valid JSON
- Normalize dates to yyyy-mm-dd format when certain
- Add warnings for uncertain data
- Never fabricate information
- Use empty strings for missing fields";

            var response = await _openAiClient.CallVisionAsync(prompt, imageUrl, imageFile);
            
            var result = await ParseOpenAiResponse(response, imageUrl, imageFile);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing image");
            throw;
        }
    }

    private async Task<IdParsed> ParseOpenAiResponse(string response, string? imageUrl, IFormFile? imageFile)
    {
        try
        {
            var result = JsonSerializer.Deserialize<IdParsed>(response, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (result == null)
            {
                throw new InvalidOperationException("Failed to deserialize response");
            }

            result = PostProcessResult(result);
            
            return result;
        }
        catch (JsonException ex)
        {
            _logger.LogWarning("Invalid JSON response, retrying with stricter prompt");
            
            var retryPrompt = "The previous response was not valid JSON. Return only a valid JSON object with the driver's license data. No explanatory text.";
            var retryResponse = await _openAiClient.CallVisionAsync(retryPrompt, imageUrl, imageFile);
            
            try
            {
                var result = JsonSerializer.Deserialize<IdParsed>(retryResponse, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                
                return result ?? new IdParsed();
            }
            catch (JsonException)
            {
                _logger.LogError("Failed to parse JSON response from OpenAI after retry");
                throw new InvalidOperationException("Failed to parse response from AI model", ex);
            }
        }
    }

    private IdParsed PostProcessResult(IdParsed result)
    {
        if (!string.IsNullOrEmpty(result.DocumentNumber) && result.DocumentNumber.Length > 4)
        {
            _logger.LogInformation("Processed document with number ending in: {LastFour}", result.DocumentNumber.Substring(result.DocumentNumber.Length - 4));
        }

        if (!string.IsNullOrEmpty(result.DateOfBirth) && !IsValidDateFormat(result.DateOfBirth))
        {
            result.Warnings.Add("Date of birth format may be uncertain");
        }
        
        if (!string.IsNullOrEmpty(result.ExpirationDate) && !IsValidDateFormat(result.ExpirationDate))
        {
            result.Warnings.Add("Expiration date format may be uncertain");
        }

        if (!string.IsNullOrEmpty(result.IssueDate) && !IsValidDateFormat(result.IssueDate))
        {
            result.Warnings.Add("Issue date format may be uncertain");
        }

        return result;
    }

    private static bool IsValidDateFormat(string date)
    {
        return DateTime.TryParseExact(date, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out _);
    }
}