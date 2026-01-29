// Services/Implementations/MaterialPriceRequestService.cs
using NCRManagementSystem.Models.DTOs;
using NCRManagementSystem.Models.Entities;
using NCRManagementSystem.Models.ViewModels;
using NCRManagementSystem.Repositories.Interfaces;
using NCRManagementSystem.Services.Interfaces;

namespace NCRManagementSystem.Services.Implementations
{
    public class MaterialPriceRequestService : IMaterialPriceRequestService
    {
        private readonly IMaterialPriceRequestRepository _repository;
        private readonly ILineNotificationService _lineService;
        private readonly IEmailService _emailService;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<MaterialPriceRequestService> _logger;
        private readonly IConfiguration _configuration;

        public MaterialPriceRequestService(
            IMaterialPriceRequestRepository repository,
            ILineNotificationService lineService,
            IEmailService emailService,
            IWebHostEnvironment environment,
            ILogger<MaterialPriceRequestService> logger,
            IConfiguration configuration)
        {
            _repository = repository;
            _lineService = lineService;
            _emailService = emailService;
            _environment = environment;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task<MaterialPriceRequestDto?> GetDetailsAsync(int requestId)
        {
            return await _repository.GetDetailsAsync(requestId);
        }

        public async Task<PagedResult<MaterialPriceRequestDto>> GetPagedAsync(int pageNumber, int pageSize,
            string? searchTerm = null, string? status = null,
            DateTime? fromDate = null, DateTime? toDate = null,
            int? userId = null, string? userRole = null)
        {
            return await _repository.GetPagedAsync(pageNumber, pageSize, searchTerm, status, fromDate, toDate, userId, userRole);
        }

        public async Task<MaterialPriceRequestStatsDto> GetStatsAsync(int? userId = null, string? userRole = null)
        {
            return await _repository.GetStatsAsync(userId, userRole);
        }

        public async Task<string> GenerateRequestNumberAsync()
        {
            return await _repository.GenerateRequestNumberAsync();
        }

        public async Task<int> CreateRequestAsync(MaterialPriceRequestCreateViewModel model, int userId, string? department)
        {
            try
            {
                // Create request header
                var request = new MaterialPriceRequest
                {
                    RequestNumber = await GenerateRequestNumberAsync(),
                    RequestDate = DateTime.Now,
                    RequestBy = userId,
                    Department = department,
                    Status = "Pending",
                    Remarks = model.Remarks,
                    CreatedDate = DateTime.Now,
                    CreatedBy = userId
                };

                var requestId = await _repository.CreateAsync(request);

                // Create items
                foreach (var itemModel in model.Items)
                {
                    var item = new MaterialPriceRequestItem
                    {
                        RequestId = requestId,
                        Plant = itemModel.Plant,
                        MaterialCode = itemModel.MaterialCode,
                        MaterialName = itemModel.MaterialName,
                        Quantity = itemModel.Quantity,
                        Unit = itemModel.Unit,
                        Remark = itemModel.Remark,
                        Status = "Pending",
                        CreatedDate = DateTime.Now
                    };

                    var itemId = await _repository.CreateItemAsync(item);

                    // Save files
                    if (itemModel.Files != null)
                    {
                        foreach (var file in itemModel.Files)
                        {
                            await SaveFileAsync(file, itemId, userId);
                        }
                    }
                }

                // Send notifications
                await SendNotificationToPurchasingAsync(requestId);

                return requestId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating material price request");
                throw;
            }
        }

        public async Task<bool> UpdatePricesAsync(int requestId, List<PriceUpdateItemModel> items, int userId)
        {
            try
            {
                var request = await _repository.GetByIdAsync(requestId);
                if (request == null) return false;

                // Update status to InProgress if still Pending
                if (request.Status == "Pending")
                {
                    await _repository.UpdateStatusAsync(requestId, "InProgress", userId);
                }

                foreach (var item in items.Where(i => i.UnitPrice.HasValue && i.UnitPrice > 0))
                {
                    await _repository.UpdateItemPriceAsync(
                        item.ItemId,
                        item.UnitPrice!.Value,
                        item.Currency,
                        item.SupplierName,
                        item.LeadTimeDays,
                        item.PriceRemark,
                        userId);

                    await _repository.AddHistoryAsync(requestId, "PriceUpdated",
                        $"อัพเดตราคา {item.MaterialCode}: {item.UnitPrice:N2} {item.Currency}",
                        null, null, userId);
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating prices for request {RequestId}", requestId);
                throw;
            }
        }

        public async Task<bool> CompleteRequestAsync(int requestId, int userId)
        {
            try
            {
                var result = await _repository.UpdateStatusAsync(requestId, "Completed", userId);

                if (result)
                {
                    // Send notification to production
                    await SendNotificationToProductionAsync(requestId);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing request {RequestId}", requestId);
                throw;
            }
        }

        public async Task<bool> CancelRequestAsync(int requestId, int userId)
        {
            return await _repository.UpdateStatusAsync(requestId, "Cancelled", userId);
        }

        public async Task<MaterialPriceRequestFileDto?> SaveFileAsync(IFormFile file, int itemId, int uploadedBy)
        {
            try
            {
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "material-price-requests");
                Directory.CreateDirectory(uploadsFolder);

                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }

                var fileEntity = new MaterialPriceRequestFile
                {
                    ItemId = itemId,
                    FileName = fileName,
                    OriginalFileName = file.FileName,
                    FilePath = filePath,
                    FileSize = file.Length,
                    FileType = file.ContentType,
                    UploadedDate = DateTime.Now,
                    UploadedBy = uploadedBy
                };

                var fileId = await _repository.CreateFileAsync(fileEntity);

                return new MaterialPriceRequestFileDto
                {
                    FileId = fileId,
                    ItemId = itemId,
                    FileName = fileName,
                    OriginalFileName = file.FileName,
                    FilePath = filePath,
                    FileSize = file.Length,
                    FileType = file.ContentType,
                    UploadedDate = DateTime.Now
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving file for item {ItemId}", itemId);
                return null;
            }
        }

        public async Task<bool> DeleteFileAsync(int fileId)
        {
            try
            {
                var file = await _repository.GetFileByIdAsync(fileId);
                if (file == null) return false;

                // Delete physical file
                if (File.Exists(file.FilePath))
                {
                    File.Delete(file.FilePath);
                }

                return await _repository.DeleteFileAsync(fileId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting file {FileId}", fileId);
                return false;
            }
        }

        public async Task<(byte[]? content, string? fileName, string? contentType)> GetFileContentAsync(int fileId)
        {
            var file = await _repository.GetFileByIdAsync(fileId);
            if (file == null || !File.Exists(file.FilePath))
                return (null, null, null);

            var content = await File.ReadAllBytesAsync(file.FilePath);
            return (content, file.OriginalFileName, file.FileType);
        }

        public async Task SendNotificationToPurchasingAsync(int requestId)
        {
            try
            {
                var request = await _repository.GetDetailsAsync(requestId);
                if (request == null) return;

                // Build message
                var message = $@"📦 มีใบร้องขอราคาวัตถุดิบใหม่

เลขที่: {request.RequestNumber}
ผู้ร้องขอ: {request.RequestByName}
จำนวนรายการ: {request.TotalItems} รายการ

รายการวัตถุดิบ:
{string.Join("\n", request.Items.Select(i => $"• {i.MaterialCode} - {i.MaterialName}"))}

กรุณาเข้าระบบเพื่ออัพเดตราคา";

                // Send Line notification
                await _lineService.SendMessageAsync(message, "Purchasing");

                // Send Email
                var purchasingEmail = _configuration["NotificationSettings:PurchasingEmail"];
                if (!string.IsNullOrEmpty(purchasingEmail))
                {
                    await _emailService.SendNCRNotificationAsync(
                        purchasingEmail,
                        $"[MPR] ใบร้องขอราคาวัตถุดิบใหม่ - {request.RequestNumber}",
                        message);
                }

                // Log notification
                await _repository.CreateNotificationAsync(new MaterialPriceRequestNotification
                {
                    RequestId = requestId,
                    NotificationType = "Line",
                    RecipientType = "Purchasing",
                    IsSent = true,
                    SentDate = DateTime.Now,
                    CreatedDate = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending notification to purchasing for request {RequestId}", requestId);
            }
        }

        public async Task SendNotificationToProductionAsync(int requestId)
        {
            try
            {
                var request = await _repository.GetDetailsAsync(requestId);
                if (request == null) return;

                var pricedItems = request.Items.Where(i => i.Status == "Priced").ToList();

                var message = $@"✅ อัพเดตราคาวัตถุดิบเสร็จสิ้น

เลขที่: {request.RequestNumber}
สถานะ: เสร็จสิ้น

รายการที่ได้ราคา ({pricedItems.Count} รายการ):
{string.Join("\n", pricedItems.Select(i => $"• {i.MaterialCode}: {i.UnitPrice:N2} {i.Currency} ({i.SupplierName})"))}

กรุณาเข้าระบบเพื่อดูรายละเอียด";

                // Send Line notification
                await _lineService.SendMessageAsync(message, "Production");

                // Log notification
                await _repository.CreateNotificationAsync(new MaterialPriceRequestNotification
                {
                    RequestId = requestId,
                    NotificationType = "Line",
                    RecipientType = "Production",
                    IsSent = true,
                    SentDate = DateTime.Now,
                    CreatedDate = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending notification to production for request {RequestId}", requestId);
            }
        }

        public async Task ProcessPendingRemindersAsync()
        {
            try
            {
                var pendingItems = await _repository.GetPendingItemsForReminderAsync();

                if (!pendingItems.Any()) return;

                // Group by request
                var groupedItems = pendingItems.GroupBy(i => i.RequestId);

                foreach (var group in groupedItems)
                {
                    var firstItem = group.First();
                    var message = $@"⏰ แจ้งเตือน: รายการรอราคาเกิน 3 ชั่วโมง

                        เลขที่: {firstItem.RequestNumber}
                        ผู้ร้องขอ: {firstItem.RequestByName}

                        รายการที่รอ ({group.Count()} รายการ):
                        {string.Join("\n", group.Select(i => $"• {i.MaterialCode} - {i.MaterialName} (รอ {i.HoursPending} ชม.)"))}

                        กรุณาเข้าระบบเพื่ออัพเดตราคาโดยเร็ว";

                    await _lineService.SendMessageAsync(message, "Purchasing");

                    await _repository.CreateNotificationAsync(new MaterialPriceRequestNotification
                    {
                        RequestId = firstItem.RequestId,
                        NotificationType = "Line",
                        RecipientType = "Purchasing",
                        IsSent = true,
                        SentDate = DateTime.Now,
                        CreatedDate = DateTime.Now
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing pending reminders");
            }
        }
    }
}