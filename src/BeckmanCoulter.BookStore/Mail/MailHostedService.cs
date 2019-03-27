using BeckmanCoulter.BookStore.Extensions;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace BeckmanCoulter.BookStore.Mail
{
    public class MailHostedService : IHostedService, IDisposable
    {
        private readonly ILogger _logger;
        private readonly IHostingEnvironment _environment;
        private SendGridClient _sendGridClient;
        private IQueueClient _queueClient;

        public MailHostedService(ILogger<MailHostedService> logger, IHostingEnvironment environment)
        {
            _logger = logger;
            _environment = environment;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Mail Background Service is starting.");
            var configuration = _environment.GetAppConfiguration();
            var apiKey = configuration["App:SendGrid:ApiKey"];
            _sendGridClient = new SendGridClient(apiKey);
            // Connection String for the namespace can be obtained from the Azure portal under the
            // 'Shared Access policies' section.
            _queueClient = new QueueClient(configuration["App:ServiceBus:ConnectionString"], configuration["App:ServiceBus:QueueName"]);
            // Register QueueClient's MessageHandler and receive messages in a loop
            RegisterMessageConsumer();

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Mail Background Service is stopping.");
            _queueClient?.CloseAsync();
            return Task.CompletedTask;
        }

        private void RegisterMessageConsumer()
        {
            // Configure the MessageHandler Options in terms of exception handling, number of concurrent messages to deliver etc.
            var messageHandlerOptions = new MessageHandlerOptions(ExceptionReceivedHandler)
            {
                // Maximum number of Concurrent calls to the callback `ProcessMessagesAsync`, set to 1 for simplicity.
                // Set it according to how many messages the application wants to process in parallel.
                MaxConcurrentCalls = 1,

                // Indicates whether MessagePump should automatically complete the messages after returning from User Callback.
                // False below indicates the Complete will be handled by the User Callback as in `ProcessMessagesAsync` below.
                AutoComplete = false
            };

            // Register the function that will process messages
            _queueClient.RegisterMessageHandler(MessageHandlerAsync, messageHandlerOptions);
        }

        private async Task MessageHandlerAsync(Message message, CancellationToken token)
        {
            try
            {
                // Process the message
                _logger.LogError(
                    $"Received message: SequenceNumber:{message.SystemProperties.SequenceNumber} Body:{Encoding.UTF8.GetString(message.Body)}");
                string mailJson = Encoding.UTF8.GetString(message.Body);
                SendGridMessage mailMessage = JsonConvert.DeserializeObject<SendGridMessage>(mailJson);

                var response = await _sendGridClient.SendEmailAsync(mailMessage, token);
                // Complete the message so that it is not received again.
                // This can be done only if the queueClient is created in ReceiveMode.PeekLock mode (which is default).
                await _queueClient.CompleteAsync(message.SystemProperties.LockToken);

                // Note: Use the cancellationToken passed as necessary to determine if the queueClient has already been closed.
                // If queueClient has already been Closed, you may chose to not call CompleteAsync() or AbandonAsync() etc. calls
                // to avoid unnecessary exceptions.
            }
            catch (Exception ex)
            {
                _logger.LogError($"Message handler encountered an exception.", ex);
                throw;
            }
        }

        private Task ExceptionReceivedHandler(ExceptionReceivedEventArgs exceptionReceivedEventArgs)
        {
            _logger.LogError($"Message handler encountered an exception .", exceptionReceivedEventArgs.Exception);
            var context = exceptionReceivedEventArgs.ExceptionReceivedContext;
            _logger.LogError("Exception context for troubleshooting:");
            _logger.LogError($"- Endpoint: {context.Endpoint}");
            _logger.LogError($"- Entity Path: {context.EntityPath}");
            _logger.LogError($"- Executing Action: {context.Action}");
            return Task.CompletedTask;
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