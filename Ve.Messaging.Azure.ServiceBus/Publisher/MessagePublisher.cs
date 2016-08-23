using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Ve.Messaging.Azure.ServiceBus.Infrastructure;
using Ve.Messaging.Azure.ServiceBus.Publisher.Interfaces;
using Ve.Messaging.Publisher;
using Ve.Messaging.Model;

namespace Ve.Messaging.Azure.ServiceBus.Publisher
{
    public class MessagePublisher : IMessagePublisher
    {
        private readonly IPublisherClientResolver _publisherClientResolver;

        public MessagePublisher(IPublisherClientResolver publisherClientResolver)
        {
            _publisherClientResolver = publisherClientResolver;
        }

        public async Task SendAsync(Message message)
        {
            var topicClient = _publisherClientResolver.GetClient();
            var brokeredMessage = BrokeredMessageBuilder.SerializeToBrokeredMessage(message);

            try
            {
                await topicClient.SendAsync(brokeredMessage).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _publisherClientResolver.ReportFailure(topicClient, message, ex);
            }
        }

        public async Task SendBatchAsync(IEnumerable<Message> messages)
        {
            ICollection<Task> tasks = new List<Task>();

            foreach (var message in messages)
            {
                tasks.Add(SendAsync(message));
            }

            await Task.WhenAll(tasks).ConfigureAwait(false);
        }
    }
}