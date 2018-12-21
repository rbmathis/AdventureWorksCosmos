using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;

namespace AdventureWorksCosmos.Core.Infrastructure
{
    public interface IDocumentDBRepository<T> where T : DocumentBase
    {
        Task<Document> CreateAsync(T item);
        Task DeleteAsync(Guid id);
        Task<T> LoadAsync(Guid id);
        Task<IEnumerable<T>> ListAsync(Expression<Func<T, bool>> predicate);
        Task<Document> UpdateAsync(T item);
    }
}