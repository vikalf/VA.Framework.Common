using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using System;
using System.Threading.Tasks;
using VA.Framework.Common.Azure.KeyVault.Definition;

namespace VA.Framework.Common.Azure.KeyVault.Implementation
{
    public class AzureKeyVaultSecretsClient : IAzureKeyVaultSecretsClient
    {
        private readonly string _keyVaultBaseUrl;
        private readonly string _clientId;
        private readonly string _clientSecret;

        /// <summary>
        ///    Obtains a secret from the Azure Active Directory service, using the specified
        //     client details specified in the environment variables AZURE_TENANT_ID, AZURE_CLIENT_ID,
        //     and AZURE_CLIENT_SECRET or AZURE_USERNAME and AZURE_PASSWORD to authenticate.
        /// </summary>
        /// <param name="keyVaultBaseUrl"></param>
        /// <param name="clientId"></param>
        /// <param name="clientSecret"></param>
        public AzureKeyVaultSecretsClient(string keyVaultBaseUrl, string clientId, string clientSecret)
        {
            _keyVaultBaseUrl = keyVaultBaseUrl;
            _clientId = clientId;
            _clientSecret = clientSecret;
        }


        public async Task<string> GetSecret(string secretName)
        {
            var client = new SecretClient(new Uri(_keyVaultBaseUrl), new EnvironmentCredential());
            var secret = await client.GetSecretAsync(secretName);
            return secret.Value.Value;
        }
    }
}
