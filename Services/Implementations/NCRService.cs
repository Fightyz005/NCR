using NCRManagementSystem.Models.DTOs;
using NCRManagementSystem.Models.Entities;
using NCRManagementSystem.Repositories.Interfaces;
using NCRManagementSystem.Services.Interfaces;

namespace NCRManagementSystem.Services.Implementations
{
    public class NCRService : INCRService
    {
        private readonly INCRRepository _ncrRepository;
        private readonly INCRFileRepository _fileRepository;
        private readonly INCRHistoryRepository _historyRepository;
        private readonly INCRCommentRepository _commentRepository;
        private readonly ILogger<NCRService> _logger;

        public NCRService(
            INCRRepository ncrRepository,
            INCRFileRepository fileRepository,
            INCRHistoryRepository historyRepository,
            INCRCommentRepository commentRepository,
            ILogger<NCRService> logger)
        {
            _ncrRepository = ncrRepository;
            _fileRepository = fileRepository;
            _historyRepository = historyRepository;
            _commentRepository = commentRepository;
            _logger = logger;
        }

        public async Task<NCRDto?> GetNCRDetailsAsync(int ncrId)
        {
            try
            {
                var ncr = await _ncrRepository.GetByIdAsync(ncrId);
                if (ncr == null) return null;

                var files = await _fileRepository.GetByNCRIdAsync(ncrId);
                var history = await _historyRepository.GetByNCRIdAsync(ncrId);
                var comments = await _commentRepository.GetByNCRIdAsync(ncrId);

                return new NCRDto
                {
                    NCRId = ncr.NCRId,
                    NCRNumber = ncr.NCRNumber,
                    ProductName = ncr.ProductName,
                    ItemCode = ncr.ItemCode,
                    SupplierId = ncr.SupplierId,
                    Grade = ncr.Grade,
                    Priority = ncr.Priority,
                    ProblemDescription = ncr.ProblemDescription,
                    Status = ncr.Status,
                    CreatedDate = ncr.CreatedDate,
                    DueDate = ncr.DueDate,
                    QAComments = ncr.QAComments,
                    RootCause = ncr.RootCause,
                    CorrectiveAction = ncr.CorrectiveAction,
                    PreventiveAction = ncr.PreventiveAction,
                    ResponsiblePerson = ncr.ResponsiblePerson,
                    ManagerComments = ncr.ManagerComments,
                    Files = files.Select(f => new NCRFileDto
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
                    }).ToList(),
                    History = history.Select(h => new NCRHistoryDto
                    {
                        HistoryId = h.HistoryId,
                        NCRId = h.NCRId,
                        Action = h.Action,
                        Description = h.Description,
                        OldStatus = h.OldStatus,
                        NewStatus = h.NewStatus,
                        ActionDate = h.ActionDate,
                        Comments = h.Comments
                    }).ToList(),
                    Comments = comments.Select(c => new NCRCommentDto
                    {
                        CommentId = c.CommentId,
                        NCRId = c.NCRId,
                        CommentText = c.CommentText,
                        CommentType = c.CommentType,
                        CreatedDate = c.CreatedDate,
                        IsResolved = c.IsResolved,
                        ParentCommentId = c.ParentCommentId
                    }).ToList()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting NCR details for ID {NCRId}", ncrId);
                return null;
            }
        }

        public async Task<PagedResult<NCRDto>> GetPagedNCRsAsync(int pageNumber, int pageSize, string? searchTerm = null,
            string? status = null, string? grade = null, int? supplierId = null,
            DateTime? fromDate = null, DateTime? toDate = null, int? userId = null, string? userRole = null)
        {
            try
            {
                return await _ncrRepository.GetPagedAsync(pageNumber, pageSize, searchTerm, status, grade,
                    supplierId, fromDate, toDate, userId, userRole);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting paged NCRs");
                return new PagedResult<NCRDto>();
            }
        }

        public async Task<List<NCRDto>> GetRecentNCRsAsync(int count = 10)
        {
            try
            {
                return await _ncrRepository.GetRecentAsync(count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting recent NCRs");
                return new List<NCRDto>();
            }
        }

        public async Task<List<PendingTaskDto>> GetPendingNCRsAsync(string userRole, int? userId = null)
        {
            try
            {
                return await _ncrRepository.GetPendingByRoleAsync(userRole, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting pending NCRs");
                return new List<PendingTaskDto>();
            }
        }

        public async Task<int> CreateNCRAsync(NCR ncr)
        {
            try
            {
                return await _ncrRepository.CreateAsync(ncr);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating NCR");
                throw;
            }
        }

        public async Task<bool> UpdateNCRAsync(NCR ncr)
        {
            try
            {
                return await _ncrRepository.UpdateAsync(ncr);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating NCR {NCRId}", ncr.NCRId);
                throw;
            }
        }

        public async Task<bool> UpdateNCRStatusAsync(int ncrId, string newStatus, int updatedBy, string? comments = null)
        {
            try
            {
                return await _ncrRepository.UpdateStatusAsync(ncrId, newStatus, updatedBy, comments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating NCR status for ID {NCRId}", ncrId);
                throw;
            }
        }

        public async Task<bool> DeleteNCRAsync(int ncrId)
        {
            try
            {
                return await _ncrRepository.DeleteAsync(ncrId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting NCR {NCRId}", ncrId);
                throw;
            }
        }

        public async Task<string> GenerateNCRNumberAsync()
        {
            try
            {
                return await _ncrRepository.GenerateNCRNumberAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating NCR number");
                return $"NCR-{DateTime.Now.Year}-001";
            }
        }

        public async Task<bool> AddCommentAsync(int ncrId, string commentText, string commentType, int userId)
        {
            try
            {
                var comment = new NCRComment
                {
                    NCRId = ncrId,
                    CommentText = commentText,
                    CommentType = commentType,
                    CreatedBy = userId,
                    CreatedDate = DateTime.Now,
                    IsResolved = false
                };

                var commentId = await _commentRepository.CreateAsync(comment);
                return commentId > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding comment to NCR {NCRId}", ncrId);
                return false;
            }
        }
    }
}
