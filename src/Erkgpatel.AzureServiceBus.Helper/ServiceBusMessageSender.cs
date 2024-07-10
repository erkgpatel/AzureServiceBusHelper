namespace Erkgpatel.AzureServiceBus.Helper
{
    using Azure.Messaging.ServiceBus;
    using Microsoft.Extensions.Configuration;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading.Tasks;

    namespace ServiceBusLibrary
    {
        public class ServiceBusMessageSender<T>
        {
            private readonly IConfiguration _configuration;
            private readonly Dictionary<string, ServiceBusSender> _queueSenders;
            private readonly List<QueueConfig> queueConfigs;

            public ServiceBusMessageSender(IConfiguration configuration)
            {
                _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
                _queueSenders = new Dictionary<string, ServiceBusSender>();
                queueConfigs = (List<QueueConfig>)_configuration.GetSection(typeof(T).Name);

                InitializeQueueSenders();
            }


            private void InitializeQueueSenders()
            {

                foreach (var queueConfig in queueConfigs)
                {
                    _queueSenders[queueConfig.Name] = new ServiceBusClient(queueConfig.ServiceBusConnectionString).CreateSender(queueConfig.Name);
                }
            }

            public async Task SendMessagesAsync(T message)
            {
                foreach (var queueConfig in queueConfigs)
                {
                    if (_queueSenders.ContainsKey(queueConfig.Name))
                    {
                        await SendToQueueAsync(queueConfig.Name, message);
                    }
                    else
                    {
                        Console.WriteLine($"Queue sender not found for queue '{queueConfig.Name}'.");
                    }
                }
            }

            private async Task SendToQueueAsync(string queueName, T message)
            {
                var sender = _queueSenders[queueName];

                try
                {
                    string serializedMessage = JsonConvert.SerializeObject(message);
                    var serviceBusMessage = new ServiceBusMessage(Encoding.UTF8.GetBytes(serializedMessage));

                    await sender.SendMessageAsync(serviceBusMessage);

                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error sending message {ex.Message}");
                    throw;
                }
            }

            public async Task CloseAsync()
            {
                foreach (var sender in _queueSenders.Values)
                {
                    await sender.DisposeAsync();
                }
            }
        }

        public class QueueConfig
        {
            public string? Name { get; set; }
            public string? ServiceBusConnectionString { get; set; }
        }
    }
}