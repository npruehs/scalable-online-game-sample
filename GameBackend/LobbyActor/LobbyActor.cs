using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using Microsoft.ServiceFabric.Actors.Client;
using LobbyActor.Interfaces;

namespace LobbyActor
{
    /// <remarks>
    /// This class represents an actor.
    /// Every ActorID maps to an instance of this class.
    /// The StatePersistence attribute determines persistence and replication of actor state:
    ///  - Persisted: State is written to disk and replicated.
    ///  - Volatile: State is kept in memory only and replicated.
    ///  - None: State is kept in memory only and not replicated.
    /// </remarks>
    [StatePersistence(StatePersistence.Persisted)]
    internal class LobbyActor : Actor, ILobbyActor
    {
        private const string DatabaseName = "GameDatabase";

        private const string CollectionName = "Players";

        private Microsoft.Azure.Documents.Client.DocumentClient client;

        /// <summary>
        /// Initializes a new instance of LobbyActor
        /// </summary>
        /// <param name="actorService">The Microsoft.ServiceFabric.Actors.Runtime.ActorService that will host this actor instance.</param>
        /// <param name="actorId">The Microsoft.ServiceFabric.Actors.ActorId for this actor instance.</param>
        public LobbyActor(ActorService actorService, ActorId actorId)
            : base(actorService, actorId)
        {
        }

        /// <summary>
        /// This method is called whenever an actor is activated.
        /// An actor is activated the first time any of its methods are invoked.
        /// </summary>
        protected override async Task OnActivateAsync()
        {
            ActorEventSource.Current.ActorMessage(this, "Actor activated.");

            // Create the client.
            this.client = new Microsoft.Azure.Documents.Client.DocumentClient(
                new Uri("https://localhost:8081"),
                "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==");

            // Verify the database exist.
            try
            {
                var uri = Microsoft.Azure.Documents.Client.UriFactory.CreateDatabaseUri(DatabaseName);
                await this.client.ReadDatabaseAsync(uri);
            }
            catch (Microsoft.Azure.Documents.DocumentClientException e)
            {
                // If the database does not exist, create a new database.
                if (e.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    var database = new Microsoft.Azure.Documents.Database { Id = DatabaseName };
                    await this.client.CreateDatabaseAsync(database);
                }
                else
                {
                    throw;
                }
            }

            // Verify player collection exists.
            try
            {
                var uri = Microsoft.Azure.Documents.Client.UriFactory.CreateDocumentCollectionUri(DatabaseName, CollectionName);
                await this.client.ReadDocumentCollectionAsync(uri);
            }
            catch (Microsoft.Azure.Documents.DocumentClientException e)
            {
                // If the document collection does not exist, create a new collection.
                if (e.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    var databaseUri = Microsoft.Azure.Documents.Client.UriFactory.CreateDatabaseUri(DatabaseName);
                    var collection = new Microsoft.Azure.Documents.DocumentCollection() { Id = CollectionName };
                    await this.client.CreateDocumentCollectionAsync(databaseUri, collection);
                }
                else
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// TODO: Replace with your own actor method.
        /// </summary>
        /// <returns></returns>
        Task<int> ILobbyActor.GetCountAsync(CancellationToken cancellationToken)
        {
            return this.StateManager.GetStateAsync<int>("count", cancellationToken);
        }

        /// <summary>
        /// TODO: Replace with your own actor method.
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        Task ILobbyActor.SetCountAsync(int count, CancellationToken cancellationToken)
        {
            // Requests are not guaranteed to be processed in order nor at most once.
            // The update function here verifies that the incoming count is greater than the current count to preserve order.
            return this.StateManager.AddOrUpdateStateAsync("count", count, (key, value) => count > value ? count : value, cancellationToken);
        }
    }
}
