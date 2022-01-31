namespace Futurum.RabbitMQ.EventApiEndpoint.Sample;

public class Worker : BackgroundService
{
    private readonly IRabbitMQEventApiEndpointWorkerService _workerService;

    public Worker(IRabbitMQEventApiEndpointWorkerService workerService)
    {
        _workerService = workerService;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _workerService.Execute();

        return Task.CompletedTask;
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _workerService.Stop();

        return Task.CompletedTask;
    }
}