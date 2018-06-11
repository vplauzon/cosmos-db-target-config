﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;

namespace CosmosTargetConsole.Models
{
    public class ThroughputModel
    {
        public int RU { get; set; }

        public static ThroughputModel CreateDefaultUnpartitioned()
        {
            return new ThroughputModel
            {
                RU = 400
            };
        }

        public static ThroughputModel CreateDefaultPartitioned()
        {
            return new ThroughputModel
            {
                RU = 10000
            };
        }
    }
}