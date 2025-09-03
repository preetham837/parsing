namespace ApiService.Todo.Commands;

using MediatR;
using Data;
using Python;
using Python.Models;
using System.Text.Json;

public record UpdateTodoCommandDto(string Title, bool IsComplete);
public record UpdateTodoCommand(int Id, UpdateTodoCommandDto Dto) : IRequest<Unit>;

public class UpdateTodoCommandHandler(TodoDbContext context, PythonClient pythonClient) : IRequestHandler<UpdateTodoCommand, Unit>
{
    public async Task<Unit> Handle(UpdateTodoCommand request, CancellationToken cancellationToken)
    {
        var todo = await context.Todos.FindAsync([request.Id], cancellationToken: cancellationToken);

        if (todo == null)
        {
            // Handle not found
            return Unit.Value;
        }

        var titleChanged = todo.Title != request.Dto.Title;
        todo.Title = request.Dto.Title;
        todo.IsComplete = request.Dto.IsComplete;

        // Re-classify if title changed
        if (titleChanged && !string.IsNullOrWhiteSpace(request.Dto.Title))
        {
            try
            {
                var classificationResult = await pythonClient.ClassifyTodo(request.Dto.Title);
                var classification = JsonSerializer.Deserialize<TodoClassification>(classificationResult, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                todo.Category = classification?.Category;
            }
            catch
            {
                // If classification fails, keep existing category
            }
        }

        await context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}

