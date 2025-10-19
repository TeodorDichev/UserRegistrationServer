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

                if (result.Success && result.User != null)
                {
                    string token = JwtManager.GenerateToken(result.User.Email);
                    context.Response.AddHeader("Set-Cookie",
                        $"jwt={token}; HttpOnly; Secure; SameSite=Strict; Path=/; Max-Age=3600");
                }

                await WriteJsonResponse(context, result);
            }

            public async Task Login(HttpListenerContext context)
            {
                var data = await ReadRequestBody(context);
                var result = await _service.LoginAsync(data);

                if (result.Success && result.User != null)
                {
                    string token = JwtManager.GenerateToken(result.User.Email);
                    context.Response.AddHeader("Set-Cookie",
                        $"jwt={token}; HttpOnly; Secure; SameSite=Strict; Path=/; Max-Age=3600");
                }

                await WriteJsonResponse(context, result);
            }

            public async Task Me(HttpListenerContext context)
            {
                string? token = context.Request.Cookies["jwt"]?.Value;
                if (string.IsNullOrEmpty(token))
                {
                    context.Response.StatusCode = 401;
                    await WriteJsonResponse(context, new { Success = false, Message = "Not authenticated" });
                    return;
                }

                string? email = JwtManager.ValidateToken(token);
                if (email == null)
                {
                    context.Response.StatusCode = 401;
                    await WriteJsonResponse(context, new { Success = false, Message = "Invalid or expired token" });
                    return;
                }

                var user = await _service.GetUserByEmailAsync(email);
                if (user == null)
                {
                    context.Response.StatusCode = 404;
                    await WriteJsonResponse(context, new { Success = false, Message = "User not found" });
                    return;
                }

                context.Response.StatusCode = 200;
                await WriteJsonResponse(context, user);
            }

            public async Task Logout(HttpListenerContext context)
            {
                context.Response.Headers.Add("Set-Cookie", $"jwt=deleted; HttpOnly; Path=/; Expires=Thu, 01 Jan 1970 00:00:00 GMT; SameSite=Strict");
                context.Response.StatusCode = 200;
                await WriteJsonResponse(context, new { Success = true, Message = "Logged out" });
            }

            public async Task UpdateNames(HttpListenerContext context)
            {
                string? token = context.Request.Cookies["jwt"]?.Value;
                string? email = JwtManager.ValidateToken(token);

                if (email == null)
                {
                    context.Response.StatusCode = 401;
                    await WriteJsonResponse(context, new { Success = false, Message = "Unauthorized" });
                    return;
                }

                var data = await ReadRequestBody(context);
                data["email"] = email;

                var result = await _service.UpdateNamesAsync(data);
                await WriteJsonResponse(context, result);
            }

            public async Task ChangePassword(HttpListenerContext context)
            {
                string? token = context.Request.Cookies["jwt"]?.Value;
                string? email = JwtManager.ValidateToken(token);

                if (email == null)
                {
                    context.Response.StatusCode = 401;
                    await WriteJsonResponse(context, new { Success = false, Message = "Unauthorized" });
                    return;
                }

                var data = await ReadRequestBody(context);
                data["email"] = email;

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
