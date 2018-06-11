using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace CosmosTargetConsole
{
    public static class ContentHelper
    {
        public static async Task<string> GetContentAsync(string url)
        {
            var client = new HttpClient();
            var content = await client.GetStringAsync(url);

            return content;
        }
    }
}