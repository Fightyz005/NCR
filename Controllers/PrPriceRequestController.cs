using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NCRManagementSystem.Models.ViewModels;
using NCRManagementSystem.Services.Interfaces;
using System.Security.Claims;

namespace NCRManagementSystem.Controllers
{
    [Authorize]
    public class PrPriceRequestController : Controller
    {
        private readonly IPrPriceRequestService _service;
        private readonly ILogger<PrPriceRequestController> _logger;

        public PrPriceRequestController(IPrPriceRequestService service, ILogger<PrPriceRequestController> logger)
        {
            _service = service;
            _logger = logger;
        }

        public async Task<IActionResult> Index(int page = 1, string? search = null, string? status = null)
        {
            var pageSize = 10;
            var result = await _service.GetPagedAsync(page, pageSize, search, status);
            
            return View(new PrPriceRequestListViewModel
            {
                Requests = result.Data,
                TotalRecords = result.TotalRecords,
                PageNumber = page,
                PageSize = pageSize,
                SearchTerm = search,
                Status = status
            });
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new PrPriceRequestCreateViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PrPriceRequestCreateViewModel model)
        {
            if (!ModelState.IsValid) 
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                _logger.LogWarning("Invalid Model State: {Errors}", string.Join(", ", errors));
                return View(model);
            }
            
            // Remove empty items
            model.Items = model.Items.Where(i => !string.IsNullOrWhiteSpace(i.MaterialCode)).ToList();

            if (model.Items == null || !model.Items.Any())
            {
                ModelState.AddModelError("", "กรุณาเพิ่มรายการอย่างน้อย 1 รายการ");
                return View(model);
            }

            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var department = User.FindFirst("Department")?.Value;
                
                var requestId = await _service.CreateRequestAsync(model, userId, department);
                TempData["SuccessMessage"] = "สร้างใบขอราคาเรียบร้อยแล้ว";
                return RedirectToAction(nameof(Index)); // Or Details
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating PR Price Request");
                ModelState.AddModelError("", "เกิดข้อผิดพลาดในการบันทึกข้อมูล");
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> FetchPrItems(string banfn)
        {
            try
            {
                var items = await _service.GetPrItemsAsync(banfn);
                return Json(items);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fetch PR Error");
                return StatusCode(500, "Error fetching PR data");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var request = await _service.GetDetailsAsync(id);
            if (request == null) return NotFound();

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            var model = new PrPriceRequestDetailViewModel
            {
                Request = request,
                CanUpdatePrice = userRole == "Purchasing" || userRole == "Admin", // Simplified logic
                CanComplete = request.Status == "InProgress" && (userRole == "Purchasing" || userRole == "Admin"),
                IsRequester = false // Simplified
            };
            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Purchasing,Admin")]
        public async Task<IActionResult> UpdatePrice(int id, PrPriceUpdateViewModel model)
        {
             // Implementation pending - adding basic redirection for now to allow view creation
             return RedirectToAction(nameof(Details), new { id });
        }
    }
}
