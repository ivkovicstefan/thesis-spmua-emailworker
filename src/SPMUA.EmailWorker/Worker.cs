using SPMUA.EmailWorker.Models.Config;
using System.Net.Mail;
using System.Net;
using SPMUA.EmailWorker.Models;
using SPMUA.EmailWorker.Models.Dictionaries;
using System.Data;
using System.Data.SqlClient;

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

                await SendEmailsAsync();

                await Task.Delay(_workerConfig.Delay, stoppingToken);
            }
        }

        private async Task SendEmailsAsync()
        {
            _logger.LogInformation($"{nameof(SendEmailsAsync)} Start");

            List<EmailQueueItemDTO> emailQueueItems = GetEmailQueueItems();
            bool isSuccessful = false;

            foreach (var emailQueueItem in emailQueueItems)
            {
                int noOfAttempts = 0;

                while (noOfAttempts < _workerConfig.MaxNoOfAttemps)
                {
                    isSuccessful = await SendEmailAsync(emailQueueItem);

                    noOfAttempts++;

                    if (isSuccessful)
                    {
                        break;
                    }
                }

                UpdateEmailQueueItem(emailQueueItem.EmailQueueId, 
                    noOfAttempts, 
                    isSuccessful ? (int)EmailQueueStatusEnum.Sent : (int)EmailQueueStatusEnum.Failed);
            }

            _logger.LogInformation($"{nameof(SendEmailsAsync)} End");
        }

        private List<EmailQueueItemDTO> GetEmailQueueItems()
        {
            var emailQueueItems = new List<EmailQueueItemDTO>();

            using (var connection = new SqlConnection(_configuration.GetConnectionString("SPMUADB")))
            {
                connection.Open();

                using (var command = new SqlCommand("USP_EmailQueueItems_Get", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@NoOfItems", _workerConfig.MaxNoOfItems);
                    command.Parameters.AddWithValue("@MaxNoOfAttempts", _workerConfig.MaxNoOfAttemps);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var item = new EmailQueueItemDTO
                            {
                                EmailQueueId = Convert.ToInt32(reader["EmailQueueId"]),
                                ToEmail = reader["ToEmail"].ToString() ?? String.Empty,
                                Subject = reader["EmailSubject"].ToString() ?? String.Empty,
                                Body = reader["EmailBody"].ToString() ?? String.Empty,
                                NoOfAttempts = Convert.ToInt32(reader["NoOfAttempts"]),
                                EmailQueueStatus = (EmailQueueStatusEnum)Convert.ToInt32(reader["EmailQueueStatusId"])
                            };

                            emailQueueItems.Add(item);
                        }
                    }
                }
            }

            return emailQueueItems;
        }

        private async Task<bool> SendEmailAsync(EmailQueueItemDTO emailQueueItem)
        {
            bool isSuccessful = false;

            try
            {
                using SmtpClient smtpClient = new SmtpClient(_emailConfig.SmtpClientHost, _emailConfig.SmtpClientPort)
                {
                    UseDefaultCredentials = false,
                    EnableSsl = true,
                    Credentials = new NetworkCredential(_emailConfig.FromEmail, _emailConfig.Password)
                };

                MailMessage email = new MailMessage(from: _emailConfig.FromEmail,
                                                    to: emailQueueItem.ToEmail,
                                                    subject: emailQueueItem.Subject,
                                                    body: emailQueueItem.Body)
                {
                    IsBodyHtml = true
                };

                await smtpClient.SendMailAsync(email);

                isSuccessful = true;
            }
            catch
            {
                isSuccessful = false;
            }

            return isSuccessful;
        }

        public void UpdateEmailQueueItem(int emailQueueId, int noOfAttempts, int emailQueueStatusId)
        {
            using (var connection = new SqlConnection(_configuration.GetConnectionString("SPMUADB")))
            {
                connection.Open();

                using (var command = new SqlCommand("USP_EmailQueueItem_Set", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@EmailQueueId", emailQueueId);
                    command.Parameters.AddWithValue("@NoOfAttempts", noOfAttempts);
                    command.Parameters.AddWithValue("@EmailQueueStatusId", emailQueueStatusId);

                    command.ExecuteNonQuery();
                }
            }
        }
    }
}