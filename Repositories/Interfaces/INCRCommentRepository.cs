using NCRManagementSystem.Models.Entities;

namespace NCRManagementSystem.Repositories.Interfaces
{
    public interface INCRCommentRepository
    {
        Task<List<NCRComment>> GetByNCRIdAsync(int ncrId);
        Task<int> CreateAsync(NCRComment comment);
        Task<bool> UpdateAsync(NCRComment comment);
        Task<bool> DeleteAsync(int commentId);
    }
}
