using CosmosTargetConsole.Models;
using Microsoft.Azure.Documents.Client;
using Newtonsoft.Json;
using System;
using System.Net.Http;
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
            var client = new DocumentClient(accountEndpoint, key);

            Console.WriteLine($"Account Endpoint:  {accountEndpoint}");
            Console.WriteLine($"Account Key:  {key}");
            Console.WriteLine($"Target URL:  {targetUrl}");
            Console.WriteLine();
            Console.WriteLine("Target Content:");

            var targetContent = GetContentAsync(targetUrl).Result;

            Console.WriteLine();
            Console.WriteLine(targetContent);
            Console.WriteLine();

            var target = JsonConvert.DeserializeObject<TargetModel>(targetContent);
        }

        private static async Task<string> GetContentAsync(string url)
        {
            var client = new HttpClient();
            var content = await client.GetStringAsync(url);

            return content;
        }
    }
}