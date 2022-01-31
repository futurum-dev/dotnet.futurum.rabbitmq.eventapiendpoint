namespace Futurum.RabbitMQ.EventApiEndpoint.Metadata;

public static class EventApiEndpointDefinitionExtensions
{
    public static EventApiEndpointDefinition RabbitMQ(this Futurum.EventApiEndpoint.EventApiEndpointDefinition eventApiEndpointDefinition)
    {
        var rabbitMQEventApiEndpointDefinition = new EventApiEndpointDefinition();
        
        eventApiEndpointDefinition.Add(rabbitMQEventApiEndpointDefinition);

        return rabbitMQEventApiEndpointDefinition;
    }
}