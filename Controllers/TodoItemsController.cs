using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using TodoApi.Models;
using TodoApi.Models.JsonModel;
using TodoApi.ConnectInfo;
using System.Text;

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
                var response = new
                {
                    title = todoItem.Title,
                    text = todoItem.Body,
                    completed = GetItemStatus(id),
                    created = todoItem.OnCreate
                };
                return Ok(response);
            }
            return StatusCode(403);
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
                bool deleted = await DeleteItemStatus(todoItem.id);
                if (deleted)
                {
                    _dbcontext.TodoItems.Remove(todoItem);
                    await _dbcontext.SaveChangesAsync();
                    return Ok();
                }
                else
                    return BadRequest(new { message = "Server isn't connected" });
            }
            
            Response.StatusCode = 403;
            await Response.WriteAsync("Ошибка доступа");
            return null;
        }

        // POST: api/TodoItems  (Added Item)
        [HttpPost]
        public async Task<ActionResult<String>> CreateTodoItem(TodoItemAdd todoItemAdd)
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
                UserId = userinfo.id,
            };
            
            try{
                _dbcontext.Add(item);
                _dbcontext.SaveChanges();
            }catch (InvalidOperationException){
                return BadRequest("can't be created task");
            }

            long statusId = await CreateItem(item.id);
            if (statusId == -1)
            {
                return BadRequest("Status can't be saved");
            }

            
            
            var response = new { id = item.id };
            return Ok(response);
        }

        [HttpPut]
        public async Task<ActionResult<String>> PutTodoItem(TodoItem changedItem, bool completed)
        {
            Getlogin userinfo = await GetloginUser();

            if (userinfo == null)
            {
                Response.StatusCode = 401;
                await Response.WriteAsync("Пользователь не авторизован");
            }
            TodoItem item = _dbcontext.TodoItems.SingleOrDefault(c => c.id==changedItem.id);
            if (item!=null && item.UserId == userinfo.id)
            {
                bool done = await PutItem(item.id, completed);
                if (!done)
                    return StatusCode(500, "Something went wrong");
                _dbcontext.Update(changedItem);
                _dbcontext.SaveChanges();
                var response = new { id = item.id };
                return Ok(response);
            }
            return BadRequest();
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
                Getlogin userinfo = System.Text.Json.JsonSerializer.Deserialize<TodoApi.Models.JsonModel.Getlogin>(data, options);
                return userinfo;
            }
            return null;   
        }


        private async Task<bool> DeleteItemStatus(long id)
        {
            String uri = ConnectionsInfo.StatusHost + $"api/todostatus/{id}";
            HttpClient _client = _clientFactory.CreateClient();
            var requestMessage = new HttpRequestMessage(HttpMethod.Delete, uri);
            requestMessage.Headers.Add("User-Agent", "HttpClientFactory-Sample");
            var Authorization = Request.Headers["Authorization"].ToString();
            requestMessage.Headers.Add("Autthorization", Authorization);
            var response = await _client.SendAsync(requestMessage);
            if (response.IsSuccessStatusCode)
            {
                return true;
            }
            else
                return false;
        }
        private async Task<bool> PutItem(int task_id, bool completed)
        {
            string uri = ConnectionsInfo.StatusHost + $"/api/todostatus/new/{task_id}/{completed}";
            HttpClient _client = _clientFactory.CreateClient();
            var requestMessage = new HttpRequestMessage(HttpMethod.Put, uri);
            requestMessage.Headers.Add("User-Agent", "HttpClientFactory-Sample");
            var Authorization = Request.Headers["Authorization"].ToString();
            requestMessage.Headers.Add("Autthorization", Authorization);
            var response = await _client.SendAsync(requestMessage);
            var content = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        private async Task<long> CreateItem(long id)
        {
            string uri = ConnectionsInfo.StatusHost + $"/api/todostatus/new/{id}/";
            HttpClient _client = _clientFactory.CreateClient();
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, uri);
            var Authorization = Request.Headers["Authorization"].ToString();
            requestMessage.Headers.Add("Authorization", Authorization);
            var response = await _client.SendAsync(requestMessage);
            var content = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                var res = JsonConvert.DeserializeObject<Dictionary<string, string>>(content);
                return Convert.ToInt64(res["id"]);
            }
            else
            {
                return -1;
            }
        }
        private async Task<String> GetItemStatus(int id)
        {
            string uri = ConnectionsInfo.StatusHost + $"/api/todostatus/{id}";
            HttpClient _client = _clientFactory.CreateClient();
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, uri);
            requestMessage.Headers.Add("User-Agent", "HttpClientFactory-Sample");
            var response = await _client.SendAsync(requestMessage);
            var content = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                var res = JsonConvert.DeserializeObject<Dictionary<string, string>>(content);
                return res["status"];
            }
            else
            {
                return "";
            }
        }

    }
}
