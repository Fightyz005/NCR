using NCRManagementSystem.Models.DTOs;
using NCRManagementSystem.Models.Entities;
using NCRManagementSystem.Repositories.Interfaces;
using NCRManagementSystem.Services.Interfaces;

namespace NCRManagementSystem.Services.Implementations
{
    public class FileService : IFileService
    {
        private readonly INCRFileRepository _fileRepository;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<FileService> _logger;
        private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".pdf", ".xlsx", ".xls", ".doc", ".docx" };
        private readonly long _maxFileSize = 10 * 1024 * 1024; // 10MB

        public FileService(INCRFileRepository fileRepository, IWebHostEnvironment environment, ILogger<FileService> logger)
        {
            _fileRepository = fileRepository;
            _environment = environment;
            _logger = logger;
        }

        public async Task<NCRFileDto?> SaveNCRFileAsync(IFormFile file, int ncrId, int uploadedBy, string category = "General")
        {
            try
            {
                if (file == null || file.Length == 0)
                    return null;

                if (!IsValidFileType(file.FileName) || !IsValidFileSize(file.Length))
                    return null;

                var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "ncr-files");
                Directory.CreateDirectory(uploadsFolder);

                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }

                var ncrFile = new NCRFile
                {
                    NCRId = ncrId,
                    FileName = fileName,
                    OriginalFileName = file.FileName,
                    FilePath = filePath,
                    FileSize = file.Length,
                    FileType = file.ContentType,
                    UploadedBy = uploadedBy,
                    UploadedDate = DateTime.Now,
                    FileCategory = category
                };

                var fileId = await _fileRepository.CreateAsync(ncrFile);

                return new NCRFileDto
                {
                    FileId = fileId,
                    NCRId = ncrId,
                    FileName = fileName,
                    OriginalFileName = file.FileName,
                    FilePath = filePath,
                    FileSize = file.Length,
                    FileType = file.ContentType,
                    UploadedDate = DateTime.Now,
                    FileCategory = category
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving NCR file");
                return null;
            }
        }

        public async Task<NCRFileDto?> GetNCRFileAsync(int fileId)
        {
            try
            {
                var file = await _fileRepository.GetByIdAsync(fileId);
                if (file == null) return null;

                return new NCRFileDto
                {
                    FileId = file.FileId,
                    NCRId = file.NCRId,
                    FileName = file.FileName,
                    OriginalFileName = file.OriginalFileName,
                    FilePath = file.FilePath,
                    FileSize = file.FileSize,
                    FileType = file.FileType,
                    UploadedDate = file.UploadedDate,
                    FileCategory = file.FileCategory
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting NCR file {FileId}", fileId);
                return null;
            }
        }

        public async Task<List<NCRFileDto>> GetNCRFilesAsync(int ncrId)
        {
            try
            {
                var files = await _fileRepository.GetByNCRIdAsync(ncrId);
                return files.Select(f => new NCRFileDto
                {
                    FileId = f.FileId,
                    NCRId = f.NCRId,
                    FileName = f.FileName,
                    OriginalFileName = f.OriginalFileName,
                    FilePath = f.FilePath,
                    FileSize = f.FileSize,
                    FileType = f.FileType,
                    UploadedDate = f.UploadedDate,
                    FileCategory = f.FileCategory
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting NCR files for NCR {NCRId}", ncrId);
                return new List<NCRFileDto>();
            }
        }

        public async Task<bool> DeleteNCRFileAsync(int fileId)
        {
            try
            {
                var file = await _fileRepository.GetByIdAsync(fileId);
                if (file == null) return false;

                // Delete physical file
                if (File.Exists(file.FilePath))
                {
                    File.Delete(file.FilePath);
                }

                // Delete from database
                return await _fileRepository.DeleteAsync(fileId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting NCR file {FileId}", fileId);
                return false;
            }
        }

        public async Task<byte[]?> GetFileContentAsync(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                    return null;

                return await File.ReadAllBytesAsync(filePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading file content from {FilePath}", filePath);
                return null;
            }
        }

        public bool IsValidFileType(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            return _allowedExtensions.Contains(extension);
        }

        public bool IsValidFileSize(long fileSize)
        {
            return fileSize <= _maxFileSize;
        }
    }
}
