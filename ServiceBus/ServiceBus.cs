using Azure.Messaging.ServiceBus;

namespace azure_learn.ServiceBus
{
    public class ServiceBus
    {
        private readonly string _serviceBusConnectionString;
        private const string QueueName = "az204-queue";

        public ServiceBus()
        {
            _serviceBusConnectionString = KeyVault.KeyVault.GetSecret("ServiceBusConnectionString");
        }

        public async Task Exec()
        {
            await Case1();
            await Case2();
        }

        private async Task Case1()
        {
            // send message to servicebus queue
            await using var client = GetServiceBusClient();
            await using var sender = client.CreateSender(QueueName);
            using var messageBatch = await sender.CreateMessageBatchAsync();

            Enumerable.Range(0, 5).Select(e => messageBatch.TryAddMessage(new ServiceBusMessage($"Message {e}"))).ToList();

            await sender.SendMessagesAsync(messageBatch);
            Console.WriteLine("A batch of five messages has been published to the queue.");
        }

        private async Task Case2()
        {
            await using var client = GetServiceBusClient();
            await using var processor = client.CreateProcessor(QueueName, new ServiceBusProcessorOptions());

            processor.ProcessMessageAsync += new Func<ProcessMessageEventArgs, Task>(async args =>
            {
                var body = args.Message.Body.ToString();
                Console.WriteLine($"Received: {body}");

                // メッセージを完了にする。(キューからメッセージが削除される)
                await args.CompleteMessageAsync(args.Message);
            });

            processor.ProcessErrorAsync += new Func<ProcessErrorEventArgs, Task>(args =>
            {
                Console.WriteLine(args.Exception.ToString());
                return Task.CompletedTask;
            });

            // start processing
            await processor.StartProcessingAsync();

            Console.WriteLine("Wait for a minute and then press any key to end the processing");
            Console.ReadKey();

            Console.WriteLine("Stopping then receiver...");
            await processor.StopProcessingAsync();
            Console.WriteLine("Stopped receiving messages");

        }

        private ServiceBusClient GetServiceBusClient()
            => new ServiceBusClient(_serviceBusConnectionString);
    }
}
