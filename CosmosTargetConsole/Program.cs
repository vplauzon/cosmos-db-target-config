using CosmosTargetConsole.Models;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace CosmosTargetConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            var accountEndpointText = Environment.GetEnvironmentVariable("ACCOUNT_ENDPOINT");
            var key = Environment.GetEnvironmentVariable("ACCOUNT_KEY");
            var targetUrl = Environment.GetEnvironmentVariable("TARGET_URL");

            Console.WriteLine($"Account Endpoint:  {accountEndpointText}");
            Console.WriteLine($"Account Key:  {key}");
            Console.WriteLine($"Target URL:  {targetUrl}");
            Console.WriteLine();

            if (string.IsNullOrWhiteSpace(accountEndpointText))
            {
                Console.WriteLine("Environment Variable ACCOUNT_ENDPOINT missing");

                return;
            }
            if (string.IsNullOrWhiteSpace(key))
            {
                Console.WriteLine("Environment Variable ACCOUNT_KEY missing");

                return;
            }
            if (string.IsNullOrWhiteSpace(targetUrl))
            {
                Console.WriteLine("Environment Variable TARGET_URL missing");

                return;
            }

            var accountEndpoint = new Uri(accountEndpointText);

            Console.WriteLine("Target Content:");

            var gateway = new CosmosGateway(accountEndpoint, key);
            var targetUri = new Uri(targetUrl);
            var targetContent = ContentHelper.GetContentAsync(targetUri).Result;

            Console.WriteLine();
            Console.WriteLine(targetContent);
            Console.WriteLine();

            var account = JsonConvert.DeserializeObject<AccountModel>(targetContent);
            var context = new ExecutionContext(
                gateway,
                targetUri,
                account.DestructiveFlags);

            account.ConvergeTargetAsync(context).Wait();
            Console.WriteLine();
            Console.WriteLine("Successfully apply configuration");
        }
    }
}