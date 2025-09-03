using Microsoft.EntityFrameworkCore;
using Data;
using MigrationService;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

builder.Services.AddHostedService<Worker>();

builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing.AddSource(Worker.ActivitySourceName));

// Add database context
builder.AddSqlServerDbContext<TodoDbContext>("tododb", null,
    optionsBuilder => optionsBuilder.UseSqlServer(options => 
    options.MigrationsAssembly(typeof(Program).Assembly.GetName().Name)));

var app = builder.Build();

app.MapDefaultEndpoints();
app.Run();

