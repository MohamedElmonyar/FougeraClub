using Application.DTOs.PurchaseOrders;
using Application.Services.Identity; // For IOtpService
using Application.Services.PurchaseOrder;
using Application.Services.Suppliers;
using Domain.Common;
using Microsoft.AspNetCore.Mvc;

namespace FougeraClub.Controllers
{
    public class PurchaseOrderController : Controller
    {
        private readonly IPurchaseOrderService _service;
        private readonly ISupplierService _supplierService;
        private readonly IOtpService _otpService;

        public PurchaseOrderController(
            IPurchaseOrderService service,
            ISupplierService supplierService,
            IOtpService otpService)
        {
            _service = service;
            _supplierService = supplierService;
            _otpService = otpService;
        }

        // GET: Index
        public async Task<IActionResult> Index(int pageNumber = 1)
        {
            var pagination = new PaginationParameters { PageNumber = pageNumber, PageSize = 10 };
            var result = await _service.GetOrdersAsync(pagination, CancellationToken.None);
            return View(result);
        }

        // GET: Create
        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.Suppliers = _supplierService.GetSuppliersDropdown();
            // Initialize with default date
            return View(new SavePurchaseOrderDto { Date = DateTime.Now, Items = new List<PurchaseOrderItemDto>() });
        }

        // POST: Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SavePurchaseOrderDto model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Suppliers = _supplierService.GetSuppliersDropdown();
                return View(model);
            }

            try
            {
                await _service.CreateAsync(model, CancellationToken.None);
                TempData["Success"] = "Purchase Order created successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                ViewBag.Suppliers = _supplierService.GetSuppliersDropdown();
                return View(model);
            }
        }

        // GET: Edit
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var dto = await _service.GetForEditAsync(id, CancellationToken.None);
            if (dto == null) return NotFound();

            ViewBag.Suppliers = _supplierService.GetSuppliersDropdown();
            return View(dto);
        }

        // POST: Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(SavePurchaseOrderDto model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Suppliers = _supplierService.GetSuppliersDropdown();
                return View(model);
            }

            try
            {
                await _service.UpdateAsync(model, CancellationToken.None);
                TempData["Success"] = "Purchase Order updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                ViewBag.Suppliers = _supplierService.GetSuppliersDropdown();
                return View(model);
            }
        }

        // POST: Delete
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _service.DeleteAsync(id, CancellationToken.None);
                return Ok(new { success = true, message = "Deleted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        // ==========================
        // OTP & Signature Actions
        // ==========================

        [HttpPost]
        public async Task<IActionResult> RequestOtp(int id)
        {
            try
            {
                await _otpService.GenerateAndSendOtpAsync(id);
                return Ok(new { success = true, message = "OTP sent successfully via notification." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> VerifyAndSign(int id, string code)
        {
            bool isValid = _otpService.VerifyOtp(id, code);

            if (isValid)
            {
                // Optional: Update Order Status in DB to 'Signed'
                // await _service.SignOrderAsync(id); 
                return Ok(new { success = true, message = "OTP Verified. Order Signed." });
            }
            else
            {
                return BadRequest(new { success = false, message = "Invalid or expired OTP." });
            }
        }
    }
}