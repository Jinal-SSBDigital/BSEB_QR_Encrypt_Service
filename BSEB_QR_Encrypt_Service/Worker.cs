public class Worker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<Worker> _logger;

    public Worker(IServiceProvider serviceProvider, ILogger<Worker> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Service Started");

        using var scope = _serviceProvider.CreateScope();
        var processor = scope.ServiceProvider.GetRequiredService<ProcessingService>();

        await processor.ProcessAsync(stoppingToken);
    }
}