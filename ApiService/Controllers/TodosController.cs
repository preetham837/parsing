namespace ApiService.Controllers;

using MediatR;
using Microsoft.AspNetCore.Mvc;
using Todo.Commands;
using Todo.Queries;
using Todo.DTO;
using ApiService.Python;
using System.Text.Json;

[ApiController]
[Route("api/[controller]")]
public class TodosController(IMediator mediator, PythonClient pythonClient) : ControllerBase
{
    [HttpGet(Name = nameof(GetTodos))]
    public async Task<IEnumerable<TodoItem>> GetTodos()
    {
        return await mediator.Send(new GetTodosQuery());
    }

    [HttpPost(Name = nameof(CreateTodo))]
    public async Task<ActionResult<int>> CreateTodo(CreateTodoCommand command)
    {
        return await mediator.Send(command);
    }

    [HttpPut("{id}", Name = nameof(UpdateTodo))]
    public async Task<IActionResult> UpdateTodo(int id, UpdateTodoCommandDto dto)
    {
        await mediator.Send(new UpdateTodoCommand(id, dto));
        return NoContent();
    }

    [HttpDelete("{id}", Name = nameof(DeleteTodo))]
    public async Task<IActionResult> DeleteTodo(int id)
    {
        await mediator.Send(new DeleteTodoCommand(id));
        return NoContent();
    }

    [HttpPost("classify", Name = nameof(ClassifyTodo))]
    public async Task<IActionResult> ClassifyTodo([FromBody] ClassifyTodoRequest request)
    {
        var result = await pythonClient.ClassifyTodo(request.Description);
        var classification = JsonSerializer.Deserialize<TodoClassification>(result, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        return Ok(classification);
    }
}

public record ClassifyTodoRequest(string Description);
public record TodoClassification(string Description, string Category, double Confidence);
