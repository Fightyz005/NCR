using NCRManagementSystem.Models.Entities;
using NCRManagementSystem.Models.DTOs;

namespace NCRManagementSystem.Repositories.Interfaces
{
    public interface ISupplierRepository
    {
        Task<Supplier?> GetByIdAsync(int supplierId);
        Task<List<Supplier>> GetAllActiveAsync();
        Task<List<Supplier>> GetAllAsync();
        Task<int> CreateAsync(Supplier supplier);
        Task<bool> UpdateAsync(Supplier supplier);
        Task<bool> DeleteAsync(int supplierId);
    }
}
