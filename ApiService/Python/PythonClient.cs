using System.Text;
using System.Text.Json;

namespace ApiService.Python;

public class PythonClient(HttpClient httpClient)
{
    public async Task<string> ClassifyTodo(string description)
    {
        var request = new { description };
        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        var response = await httpClient.PostAsync("/api/todos/classify", content);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }
}
