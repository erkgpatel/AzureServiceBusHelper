namespace Erkgpatel.AzureServiceBus.Helper
{
    public class ServiceBusConfigs
    {
        public List<ServiceBusConfig> Configurations { get; set; }

    }
    public class ServiceBusConfig
    {
        public string ConnectionString { get; set; }
        public string QueueName { get; set; }
        public string TopicName { get; set; }
        public string SubscriptionName { get; set; }
    }
}