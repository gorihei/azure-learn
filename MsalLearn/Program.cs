using Microsoft.Identity.Client;
namespace MsalLearn
{
    public class Program
    {
        private const string _clientId = "9eaa2ff9-460c-4b17-bb0c-086288b178a1";
        private const string _tenantId = "f73db128-b025-4ec6-b63a-c3b4eff7a04b";

        static async Task Main(string[] args)
        {
            var app = PublicClientApplicationBuilder
                .Create(_clientId)
                .WithAuthority(AzureCloudInstance.AzurePublic, _tenantId)
                .WithRedirectUri("http://localhost")
                .Build();

            string[] scopes = { "user.read" };
            var result = await app.AcquireTokenInteractive(scopes).ExecuteAsync();
            Console.WriteLine($"Token:\t{result.AccessToken}");
        }
    }
}
