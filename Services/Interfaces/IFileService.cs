using NCRManagementSystem.Models.DTOs;

namespace NCRManagementSystem.Services.Interfaces
{
    public interface IFileService
    {
        Task<NCRFileDto?> SaveNCRFileAsync(IFormFile file, int ncrId, int uploadedBy, string category = "General");
        Task<NCRFileDto?> GetNCRFileAsync(int fileId);
        Task<List<NCRFileDto>> GetNCRFilesAsync(int ncrId);
        Task<bool> DeleteNCRFileAsync(int fileId);
        Task<byte[]?> GetFileContentAsync(string filePath);
        bool IsValidFileType(string fileName);
        bool IsValidFileSize(long fileSize);
    }
}
