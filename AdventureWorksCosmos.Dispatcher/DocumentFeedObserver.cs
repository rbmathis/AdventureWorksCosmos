using AdventureWorksCosmos.Core;
using AdventureWorksCosmos.Core.Commands;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.ChangeFeedProcessor.FeedProcessing;
using NServiceBus;
using NServiceBus.Logging;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AdventureWorksCosmos.Dispatcher
{
    public class DocumentFeedObserver<T> : IChangeFeedObserver where T : DocumentBase
    {
        static ILog log = LogManager.GetLogger<DocumentFeedObserver<T>>();

        public Task OpenAsync(IChangeFeedObserverContext context) => Task.CompletedTask;

        public Task CloseAsync(IChangeFeedObserverContext context, ChangeFeedObserverCloseReason reason)  => Task.CompletedTask;

        public async Task ProcessChangesAsync(IChangeFeedObserverContext context, IReadOnlyList<Document> docs, CancellationToken cancellationToken)
        {
            foreach (var doc in docs)
            {
                log.Info($"Processing changes for document {doc.Id}");

                var item = (dynamic)doc;

                if (item.Outbox.Count > 0)
                {
                    SagaCommand message = SagaCommand.New<T>(item);

                    await Program.Endpoint.SendLocal(message);
                }
            }
        }
    }
}