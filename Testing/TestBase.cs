using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TaRge25Shop.ApplicationServices.Services;
using TaRge25Shop.Core.ServiceInterface;
using TaRge25Shop.Data;
using TaRge25Shop.Testing.Macros;
using TaRge25Shop.Testing.MockServices;

namespace TaRge25Shop.Testing
{
    public abstract class TestBase : IDisposable
    {
        private readonly ServiceProvider _serviceProvider;

        protected TestBase()
        {
            var services = new ServiceCollection();
            SetupServices(services);
            _serviceProvider = services.BuildServiceProvider();
        }
        /// <summary>
        /// Seame üles testide läbiviimiseks vajalikud teenused mujalt projektist
        /// See meetod annab ka mälusoleva andmebaasi mida testideks kasutada, 
        /// toimib kui "program.cs"-i sisu testide jooksutamiseks, ent lühidal kujul.
        /// </summary>
        /// <param name="services">tühi ServiceCollection-tüüpi muutuja kuhu asetame 
        /// teenused, sh ka andmebaasi.</param>
        public virtual void SetupServices(ServiceCollection services)
        {
            services.AddScoped<ISpaceshipServices, SpaceshipServices>();
            services.AddScoped<IFileServices, FileServices>();
            services.AddScoped<IHostEnvironment, MockIHostEnvironment>();

            services.AddDbContext<TaRge25ShopContext>(
                x =>
                {
                    x.UseInMemoryDatabase(Guid.NewGuid().ToString());
                    //vaigistame errorid (kui andmebaasi CRUD ei toimi, siis DB errorit ei anna)
                    x.ConfigureWarnings(b => b.Ignore(InMemoryEventId.TransactionIgnoredWarning));
                }
                );

            RegisterMacros(services);
        }

        public void Dispose()
        {
            _serviceProvider.Dispose();
        }

        /// <summary>
        /// Leia üles kindel teenus, teenusepakkujalt.
        /// serviceProvider omab teenuseid, GetService hangib X tüüpi teenuse,
        /// C# on ükskõik mis tüüpi võimalik ilma tüübita näidata tähe "T"-ga ehk "Template"
        /// </summary>
        /// <typeparam name="T">teenuse tüüp</typeparam>
        /// <returns></returns>
        protected T Svc<T>() where T : notnull
        {
            return _serviceProvider.GetRequiredService<T>();
        }

        /// <summary>
        /// Registreerib macrodest teenuseid kui nad ei ole liidesed ja ei ole abstraktsed
        /// On vaja testi setupide seadistuseks.
        /// Makro --> Teenus
        /// </summary>
        /// <param name="services">Teenused, kuhu lisab makrodest muid teenuseid</param>
        private void RegisterMacros(ServiceCollection services)
        {
            var macroBaseType = typeof(IMacros);

            var macros = macroBaseType.Assembly.GetTypes()
                .Where(t => macroBaseType.IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

            foreach (var macro in macros)
            {
                services.AddSingleton(macro);
            }
        }
    }
}
