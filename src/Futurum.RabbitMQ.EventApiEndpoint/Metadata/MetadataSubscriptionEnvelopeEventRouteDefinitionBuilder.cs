using Futurum.ApiEndpoint.DebugLogger;
using Futurum.EventApiEndpoint;
using Futurum.EventApiEndpoint.Metadata;

namespace Futurum.RabbitMQ.EventApiEndpoint.Metadata;

public class MetadataSubscriptionEnvelopeEventRouteDefinitionBuilder
{
    private readonly string _fromQueue;
    private readonly List<MetadataSubscriptionEnvelopeEventDefinition> _metadataSubscriptionEnvelopeEventDefinitions = new();

    public MetadataSubscriptionEnvelopeEventRouteDefinitionBuilder(string fromQueue)
    {
        _fromQueue = fromQueue;
    }

    public MetadataSubscriptionEnvelopeEventRouteDefinitionBuilder Route<TEventApiEndpoint>(string route)
        where TEventApiEndpoint : IEventApiEndpoint
    {
        _metadataSubscriptionEnvelopeEventDefinitions.Add(new MetadataSubscriptionEnvelopeEventDefinition(new MetadataTopic(_fromQueue), new MetadataRoute(route), typeof(TEventApiEndpoint)));

        return this;
    }

    public IEnumerable<MetadataSubscriptionEnvelopeEventDefinition> Build() =>
        _metadataSubscriptionEnvelopeEventDefinitions;

    public IEnumerable<ApiEndpointDebugNode> Debug() =>
        _metadataSubscriptionEnvelopeEventDefinitions.Select(x => new ApiEndpointDebugNode { Name = $"{x.Route} ({x.ApiEndpointType.FullName})" });
}