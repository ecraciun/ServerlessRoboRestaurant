using Core.Services;
using Core.Services.Interfaces;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Willezone.Azure.WebJobs.Extensions.DependencyInjection;

[assembly: WebJobsStartup(typeof(ClientAPI.Startup))]
namespace ClientAPI
{
    internal class Startup : IWebJobsStartup
    {
        public void Configure(IWebJobsBuilder builder) => builder.AddDependencyInjection(ConfigureServices);

        private void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped(typeof(ITableRepositoryFactory<>), typeof(TableStorageRepositoryFactory<>));
        }
    }
}