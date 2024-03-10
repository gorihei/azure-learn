using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using System.Configuration;

namespace azure_learn.KeyVault
{
    public static class KeyVault
    {
        private static readonly string? BaseUrl;

        static KeyVault()
        {
            BaseUrl = ConfigurationManager.AppSettings.GetValues("KeyVault")?.FirstOrDefault();
        }

        public static string GetSecret(string secretName)
        {
            if (BaseUrl == null) throw new ArgumentNullException(nameof(BaseUrl));
            var client = new SecretClient(new Uri(BaseUrl!), new DefaultAzureCredential());
            var secret = client.GetSecret(secretName).Value;
            return secret.Value;
        }
    }
}
