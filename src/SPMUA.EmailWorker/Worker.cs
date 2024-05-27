using SPMUA.EmailWorker.Models.Config;

namespace SPMUA.EmailWorker
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IConfiguration _configuration;

        private readonly WorkerConfig _workerConfig;
        private readonly EmailConfig _emailConfig;

        public Worker(ILogger<Worker> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;

            _workerConfig = _configuration.GetSection("WorkerConfig").Get<WorkerConfig>();
            _emailConfig = _configuration.GetSection("EmailConfig").Get<EmailConfig>();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Email Worker running at: {time}", DateTimeOffset.Now);

                await SendEmailAsync();

                await Task.Delay(_workerConfig.Delay, stoppingToken);
            }
        }

        private async Task SendEmailAsync()
        {
            _logger.LogInformation($"{nameof(SendEmailAsync)} Start");

            await Task.Delay(500);

            _logger.LogInformation($"{nameof(SendEmailAsync)} End");
        }
    }
}