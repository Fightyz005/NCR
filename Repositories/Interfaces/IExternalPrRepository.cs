using NCRManagementSystem.Models.DTOs;

namespace NCRManagementSystem.Repositories.Interfaces
{
    public interface IExternalPrRepository
    {
        Task<List<ExternalPrItemDto>> GetPrItemsAsync(string banfn);
    }
}
