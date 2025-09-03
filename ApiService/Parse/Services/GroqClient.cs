using System.Text;
using System.Text.Json;

namespace ApiService.Parse.Services;

public class GroqClient
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly ILogger<GroqClient> _logger;

    public GroqClient(HttpClient httpClient, IConfiguration configuration, ILogger<GroqClient> logger)
    {
        _httpClient = httpClient;
        _apiKey = Environment.GetEnvironmentVariable("GROQ_API_KEY") ?? configuration["GROQ_API_KEY"] ?? throw new InvalidOperationException("GROQ_API_KEY not configured");
        _logger = logger;
        
        _httpClient.BaseAddress = new Uri("https://api.groq.com/openai/v1/");
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
    }

    public async Task<string> ChatCompletionAsync(string model, string prompt, string? imageBase64 = null)
    {
        try
        {
            var messages = new List<object>();
            
            if (imageBase64 != null)
            {
                messages.Add(new
                {
                    role = "user",
                    content = new object[]
                    {
                        new { type = "text", text = prompt },
                        new { 
                            type = "image_url", 
                            image_url = new { url = $"data:image/jpeg;base64,{imageBase64}" }
                        }
                    }
                });
            }
            else
            {
                messages.Add(new { role = "user", content = prompt });
            }

            var requestBody = new
            {
                model,
                messages,
                max_tokens = 2048,
                temperature = 0.0,
                response_format = new { type = "json_object" }
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _logger.LogInformation("Calling GROQ API with model: {Model}", model);
            
            var response = await _httpClient.PostAsync("chat/completions", content);
            response.EnsureSuccessStatusCode();
            
            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<JsonElement>(responseContent);
            
            return result.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString() ?? string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling GROQ API");
            throw;
        }
    }
}