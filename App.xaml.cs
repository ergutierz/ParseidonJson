using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using ParseidonJson.core;
using ParseidonJson.remote;

namespace ParseidonJson;

public partial class App : Application
{
    public IServiceProvider ServiceProvider { get; private set; }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var serviceCollection = new ServiceCollection();
        ConfigureServices(serviceCollection);

        ServiceProvider = serviceCollection.BuildServiceProvider();

        var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }

    private void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IJsonParser, JsonParser>();
        services.AddSingleton<IJsonQuery, JsonQuery>();
        services.AddSingleton<SportsService>();
        services.AddSingleton<MainWindow>();
    }
}