using System.Net;
using System.Text;
using System.Text.Json;
using UserRegistrationServer.services;

namespace UserRegistrationServer.controllers
{
    namespace UserRegistrationServer.controllers
    {
        public class UserController
        {
            private readonly UserService _service;

            public UserController(UserService service)
            {
                _service = service;
            }

            public async Task Register(HttpListenerContext context)
            {
                var data = await ReadRequestBody(context);
                var result = await _service.RegisterAsync(data);
                await WriteJsonResponse(context, result);
            }

            public async Task Login(HttpListenerContext context)
            {
                var data = await ReadRequestBody(context);
                var result = await _service.LoginAsync(data);
                await WriteJsonResponse(context, result);
            }

            public async Task UpdateNames(HttpListenerContext context)
            {
                var data = await ReadRequestBody(context);
                var result = await _service.UpdateNamesAsync(data);
                await WriteJsonResponse(context, result);
            }

            public async Task ChangePassword(HttpListenerContext context)
            {
                var data = await ReadRequestBody(context);
                var result = await _service.UpdatePasswordAsync(data);
                await WriteJsonResponse(context, result);
            }

            private async Task<Dictionary<string, string>> ReadRequestBody(HttpListenerContext context)
            {
                using var reader = new StreamReader(context.Request.InputStream, context.Request.ContentEncoding);
                string body = await reader.ReadToEndAsync();
                return JsonSerializer.Deserialize<Dictionary<string, string>>(body) ?? new Dictionary<string, string>();
            }

            private async Task WriteJsonResponse(HttpListenerContext context, object responseObj)
            {
                context.Response.ContentType = "application/json; charset=utf-8";
                string json = JsonSerializer.Serialize(responseObj);
                byte[] buffer = Encoding.UTF8.GetBytes(json);
                await context.Response.OutputStream.WriteAsync(buffer);
                context.Response.OutputStream.Close();
            }
        }
    }
}
