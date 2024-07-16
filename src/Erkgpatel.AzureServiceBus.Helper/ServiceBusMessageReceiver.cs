using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Options;
using System.Text.Json;


namespace Erkgpatel.AzureServiceBus.Helper
{
    internal class ServiceBusMessageReceiver<T>
    {
        private readonly Dictionary<string, ServiceBusProcessor> _processors = new Dictionary<string, ServiceBusProcessor>();

        public ServiceBusMessageReceiver(IOptions<ServiceBusConfigs> configs)
        {
            foreach (var config in configs.Value.Configurations)
            {
                var client = new ServiceBusClient(config.ConnectionString);
                ServiceBusProcessor processor;

                if (config.SubscriptionName != null)
                {
                    processor = client.CreateProcessor(config.QueueName, new ServiceBusProcessorOptions());
                }
                else
                {
                    processor = client.CreateProcessor(config.TopicName, config.SubscriptionName, new ServiceBusProcessorOptions());
                }

                _processors.Add(typeof(T).ToString(), processor);
            }
        }

        public async Task StartProcessingAsync(Func<T, Task> messageHandler)
        {
            foreach (var processor in _processors)
            {
                processor.Value.ProcessMessageAsync += async args =>
                {
                    string body = args.Message.Body.ToString();
                    T? message = JsonSerializer.Deserialize<T>(body);
                    await messageHandler(message);
                    await args.CompleteMessageAsync(args.Message);
                };

                processor.Value.ProcessErrorAsync += ErrorHandler;

                await processor.Value.StartProcessingAsync();
            }
        }

        public async Task StopProcessingAsync()
        {
            foreach (var processor in _processors)
            {
                await processor.Value.StopProcessingAsync();
                await processor.Value.DisposeAsync();
            }
        }

        private Task ErrorHandler(ProcessErrorEventArgs args)
        {
            Console.WriteLine($"Error occurred: {args.Exception.Message}");
            return Task.CompletedTask;
        }
    }
}
