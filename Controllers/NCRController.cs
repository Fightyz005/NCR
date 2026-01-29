using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using NCRManagementSystem.Models.ViewModels;
using NCRManagementSystem.Models.Entities;
using NCRManagementSystem.Services.Interfaces;
using System.Security.Claims;

namespace NCRManagementSystem.Controllers
{
    [Authorize]
    public class NCRController : Controller
    {
        private readonly INCRService _ncrService;
        private readonly ISupplierService _supplierService;
        private readonly IFileService _fileService;
        private readonly ILogger<NCRController> _logger;

        public NCRController(
            INCRService ncrService,
            ISupplierService supplierService,
            IFileService fileService,
            ILogger<NCRController> logger)
        {
            _ncrService = ncrService;
            _supplierService = supplierService;
            _fileService = fileService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int page = 1, string? search = null, string? status = null,
            string? grade = null, int? supplierId = null, DateTime? fromDate = null, DateTime? toDate = null)
        {
            try
            {
                const int pageSize = 10;
                var userId = GetCurrentUserId();
                var userRole = GetCurrentUserRole();

                var result = await _ncrService.GetPagedNCRsAsync(page, pageSize, search, status, grade,
                    supplierId, fromDate, toDate, userId, userRole);

                var suppliers = await _supplierService.GetAllActiveSuppliersAsync();

                var viewModel = new NCRListViewModel
                {
                    NCRs = result.Data,
                    TotalRecords = result.TotalRecords,
                    PageNumber = page,
                    PageSize = pageSize,
                    SearchTerm = search,
                    Status = status,
                    Grade = grade,
                    SupplierId = supplierId,
                    FromDate = fromDate,
                    ToDate = toDate,
                    Suppliers = suppliers.Select(s => new SelectListItem
                    {
                        Value = s.SupplierId.ToString(),
                        Text = s.SupplierName
                    }).ToList()
                };

                // Add empty option for supplier filter
                viewModel.Suppliers.Insert(0, new SelectListItem { Value = "", Text = "ทุก Supplier" });

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading NCR list");
                TempData["ErrorMessage"] = "เกิดข้อผิดพลาดในการโหลดรายการ NCR";
                return View(new NCRListViewModel());
            }
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            try
            {
                var suppliers = await _supplierService.GetAllActiveSuppliersAsync();
                var ncrNumber = await _ncrService.GenerateNCRNumberAsync();

                var viewModel = new NCRViewModel
                {
                    NCRNumber = ncrNumber,
                    CreatedDate = DateTime.Now,
                    Suppliers = suppliers.Select(s => new SelectListItem
                    {
                        Value = s.SupplierId.ToString(),
                        Text = s.SupplierName
                    }).ToList()
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading NCR create form");
                TempData["ErrorMessage"] = "เกิดข้อผิดพลาดในการโหลดฟอร์ม";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(NCRViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Log validation errors
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    _logger.LogError("Validation error: {Error}", error.ErrorMessage);
                }

                await LoadSuppliers(model);
                return View(model);
            }

            try
            {
                var userId = GetCurrentUserId();
                var ncr = new NCR
                {
                    NCRNumber = await _ncrService.GenerateNCRNumberAsync(),
                    ProductName = model.ProductName,
                    ItemCode = model.ItemCode,
                    SupplierId = model.SupplierId,
                    LotNumber = model.LotNumber,
                    Grade = model.Grade,
                    Priority = model.Priority,
                    ProblemDescription = model.ProblemDescription,
                    Status = "New",
                    CreatedBy = userId,
                    CreatedDate = DateTime.Now,
                    DueDate = CalculateDueDate(model.Grade, model.Priority)
                };

                var ncrId = await _ncrService.CreateNCRAsync(ncr);

                // Handle file uploads
                if (model.Files != null && model.Files.Any())
                {
                    foreach (var file in model.Files)
                    {
                        if (file.Length > 0)
                        {
                            await _fileService.SaveNCRFileAsync(file, ncrId, userId, "General");
                        }
                    }
                }

                TempData["SuccessMessage"] = $"สร้าง NCR {ncr.NCRNumber} เรียบร้อยแล้ว";
                return RedirectToAction("Details", new { id = ncrId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating NCR");
                ModelState.AddModelError("", "เกิดข้อผิดพลาดในการสร้าง NCR");
                await LoadSuppliers(model);
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var ncr = await _ncrService.GetNCRDetailsAsync(id);
                if (ncr == null)
                {
                    TempData["ErrorMessage"] = "ไม่พบ NCR ที่ระบุ";
                    return RedirectToAction("Index");
                }

                var viewModel = new NCRViewModel
                {
                    NCRId = ncr.NCRId,
                    NCRNumber = ncr.NCRNumber,
                    ProductName = ncr.ProductName,
                    ItemCode = ncr.ItemCode,
                    SupplierId = ncr.SupplierId,
                    SupplierName = ncr.SupplierName,
                    LotNumber = ncr.LotNumber,
                    Grade = ncr.Grade,
                    Priority = ncr.Priority,
                    ProblemDescription = ncr.ProblemDescription,
                    Status = ncr.Status,
                    CreatedDate = ncr.CreatedDate,
                    CreatedByName = ncr.CreatedBy,
                    DueDate = ncr.DueDate,
                    ExistingFiles = ncr.Files,
                    History = ncr.History,
                    Comments = ncr.Comments
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading NCR details for ID {NCRId}", id);
                TempData["ErrorMessage"] = "เกิดข้อผิดพลาดในการโหลดข้อมูล NCR";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddComment(int ncrId, string commentText, string commentType = "General")
        {
            try
            {
                if (string.IsNullOrWhiteSpace(commentText))
                {
                    return Json(new { success = false, message = "กรุณากรอกข้อความ" });
                }

                var userId = GetCurrentUserId();
                var result = await _ncrService.AddCommentAsync(ncrId, commentText, commentType, userId);

                if (result)
                {
                    return Json(new { success = true, message = "เพิ่มความคิดเห็นเรียบร้อยแล้ว" });
                }
                else
                {
                    return Json(new { success = false, message = "ไม่สามารถเพิ่มความคิดเห็นได้" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding comment to NCR {NCRId}", ncrId);
                return Json(new { success = false, message = "เกิดข้อผิดพลาดในการเพิ่มความคิดเห็น" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadFile(int ncrId, IFormFile file, string category = "General")
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return Json(new { success = false, message = "กรุณาเลือกไฟล์" });
                }

                var userId = GetCurrentUserId();
                var fileInfo = await _fileService.SaveNCRFileAsync(file, ncrId, userId, category);

                if (fileInfo != null)
                {
                    return Json(new
                    {
                        success = true,
                        message = "อัปโหลดไฟล์เรียบร้อยแล้ว",
                        fileInfo = new
                        {
                            fileInfo.FileId,
                            fileInfo.OriginalFileName,
                            fileInfo.FileSizeFormatted,
                            fileInfo.UploadedDate
                        }
                    });
                }
                else
                {
                    return Json(new { success = false, message = "ไม่สามารถอัปโหลดไฟล์ได้" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading file for NCR {NCRId}", ncrId);
                return Json(new { success = false, message = "เกิดข้อผิดพลาดในการอัปโหลดไฟล์" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteFile(int fileId)
        {
            try
            {
                var result = await _fileService.DeleteNCRFileAsync(fileId);

                if (result)
                {
                    return Json(new { success = true, message = "ลบไฟล์เรียบร้อยแล้ว" });
                }
                else
                {
                    return Json(new { success = false, message = "ไม่สามารถลบไฟล์ได้" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting file {FileId}", fileId);
                return Json(new { success = false, message = "เกิดข้อผิดพลาดในการลบไฟล์" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> DownloadFile(int fileId)
        {
            try
            {
                var fileInfo = await _fileService.GetNCRFileAsync(fileId);
                if (fileInfo == null)
                {
                    return NotFound("ไม่พบไฟล์ที่ระบุ");
                }

                var fileBytes = await _fileService.GetFileContentAsync(fileInfo.FilePath);
                if (fileBytes == null)
                {
                    return NotFound("ไม่พบไฟล์ในระบบ");
                }

                return File(fileBytes, fileInfo.FileType, fileInfo.OriginalFileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading file {FileId}", fileId);
                return BadRequest("เกิดข้อผิดพลาดในการดาวน์โหลดไฟล์");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int ncrId, string newStatus, string? comments = null)
        {
            try
            {
                // Log เพื่อ debug
                _logger.LogInformation("UpdateStatus called: NCRId={NCRId}, Status={Status}", ncrId, newStatus);

                if (ncrId <= 0 || string.IsNullOrEmpty(newStatus))
                {
                    return Json(new { success = false, message = "ข้อมูลไม่ถูกต้อง" });
                }

                var userId = GetCurrentUserId();
                var result = await _ncrService.UpdateNCRStatusAsync(ncrId, newStatus, userId, comments);

                if (result)
                {
                    return Json(new { success = true, message = "อัปเดตสถานะเรียบร้อยแล้ว" });
                }
                else
                {
                    return Json(new { success = false, message = "ไม่สามารถอัปเดตสถานะได้" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating NCR status for ID {NCRId}", ncrId);
                return Json(new { success = false, message = "เกิดข้อผิดพลาดในการอัปเดตสถานะ" });
            }
        }

        private async Task LoadSuppliers(NCRViewModel model)
        {
            var suppliers = await _supplierService.GetAllActiveSuppliersAsync();
            model.Suppliers = suppliers.Select(s => new SelectListItem
            {
                Value = s.SupplierId.ToString(),
                Text = s.SupplierName
            }).ToList();
        }

        private DateTime CalculateDueDate(string grade, string priority)
        {
            var baseDays = grade switch
            {
                "A" => 2,
                "B" => 5,
                "C" => 7,
                _ => 5
            };

            // Urgent items get half the time
            if (priority == "Urgent")
            {
                baseDays = Math.Max(1, baseDays / 2);
            }

            return DateTime.Now.AddDays(baseDays);
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : 0;
        }

        private string GetCurrentUserRole()
        {
            return User.FindFirst(ClaimTypes.Role)?.Value ?? "";
        }
    }
}