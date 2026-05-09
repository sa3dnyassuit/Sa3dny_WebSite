// FILE: Sa3dny.Api/Controllers/AdminController.cs
// NEW ENDPOINTS:
//   POST /api/admin/chat/open/{requestId}  → يفتح الشات
//   POST /api/admin/chat/close/{requestId} → يقفل الشات

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sa3dny.Data;
using Sa3dny.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Sa3dny.Api.Controllers
{
    [Route("api/admin")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly NotificationService _notificationService;

        public AdminController(AppDbContext context, NotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        // =========================
        // GET PENDING PAYMENTS
        // =========================
        [HttpGet("payments/pending")]
        public async Task<IActionResult> GetPendingPayments()
        {
            var payments = await _context.Requests
                .Where(r => r.Status == "PaymentSubmitted")
                .Select(r => new
                {
                    r.Request_Id,
                    r.Description_Req,
                    r.Status,
                    r.Time,
                    r.AcceptedOfferId
                })
                .ToListAsync();

            return Ok(payments);
        }

        // =========================
        // APPROVE PAYMENT → +10 نقاط + فتح الشات
        // =========================
        [HttpPost("payments/approve/{requestId}")]
        public async Task<IActionResult> ApprovePayment(Guid requestId)
        {
            var request = await _context.Requests
                .FirstOrDefaultAsync(r => r.Request_Id == requestId);

            if (request == null) return NotFound("Request not found");

            var offer = await _context.ProviderOffers
                .FirstOrDefaultAsync(o => o.offerId == request.AcceptedOfferId);

            if (offer == null) return BadRequest("Offer not found");

            request.Status = "ChatOpen";
            request.Updated_At = DateTime.UtcNow;


            await _context.Customers
                .Where(c => c.Id_Customer == request.Customer_Id)
                .ExecuteUpdateAsync(s => s.SetProperty(c => c.Points, c => c.Points + 10));

            request.Status = "ChatOpen";
            request.Updated_At = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // إشعار الـ Customer
            var customerUserId = await _context.Customers
                .Where(c => c.Id_Customer == request.Customer_Id)
                .Select(c => c.UserId)
                .FirstOrDefaultAsync();

            if (!string.IsNullOrEmpty(customerUserId))
            {
                await _notificationService.Send(
                    customerUserId,
                    "Payment Approved ✅",
                    "Your payment is approved. Chat is now open.",
                    request.Request_Id
                );
            }

            // إشعار الـ Provider
            var providerUserId = await _context.Providers
                .Where(p => p.provider_id == offer.ProviderId)
                .Select(p => p.UserId)
                .FirstOrDefaultAsync();

            if (!string.IsNullOrEmpty(providerUserId))
            {
                await _notificationService.Send(
                    providerUserId,
                    "Payment Approved ✅",
                    "Payment confirmed. You can now chat with customer.",
                    request.Request_Id
                );
            }

            return Ok(new { message = "Payment approved and chat opened" });
        }

        // =========================
        // REJECT PAYMENT
        // =========================
        [HttpPost("payments/reject/{requestId}")]
        public async Task<IActionResult> RejectPayment(Guid requestId)
        {
            var request = await _context.Requests
                .FirstOrDefaultAsync(r => r.Request_Id == requestId);

            if (request == null) return NotFound("Request not found");

            request.Status = "PaymentRejected";
            request.Updated_At = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            var customerUserId = await _context.Customers
                .Where(c => c.Id_Customer == request.Customer_Id)
                .Select(c => c.UserId)
                .FirstOrDefaultAsync();

            if (!string.IsNullOrEmpty(customerUserId))
            {
                await _notificationService.Send(
                    customerUserId,
                    "Payment Rejected ❌",
                    "Your payment was rejected. Please try again.",
                    request.Request_Id
                );
            }

            return Ok(new { message = "Payment rejected" });
        }

        // =========================
        // ✅ OPEN CHAT
        // POST /api/admin/chat/open/{requestId}
        // =========================
        [HttpPost("chat/open/{requestId}")]
        public async Task<IActionResult> OpenChat(Guid requestId)
        {
            var request = await _context.Requests
                .FirstOrDefaultAsync(r => r.Request_Id == requestId);

            if (request == null) return NotFound("Request not found");

            request.Status = "ChatOpen";
            request.Updated_At = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // إشعار الطرفين
            var customerUserId = await _context.Customers
                .Where(c => c.Id_Customer == request.Customer_Id)
                .Select(c => c.UserId)
                .FirstOrDefaultAsync();

            if (!string.IsNullOrEmpty(customerUserId))
            {
                await _notificationService.Send(
                    customerUserId,
                    "المحادثة مفتوحة 💬",
                    "تم فتح المحادثة، يمكنك التواصل مع مزود الخدمة.",
                    request.Request_Id
                );
            }

            return Ok(new { message = "Chat opened successfully", status = "ChatOpen" });
        }

        // =========================
        // ✅ CLOSE CHAT
        // POST /api/admin/chat/close/{requestId}
        // =========================
        [HttpPost("chat/close/{requestId}")]
        public async Task<IActionResult> CloseChat(Guid requestId)
        {
            var request = await _context.Requests
                .FirstOrDefaultAsync(r => r.Request_Id == requestId);

            if (request == null) return NotFound("Request not found");

            request.Status = "ChatClosed";
            request.Updated_At = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // إشعار الطرفين
            var customerUserId = await _context.Customers
                .Where(c => c.Id_Customer == request.Customer_Id)
                .Select(c => c.UserId)
                .FirstOrDefaultAsync();

            if (!string.IsNullOrEmpty(customerUserId))
            {
                await _notificationService.Send(
                    customerUserId,
                    "المحادثة مغلقة 🔒",
                    "تم إغلاق المحادثة من قِبل الإدارة.",
                    request.Request_Id
                );
            }

            return Ok(new { message = "Chat closed successfully", status = "ChatClosed" });
        }
    }
}