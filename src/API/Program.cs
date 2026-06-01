using API.Modules;
using API.Services.UserProvider;
using BLL;
using BLL.Common.Interfaces;
using BLL.Hubs;
using BLL.Middlewares;
using DAL;
using Microsoft.Extensions.FileProviders;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();

builder.Services.AddHttpContextAccessor();

builder.Services.AddDataAccess(builder);
builder.Services.AddBusinessLogic(builder);

builder.Services.AddScoped<IUserProvider, UserProvider>();

builder.Services.AddSignalR();

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

var app = builder.Build();

await app.InitialiseDb();

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors(options => options
    .WithOrigins("http://localhost:3000", "http://localhost:80",
        "https://localhost:3000", "https://freelance-marketplace.pp.ua")
    .SetIsOriginAllowed(_ => true)
    .AllowAnyMethod()
    .AllowAnyHeader()
    .AllowCredentials()
);

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.UseMiddleware<MiddlewareExceptionsHandling>();

app.MapHub<NotificationHub>("/notifications");
app.MapHub<ChatHub>("/chat");

var imagesPath = Path.Combine(builder.Environment.ContentRootPath, Settings.ImagesPathSettings.ImagesPath);

if (!Directory.Exists(imagesPath))
{
    Directory.CreateDirectory(imagesPath);
}

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(imagesPath),
    RequestPath = $"/{Settings.ImagesPathSettings.StaticFileRequestPath}"
});

app.Run();

namespace API
{
    public class Program;
}