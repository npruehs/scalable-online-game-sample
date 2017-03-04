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

        public async Task<LoginResponse> LoginAsync(string playerId, CancellationToken cancellationToken)
        {
            PlayerDocument playerDocument = null;

            try
            {
                // Ensure player document exists.
                var documentUri = Microsoft.Azure.Documents.Client.UriFactory.CreateDocumentUri
                    (DatabaseName, CollectionName, playerId);
                await this.client.ReadDocumentAsync(documentUri);

                // Get player document.
                var feedOptions =
                    new Microsoft.Azure.Documents.Client.FeedOptions { MaxItemCount = 1 };

                var collectionUri =
                    Microsoft.Azure.Documents.Client.UriFactory.CreateDocumentCollectionUri(DatabaseName, CollectionName);
                var playerQuery =
                    client.CreateDocumentQuery<PlayerDocument>(collectionUri, feedOptions)
                    .Where(p => p.Id == playerId);

                // Execute query.
                foreach (PlayerDocument player in playerQuery)
                {
                    playerDocument = player;

                    // Increase login count.
                    ++player.LoginCount;
                    await client.ReplaceDocumentAsync(documentUri, player);
                }
            }
            catch (Microsoft.Azure.Documents.DocumentClientException e)
            {
                if (e.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    // Create new player document.
                    var collectionUri = Microsoft.Azure.Documents.Client.UriFactory.CreateDocumentCollectionUri
                        (DatabaseName, CollectionName);
                    playerDocument = new PlayerDocument { Id = playerId, LoginCount = 1 };
                    await this.client.CreateDocumentAsync(collectionUri, playerDocument);
                }
                else
                {
                    throw;
                }
            }

            // Return response.
            var loginReponse = new LoginResponse
            {
                Id = playerDocument.Id,
                LoginCount = playerDocument.LoginCount
            };

            return loginReponse;
        }
    }
}
