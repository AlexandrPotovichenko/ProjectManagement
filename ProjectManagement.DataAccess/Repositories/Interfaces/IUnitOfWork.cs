using System.Threading;
using System.Threading.Tasks;

namespace ProjectManagement.DataAccess.Repositories.Interfaces
{
    public interface IUnitOfWork
    {
        Task<int> SaveChangesAsync(CancellationToken token = default);
    }
}
