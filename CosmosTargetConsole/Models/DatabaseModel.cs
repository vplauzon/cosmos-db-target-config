using System;
using System.Collections.Generic;
using System.Text;

namespace CosmosTargetConsole.Models
{
    public class DatabaseModel
    {
        public string Name { get; set; }

        public CollectionModel[] Collections { get; set; }
    }
}