using System.Net;
using System.Text;
using UserRegistrationServer.controllers.UserRegistrationServer.controllers;
using UserRegistrationServer.repositories;
using UserRegistrationServer.services;

class Program
{
    static async Task Main()
    {
        var listener = new HttpListener();
        listener.Prefixes.Add("http://localhost:8080/");
        listener.Start();

        IUserRepository repo = new UserRepository();
        var userService = new UserService(repo);
        var userController = new UserController(userService);

        Console.WriteLine("Server running on http://localhost:8080/");

        while (true)
        {
            var context = await listener.GetContextAsync();
            _ = Task.Run(async () =>
            {
                string path = context.Request.Url.AbsolutePath.ToLower();
                string method = context.Request.HttpMethod;

                try
                {
                    if (path == "/register" && method == "POST")
                        await userController.Register(context);
                    else if (path == "/login" && method == "POST")
                        await userController.Login(context);
                    else if (path == "/update-names" && method == "POST")
                        await userController.UpdateNames(context);
                    else if (path == "/update-password" && method == "POST")
                        await userController.ChangePassword(context);
                    else
                        await ServeStaticFileAsync(context);
                }
                catch (Exception ex)
                {
                    context.Response.StatusCode = 500;
                    byte[] buffer = Encoding.UTF8.GetBytes($"Server error: {ex.Message}");
                    await context.Response.OutputStream.WriteAsync(buffer);
                    context.Response.OutputStream.Close();
                }
            });
        }
    }

    static async Task ServeStaticFileAsync(HttpListenerContext context)
    {
        string root = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        string path = context.Request.Url.AbsolutePath.TrimStart('/');
        if (string.IsNullOrEmpty(path)) path = "index.html";

        string filePath = Path.Combine(root, path);
        if (File.Exists(filePath))
        {
            string ext = Path.GetExtension(filePath).ToLowerInvariant();
            string contentType = ext switch
            {
                ".html" => "text/html; charset=utf-8",
                ".css" => "text/css; charset=utf-8",
                ".js" => "application/javascript; charset=utf-8",
                ".png" => "image/png",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".gif" => "image/gif",
                _ => "application/octet-stream"
            };

            byte[] content = await File.ReadAllBytesAsync(filePath);
            context.Response.ContentType = contentType;
            await context.Response.OutputStream.WriteAsync(content);
        }
        else
        {
            context.Response.StatusCode = 404;
            byte[] buffer = Encoding.UTF8.GetBytes("404 Not Found");
            context.Response.ContentType = "text/plain; charset=utf-8";
            await context.Response.OutputStream.WriteAsync(buffer);
        }

        context.Response.OutputStream.Close();
    }
}
