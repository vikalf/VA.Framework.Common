using Microsoft.Azure.ServiceBus;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VA.Framework.Common.Azure.ServiceBus.Definition;

namespace VA.Framework.Common.Azure.ServiceBus.Implementation
{
    public class AzureServiceBusQueueClient : IAzureServiceBusQueueClient
    {
        private readonly string _serviceBusConnectionString;

        public AzureServiceBusQueueClient(string serviceBusConnectionString)
        {
            _serviceBusConnectionString = serviceBusConnectionString;
        }

        public async Task SendMessage(string queueName, string json)
        {
            var _queueClient = new QueueClient(_serviceBusConnectionString, queueName);
            var message = new Message(Encoding.UTF8.GetBytes(json));
            await _queueClient.SendAsync(message);
        }

        public async Task SendMessages(string queueName, List<string> jsonItems)
        {
            var _queueClient = new QueueClient(_serviceBusConnectionString, queueName);

            var messages = new List<Message>();
            jsonItems.ForEach(j => messages.Add(new Message(Encoding.UTF8.GetBytes(j))));
            await _queueClient.SendAsync(messages);
        }

    }
}
