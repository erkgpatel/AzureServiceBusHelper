using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace Erkgpatel.AzureServiceBus.Helper
{
    public class AzureServiceBusHelper : IAzureServiceBusHelper
    {
        private readonly ServiceBusClient _client;
        private readonly ServiceBusSender _sender;
        private readonly ServiceBusProcessor _processor;

        public AzureServiceBusHelper(IConfiguration configuration)
        {
            string connectionString = configuration["AzureServiceBusConnectionString"];
            string queueName = configuration["AzureServiceBusQueueName"];

            _client = new ServiceBusClient(connectionString);
            _sender = _client.CreateSender(queueName);

            _processor = _client.CreateProcessor(queueName, new ServiceBusProcessorOptions
            {
                AutoCompleteMessages = false,
                MaxConcurrentCalls = 10
            });

            _processor.ProcessMessageAsync += ProcessMessagesAsync;
            _processor.ProcessErrorAsync += ErrorHandlerAsync;

            _processor.StartProcessingAsync();
        }

        public async Task SendMessageAsync<T>(T message)
        {
            string messageBody = JsonSerializer.Serialize(message);
            ServiceBusMessage msg = new ServiceBusMessage(messageBody);

            await _sender.SendMessageAsync(msg);
        }

        private async Task ProcessMessagesAsync(ProcessMessageEventArgs args)
        {
            string messageBody = args.Message.Body.ToString();
            await args.CompleteMessageAsync(args.Message);
        }

        private Task ErrorHandlerAsync(ProcessErrorEventArgs args)
        {
            return Task.CompletedTask;
        }

        public async Task CloseAsync()
        {
            await _sender.DisposeAsync();
            await _processor.StopProcessingAsync();
            await _client.DisposeAsync();
        }
    }

}