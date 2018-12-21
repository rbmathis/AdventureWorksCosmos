using System.Threading.Tasks;
using AdventureWorksCosmos.Core.Commands;
using AdventureWorksCosmos.Core.Infrastructure;
using NServiceBus;

namespace AdventureWorksCosmos.Dispatcher
{
    public class SagaDocumentMessageHandler : IHandleMessages<SagaCommand>
    {
        private readonly IDocumentMessageDispatcher _dispatcher;

        public SagaDocumentMessageHandler(IDocumentMessageDispatcher dispatcher)  => _dispatcher = dispatcher;

        public Task Handle(SagaCommand message, IMessageHandlerContext context)  => _dispatcher.Dispatch(message);
    }
}