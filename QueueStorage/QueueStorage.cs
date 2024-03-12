using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;

namespace azure_learn.QueueStorage
{
    public class QueueStorage
    {
        private readonly string _connectionString;

        public QueueStorage()
        {
            _connectionString = KeyVault.KeyVault.GetSecret("StorageAccountConnectionString");
        }

        public async Task Exec()
        {
            await Case1();
        }

        private async Task Case1()
        {
            var client = CreateQueueClient("az204-queue-storage");

            // キューが存在しない場合は作成する
            client.CreateIfNotExists();

            if (!client.Exists()) return;

            // send message to queue storage
            await client.SendMessageAsync("queue storage send message");

            // get number of messages
            var properties = await client.GetPropertiesAsync();
            var messageCount = properties.Value.ApproximateMessagesCount;
            Console.WriteLine($"number of messages in queue: {messageCount}");

            // peek messages
            var peekMessages = await client.PeekMessagesAsync();
            foreach (var message in peekMessages.Value)
            {
                Console.WriteLine(message.Body.ToString());
            }

            // receive message
            QueueMessage receiveMessage = await client.ReceiveMessageAsync();

            // update message content
            var r = await client.UpdateMessageAsync(
                receiveMessage.MessageId,
                receiveMessage.PopReceipt,
                "Update contents",
                TimeSpan.FromSeconds(60));

            // delete message
            await client.DeleteMessageAsync(receiveMessage.MessageId, r.Value.PopReceipt);

            // delete all messages and queue
            await client.DeleteIfExistsAsync();
        }

        private QueueClient CreateQueueClient(string queueName)
            => new QueueClient(_connectionString, queueName);
    }
}
