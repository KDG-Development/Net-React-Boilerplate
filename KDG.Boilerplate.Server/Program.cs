using System.Reflection;
using KDG.Boilerplate.Services;
using KDG.Database.Interfaces;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
var assemblyLocation = Assembly.GetExecutingAssembly().Location;
var basePath = Path.GetDirectoryName(assemblyLocation) ?? AppContext.BaseDirectory;
builder.Configuration
  .SetBasePath(basePath)
  .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// configure datastore
var connectionString = builder.Configuration["ConnectionString"] ?? throw new Exception("Connection string not configured");

// add DI services
builder.Services.AddScoped<IDatabase<Npgsql.NpgsqlConnection, Npgsql.NpgsqlTransaction>>(provider => 
  new KDG.Database.PostgreSQL(connectionString));
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAuthService>(provider => new AuthService(
    builder.Configuration["Jwt:Key"] ?? throw new Exception("JWT Key not configured"),
    builder.Configuration["Jwt:Issuer"] ?? throw new Exception("JWT Issuer not configured"), 
    builder.Configuration["Jwt:Audience"] ?? throw new Exception("JWT Audience not configured")
));

builder.Services.AddScoped<IExampleService, ExampleService>();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? throw new Exception("JWT Key not configured")))
        };
    });

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(builder.Configuration["BaseUrl"] ?? throw new Exception("BaseUrl not configured"))
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) {
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseDeveloperExceptionPage();
} else {
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseDefaultFiles();
app.UseStaticFiles();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
  app.UseSwagger();
  app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapFallbackToFile("/index.html");

app.Run();
