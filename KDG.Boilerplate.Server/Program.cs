using System.Reflection;
using KDG.Boilerplate.Configuration;

var builder = WebApplication.CreateBuilder(args);

var assemblyLocation = Assembly.GetExecutingAssembly().Location;
var basePath = Path.GetDirectoryName(assemblyLocation) ?? AppContext.BaseDirectory;
builder.Configuration
    .SetBasePath(basePath)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services
    .AddDatabase(builder.Configuration)
    .AddRepositories()
    .AddApplicationServices()
    .AddBlobStorage(builder.Configuration)
    .AddJwtAuthentication(builder.Configuration)
    .AddAppCors(builder.Configuration)
    .AddValidation();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseDefaultFiles();
app.UseStaticFiles();
app.UseHttpsRedirection();
app.UseCors();
app.UseAuthorization();
app.MapControllers();
app.MapFallbackToFile("/index.html");

app.Run();
