using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;

namespace CosmosTargetConsole.Models
{
    public class ThroughputModel
    {
        public int RU { get; set; }

        public bool EnableRUPerMinute { get; set; }
    }
}