// Controllers/MaterialPriceRequestController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using NCRManagementSystem.Models.ViewModels;
using NCRManagementSystem.Services.Interfaces;
using System.Security.Claims;

namespace NCRManagementSystem.Controllers
{
    [Authorize]
    public class MaterialPriceRequestController : Controller
    {
        private readonly IMaterialPriceRequestService _service;
        private readonly ILogger<MaterialPriceRequestController> _logger;

        public MaterialPriceRequestController(IMaterialPriceRequestService service, ILogger<MaterialPriceRequestController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int page = 1, string? search = null, string? status = null,
            DateTime? fromDate = null, DateTime? toDate = null)
        {
            try
            {
                var userId = GetCurrentUserId();
                var userRole = GetCurrentUserRole();
                const int pageSize = 10;

                var result = await _service.GetPagedAsync(page, pageSize, search, status, fromDate, toDate, userId, userRole);
                var stats = await _service.GetStatsAsync(userId, userRole);

                var viewModel = new MaterialPriceRequestListViewModel
                {
                    Requests = result.Data,
                    TotalRecords = result.TotalRecords,
                    PageNumber = page,
                    PageSize = pageSize,
                    SearchTerm = search,
                    Status = status,
                    FromDate = fromDate,
                    ToDate = toDate,
                    Stats = stats
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading material price request list");
                TempData["ErrorMessage"] = "เกิดข้อผิดพลาดในการโหลดข้อมูล";
                return View(new MaterialPriceRequestListViewModel());
            }
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            try
            {
                var requestNumber = await _service.GenerateRequestNumberAsync();

                var viewModel = new MaterialPriceRequestCreateViewModel
                {
                    RequestNumber = requestNumber,
                    Items = new List<MaterialPriceRequestItemInputModel>
                    {
                        new MaterialPriceRequestItemInputModel() // Start with one empty item
                    }
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading create form");
                TempData["ErrorMessage"] = "เกิดข้อผิดพลาดในการโหลดฟอร์ม";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MaterialPriceRequestCreateViewModel model)
        {
            try
            {
                // Remove empty items
                model.Items = model.Items.Where(i => !string.IsNullOrWhiteSpace(i.MaterialCode)).ToList();

                if (!model.Items.Any())
                {
                    ModelState.AddModelError("", "กรุณาเพิ่มรายการวัตถุดิบอย่างน้อย 1 รายการ");
                    return View(model);
                }

                var userId = GetCurrentUserId();
                var department = User.FindFirst("Department")?.Value;

                var requestId = await _service.CreateRequestAsync(model, userId, department);

                TempData["SuccessMessage"] = "สร้างใบร้องขอราคาเรียบร้อยแล้ว";
                return RedirectToAction("Details", new { id = requestId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating material price request");
                ModelState.AddModelError("", "เกิดข้อผิดพลาดในการสร้างใบร้องขอ");
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var request = await _service.GetDetailsAsync(id);
                if (request == null)
                {
                    TempData["ErrorMessage"] = "ไม่พบใบร้องขอที่ระบุ";
                    return RedirectToAction("Index");
                }

                var userId = GetCurrentUserId();
                var userRole = GetCurrentUserRole();

                var viewModel = new MaterialPriceRequestDetailViewModel
                {
                    Request = request,
                    CanUpdatePrice = userRole == "Purchasing" || userRole == "Admin",
                    CanComplete = (userRole == "Purchasing" || userRole == "Admin") && request.Status != "Completed",
                    IsRequester = request.RequestBy == userId
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading request details for ID {RequestId}", id);
                TempData["ErrorMessage"] = "เกิดข้อผิดพลาดในการโหลดข้อมูล";
                return RedirectToAction("Index");
            }
        }

        [HttpGet]
        [Authorize(Roles = "Purchasing,Admin")]
        public async Task<IActionResult> UpdatePrice(int id)
        {
            try
            {
                var request = await _service.GetDetailsAsync(id);
                if (request == null)
                {
                    TempData["ErrorMessage"] = "ไม่พบใบร้องขอที่ระบุ";
                    return RedirectToAction("Index");
                }

                if (request.Status == "Completed" || request.Status == "Cancelled")
                {
                    TempData["ErrorMessage"] = "ไม่สามารถอัพเดตราคาได้ เนื่องจากใบร้องขอปิดงานแล้ว";
                    return RedirectToAction("Details", new { id });
                }

                var viewModel = new MaterialPriceUpdateViewModel
                {
                    Request = request,
                    Items = request.Items.Select(i => new PriceUpdateItemModel
                    {
                        ItemId = i.ItemId,
                        MaterialCode = i.MaterialCode,
                        MaterialName = i.MaterialName,
                        Remark = i.Remark,
                        Status = i.Status,
                        UnitPrice = i.UnitPrice,
                        Currency = i.Currency ?? "THB",
                        SupplierName = i.SupplierName,
                        LeadTimeDays = i.LeadTimeDays,
                        PriceRemark = i.PriceRemark,
                        Files = i.Files
                    }).ToList()
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading price update form for ID {RequestId}", id);
                TempData["ErrorMessage"] = "เกิดข้อผิดพลาดในการโหลดข้อมูล";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Purchasing,Admin")]
        public async Task<IActionResult> UpdatePrice(int id, MaterialPriceUpdateViewModel model)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _service.UpdatePricesAsync(id, model.Items, userId);

                if (result)
                {
                    TempData["SuccessMessage"] = "อัพเดตราคาเรียบร้อยแล้ว";
                }
                else
                {
                    TempData["ErrorMessage"] = "ไม่สามารถอัพเดตราคาได้";
                }

                return RedirectToAction("Details", new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating prices for request {RequestId}", id);
                TempData["ErrorMessage"] = "เกิดข้อผิดพลาดในการอัพเดตราคา";
                return RedirectToAction("UpdatePrice", new { id });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Purchasing,Admin")]
        public async Task<IActionResult> Complete(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _service.CompleteRequestAsync(id, userId);

                if (result)
                {
                    return Json(new { success = true, message = "ปิดงานเรียบร้อยแล้ว" });
                }
                else
                {
                    return Json(new { success = false, message = "ไม่สามารถปิดงานได้" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing request {RequestId}", id);
                return Json(new { success = false, message = "เกิดข้อผิดพลาดในการปิดงาน" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _service.CancelRequestAsync(id, userId);

                if (result)
                {
                    return Json(new { success = true, message = "ยกเลิกใบร้องขอเรียบร้อยแล้ว" });
                }
                else
                {
                    return Json(new { success = false, message = "ไม่สามารถยกเลิกได้" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling request {RequestId}", id);
                return Json(new { success = false, message = "เกิดข้อผิดพลาดในการยกเลิก" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadFile(int itemId, IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return Json(new { success = false, message = "กรุณาเลือกไฟล์" });
                }

                var userId = GetCurrentUserId();
                var fileInfo = await _service.SaveFileAsync(file, itemId, userId);

                if (fileInfo != null)
                {
                    return Json(new { success = true, message = "อัปโหลดไฟล์เรียบร้อยแล้ว", fileInfo });
                }
                else
                {
                    return Json(new { success = false, message = "ไม่สามารถอัปโหลดไฟล์ได้" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading file for item {ItemId}", itemId);
                return Json(new { success = false, message = "เกิดข้อผิดพลาดในการอัปโหลดไฟล์" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteFile(int fileId)
        {
            try
            {
                var result = await _service.DeleteFileAsync(fileId);

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
                var (content, fileName, contentType) = await _service.GetFileContentAsync(fileId);

                if (content == null)
                {
                    return NotFound("ไม่พบไฟล์ที่ระบุ");
                }

                return File(content, contentType ?? "application/octet-stream", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading file {FileId}", fileId);
                return BadRequest("เกิดข้อผิดพลาดในการดาวน์โหลดไฟล์");
            }
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