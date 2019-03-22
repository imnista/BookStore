using BeckmanCoulter.BookStore.Extensions;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SendGrid.Helpers.Mail;
using System;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace BeckmanCoulter.BookStore.Mail
{
    public class MailQueueService : IMailQueueService, IDisposable
    {
        private readonly ILogger _logger;
        private readonly IHostingEnvironment _environment;
        private IQueueClient _queueClient;

        public MailQueueService(ILogger<MailHostedService> logger, IHostingEnvironment environment)
        {
            _logger = logger;
            _environment = environment;
            var configuration = _environment.GetAppConfiguration();
            // Connection String for the namespace can be obtained from the Azure portal under the
            // 'Shared Access policies' section.
            _queueClient = new QueueClient(configuration["App:ServiceBus:ConnectionString"], configuration["App:ServiceBus:QueueName"]);
            // Register QueueClient's MessageHandler and receive messages in a loop
        }

        public async Task SendMessage(SendGridMessage mail)
        {
            try
            {
                // Create a new message to send to the queue.
                string messageBody = JsonConvert.SerializeObject(mail);
                var message = new Message(Encoding.UTF8.GetBytes(messageBody));
                // Write the body of the message to the console.
                _logger.LogDebug($"Sending message: {messageBody}");
                // Send the message to the queue.
                await _queueClient.SendAsync(message);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Message sender encountered an exception.", ex);
                throw;
            }
        }

        // Flag: Has Dispose already been called?
        private bool _disposed;

        // Public implementation of Dispose pattern callable by consumers.
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // Protected implementation of Dispose pattern.
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                // Free any other managed objects here.
                //Free Database
                _queueClient?.CloseAsync();
            }

            _disposed = true;
        }
    }
}