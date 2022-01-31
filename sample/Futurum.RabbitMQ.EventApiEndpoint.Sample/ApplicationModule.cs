using Futurum.Microsoft.Extensions.DependencyInjection;

namespace Futurum.RabbitMQ.EventApiEndpoint.Sample;

public class ApplicationModule : IModule
{
    public void Load(IServiceCollection services)
    {
        services.AddSingleton<Worker>();
    }
}