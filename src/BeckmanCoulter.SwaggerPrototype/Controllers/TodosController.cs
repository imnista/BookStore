using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BeckmanCoulter.SwaggerPrototype.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BeckmanCoulter.SwaggerPrototype.Controllers
{
  [Produces("application/json")]
  [Route("api/[controller]")]
  [ApiController]
  public class TodosController : ControllerBase
  {
    private readonly TodoContext _context;

    public TodosController(TodoContext context)
    {
      _context = context;

      if (_context.TodoItems.Count() == 0)
      {
        // Create a new TodoItem if collection is empty,
        // which means you can't delete all TodoItems.
        _context.TodoItems.Add(new TodoItem { Name = "Item1" });
        _context.SaveChanges();
      }
    }

    // GET: api/Todo
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TodoItem>>> GetTodoItems()
    {
      return await _context.TodoItems.ToListAsync();
    }

    // GET: api/Todo/5
    [HttpGet("{id}")]
    public async Task<ActionResult<TodoItem>> GetTodoItem(long id)
    {
      var todoItem = await _context.TodoItems.FindAsync(id);

      if (todoItem == null)
      {
        return NotFound();
      }

      return todoItem;
    }

    /// <summary>
    /// Creates a TodoItem.
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     POST /Todo
    ///     {
    ///        "id": 1,
    ///        "name": "Item1",
    ///        "isComplete": true
    ///     }
    ///
    /// </remarks>
    /// <param name="todoItem"></param>
    /// <returns>A newly created TodoItem</returns>
    /// <response code="201">Returns the newly created item</response>
    /// <response code="400">If the item is null</response>            
    [HttpPost]
    [ProducesResponseType(201)]
    [ProducesResponseType(400)]
    [HttpPost]
    public async Task<ActionResult<TodoItem>> PostTodoItem(TodoItem todoItem)
    {
      _context.TodoItems.Add(todoItem);
      await _context.SaveChangesAsync();

      return CreatedAtAction("GetTodoItem", new { id = todoItem.Id }, todoItem);
    }

    // PUT: api/Todo/5
    [HttpPut("{id}")]
    public async Task<IActionResult> PutTodoItem(long id, TodoItem todoItem)
    {
      if (id != todoItem.Id)
      {
        return BadRequest();
      }

      _context.Entry(todoItem).State = EntityState.Modified;
      await _context.SaveChangesAsync();

      return NoContent();
    }

    /// <summary>
    /// Deletes a specific TodoItem.
    /// </summary>
    /// <param name="id"></param> 
    [HttpDelete("{id}")]
    public async Task<ActionResult<TodoItem>> DeleteTodoItem(long id)
    {
      var todoItem = await _context.TodoItems.FindAsync(id);
      if (todoItem == null)
      {
        return NotFound();
      }

      _context.TodoItems.Remove(todoItem);
      await _context.SaveChangesAsync();

      return todoItem;
    }
  }
}