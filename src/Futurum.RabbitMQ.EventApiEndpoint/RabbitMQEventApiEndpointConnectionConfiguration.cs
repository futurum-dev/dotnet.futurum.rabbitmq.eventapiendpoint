using Futurum.Core.Option;

namespace Futurum.RabbitMQ.EventApiEndpoint;

/// <summary>
/// RabbitMQ EventApiEndpoint connection
/// </summary>
public record RabbitMQEventApiEndpointConnectionConfiguration(string HostName, Option<int> Port);