using Microsoft.Extensions.DependencyInjection;
using ParseidonJson.parser;

namespace ParseidonJson;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        // Registering the JSON parser service
        services.AddSingleton<IJsonParser, JsonParser>();
        // Add other services as needed
    }
}
