using System.Text;
using System.Text.Json;

namespace ApiService.Parse.Services;

public class OpenAiClient
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _model;
    private readonly ILogger<OpenAiClient> _logger;

    public OpenAiClient(HttpClient httpClient, IConfiguration configuration, ILogger<OpenAiClient> logger)
    {
        _httpClient = httpClient;
        _apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY") ?? configuration["OPENAI_API_KEY"] ?? throw new InvalidOperationException("OPENAI_API_KEY not configured");
        _model = configuration["OpenAiVisionModel"] ?? "gpt-4o";
        _logger = logger;
        
        // Log API key validation (mask most of it for security)
        if (string.IsNullOrEmpty(_apiKey))
        {
            _logger.LogError("OPENAI_API_KEY is null or empty!");
            throw new InvalidOperationException("OPENAI_API_KEY not configured");
        }
        
        var maskedKey = _apiKey.Length > 10 ? 
            _apiKey.Substring(0, 10) + "..." + _apiKey.Substring(_apiKey.Length - 4) : 
            "***";
        _logger.LogInformation("OpenAI API Key loaded: {MaskedKey}, Model: {Model}", maskedKey, _model);
        
        _httpClient.BaseAddress = new Uri("https://api.openai.com/v1/");
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
    }

    public async Task<string> CallVisionAsync(string prompt, string? imageUrl = null, IFormFile? imageFile = null)
    {
        try
        {
            _logger.LogInformation("Starting OpenAI Vision API call with model: {Model}", _model);
            
            var messages = new List<object>();
            
            var contentParts = new List<object> { new { type = "text", text = prompt } };
            
            if (imageFile != null)
            {
                _logger.LogInformation("Processing image file: {FileName}, Size: {Size} bytes", 
                    imageFile.FileName, imageFile.Length);
                
                using var memoryStream = new MemoryStream();
                await imageFile.CopyToAsync(memoryStream);
                var imageBytes = memoryStream.ToArray();
                var base64Image = Convert.ToBase64String(imageBytes);
                var mimeType = GetMimeType(imageFile.FileName);
                
                _logger.LogInformation("Image converted to base64, MIME type: {MimeType}, Base64 length: {Length}", 
                    mimeType, base64Image.Length);
                
                contentParts.Add(new
                {
                    type = "image_url",
                    image_url = new { url = $"data:{mimeType};base64,{base64Image}" }
                });
            }
            else if (!string.IsNullOrEmpty(imageUrl))
            {
                _logger.LogInformation("Using image URL: {ImageUrl}", imageUrl);
                contentParts.Add(new
                {
                    type = "image_url",
                    image_url = new { url = imageUrl }
                });
            }

            messages.Add(new { role = "user", content = contentParts.ToArray() });

            var requestBody = new
            {
                model = _model,
                messages,
                max_tokens = 2048,
                temperature = 0.0
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _logger.LogInformation("Sending request to OpenAI API");
            
            var response = await _httpClient.PostAsync("chat/completions", content);
            
            var responseContent = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("Received response from OpenAI. Status: {StatusCode}, Content length: {Length}", 
                response.StatusCode, responseContent.Length);
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("OpenAI API returned error. Status: {StatusCode}, Response: {Response}", 
                    response.StatusCode, responseContent);
                throw new HttpRequestException($"OpenAI API returned {response.StatusCode}: {responseContent}");
            }
            
            var result = JsonSerializer.Deserialize<JsonElement>(responseContent);
            var content_text = result.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString() ?? string.Empty;
            
            _logger.LogInformation("Successfully extracted content from OpenAI response, length: {Length}", content_text.Length);
            
            return content_text;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling OpenAI Vision API");
            throw;
        }
    }

    private static string GetMimeType(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return extension switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".webp" => "image/webp",
            ".gif" => "image/gif",
            _ => "image/jpeg"
        };
    }
}