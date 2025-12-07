using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecureTodo.Application.DTOs;
using SecureTodo.Application.Services;
using SecureTodo.Shared.Results;
using System.Security.Claims;

namespace SecureTodo.API.Controllers;

/// <summary>
/// Todos controller
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TodosController : ControllerBase
{
    private readonly ITodoService _todoService;
    private readonly ILogger<TodosController> _logger;

    public TodosController(ITodoService todoService, ILogger<TodosController> logger)
    {
        _todoService = todoService;
        _logger = logger;
    }

    /// <summary>
    /// Get all todos for authenticated user
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(Result<IEnumerable<TodoDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetUserId();
            var todos = await _todoService.GetAllAsync(userId, cancellationToken);
            
            return Ok(Result<IEnumerable<TodoDto>>.SuccessResult(todos, "Todos retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get todos");
            return StatusCode(500, Result<IEnumerable<TodoDto>>.FailureResult("Failed to retrieve todos", ex.Message));
        }
    }

    /// <summary>
    /// Get a specific todo by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(Result<TodoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<string>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetUserId();
            var todo = await _todoService.GetByIdAsync(id, userId, cancellationToken);
            
            if (todo == null)
            {
                return NotFound(Result<string>.FailureResult("Todo not found", "The requested todo does not exist"));
            }
            
            return Ok(Result<TodoDto>.SuccessResult(todo, "Todo retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get todo {TodoId}", id);
            return StatusCode(500, Result<TodoDto>.FailureResult("Failed to retrieve todo", ex.Message));
        }
    }

    /// <summary>
    /// Create a new todo
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(Result<TodoDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(Result<string>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateTodoDto dto, CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetUserId();
            var todo = await _todoService.CreateAsync(dto, userId, cancellationToken);
            
            return CreatedAtAction(nameof(GetById), new { id = todo.Id }, 
                Result<TodoDto>.SuccessResult(todo, "Todo created successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create todo");
            return BadRequest(Result<string>.FailureResult("Failed to create todo", ex.Message));
        }
    }

    /// <summary>
    /// Update an existing todo
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(Result<TodoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<string>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTodoDto dto, CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetUserId();
            var todo = await _todoService.UpdateAsync(id, dto, userId, cancellationToken);
            
            return Ok(Result<TodoDto>.SuccessResult(todo, "Todo updated successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update todo {TodoId}", id);
            
            if (ex.Message.Contains("not found"))
            {
                return NotFound(Result<string>.FailureResult("Todo not found", ex.Message));
            }
            
            return StatusCode(500, Result<string>.FailureResult("Failed to update todo", ex.Message));
        }
    }

    /// <summary>
    /// Delete a todo
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(Result<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<string>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetUserId();
            await _todoService.DeleteAsync(id, userId, cancellationToken);
            
            return Ok(Result<string>.SuccessResult("Todo deleted successfully", "Todo deleted successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete todo {TodoId}", id);
            
            if (ex.Message.Contains("not found"))
            {
                return NotFound(Result<string>.FailureResult("Todo not found", ex.Message));
            }
            
            return StatusCode(500, Result<string>.FailureResult("Failed to delete todo", ex.Message));
        }
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(userIdClaim!);
    }
}
