using NCRManagementSystem.Models.DTOs;
using NCRManagementSystem.Models.Entities;
using NCRManagementSystem.Models.ViewModels;
using NCRManagementSystem.Repositories.Interfaces;
using NCRManagementSystem.Services.Interfaces;

namespace NCRManagementSystem.Services.Implementations
{
    public class PrPriceRequestService : IPrPriceRequestService
    {
        private readonly IPrPriceRequestRepository _repository;
        private readonly IExternalPrRepository _externalPrRepository;
        private readonly ILineNotificationService _lineService;
        private readonly IEmailService _emailService;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<PrPriceRequestService> _logger;
        private readonly IConfiguration _configuration;

        public PrPriceRequestService(
            IPrPriceRequestRepository repository,
            IExternalPrRepository externalPrRepository,
            ILineNotificationService lineService,
            IEmailService emailService,
            IWebHostEnvironment environment,
            ILogger<PrPriceRequestService> logger,
            IConfiguration configuration)
        {
            _repository = repository;
            _externalPrRepository = externalPrRepository;
            _lineService = lineService;
            _emailService = emailService;
            _environment = environment;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task<List<ExternalPrItemDto>> GetPrItemsAsync(string banfn)
        {
            return await _externalPrRepository.GetPrItemsAsync(banfn);
        }

        public async Task<int> CreateRequestAsync(PrPriceRequestCreateViewModel model, int userId, string? department)
        {
            try
            {
                var request = new PrPriceRequest
                {
                    PrNumber = model.PrNumber,
                    RequestDate = DateTime.Now,
                    RequestBy = userId,
                    Department = department,
                    VendorEmail = model.VendorEmail,
                    Status = "Pending",
                    Remarks = model.Remarks,
                    CreatedDate = DateTime.Now,
                    CreatedBy = userId
                };

                var requestId = await _repository.CreateAsync(request);

                foreach (var itemModel in model.Items)
                {
                    var item = new PrPriceRequestItem
                    {
                        RequestId = requestId,
                        Plant = itemModel.Plant ?? string.Empty,
                        MaterialCode = itemModel.MaterialCode ?? string.Empty,
                        MaterialName = itemModel.MaterialName ?? string.Empty,
                        Quantity = itemModel.Quantity,
                        Unit = itemModel.Unit ?? string.Empty,
                        Remark = itemModel.Remark,
                        Status = "Pending",
                        CreatedDate = DateTime.Now
                    };

                    var itemId = await _repository.CreateItemAsync(item);

                    if (itemModel.Files != null)
                    {
                        foreach (var file in itemModel.Files)
                        {
                            await SaveFileAsync(file, itemId, userId);
                        }
                    }
                }

                await SendNotificationToPurchasingAsync(requestId);

                return requestId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating PR price request");
                throw;
            }
        }

        public async Task<PagedResult<PrPriceRequestDto>> GetPagedAsync(int pageNumber, int pageSize, string? searchTerm = null, string? status = null)
        {
            return await _repository.GetPagedAsync(pageNumber, pageSize, searchTerm, status);
        }

        public async Task<PrPriceRequestDto?> GetDetailsAsync(int requestId)
        {
            return await _repository.GetDetailsAsync(requestId);
        }

        public async Task SendNotificationToPurchasingAsync(int requestId)
        {
            try
            {
                var request = await _repository.GetDetailsAsync(requestId);
                if (request == null) return;

                var message = $@"📦 มีใบขอราคา PR ใหม่
เลขที่ PR: {request.PrNumber}
ผู้ร้องขอ: {request.RequestByName}
Vendor Email: {request.VendorEmail}
จำนวนรายการ: {request.TotalItems} รายการ

รายการ:
{string.Join("\n", request.Items.Select(i => $"• {i.MaterialCode} - {i.MaterialName}"))}

กรุณาดำเนินการ";

                await _lineService.SendMessageAsync(message, "Purchasing");

                 // Send Email
                var purchasingEmail = _configuration["NotificationSettings:PurchasingEmail"];
                if (!string.IsNullOrEmpty(purchasingEmail))
                {
                    await _emailService.SendNCRNotificationAsync(
                        purchasingEmail,
                        $"[PR-Price] ใบขอราคาใหม่ - {request.PrNumber}",
                        message);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending notification");
            }
        }

        private async Task SaveFileAsync(IFormFile file, int itemId, int userId)
        {
            try
            {
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "pr-price-requests");
                Directory.CreateDirectory(uploadsFolder);
                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                var filePath = Path.Combine(uploadsFolder, fileName);
                
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                await _repository.CreateFileAsync(new PrPriceRequestFile
                {
                    ItemId = itemId,
                    FileName = fileName,
                    OriginalFileName = file.FileName,
                    FilePath = filePath,
                    FileSize = file.Length,
                    FileType = file.ContentType,
                    UploadedBy = userId,
                    UploadedDate = DateTime.Now
                });
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error saving file");
            }
        }
    }
}
