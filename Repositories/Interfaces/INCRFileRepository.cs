using NCRManagementSystem.Models.Entities;

namespace NCRManagementSystem.Repositories.Interfaces
{
    public interface INCRFileRepository
    {
        Task<NCRFile?> GetByIdAsync(int fileId);
        Task<List<NCRFile>> GetByNCRIdAsync(int ncrId);
        Task<int> CreateAsync(NCRFile file);
        Task<bool> DeleteAsync(int fileId);
        Task<bool> DeleteByNCRIdAsync(int ncrId);
    }
}
