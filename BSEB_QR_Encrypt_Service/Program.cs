using BSEBExamResult_QRGenerate.Data;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddWindowsService();

// Logger
Log.Logger = new LoggerConfiguration()
    .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Logging.ClearProviders();
builder.Logging.AddSerilog();

// DB
builder.Services.AddDbContext<AppDBContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("dbcs")));

// DI
builder.Services.AddScoped<DbHelper>();
builder.Services.AddScoped<EncryptionService>();
builder.Services.AddScoped<ProcessingService>();

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();