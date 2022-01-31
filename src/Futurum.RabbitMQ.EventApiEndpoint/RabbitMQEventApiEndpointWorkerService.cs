using System.Text;

using Futurum.Core.Functional;
using Futurum.Core.Result;
using Futurum.EventApiEndpoint;
using Futurum.EventApiEndpoint.Metadata;
using Futurum.Microsoft.Extensions.DependencyInjection;
using Futurum.RabbitMQ.EventApiEndpoint.Metadata;

using Microsoft.Extensions.DependencyInjection;

using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Futurum.RabbitMQ.EventApiEndpoint;

public interface IRabbitMQEventApiEndpointWorkerService
{
    void Execute();

    void Stop();
}

public class RabbitMQEventApiEndpointWorkerService : IRabbitMQEventApiEndpointWorkerService
{
    private readonly IEventApiEndpointLogger _logger;
    private readonly RabbitMQEventApiEndpointConnectionConfiguration _connectionConfiguration;
    private readonly IServiceProvider _serviceProvider;
    private readonly IEventApiEndpointMetadataCache _metadataCache;

    private IConnection? _connection;

    public RabbitMQEventApiEndpointWorkerService(IEventApiEndpointLogger logger,
                                                 RabbitMQEventApiEndpointConnectionConfiguration connectionConfiguration,
                                                 IServiceProvider serviceProvider,
                                                 IEventApiEndpointMetadataCache metadataCache)
    {
        _logger = logger;
        _connectionConfiguration = connectionConfiguration;
        _serviceProvider = serviceProvider;
        _metadataCache = metadataCache;
    }

    public void Execute()
    {
        CreateConnection(_connectionConfiguration).Do(connection =>
        {
            _connection = connection;

            ConfigureEvents(connection);

            ConfigureBatchEvents(connection);
        });
    }

    private void ConfigureEvents(IConnection connection)
    {
        var metadataEventDefinitions = _metadataCache.GetMetadataEventDefinitions();

        foreach (var (metadataSubscriptionDefinition, metadataTypeDefinition) in metadataEventDefinitions)
        {
            if (metadataSubscriptionDefinition is MetadataSubscriptionEventDefinition metadataSubscriptionEventDefinition)
            {
                ConfigureSubscription(metadataSubscriptionEventDefinition, metadataTypeDefinition.EventApiEndpointExecutorServiceType, connection);
            }
        }
    }

    private void ConfigureBatchEvents(IConnection connection)
    {
        var metadataEnvelopeEventDefinitions = _metadataCache.GetMetadataEnvelopeEventDefinitions();

        var metadataSubscriptionEventDefinitions = metadataEnvelopeEventDefinitions.Select(x => x.MetadataSubscriptionEventDefinition)
                                                                                   .Select(x => x.FromTopic)
                                                                                   .Distinct()
                                                                                   .Select(topic => new MetadataSubscriptionEventDefinition(topic));

        foreach (var metadataSubscriptionEventDefinition in metadataSubscriptionEventDefinitions)
        {
            var apiEndpointExecutorServiceType = typeof(EventApiEndpointExecutorService<,>).MakeGenericType(typeof(Batch.EventDto), typeof(Batch.Event));

            ConfigureSubscription(metadataSubscriptionEventDefinition, apiEndpointExecutorServiceType, connection);
        }
    }

    public void Stop()
    {
        _connection?.Close();
    }

    private Result<IConnection> CreateConnection(RabbitMQEventApiEndpointConnectionConfiguration connectionConfiguration)
    {
        IConnection Execute()
        {
            var factory = new ConnectionFactory();

            factory.HostName = connectionConfiguration.HostName;

            connectionConfiguration.Port.DoSwitch(port => factory.Port = port, Function.DoNothing);

            factory.DispatchConsumersAsync = true;

            return factory.CreateConnection();
        }

        return Result.Try(Execute, () => $"Failed to create RabbitMQ Connection")
                     .DoWhenFailure(error => _logger.RabbitMQCreateConnectionError(connectionConfiguration, error));
    }

    private void ConfigureSubscription(MetadataSubscriptionEventDefinition metadataSubscriptionDefinition, Type apiEndpointExecutorServiceType, IConnection connection)
    {
        try
        {
            var channel = connection.CreateModel();

            channel.QueueDeclare(queue: metadataSubscriptionDefinition.Queue.Value, durable: false, exclusive: false, autoDelete: false, arguments: null);

            var consumer = new AsyncEventingBasicConsumer(channel);

            consumer.Received += async (model, basicDeliverEventArgs) => await ProcessEventAsync(metadataSubscriptionDefinition, apiEndpointExecutorServiceType, basicDeliverEventArgs);

            try
            {
                channel.BasicConsume(metadataSubscriptionDefinition.Queue.Value, true, consumer);
            }
            catch (Exception exception)
            {
                _logger.RabbitMQChannelConsumeError(exception);
            }
        }
        catch (Exception exception)
        {
            _logger.RabbitMQConfigureSubscriptionError(metadataSubscriptionDefinition, exception);
        }
    }

    private async Task ProcessEventAsync(MetadataSubscriptionEventDefinition metadataSubscriptionEventDefinition, Type apiEndpointExecutorServiceType, BasicDeliverEventArgs basicDeliverEventArgs)
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        
        var body = basicDeliverEventArgs.Body.ToArray();
        var message = Encoding.UTF8.GetString(body);

        await scope.ServiceProvider.TryGetService<IEventApiEndpointExecutorService>(apiEndpointExecutorServiceType)
                   .ThenAsync(executorService => executorService.ExecuteAsync(metadataSubscriptionEventDefinition, message, CancellationToken.None))
                   .DoWhenFailureAsync(error => _logger.RabbitMQProcessEventError(metadataSubscriptionEventDefinition, error));

        await Task.Yield();
    }
}