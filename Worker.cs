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
        private string _folderPath;

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

                if (!stoppingToken.IsCancellationRequested)
                {

                    //Lấy các danh sách các file tồn tại quá 30 ngày
                    var files = Directory.GetFiles(_folderPath)
                        .Select(file => new FileInfo(file))
                        .Where(file => file.LastWriteTime < DateTime.Now.AddMinutes(-1 * _numberOfDaysBeforeDelete))
                        .ToList();

                    //Xóa các file đó
                    files.ForEach(file => file.Delete());
                }

                await Task.Delay(TimeSpan.FromSeconds(_runIntervallInHours), stoppingToken);
            }
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            _configuration = _serviceScopeFactory.CreateScope().
                             ServiceProvider.GetRequiredService<IConfiguration>();
            _numberOfDaysBeforeDelete = int.Parse(_configuration
                                        ["Configurations:NumberOfDaysBeforeDelete"]);
            _runIntervallInHours = int.Parse(_configuration
                                            ["Configurations:RunIntervallInHours"]);

            _folderPath = _configuration["Configurations:ConfigurationFilePath"];

            return base.StartAsync(cancellationToken);
        }
    }
}
