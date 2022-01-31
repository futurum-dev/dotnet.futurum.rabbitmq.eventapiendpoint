using Futurum.ApiEndpoint;
using Futurum.EventApiEndpoint.Metadata;

namespace Futurum.RabbitMQ.EventApiEndpoint.Metadata;

public record MetadataSubscriptionEventDefinition(MetadataTopic Queue) : IMetadataDefinition;