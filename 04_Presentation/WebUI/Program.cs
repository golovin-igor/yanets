using Yanets.Core.Interfaces;
using Yanets.Core.Models;
using Yanets.Core.Vendors;
using Yanets.Application.Services;
using Yanets.WebUI.Services;
using Yanets.SharedKernel;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "YANETS - Yet Another Network Equipment Test Simulator",
        Version = "v1",
        Description = "A comprehensive network device simulation system with CLI and SNMP support"
    });
});

// Register YANETS services
builder.Services.AddSingleton<Yanets.Application.Services.ITopologyService, Yanets.Application.Services.TopologyService>();
builder.Services.AddSingleton<Yanets.Application.Services.IDeviceSimulator, Yanets.Application.Services.DeviceSimulatorService>();
builder.Services.AddSingleton<Yanets.Core.Interfaces.ICommandParser, Yanets.Application.Services.CommandParser>();
builder.Services.AddSingleton<Yanets.Core.Interfaces.IMibProvider, Yanets.Application.Services.MibProvider>();
builder.Services.AddSingleton<Yanets.Core.Interfaces.IPromptGenerator, Yanets.Application.Services.PromptGenerator>();

// Register infrastructure services
builder.Services.AddSingleton<CliServerService>();
builder.Services.AddSingleton<SnmpAgentService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "YANETS API v1");
        c.RoutePrefix = string.Empty; // Serve Swagger UI at root
    });
}

app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new { Status = "Healthy", Timestamp = DateTime.UtcNow }));

app.Run();
