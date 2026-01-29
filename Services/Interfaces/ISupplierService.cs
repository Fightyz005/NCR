using NCRManagementSystem.Models.DTOs;
using NCRManagementSystem.Models.Entities;

namespace NCRManagementSystem.Services.Interfaces
{
    public interface ISupplierService
    {
        Task<List<SupplierDto>> GetAllActiveSuppliersAsync();
        Task<List<SupplierDto>> GetAllSuppliersAsync();
        Task<SupplierDto?> GetSupplierByIdAsync(int supplierId);
        Task<int> CreateSupplierAsync(Supplier supplier);
        Task<bool> UpdateSupplierAsync(Supplier supplier);
        Task<bool> DeleteSupplierAsync(int supplierId);
    }
}
