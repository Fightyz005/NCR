using NCRManagementSystem.Models.DTOs;
using NCRManagementSystem.Models.Entities;
using NCRManagementSystem.Repositories.Interfaces;
using NCRManagementSystem.Services.Interfaces;

namespace NCRManagementSystem.Services.Implementations
{
    public class SupplierService : ISupplierService
    {
        private readonly ISupplierRepository _supplierRepository;
        private readonly ILogger<SupplierService> _logger;

        public SupplierService(ISupplierRepository supplierRepository, ILogger<SupplierService> logger)
        {
            _supplierRepository = supplierRepository;
            _logger = logger;
        }

        public async Task<List<SupplierDto>> GetAllActiveSuppliersAsync()
        {
            try
            {
                var suppliers = await _supplierRepository.GetAllActiveAsync();
                return suppliers.Select(s => new SupplierDto
                {
                    SupplierId = s.SupplierId,
                    SupplierCode = s.SupplierCode,
                    SupplierName = s.SupplierName,
                    ContactPerson = s.ContactPerson,
                    Email = s.Email,
                    Phone = s.Phone,
                    Address = s.Address,
                    IsActive = s.IsActive
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active suppliers");
                return new List<SupplierDto>();
            }
        }

        public async Task<List<SupplierDto>> GetAllSuppliersAsync()
        {
            try
            {
                var suppliers = await _supplierRepository.GetAllAsync();
                return suppliers.Select(s => new SupplierDto
                {
                    SupplierId = s.SupplierId,
                    SupplierCode = s.SupplierCode,
                    SupplierName = s.SupplierName,
                    ContactPerson = s.ContactPerson,
                    Email = s.Email,
                    Phone = s.Phone,
                    Address = s.Address,
                    IsActive = s.IsActive
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all suppliers");
                return new List<SupplierDto>();
            }
        }

        public async Task<SupplierDto?> GetSupplierByIdAsync(int supplierId)
        {
            try
            {
                var supplier = await _supplierRepository.GetByIdAsync(supplierId);
                if (supplier == null) return null;

                return new SupplierDto
                {
                    SupplierId = supplier.SupplierId,
                    SupplierCode = supplier.SupplierCode,
                    SupplierName = supplier.SupplierName,
                    ContactPerson = supplier.ContactPerson,
                    Email = supplier.Email,
                    Phone = supplier.Phone,
                    Address = supplier.Address,
                    IsActive = supplier.IsActive
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting supplier by ID {SupplierId}", supplierId);
                return null;
            }
        }

        public async Task<int> CreateSupplierAsync(Supplier supplier)
        {
            try
            {
                return await _supplierRepository.CreateAsync(supplier);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating supplier");
                throw;
            }
        }

        public async Task<bool> UpdateSupplierAsync(Supplier supplier)
        {
            try
            {
                return await _supplierRepository.UpdateAsync(supplier);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating supplier {SupplierId}", supplier.SupplierId);
                throw;
            }
        }

        public async Task<bool> DeleteSupplierAsync(int supplierId)
        {
            try
            {
                return await _supplierRepository.DeleteAsync(supplierId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting supplier {SupplierId}", supplierId);
                throw;
            }
        }
    }
}
