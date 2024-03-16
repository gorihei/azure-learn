using Microsoft.Azure.Cosmos;

namespace AzureCosmosDb
{
    public class Program
    {
        static async Task Main(string[] args)
        {
            string endPointUri = KeyVault.KeyVault.GetSecret("CosmosDbEndpoint");
            string primaryKey = KeyVault.KeyVault.GetSecret("CosmosDbPrimaryKey");

            try
            {
                // COSMOS Client作成
                var client = new CosmosClient(endPointUri, primaryKey);

                // Database作成(存在しない場合は作成、存在する場合はDatabaseを取得)
                Database database = await client.CreateDatabaseIfNotExistsAsync("az204Database");
                Console.WriteLine("Created Database:az204Database");
                Console.WriteLine($"\t id:{database.Id}");

                // Container作成(存在しない場合は作成、存在する場合はContainerを取得)
                // コンテナ名とパーティションキーを指定する
                var container = await database.CreateContainerIfNotExistsAsync(
                    new ContainerProperties("az204Container", "/key"));
                Console.WriteLine("Created Container:az204Container partitionkey:/key");
                Console.WriteLine($"\t id:{container.Resource.Id}");

                var batch = container.Container.CreateTransactionalBatch(new PartitionKey("key1"));
                for (var i = 0; i < 10; i++)
                {
                    batch.CreateItem(new ToDoItem(Guid.NewGuid().ToString(), "key1", "TestValue", DateTimeOffset.Now.ToString()));
                }
                await batch.ExecuteAsync();

                await StartChangeFeedProcessorAsync(client);
                Console.WriteLine("Start ChangeFeedProcessing...");
            }
            catch (CosmosException ex)
            {
                Console.WriteLine($"{ex.StatusCode} error occurred: {ex}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex}");
            }
            finally
            {
                Console.ReadKey();
            }
        }

        private static async Task<ChangeFeedProcessor> StartChangeFeedProcessorAsync(CosmosClient client)
        {
            var leaseContainer = client.GetContainer("az204Database", "leaseContainer");
            var builder = client.GetContainer("az204Database", "az204Container")
                    // 変更フィードプロセッサを取得
                    .GetChangeFeedProcessorBuilder<ToDoItem>(
                        "changeFeedSample", // プロセッサ名(任意)
                        HandleChangesAsync) // 変更フィードを処理するハンドラ
                    .WithInstanceName("consoleHost")            // 一意のインスタンス名を指定(コンピューティングインスタンス毎に一意である必要がある)
                    .WithLeaseContainer(leaseContainer)         // リース状態を保持するコンテナを指定
                    .WithPollInterval(TimeSpan.FromSeconds(10)) // 指定した間隔で変更フィードを検知する
                    .Build();

            // 変更フィード検出開始
            await builder.StartAsync();
            return builder;
        }

        static async Task HandleChangesAsync(
            ChangeFeedProcessorContext context,
            IReadOnlyCollection<ToDoItem> changes,
            CancellationToken cancellationToken)
        {
            Console.WriteLine($"Started handling changes for lease {context.LeaseToken}...");
            // 変更フィードリクエストで消費されたRU
            Console.WriteLine($"Change Feed request consumed {context.Headers.RequestCharge} RU.");
            // 別のクライアントインスタンスでセッションの一貫性を確保するために必要なSessionToken
            Console.WriteLine($"SessionToken ${context.Headers.Session}");

            // 操作の診断に1秒以上かかったかどうか
            if (context.Diagnostics.GetClientElapsedTime() > TimeSpan.FromSeconds(1))
            {
                Console.WriteLine($"Change Feed request took longer than expected. Diagnostics:" + context.Diagnostics.ToString());
            }

            foreach (ToDoItem item in changes)
            {
                Console.WriteLine($"Detected operation for item with id {item.id}, key {item.key}, value {item.value}, created at {item.creationTime}.");
                // Simulate some asynchronous operation
                await Task.Delay(10);
            }

            Console.WriteLine("Finished handling changes.");
        }
    }

    record ToDoItem(string id, string key, string value, string creationTime);
}
