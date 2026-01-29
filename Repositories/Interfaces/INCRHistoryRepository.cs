using NCRManagementSystem.Models.Entities;

namespace NCRManagementSystem.Repositories.Interfaces
{
    public interface INCRHistoryRepository
    {
        Task<List<NCRHistory>> GetByNCRIdAsync(int ncrId);
        Task<int> CreateAsync(NCRHistory history);
    }
}
