using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;


namespace TodoApi.Middleware
{
    public class AuthCheck
    {
        private readonly RequestDelegate _next;

        public AuthCheck(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, IHttpClientFactory clientFactory)
        {
            var Authorization = context.Request.Headers["Authorization"].ToString();

            var request = new HttpRequestMessage(HttpMethod.Get,
                "https://jsonplaceholder.typicode.com/todos/1");
            request.Headers.Add("User-Agent", "HttpClientFactory-Sample");

            var client = clientFactory.CreateClient();
            var response = await client.SendAsync(request);
            var data = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrEmpty(Authorization))
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Token is invalid ");
            }
            else
            {
                //Делаем проверку
                await _next.Invoke(context);   
            }
        }
    }
}