using Futurum.Core.Result;
using Futurum.RabbitMQ.EventApiEndpoint.Metadata;

using Serilog;

namespace Futurum.RabbitMQ.EventApiEndpoint;

public interface IEventApiEndpointLogger : Futurum.EventApiEndpoint.IEventApiEndpointLogger, ApiEndpoint.IApiEndpointLogger
{
    void RabbitMQCreateConnectionError(RabbitMQEventApiEndpointConnectionConfiguration connectionConfiguration, IResultError error);

    void RabbitMQConfigureSubscriptionError(MetadataSubscriptionEventDefinition metadataSubscriptionEventDefinition, Exception exception);

    void RabbitMQProcessEventError(MetadataSubscriptionEventDefinition metadataSubscriptionEventDefinition, IResultError error);

    void RabbitMQChannelConsumeError(Exception exception);
}

public class EventApiEndpointLogger : IEventApiEndpointLogger
{
    private readonly ILogger _logger;

    public EventApiEndpointLogger(ILogger logger)
    {
        _logger = logger;
    }

    public void EventReceived<TEvent>(TEvent @event)
    {
        var eventData = new EventReceivedData<TEvent>(typeof(TEvent), @event);

        _logger.Debug("RabbitMQ EventApiEndpoint event received {@eventData}", eventData);
    }

    public void RabbitMQCreateConnectionError(RabbitMQEventApiEndpointConnectionConfiguration connectionConfiguration, IResultError error)
    {
        var eventData = new CreateConnectionErrorData(connectionConfiguration, error.ToErrorString());

        _logger.Error("RabbitMQ EventApiEndpoint CreateConnection error {@eventData}", eventData);
    }

    public void RabbitMQConfigureSubscriptionError(MetadataSubscriptionEventDefinition metadataSubscriptionEventDefinition, Exception exception)
    {
        var eventData = new ConfigureSubscriptionErrorData(metadataSubscriptionEventDefinition);

        _logger.Error(exception, "RabbitMQ EventApiEndpoint ConfigureSubscription error {@eventData}", eventData);
    }

    public void RabbitMQProcessEventError(MetadataSubscriptionEventDefinition metadataSubscriptionEventDefinition, IResultError error)
    {
        var eventData = new ProcessEventErrorData(metadataSubscriptionEventDefinition, error.ToErrorString());

        _logger.Error("RabbitMQ EventApiEndpoint ProcessEventError error {@eventData}", eventData);
    }

    public void RabbitMQChannelConsumeError(Exception exception)
    {
        _logger.Error(exception, "RabbitMQ EventApiEndpoint StartProcessing error");
    }

    public void ApiEndpointDebugLog(string apiEndpointDebugLog)
    {
        var eventData = new ApiEndpoints(apiEndpointDebugLog);

        _logger.Debug("WebApiEndpoint endpoints {@eventData}", eventData);
    }

    private readonly record struct EventReceivedData<TEvent>(Type EventType, TEvent Event);

    private readonly record struct CreateConnectionErrorData(RabbitMQEventApiEndpointConnectionConfiguration ConnectionConfiguration, string Error);

    private readonly record struct ConfigureSubscriptionErrorData(MetadataSubscriptionEventDefinition MetadataSubscriptionEventDefinition);

    private readonly record struct ProcessEventErrorData(MetadataSubscriptionEventDefinition MetadataSubscriptionEventDefinition, string Error);

    private record struct ApiEndpoints(string Log);
}