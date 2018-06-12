using Microsoft.Azure.Documents;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CosmosTargetConsole
{
    public class ExecutionContext
    {
        private readonly string[] _destructiveFlags;

        public ExecutionContext(CosmosGateway gateway, Uri configUrl, string[] destructiveFlags)
        {
            Gateway = gateway ?? throw new ArgumentNullException(nameof(gateway));
            ConfigUrl = configUrl ?? throw new ArgumentNullException(nameof(configUrl));
            _destructiveFlags = destructiveFlags ?? new string[0];
        }

        private ExecutionContext(
            CosmosGateway gateway,
            Uri configUrl,
            string[] destructiveFlags,
            Database database) : this(gateway, configUrl, destructiveFlags)
        {
            Database = database ?? throw new ArgumentNullException(nameof(database));
        }

        private ExecutionContext(
            CosmosGateway gateway,
            Uri configUrl,
            string[] destructiveFlags,
            Database database,
            DocumentCollection collection) : this(gateway, configUrl, destructiveFlags, database)
        {
            Collection = collection ?? throw new ArgumentNullException(nameof(collection));
        }

        public ExecutionContext AddDatabase(Database database)
        {
            return new ExecutionContext(Gateway, ConfigUrl, _destructiveFlags, database);
        }

        public ExecutionContext AddCollection(DocumentCollection collection)
        {
            return new ExecutionContext(
                Gateway,
                ConfigUrl,
                _destructiveFlags,
                Database,
                collection);
        }

        public bool CanDestroyDatabase { get { return _destructiveFlags.Contains("database"); } }

        public bool CanDestroyCollection { get { return _destructiveFlags.Contains("collection"); } }

        public bool CanDestroyStoredProcedure { get { return _destructiveFlags.Contains("storedProcedure"); } }

        public CosmosGateway Gateway { get; }

        public Uri ConfigUrl { get; }

        public Database Database { get; }

        public DocumentCollection Collection { get; }
    }
}