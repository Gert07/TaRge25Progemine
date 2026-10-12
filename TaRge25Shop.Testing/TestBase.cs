using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TaRge25Shop.ApplicationServices.Services;
using TaRge25Shop.Core.ServiceInterface;
using TaRge25Shop.Data;
using TaRge25Shop.Testing.Mock;

namespace TaRge25Shop.Testing;

public abstract class TestBase : IDisposable
{
    private readonly ServiceProvider _serviceProvider;
    private readonly IServiceScope _scope;

    protected TestBase()
    {
        var services = new ServiceCollection();
        services.AddScoped<ISpaceshipServices, SpaceshipServices>();
        services.AddScoped<IFileServices, FileServices>();
        services.AddSingleton<IHostEnvironment, MockIHostEnvironment>();
        services.AddScoped<IKindergardenServices, KindergardenServices>();
        var databaseName = $"SpaceshipTests-{Guid.NewGuid()}";
        services.AddDbContext<TaRge25ShopContext>(options =>
            options.UseInMemoryDatabase(databaseName));

        _serviceProvider = services.BuildServiceProvider(validateScopes: true);
        _scope = _serviceProvider.CreateScope();
    }

    protected T Svc<T>() where T : notnull =>
        _scope.ServiceProvider.GetRequiredService<T>();

    public void Dispose()
    {
        _scope.Dispose();
        _serviceProvider.Dispose();
        GC.SuppressFinalize(this);
    }
}
