using System;
using NServiceBus;

namespace AdventureWorksCosmos.Core.Commands
{
    public class SagaCommand : ICommand
    {
        public Guid DocumentId { get; set; }
        public string DocumentType { get; set; }

        // For NSB
        public SagaCommand() { }

        private SagaCommand(Guid documentId, string documentType)
        {
            DocumentId = documentId;
            DocumentType = documentType;
        }

        public static SagaCommand New<TDocument>(TDocument document) where TDocument : DocumentBase
        {
            return new SagaCommand(document.Id, document.GetType().AssemblyQualifiedName);
        }
        public static SagaCommand New<TDocument>(dynamic document) where TDocument : DocumentBase
        {
            return new SagaCommand(Guid.Parse(document.Id), typeof(TDocument).AssemblyQualifiedName);
        }
    }
}