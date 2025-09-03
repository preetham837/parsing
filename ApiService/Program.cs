using System.Reflection;
using ApiService.Parse.Services;
using Data;

var builder = WebApplication.CreateBuilder(args);

// Load .env file
var envFilePath = Path.Combine(Directory.GetCurrentDirectory(), "..", ".env");
if (File.Exists(envFilePath))
{
    foreach (var line in File.ReadAllLines(envFilePath))
    {
        if (!string.IsNullOrWhiteSpace(line) && line.Contains('='))
        {
            var parts = line.Split('=', 2);
            if (parts.Length == 2)
            {
                Environment.SetEnvironmentVariable(parts[0].Trim(), parts[1].Trim());
            }
        }
    }
}

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
});
builder.Services.AddProblemDetails(options =>
{
    options.CustomizeProblemDetails = ctx =>
    {
        // In development, include detailed exception information
        if (builder.Environment.IsDevelopment() && ctx.Exception != null)
        {
            ctx.ProblemDetails.Extensions["exception"] = ctx.Exception.GetType().Name;
            ctx.ProblemDetails.Extensions["exceptionMessage"] = ctx.Exception.Message;
            ctx.ProblemDetails.Extensions["stackTrace"] = ctx.Exception.StackTrace;
        }
    };
});
builder.Services.AddCors();
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
});
builder.Services.AddEndpointsApiExplorer();

// Add database context
builder.AddSqlServerDbContext<TodoDbContext>("tododb");

builder.Services.AddOpenApiDocument(options =>
{
    options.DocumentName = "v1";
    options.Title = "Personal Information Parser API";
    options.Version = "v1";
    options.UseHttpAttributeNameAsOperationId = true;

    options.PostProcess = document =>
    {
        document.BasePath = "/";
    };
});

// Register parsing services
builder.Services.AddHttpClient<GroqClient>();
builder.Services.AddHttpClient<OpenAiClient>();
builder.Services.AddSingleton<LookupService>();
builder.Services.AddScoped<TextParserService>();
builder.Services.AddScoped<ImageParserService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();
app.UseCors(static builder =>
{
    builder.AllowAnyOrigin()
           .AllowAnyMethod()
           .AllowAnyHeader()
           .WithExposedHeaders("*");
});
app.MapDefaultEndpoints();
app.MapControllers();
app.UseOpenApi();
app.UseSwaggerUi();
app.Run();