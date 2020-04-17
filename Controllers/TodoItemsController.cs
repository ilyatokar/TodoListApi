using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoApi.Models;

namespace TodoApi.Controllers
{
    [Route("api/todo/")]
    [ApiController]
    public class TodoItemsController : Controller
    {
        private readonly TodoContext _context;
        private readonly IHttpClientFactory _clientFactory;

        public TodoItemsController(TodoContext context, IHttpClientFactory clientFactory)
        {
            _context = context;
            _clientFactory = clientFactory;
        }

        // GET: api/TodoItems (Get All Items)
        [HttpGet]
        public IActionResult GetTodoItem()
        {
            List<TodoItem> items = _context.TodoItems.ToList();
            return Ok(items);
        }

        // GET: api/TodoItems/5 (Get item)
        [HttpGet("{id}")]
        public async Task<ActionResult<TodoItem>> GetTodoItem(int id)
        {
            var todoItem = await _context.TodoItems.FindAsync(id);

            if (todoItem == null)
            {
                return NotFound();
            }

            return todoItem;
        }

        // DELETE: api/TodoItems/5 (Remove Item)
        [HttpDelete("{id}")]
        public async Task<ActionResult<TodoItem>> DeleteTodoItem(int id)
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

        // POST: api/TodoItems  (Added Item)
        [HttpPost]
        public ActionResult<String> CreateTodoItem([FromBody]TodoItemAdd todoItemAdd)
        {
            if(todoItemAdd.Title == null || todoItemAdd.Body == null)
            {
                return BadRequest(new { message = "Title or body is incorrect" });;
            }

            // Получился очень странный запрос нужно с ним поработать))
            var todoItems = _context.TodoItems
                .Where(b => b.OnCreate >= DateTime.Now && 
                    b.OnCreate <=DateTime.Now.AddDays(1) && 
                    b.Title.Contains(todoItemAdd.Title.ToString())
                ).ToList();

            if(todoItems.Count != 0){
                return BadRequest(new { message = "A task with the same Name already exists." });
            }

            TodoItem item = new TodoItem()
            {
                Title = todoItemAdd.Title,
                Body = todoItemAdd.Body,
                UserId = 12
            };

            _context.Add(item);
            _context.SaveChanges();

            return Ok("Create item");
        }


        [HttpGet("testprint")]
        public IActionResult PrintResult([FromBody]TodoItemAdd todoItemAdd)
        {
            if(todoItemAdd.Title == null || todoItemAdd.Body == null)
            {
                return BadRequest(new { message = "Username or password is incorrect" });;
            }
            return Ok(todoItemAdd);
        }

        // GET: api/TodoItems
        [HttpGet("testhttp")]
        public async Task<ActionResult<String>> OnGet()
        {
            var request = new HttpRequestMessage(HttpMethod.Get,
                "https://jsonplaceholder.typicode.com/todos/1");
            request.Headers.Add("User-Agent", "HttpClientFactory-Sample");

            var client = _clientFactory.CreateClient();
            var response = await client.SendAsync(request);
            var data = await response.Content.ReadAsStringAsync();

            return data;
        }

    }
}
