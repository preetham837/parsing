using System.Text.Json;
using ApiService.Parse.Models;

namespace ApiService.Parse.Services;

public class ImageParserService
{
    private readonly GroqClient _groqClient;
    private readonly HttpClient _httpClient;
    private readonly ILogger<ImageParserService> _logger;
    private readonly string _model;

    public ImageParserService(GroqClient groqClient, HttpClient httpClient, ILogger<ImageParserService> logger, IConfiguration configuration)
    {
        _groqClient = groqClient;
        _httpClient = httpClient;
        _logger = logger;
        _model = configuration["ImageParserModel"] ?? "llama-3.2-90b-vision-preview";
    }

    public async Task<IdParsed> ParseImageAsync(string imageBase64)
    {
        try
        {
            var prompt = @"Extract driver's license information from this image. Return ONLY valid JSON with these exact fields (use empty string for missing fields):

{
  ""full_name"": """",
  ""date_of_birth"": """",
  ""address"": {
    ""street"": """",
    ""city"": """",
    ""state"": """",
    ""country"": """",
    ""zip_code"": """"
  },
  ""document_number"": """",
  ""expiration_date"": """",
  ""issue_date"": """",
  ""license_class"": """",
  ""endorsements"": """",
  ""restrictions"": """",
  ""sex"": """",
  ""eye_color"": """",
  ""height"": """",
  ""detected_country"": """",
  ""detected_state"": """",
  ""barcode_present"": false,
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

            var response = await _groqClient.ChatCompletionAsync(_model, prompt, imageBase64);
            
            var result = await ParseGroqResponse(response);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing image");
            throw;
        }
    }

    public async Task<IdParsed> ParseImageFromUrlAsync(string imageUrl)
    {
        try
        {
            var response = await _httpClient.GetAsync(imageUrl);
            response.EnsureSuccessStatusCode();
            
            var imageBytes = await response.Content.ReadAsByteArrayAsync();
            var imageBase64 = Convert.ToBase64String(imageBytes);
            
            return await ParseImageAsync(imageBase64);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading and parsing image from URL: {Url}", imageUrl);
            throw;
        }
    }

    private async Task<IdParsed> ParseGroqResponse(string response)
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

            // Post-process dates and add warnings
            result = PostProcessResult(result);
            
            return result;
        }
        catch (JsonException ex)
        {
            _logger.LogWarning("Invalid JSON response, retrying with stricter prompt");
            
            // Retry with stricter instruction
            var retryPrompt = "The previous response was not valid JSON. Return only a valid JSON object with the driver's license data. No explanatory text.";
            var retryResponse = await _groqClient.ChatCompletionAsync(_model, retryPrompt);
            
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
                _logger.LogError("Failed to parse JSON response from GROQ after retry");
                throw new InvalidOperationException("Failed to parse response from AI model", ex);
            }
        }
    }

    private IdParsed PostProcessResult(IdParsed result)
    {
        // Mask document number for logging
        if (!string.IsNullOrEmpty(result.DocumentNumber) && result.DocumentNumber.Length > 4)
        {
            var masked = "****" + result.DocumentNumber.Substring(result.DocumentNumber.Length - 4);
            _logger.LogInformation("Processed document with number ending in: {LastFour}", result.DocumentNumber.Substring(result.DocumentNumber.Length - 4));
        }

        // Add warnings for potential issues
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