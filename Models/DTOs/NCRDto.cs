namespace NCRManagementSystem.Models.DTOs
{
    public class NCRDto
    {
        public int NCRId { get; set; }
        public string NCRNumber { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public string? ItemCode { get; set; }
        public int SupplierId { get; set; }
        public string SupplierName { get; set; } = string.Empty;
        public string? LotNumber { get; set; }
        public string Grade { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public string ProblemDescription { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime? DueDate { get; set; }
        public int? DaysRemaining { get; set; }
        public string? QAComments { get; set; }
        public string? RootCause { get; set; }
        public string? CorrectiveAction { get; set; }
        public string? PreventiveAction { get; set; }
        public string? ResponsiblePerson { get; set; }
        public string? ManagerComments { get; set; }
        public List<NCRFileDto> Files { get; set; } = new();
        public List<NCRHistoryDto> History { get; set; } = new();
        public List<NCRCommentDto> Comments { get; set; } = new();
    }

    public class NCRFileDto
    {
        public int FileId { get; set; }
        public int NCRId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string OriginalFileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public string FileType { get; set; } = string.Empty;
        public DateTime UploadedDate { get; set; }
        public string UploadedByName { get; set; } = string.Empty;
        public string FileCategory { get; set; } = string.Empty;
        public string FileSizeFormatted => FormatFileSize(FileSize);

        private static string FormatFileSize(long bytes)
        {
            string[] suffixes = { "B", "KB", "MB", "GB" };
            int counter = 0;
            decimal number = bytes;
            while (Math.Round(number / 1024) >= 1)
            {
                number /= 1024;
                counter++;
            }
            return $"{number:n1} {suffixes[counter]}";
        }
    }

    public class NCRHistoryDto
    {
        public int HistoryId { get; set; }
        public int NCRId { get; set; }
        public string Action { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? OldStatus { get; set; }
        public string? NewStatus { get; set; }
        public DateTime ActionDate { get; set; }
        public string ActionByName { get; set; } = string.Empty;
        public string? Comments { get; set; }
    }

    public class NCRCommentDto
    {
        public int CommentId { get; set; }
        public int NCRId { get; set; }
        public string CommentText { get; set; } = string.Empty;
        public string CommentType { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public string CreatedByName { get; set; } = string.Empty;
        public bool IsResolved { get; set; }
        public int? ParentCommentId { get; set; }
        public List<NCRCommentDto> Replies { get; set; } = new();
    }

    public class PendingTaskDto
    {
        public int NCRId { get; set; }
        public string NCRNumber { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public string Grade { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string SupplierName { get; set; } = string.Empty;
        public DateTime? DueDate { get; set; }
        public int? DaysRemaining { get; set; }
        public bool IsOverdue => DaysRemaining < 0;
    }

    public class NCRListDto
    {
        public List<NCRDto> NCRs { get; set; } = new();
        public int TotalRecords { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalRecords / PageSize);
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;
    }

    public class SupplierPerformanceDto
    {
        public int SupplierId { get; set; }
        public string SupplierName { get; set; } = string.Empty;
        public int TotalNCRs { get; set; }
        public int GradeACount { get; set; }
        public int GradeBCount { get; set; }
        public int GradeCCount { get; set; }
        public decimal? AvgResponseDays { get; set; }
        public decimal ClosureRate { get; set; }
    }

    public class MonthlyTrendDto
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public string MonthName { get; set; } = string.Empty;
        public int NCRCount { get; set; }
        public int ClosedCount { get; set; }
        public int GradeACount { get; set; }
        public int GradeBCount { get; set; }
        public int GradeCCount { get; set; }
    }

    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
        public List<string> Errors { get; set; } = new();

        public static ApiResponse<T> SuccessResult(T data, string message = "Success")
        {
            return new ApiResponse<T>
            {
                Success = true,
                Message = message,
                Data = data
            };
        }

        public static ApiResponse<T> ErrorResult(string message, List<string>? errors = null)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = message,
                Errors = errors ?? new List<string>()
            };
        }
    }

    public class PagedResult<T>
    {
        public List<T> Data { get; set; } = new();
        public int TotalRecords { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalRecords / PageSize);
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;
    }
}
