namespace ApiService.Todo.Commands;

using MediatR;
using Data;
using Python;
using Python.Models;
using System.Text.Json;

public record CreateTodoCommand(string Title) : IRequest<int>;

public class CreateTodoHandler(TodoDbContext context, PythonClient pythonClient) : IRequestHandler<CreateTodoCommand, int>
{
    public async Task<int> Handle(CreateTodoCommand request, CancellationToken cancellationToken)
    {
        string? category = null;
        
        try
        {
            if (!string.IsNullOrWhiteSpace(request.Title))
            {
                var classificationResult = await pythonClient.ClassifyTodo(request.Title);
                var classification = JsonSerializer.Deserialize<TodoClassification>(classificationResult, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                category = classification?.Category;
            }
        }
        catch
        {
            // If classification fails, continue without category
            category = null;
        }

        var entity = new Todo
        {
            Title = request.Title,
            Category = category,
            IsComplete = false
        };

        context.Todos.Add(entity);

        await context.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}
