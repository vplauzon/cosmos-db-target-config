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
            var accountEndpoint = new Uri(Environment.GetEnvironmentVariable("ACCOUNT_ENDPOINT"));
            var key = Environment.GetEnvironmentVariable("ACCOUNT_KEY");
            var targetUrl = Environment.GetEnvironmentVariable("TARGET_URL");
            var gateway = new CosmosGateway(accountEndpoint, key);

            Console.WriteLine($"Account Endpoint:  {accountEndpoint}");
            Console.WriteLine($"Account Key:  {key}");
            Console.WriteLine($"Target URL:  {targetUrl}");
            Console.WriteLine();
            Console.WriteLine("Target Content:");

            var targetContent = ContentHelper.GetContentAsync(targetUrl).Result;

            Console.WriteLine();
            Console.WriteLine(targetContent);
            Console.WriteLine();

            var account = JsonConvert.DeserializeObject<AccountModel>(targetContent);

            account.ConvergeTargetAsync(gateway).Wait();
        }
    }
}