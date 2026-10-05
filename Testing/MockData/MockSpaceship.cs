using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;

namespace TaRge25Shop.Testing.MockServices
{   
    public class MockIHostEnvironment : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = Environments.Development;
        public string ApplicationName { get; set; } = "Testing";
        public string ContentRootPath { get; set; } = Directory.GetCurrentDirectory();
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
