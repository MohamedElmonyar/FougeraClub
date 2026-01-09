using Application.DTOs.PurchaseOrders;
using Application.Services.PurchaseOrder;
using Application.Services.Suppliers;
using Domain.Common;
using Microsoft.AspNetCore.Mvc;
using Application.Services.OTP;
using Domain.Enums;

namespace FougeraClub.Controllers
{
    public class PurchaseOrderController : Controller
    {
        private readonly IPurchaseOrderService _purchaseOrderService;
        private readonly ISupplierService _supplierService;
        private readonly IOtpService _otpService;
        private readonly ILogger<PurchaseOrderController> _logger;

        public PurchaseOrderController(
            IPurchaseOrderService purchaseOrderService,
            ISupplierService supplierService,
            IOtpService otpService,
            ILogger<PurchaseOrderController> logger)
        {
            _purchaseOrderService = purchaseOrderService;
            _supplierService = supplierService;
            _otpService = otpService;
            _logger = logger;
        }

        // GET: /PurchaseOrder/Index
        [HttpGet]
        public async Task<IActionResult> Index(
            [FromQuery] GetOrdersFilterDto filter,
            CancellationToken cancellationToken)
        {
            try
            {
                ViewBag.Suppliers = _supplierService.GetSuppliersDropdown();

                var result = await _purchaseOrderService.GetFilteredOrdersAsync(
                    filter.DateFrom,
                    filter.DateTo,
                    filter.SupplierId,
                    filter.PageNumber,
                    filter.PageSize,
                    cancellationToken);

                ViewBag.Filter = filter;
                return View(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading purchase orders");
                TempData["ToastMessage"] = "Error loading purchase orders";
                TempData["ToastType"] = "error";
                return View(new PagedList<PurchaseOrderDto>(
                    new List<PurchaseOrderDto>(), 0, 1, 50));
            }
        }

        // GET: /PurchaseOrder/AddEdit/{id?}
        [HttpGet]
        public async Task<IActionResult> AddEdit(int? id, CancellationToken cancellationToken)
        {
            try
            {
                ViewBag.Suppliers = _supplierService.GetSuppliersDropdown();

                if (id.HasValue)
                {
                    var order = await _purchaseOrderService.GetByIdWithItemsAsync(id.Value, cancellationToken);
                    return View(order);
                }

                // New order - generate code
                var newOrder = new PurchaseOrderDto
                {
                    PurchaseOrderCode = GeneratePurchaseOrderCode(),
                    Date = DateTime.Now,
                    Items = new List<PurchaseOrderItemDto>()
                };

                return View(newOrder);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading purchase order form");
                TempData["ToastMessage"] = "Error loading form";
                TempData["ToastType"] = "error";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: /PurchaseOrder/AddEdit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddEdit(PurchaseOrderDto model, CancellationToken cancellationToken)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    ViewBag.Suppliers = _supplierService.GetSuppliersDropdown();
                    return View(model);
                }

                if (model.Id == 0)
                {
                    // Create new
                    var id = await _purchaseOrderService.CreateAsync(model, cancellationToken);
                    TempData["ToastMessage"] = "Purchase order created successfully";
                    TempData["ToastType"] = "success";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    // Update existing
                    await _purchaseOrderService.UpdateAsync(model, cancellationToken);
                    TempData["ToastMessage"] = "Purchase order updated successfully";
                    TempData["ToastType"] = "success";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving purchase order");
                ModelState.AddModelError("", "Error saving purchase order: " + ex.Message);
                ViewBag.Suppliers = _supplierService.GetSuppliersDropdown();
                return View(model);
            }
        }

        // POST: /PurchaseOrder/Delete/{id}
        [HttpPost]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            try
            {
                await _purchaseOrderService.DeleteAsync(id, cancellationToken);
                return Json(new { success = true, message = "Purchase order deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting purchase order {OrderId}", id);
                return Json(new { success = false, message = "Error deleting purchase order" });
            }
        }

        // POST: /PurchaseOrder/GenerateOTP
        [HttpPost]
        public async Task<IActionResult> GenerateOTP(int orderId)
        {
            try
            {
                var order = await _purchaseOrderService.GetByIdAsync(orderId, CancellationToken.None);
                
                if (order.Status == PurchaseOrderStatus.Signed)
                {
                    return Json(new { success = false, message = "Purchase order is already signed" });
                }

                var otp = _otpService.GenerateOTP(orderId);
                
                // In real scenario, OTP would be sent via SignalR
                // For this demo, we'll return it in the response
                return Json(new { success = true, otp = otp, message = "OTP generated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating OTP for order {OrderId}", orderId);
                return Json(new { success = false, message = "Error generating OTP" });
            }
        }

        // POST: /PurchaseOrder/VerifyOTPAndSign
        [HttpPost]
        public async Task<IActionResult> VerifyOTPAndSign(int orderId, string otp, CancellationToken cancellationToken)
        {
            try
            {
                var isValid = _otpService.ValidateOTP(orderId, otp);
                
                if (!isValid)
                {
                    return Json(new { success = false, message = "Invalid or expired OTP" });
                }

                // Get order and update signature
                var order = await _purchaseOrderService.GetByIdAsync(orderId, cancellationToken);
                
                // Hardcoded current user (as per requirements)
                order.SignedByUserId = "admin-001";
                order.SignedAt = DateTime.Now;
                order.Status = PurchaseOrderStatus.Signed;

                await _purchaseOrderService.UpdateAsync(order, cancellationToken);

                return Json(new { 
                    success = true, 
                    message = "Purchase order signed successfully",
                    printUrl = Url.Action("Print", new { id = orderId })
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying OTP for order {OrderId}", orderId);
                return Json(new { success = false, message = "Error verifying OTP" });
            }
        }

        // GET: /PurchaseOrder/Print/{id}
        [HttpGet]
        public async Task<IActionResult> Print(int id, CancellationToken cancellationToken)
        {
            try
            {
                var order = await _purchaseOrderService.GetByIdWithItemsAsync(id, cancellationToken);
                return View(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading purchase order for print");
                TempData["ToastMessage"] = "Error loading purchase order";
                TempData["ToastType"] = "error";
                return RedirectToAction(nameof(Index));
            }
        }

        // Helper method to generate purchase order code
        private string GeneratePurchaseOrderCode()
        {
            return $"PO-{DateTime.Now:yyyyMMdd}-{new Random().Next(1000, 9999)}";
        }
    }
}