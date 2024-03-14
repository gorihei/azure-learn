using StackExchange.Redis;

string connectionString = "my connection string";

using (var cache = ConnectionMultiplexer.Connect(connectionString))
{
    var db = cache.GetDatabase();

    // サーバーに対してPINGコマンドを実行して接続をテストする
    var result = await db.ExecuteAsync("ping");
    Console.WriteLine($"PING = {result.Type} : {result}");

    // StringSetAsyncメソッドを使用してIDatabaseオブジェクトにキーと値を設定する
    var setResult = await db.StringSetAsync("test:key", "test-value");
    Console.WriteLine($"SET: {setResult}");

    // StringGetAsyncメソッドを使用してキーに紐づく値を取得する
    var getResult = await db.StringGetAsync("test:key");
    Console.WriteLine($"GET: {getResult}");
}