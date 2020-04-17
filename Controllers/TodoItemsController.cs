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
using TodoApi.Models.JsonModel;

namespace TodoApi.Controllers
{
    [Route("api/todo/")]
    [ApiController]
    public class TodoItemsController : Controller
    {
        private readonly TodoContext _dbcontext;
        private readonly IHttpClientFactory _clientFactory;

        private Getlogin _userinfo;

        public TodoItemsController(TodoContext dbcontext, IHttpClientFactory clientFactory)
        {
            _dbcontext = dbcontext;
            _clientFactory = clientFactory;
        }

        // GET: api/TodoItems (Get All Items)
        [HttpGet]
        public async Task<ActionResult<TodoItem>> GetTodoItem()
        {
            Getlogin userinfo = await GetloginUser();

            var todoItems = _dbcontext.TodoItems
                .Where(b => b.UserId == userinfo.id).ToList();
            return Ok(todoItems);
        }

        // GET: api/TodoItems/5 (Get item)
        [HttpGet("{id}")]
        public async Task<ActionResult<TodoItem>> GetTodoItem(int id)
        {
            Getlogin userinfo = await GetloginUser();

            var todoItem = await _dbcontext.TodoItems.FindAsync(id);

            if (todoItem == null)
            {
                return NotFound();
            }

            if(userinfo.id == todoItem.UserId)
            {
                return todoItem;
            }

            Response.StatusCode = 403;
            await Response.WriteAsync("Ошибка доступа");
            return null;
        }

        // DELETE: api/TodoItems/5 (Remove Item)
        [HttpDelete("{id}")]
        public async Task<ActionResult<TodoItem>> DeleteTodoItem(int id)
        {
            Getlogin userinfo = await GetloginUser();

            if(userinfo == null)
            {
                Response.StatusCode = 401;
                await Response.WriteAsync("Пользователь не авторизован");
            }

            var todoItem = await _dbcontext.TodoItems.FindAsync(id);
            if (todoItem == null)
            {
                return NotFound();
            }

            if(userinfo.id == todoItem.UserId)
            {
                _dbcontext.TodoItems.Remove(todoItem);
                await _dbcontext.SaveChangesAsync();
                return todoItem;
            }
            
            Response.StatusCode = 403;
            await Response.WriteAsync("Ошибка доступа");
            return null;
        }

        // POST: api/TodoItems  (Added Item)
        [HttpPost]
        public async Task<ActionResult<String>> CreateTodoItem([FromBody]TodoItemAdd todoItemAdd)
        {
            Getlogin userinfo = await GetloginUser();

            if(userinfo == null){
                Response.StatusCode = 401;
                await Response.WriteAsync("Пользователь не авторизован");
            }

            if(todoItemAdd.Title == null || todoItemAdd.Body == null)
            {
                return BadRequest(new { message = "Title or body is incorrect" });
            }

            // Получился очень странный запрос нужно с ним поработать))
            var todoItems = _dbcontext.TodoItems
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
                UserId = userinfo.id
            };

            _dbcontext.Add(item);
            _dbcontext.SaveChanges();

            return Ok("Create item");
        }


        public async Task<Getlogin> GetloginUser()
        {
            var Authorization = Request.Headers["Authorization"].ToString();
            var request = new HttpRequestMessage(HttpMethod.Post,
                "http://localhost:4000/api/auth/getlogin");
            request.Headers.Add("Authorization", Authorization);

            var client = _clientFactory.CreateClient();
            var response = await client.SendAsync(request);
            var data = await response.Content.ReadAsStringAsync();
            
            if(response.StatusCode.ToString() == "OK"){
                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = true
                };
                Getlogin userinfo = JsonSerializer.Deserialize<TodoApi.Models.JsonModel.Getlogin>(data, options);
                return userinfo;
            }
            return null;   
        }

    }
}
