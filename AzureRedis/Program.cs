using StackExchange.Redis;
namespace AzureRedis
{
    public class Program
    {
        private readonly static string connectionString = "my connection string";

        static async Task Main(string[] args)
        {
            using (var cache = ConnectionMultiplexer.Connect(connectionString))
            {
                var db = cache.GetDatabase();

                // サーバーに対してPINGコマンドを実行して接続をテストする
                var result = await db.ExecuteAsync("ping");
                Console.WriteLine($"PING = {result.Resp2Type} : {result}");

                // StringSetAsyncメソッドを使用してIDatabaseオブジェクトにキーと値を設定する
                var setResult = await db.StringSetAsync("test:key", "test-value");
                Console.WriteLine($"SET: {setResult}");

                // StringGetAsyncメソッドを使用してキーに紐づく値を取得する
                var getResult = await db.StringGetAsync("test:key");
                Console.WriteLine($"GET: {getResult}");
            }
        }
    }
}
