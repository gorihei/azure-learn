using azure_learn.StorageAccount;

namespace azure_learn
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            await new BlobLearn().Exec();
        }
    }
}
