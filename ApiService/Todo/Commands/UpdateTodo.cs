namespace ApiService.Todo.Commands;

using MediatR;
using Data;

public record UpdateTodoCommandDto(string Title, bool IsComplete);
public record UpdateTodoCommand(int Id, UpdateTodoCommandDto Dto) : IRequest<Unit>;

public class UpdateTodoCommandHandler(TodoDbContext context) : IRequestHandler<UpdateTodoCommand, Unit>
{
    public async Task<Unit> Handle(UpdateTodoCommand request, CancellationToken cancellationToken)
    {
        var todo = await context.Todos.FindAsync([request.Id], cancellationToken: cancellationToken);

        if (todo == null)
        {
            // Handle not found
            return Unit.Value;
        }

        todo.Title = request.Dto.Title;
        todo.IsComplete = request.Dto.IsComplete;
        // No AI classification anymore - category remains unchanged

        await context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}

