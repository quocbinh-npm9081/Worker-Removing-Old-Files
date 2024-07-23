using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace cci.rof
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private IConfiguration _configuration;
        private int _numberOfDaysBeforeDelete;
        private int _runIntervallInHours;
        private object _folderPaths;

        public Worker(ILogger<Worker> logger, IServiceScopeFactory serviceScopeFactory)
        {
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                await Task.Delay(1000, stoppingToken);
            }
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            _configuration = _serviceScopeFactory.CreateScope().
                             ServiceProvider.GetRequiredService<IConfiguration>();
            _numberOfDaysBeforeDelete = int.Parse(_configuration
                                        ["App.Configurations:NumberOfDaysBeforeDelete"]);
            _runIntervallInHours = int.Parse(_configuration
                                            ["App.Configurations:RunIntervallInHours"]);
            _folderPaths = File.ReadAllLines(_configuration
            ["App.Configurations:ConfigurationFilePath"]).Select(x => x.Trim()).ToList();

            return base.StartAsync(cancellationToken);
        }
    }
}
