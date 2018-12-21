using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using AdventureWorksCosmos.Core;
using AdventureWorksCosmos.Core.Infrastructure;
using AdventureWorksCosmos.Core.Models.Fulfillments;
using AdventureWorksCosmos.Core.Models.Inventory;
using AdventureWorksCosmos.Core.Models.Orders;
using MediatR;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.ChangeFeedProcessor;
using Microsoft.Azure.Documents.ChangeFeedProcessor.DataAccess;
using Microsoft.Azure.Documents.ChangeFeedProcessor.PartitionManagement;
using Microsoft.Azure.Documents.Client;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using NServiceBus;
using StructureMap;

namespace AdventureWorksCosmos.Dispatcher
{
    public class Program
    {
		private static string HostName { get; set; }
		private static string CosmosUrl { get; set; }
		private static string CosmosKey { get; set; }

		public static Container Container { get; private set; }
        public static IEndpointInstance Endpoint { get; private set; }

        static async Task Main()
        {

			var config = new ConfigurationBuilder().AddJsonFile("private.json");
			var _config = config.Build();
			HostName = _config["Dispatcher:HostName"];
			CosmosUrl = _config["Dispatcher:CosmosUrl"];
			CosmosKey = _config["Dispatcher:CosmosKey"];

			var client = new DocumentClient(new Uri(CosmosUrl), CosmosKey, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto
            });

            Container = new Container(cfg =>
            {
                cfg.Scan(scanner =>
                {
                    scanner.AssemblyContainingType<IDocumentMessage>();
                    scanner.ConnectImplementationsToTypesClosing(typeof(IRequestHandler<,>));
                    scanner.ConnectImplementationsToTypesClosing(typeof(INotificationHandler<>));
                    scanner.ConnectImplementationsToTypesClosing(typeof(IDocumentMessageHandler<>));
                });


                cfg.For<IMediator>().Use<Mediator>();
                cfg.For<ServiceFactory>().Use<ServiceFactory>(ctx => ctx.GetInstance);

                cfg.For(typeof(IPipelineBehavior<,>)).Add(typeof(UnitOfWorkBehavior<,>));
                cfg.For(typeof(IDocumentDBRepository<>)).Use(typeof(DocumentDBRepository<>));
                cfg.For<IUnitOfWork>().Use<UnitOfWork>();
                cfg.For<DocumentClient>().Use(client);
                cfg.For<IDocumentMessageDispatcher>().Use<DocumentMessageDispatcher>();
                cfg.For<IOfflineDispatcher>().Use<UniformSessionOfflineDispatcher>();
            });

            var endpointConfiguration = new EndpointConfiguration(HostName);
            endpointConfiguration.UseContainer<StructureMapBuilder>(customizations => customizations.ExistingContainer(Container));
            endpointConfiguration.EnableUniformSession();

            var transport = endpointConfiguration.UseTransport<RabbitMQTransport>();
            transport.UseConventionalRoutingTopology();
            transport.ConnectionString("host=localhost");

            endpointConfiguration.EnableInstallers();
            endpointConfiguration.UsePersistence<InMemoryPersistence>();

            Endpoint = await NServiceBus.Endpoint.Start(endpointConfiguration).ConfigureAwait(false);

            var builders = new[]
            {
                CreateBuilder<Order>(client),
                CreateBuilder<OrderSaga>(client),
                CreateBuilder<Inventory>(client),
            };

            var databases = new[] {nameof(Order), nameof(OrderSaga), nameof(Inventory)};
            foreach (var databaseId in databases)
            {
                await CreateDatabaseIfNotExistsAsync(client, databaseId);
                await CreateCollectionIfNotExistsAsync(client, databaseId, "Items");
                await CreateCollectionIfNotExistsAsync(client, databaseId, "Leases");
            }

            var processors = new List<IChangeFeedProcessor>();
            foreach (var builder in builders)
            {
                var processor = await builder.BuildAsync();
                await processor.StartAsync();
                processors.Add(processor);
            }

            Console.WriteLine("Press any key to exit");
            Console.ReadKey();

            await Endpoint.Stop().ConfigureAwait(false);

            foreach (var processor in processors)
            {
                await processor.StopAsync();
            }
        }

        private static ChangeFeedProcessorBuilder CreateBuilder<T>(DocumentClient client) where T : DocumentBase
        {
            var builder = new ChangeFeedProcessorBuilder();
            var uri = new Uri(CosmosUrl);
            var dbClient = new ChangeFeedDocumentClient(client);

            builder
                .WithHostName(HostName)
                .WithFeedCollection(new DocumentCollectionInfo
                {
                    DatabaseName = typeof(T).Name,
                    CollectionName = "Items",
                    Uri = uri,
                    MasterKey = CosmosKey
                })
                .WithLeaseCollection(new DocumentCollectionInfo
                {
                    DatabaseName = typeof(T).Name,
                    CollectionName = "Leases",
                    Uri = uri,
                    MasterKey = CosmosKey
                })
                .WithProcessorOptions(new ChangeFeedProcessorOptions
                {
                    FeedPollDelay = TimeSpan.FromSeconds(15),
                })
                .WithFeedDocumentClient(dbClient)
                .WithLeaseDocumentClient(dbClient)
                .WithObserver<DocumentFeedObserver<T>>();

            return builder;
        }

        private static async Task CreateDatabaseIfNotExistsAsync(DocumentClient client, string databaseId)
        {
            try
            {
                await client.ReadDatabaseAsync(UriFactory.CreateDatabaseUri(databaseId));
            }
            catch (DocumentClientException e)
            {
                if (e.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    await client.CreateDatabaseAsync(new Database { Id = databaseId });
                }
                else
                {
                    throw;
                }
            }
        }

        private static async Task CreateCollectionIfNotExistsAsync(DocumentClient client, string databaseId, string collectionId)
        {
            try
            {
                await client.ReadDocumentCollectionAsync(UriFactory.CreateDocumentCollectionUri(databaseId, collectionId));
            }
            catch (DocumentClientException e)
            {
                if (e.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    await client.CreateDocumentCollectionAsync(
                        UriFactory.CreateDatabaseUri(databaseId),
                        new DocumentCollection { Id = collectionId },
                        new RequestOptions { OfferThroughput = 1000 });
                }
                else
                {
                    throw;
                }
            }
        }

    }
}