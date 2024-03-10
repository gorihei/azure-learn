using Azure.Identity;
using Microsoft.Graph;

namespace azure_learn.GraphApi
{
    public class GraphApi
    {
        private readonly string _tenantId;
        private readonly string _clientId;
        private readonly string _clientSecret;
        private readonly string[] _scopes = new[] { "https://graph.microsoft.com/.default" };

        public GraphApi()
        {
            _tenantId = KeyVault.KeyVault.GetSecret("TenantId");
            _clientId = KeyVault.KeyVault.GetSecret("TestApplicationClientId");
            _clientSecret = KeyVault.KeyVault.GetSecret("TestApplicationSecret");
        }

        public async Task Exec()
        {
            await Case1();
        }

        private async Task Case1()
        {
            // show user display name
            var client = CreateGraphServiceClient();
            var users = await client.Users.GetAsync();
            foreach (var user in users!.Value!)
            {
                Console.WriteLine(user.DisplayName);
            }
        }

        private GraphServiceClient CreateGraphServiceClient()
        {
            var options = new ClientSecretCredentialOptions
            {
                AuthorityHost = AzureAuthorityHosts.AzurePublicCloud,
            };
            var clientSecretCredential = new ClientSecretCredential(
                _tenantId, _clientId, _clientSecret, options);

            return new GraphServiceClient(clientSecretCredential, _scopes);
        }
    }
}
