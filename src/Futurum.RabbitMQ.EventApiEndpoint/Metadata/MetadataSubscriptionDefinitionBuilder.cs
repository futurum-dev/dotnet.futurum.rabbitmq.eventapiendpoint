using Futurum.ApiEndpoint;
using Futurum.ApiEndpoint.DebugLogger;
using Futurum.EventApiEndpoint.Metadata;

namespace Futurum.RabbitMQ.EventApiEndpoint.Metadata;

public class MetadataSubscriptionDefinitionBuilder : IMetadataEventSubscriptionDefinitionBuilder
{
    private readonly Type _apiEndpointType;
    private MetadataSubscriptionEventDefinition _metadataSubscriptionEventDefinition;

    public MetadataSubscriptionDefinitionBuilder(Type apiEndpointType)
    {
        _apiEndpointType = apiEndpointType;
    }

    public MetadataSubscriptionDefinitionBuilder Queue(string queue)
    {
        _metadataSubscriptionEventDefinition = new MetadataSubscriptionEventDefinition(new MetadataTopic(queue));

        return this;
    }

    public IEnumerable<IMetadataDefinition> Build()
    {
        yield return _metadataSubscriptionEventDefinition;
    }

    public ApiEndpointDebugNode Debug() =>
        new()
        {
            Name = $"{_metadataSubscriptionEventDefinition.Queue} ({_apiEndpointType.FullName})"
        };
}