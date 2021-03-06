using Ardalis.Specification;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProjectManagement.DataAccess.Repositories.Interfaces
{
    public interface IRepository<T, TKey> where T : ProjectManagement.Domain.Interfaces.IEntity<TKey>
    {
        IUnitOfWork UnitOfWork { get; }
        Task<T> GetByIdAsync(TKey id);
        Task<IEnumerable<T>> GetAllAsync();
        Task<T> InsertAsync(T item);
        Task UpdateAsync(T item);
        Task DeleteAsync(T item);
        Task DeleteByIdAsync(TKey id);
        Task<T> GetSingleAsync(ISpecification<T> specification);
        Task<IEnumerable<T>> GetManyAsync(ISpecification<T> specification);
    }
}
