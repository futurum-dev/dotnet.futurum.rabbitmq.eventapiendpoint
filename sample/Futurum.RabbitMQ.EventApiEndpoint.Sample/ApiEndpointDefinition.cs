using Futurum.ApiEndpoint;
using Futurum.EventApiEndpoint;
using Futurum.RabbitMQ.EventApiEndpoint.Metadata;

namespace Futurum.RabbitMQ.EventApiEndpoint.Sample;

public class ApiEndpointDefinition : IApiEndpointDefinition
{
    public void Configure(ApiEndpointDefinitionBuilder definitionBuilder)
    {
        definitionBuilder.Event()
                         .RabbitMQ()
                         .Event<TestEventApiEndpoint.ApiEndpoint>(builder => builder.Queue("hello"))
                         .EnvelopeEvent(builder => builder.FromQueue("test-topic-batch")
                                                          .Route<TestBatchRouteEventApiEndpoint.ApiEndpoint>("test-batch-route")
                                                          .Route<TestBatchRouteEventApiEndpoint2.ApiEndpoint>("test-batch-route2"));
    }
}