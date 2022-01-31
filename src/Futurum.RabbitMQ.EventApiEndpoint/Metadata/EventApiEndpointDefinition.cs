using Futurum.ApiEndpoint;
using Futurum.ApiEndpoint.DebugLogger;
using Futurum.Core.Result;
using Futurum.EventApiEndpoint;

namespace Futurum.RabbitMQ.EventApiEndpoint.Metadata;

public class EventApiEndpointDefinition : IApiEndpointDefinitionBuilder
{
    private readonly Dictionary<Type, List<Func<MetadataSubscriptionDefinitionBuilder>>> _eventBuilders = new();
    private readonly List<Func<MetadataSubscriptionEnvelopeEventDefinitionBuilder>> _envelopeEventBuilders = new();

    Result<Dictionary<Type, List<IMetadataDefinition>>> IApiEndpointDefinitionBuilder.Build()
    {
        var eventMetadataDefinitions = _eventBuilders.TryToDictionary(keyValuePair => keyValuePair.Key,
                                                                      keyValuePair => keyValuePair.Value.SelectMany(func => func().Build()).ToList());

        var envelopeEventMetadataDefinitions = _envelopeEventBuilders.SelectMany(x => x().Build())
                                                                     .TryToDictionary(x => x.ApiEndpointType,
                                                                                      x => new List<IMetadataDefinition> { x });

        return Result.CombineAll(eventMetadataDefinitions, envelopeEventMetadataDefinitions)
                     .Then(x => x.Item1.AsEnumerable().Concat(x.Item2.AsEnumerable())
                                 .TryToDictionary(keyValuePair => keyValuePair.Key, keyValuePair => keyValuePair.Value));
    }

    ApiEndpointDebugNode IApiEndpointDefinitionBuilder.Debug() =>
        new()
        {
            Name = "RABBIT-MQ",
            Children = _eventBuilders.SelectMany(keyValuePair => keyValuePair.Value.Select(func => func().Debug()))
                                     .Concat(_envelopeEventBuilders.Select(keyValuePair => keyValuePair().Debug()))
                                     .ToList()
        };

    public EventApiEndpointDefinition Event<TEventApiEndpoint>(Action<MetadataSubscriptionDefinitionBuilder> builderFunc)
        where TEventApiEndpoint : IEventApiEndpoint
    {
        var builder = new MetadataSubscriptionDefinitionBuilder(typeof(TEventApiEndpoint));

        var key = typeof(TEventApiEndpoint);
        var value = () =>
        {
            builderFunc(builder);

            return builder;
        };

        if (_eventBuilders.ContainsKey(key))
        {
            var existingValue = _eventBuilders[key];
            existingValue.Add(value);
        }
        else
        {
            _eventBuilders.Add(key, new List<Func<MetadataSubscriptionDefinitionBuilder>> { value });
        }

        return this;
    }

    public EventApiEndpointDefinition EnvelopeEvent(Action<MetadataSubscriptionEnvelopeEventDefinitionBuilder> builderFunc)
    {
        var builder = new MetadataSubscriptionEnvelopeEventDefinitionBuilder();

        var hasRun = false;
        var value = () =>
        {
            if (!hasRun)
            {
                builderFunc(builder);

                hasRun = true;
            }

            return builder;
        };

        _envelopeEventBuilders.Add(value);

        return this;
    }
}