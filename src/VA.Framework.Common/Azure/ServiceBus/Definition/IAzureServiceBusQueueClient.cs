using System.Collections.Generic;
using System.Threading.Tasks;

namespace VA.Framework.Common.Azure.ServiceBus.Definition
{
    public interface IAzureServiceBusQueueClient
    {
        Task SendMessage(string queueName, string json);
        Task SendMessages(string queueName, List<string> jsonItems);
    }
}
