using System.Threading.Tasks;

namespace VA.Framework.Common.Azure.KeyVault.Definition
{
    public interface IAzureKeyVaultSecretsClient
    {
        Task<string> GetSecret(string secretName);
    }
}
