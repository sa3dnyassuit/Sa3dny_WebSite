// FILE: Sa3dny.Api/Controllers/CustomerController.cs
// CHAT CHANGES:
//   1. POST /api/Customer/chat/send    → يقبل FormData (نص + ملف اختياري)
//   2. GET  /api/Customer/chat/{id}    → يرجع FileUrl و FileName و MessageType
//   3. GET  /api/Customer/chat/file/{messageId} → تحميل الملف
//
// ⚠️ بعد التعديل على ChatMessage.cs شغّل:
//   Add-Migration AddFileSupport
//   Update-Database

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sa3dny.Api.DTOs.Payment;
using Sa3dny.Api.DTOs.Requests.Customer;
using Sa3dny.Data;
using Sa3dny.Data.Models;
using Sa3dny.Models;
using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Sa3dny.Api.Controllers.Requestsall
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly NotificationService _notificationService;

        public CustomerController(AppDbContext context, NotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        // =====================================================================
        // POST /api/Customer/request
        // =====================================================================
        [AllowAnonymous]
        [HttpPost("request")]
        public async Task<IActionResult> Create(CreateRequestDto dto)
        {
            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.Id_Customer == dto.Customer_Id);
            if (customer == null)
                return BadRequest(new { message = "Customer not found in database" });

            var service = await _context.Services
                .FirstOrDefaultAsync(s => s.service_id == dto.Service_Id);
            if (service == null)
                return BadRequest(new { message = "Service not found" });

            var request = new Requests
            {
                Customer_Id = customer.Id_Customer,
                Service_Id = dto.Service_Id,
                Description_Req = dto.Description_Req,
                Phone = dto.Phone,
                Address = dto.Address,
                Status = "Pending",
                Time = DateTime.UtcNow,
                Created_At = DateTime.UtcNow
            };

            _context.Requests.Add(request);
            await _context.SaveChangesAsync();

            var providers = await _context.Providers.ToListAsync();
            foreach (var p in providers)
            {
                await _notificationService.Send(
                    p.UserId,
                    "طلب جديد",
                    "في عميل محتاج خدمة، ادخل قدم عرض",
                    request.Request_Id
                );
            }

            return Ok(new
            {
                Message = "Request created successfully",
                RequestId = request.Request_Id,
                Status = request.Status
            });
        }

        // =====================================================================
        // GET /api/Customer/{requestId}/offers
        // =====================================================================
        [AllowAnonymous]
        [HttpGet("{requestId}/offers")]
        public async Task<IActionResult> GetOffers(Guid requestId)
        {
            var offers = await _context.ProviderOffers
                .Where(o => o.RequestId == requestId)
                .Select(o => new { o.offerId, o.Price, o.ProviderId })
                .ToListAsync();

            return Ok(offers);
        }

        // =====================================================================
        // POST /api/Customer/customer-approve
        // =====================================================================
        [AllowAnonymous]
        [HttpPost("customer-approve")]
        public async Task<IActionResult> Approve(ApproveOfferDto dto)
        {
            var offer = await _context.ProviderOffers
                .Include(o => o.Request)
                .FirstOrDefaultAsync(o => o.offerId == dto.OfferId);

            if (offer == null)
                return NotFound(new { message = "Offer not found" });

            offer.Request.AcceptedOfferId = offer.offerId;
            offer.Request.Status = "Approved From Customer";
            offer.Request.Updated_At = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var providerUserId = await _context.Providers
                .Where(p => p.provider_id == offer.ProviderId)
                .Select(p => p.UserId)
                .FirstOrDefaultAsync();

            if (!string.IsNullOrEmpty(providerUserId))
            {
                await _notificationService.Send(
                    providerUserId,
                    "تم قبول عرضك",
                    "في انتظار العميل للدفع",
                    offer.Request.Request_Id
                );
            }

            return Ok(new { message = "Offer approved successfully" });
        }

        // =====================================================================
        // POST /api/Customer/customer-pay
        // =====================================================================
        [AllowAnonymous]
        [HttpPost("customer-pay")]
        public async Task<IActionResult> Pay(PayRequestDto dto)
        {
            var request = await _context.Requests
                .Include(r => r.AcceptedOffer)
                .FirstOrDefaultAsync(r => r.Request_Id == dto.RequestId);

            if (request == null || request.AcceptedOffer == null)
                return BadRequest(new { message = "Invalid request or no accepted offer" });

            request.Status = "PaymentSubmitted";
            request.Updated_At = DateTime.UtcNow;

            var payment = new Payment
            {
                RequestId = request.Request_Id,
                Status = "Pending",
                Method = "Not Specified",
                Amount = request.AcceptedOffer.Price,
                UserId = Guid.Empty
            };

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            var adminUserIds = await _context.Admin
                .Select(a => a.UserId)
                .ToListAsync();

            foreach (var adminId in adminUserIds)
            {
                await _notificationService.Send(
                    adminId,
                    "New Payment Needs Review",
                    $"Request {request.Request_Id} waiting for approval",
                    request.Request_Id
                );
            }

            return Ok(new { message = "Payment submitted and sent for admin review" });
        }

        // =====================================================================
        // GET /api/Customer/all-requests
        // =====================================================================
        [AllowAnonymous]
        [HttpGet("all-requests")]
        public async Task<IActionResult> GetAllRequests()
        {
            var allRequests = await (
                from r in _context.Requests
                join c in _context.Customers on r.Customer_Id equals c.Id_Customer
                join s in _context.Services on r.Service_Id equals s.service_id
                select new
                {
                    r.Request_Id,
                    r.Description_Req,
                    r.Status,
                    r.Time,
                    r.Address,
                    r.Phone,
                    CustomerName = _context.Users
                        .Where(u => u.Id == c.UserId)
                        .Select(u => u.Name)
                        .FirstOrDefault(),
                    ServiceName = s.service_name,
                    Price = _context.ProviderOffers
                        .Where(o => o.offerId == r.AcceptedOfferId)
                        .Select(o => (decimal?)o.Price)
                        .FirstOrDefault() ??
                        _context.ProviderOffers
                        .Where(o => o.RequestId == r.Request_Id)
                        .Select(o => (decimal?)o.Price)
                        .FirstOrDefault(),
                    OfferId = _context.ProviderOffers
                        .Where(o => o.RequestId == r.Request_Id)
                        .Select(o => (Guid?)o.offerId)
                        .FirstOrDefault()
                }
            ).ToListAsync();

            return Ok(allRequests);
        }

        // =====================================================================
        // GET /api/Customer/customer/{customerId}/requests
        // =====================================================================
        [AllowAnonymous]
        [HttpGet("customer/{customerId}/requests")]
        public async Task<IActionResult> GetRequestsByCustomer(Guid customerId)
        {
            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.Id_Customer == customerId);
            if (customer == null)
                return BadRequest(new { message = "Customer not found" });

            var requests = await (
                from r in _context.Requests
                join s in _context.Services on r.Service_Id equals s.service_id
                where r.Customer_Id == customerId
                orderby r.Created_At descending
                select new
                {
                    r.Request_Id,
                    r.Description_Req,
                    r.Status,
                    r.Time,
                    r.Address,
                    r.Phone,
                    ServiceName = s.service_name,
                    Price = (
                        _context.ProviderOffers
                            .Where(o => o.offerId == r.AcceptedOfferId)
                            .Select(o => (decimal?)o.Price)
                            .FirstOrDefault()
                        ??
                        _context.ProviderOffers
                            .Where(o => o.RequestId == r.Request_Id)
                            .OrderBy(o => o.CreatedAt)
                            .Select(o => (decimal?)o.Price)
                            .FirstOrDefault()
                    ),
                    OfferId = _context.ProviderOffers
                        .Where(o => o.RequestId == r.Request_Id)
                        .OrderBy(o => o.CreatedAt)
                        .Select(o => (Guid?)o.offerId)
                        .FirstOrDefault()
                }
            ).ToListAsync();

            return Ok(requests);
        }

        // =====================================================================
        // GET /api/Customer/points/{customerId}
        // =====================================================================
        [AllowAnonymous]
        [HttpGet("points/{customerId}")]
        public async Task<IActionResult> GetPoints(Guid customerId)
        {
            var customer = await _context.Customers
                .Where(c => c.Id_Customer == customerId)
                .Select(c => new { c.Id_Customer, c.Name, c.Points })
                .FirstOrDefaultAsync();

            if (customer == null)
                return NotFound(new { message = "Customer not found" });

            return Ok(new
            {
                customerId = customer.Id_Customer,
                name = customer.Name,
                points = customer.Points
            });
        }

        // =====================================================================
        // ✅ POST /api/Customer/chat/send
        // يقبل FormData: نص + ملف اختياري (صورة أو PDF أو أي ملف)
        // =====================================================================
        [AllowAnonymous]
        [HttpPost("chat/send")]
        public async Task<IActionResult> SendMessage([FromForm] SendMessageWithFileDto dto)
        {
            var request = await _context.Requests
                .FirstOrDefaultAsync(r => r.Request_Id == dto.RequestId);
            if (request == null)
                return NotFound(new { message = "Request not found" });

            if (request.Status != "ChatOpen")
                return BadRequest(new { message = "Chat is not open for this request" });

            string? fileUrl = null;
            string? fileName = null;
            string? fileMimeType = null;
            string messageType = "text";

            // ✅ لو في ملف مرفق → احفظه
            if (dto.File != null && dto.File.Length > 0)
            {
                messageType = "file";
                fileName = dto.File.FileName;
                fileMimeType = dto.File.ContentType;

                var ext = Path.GetExtension(dto.File.FileName);
                var savedName = $"{Guid.NewGuid()}{ext}";
                var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "chat_files");
                Directory.CreateDirectory(folderPath);
                var filePath = Path.Combine(folderPath, savedName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                    await dto.File.CopyToAsync(stream);

                fileUrl = $"/chat_files/{savedName}";
            }

            var message = new ChatMessage
            {
                Id = Guid.NewGuid(),
                RequestId = dto.RequestId,
                SenderId = dto.SenderId,
                SenderName = dto.SenderName,
                Message = dto.Message,
                MessageType = messageType,
                FileName = fileName,
                FileUrl = fileUrl,
                FileMimeType = fileMimeType,
                SentAt = DateTime.UtcNow
            };

            _context.ChatMessages.Add(message);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message.Id,
                message.RequestId,
                message.SenderId,
                message.SenderName,
                message.Message,
                message.MessageType,
                message.FileName,
                message.FileUrl,
                message.FileMimeType,
                message.SentAt
            });
        }

        // =====================================================================
        // ✅ GET /api/Customer/chat/{requestId}
        // =====================================================================
        [AllowAnonymous]
        [HttpGet("chat/{requestId}")]
        public async Task<IActionResult> GetMessages(Guid requestId)
        {
            var messages = await _context.ChatMessages
                .Where(m => m.RequestId == requestId)
                .OrderBy(m => m.SentAt)
                .Select(m => new
                {
                    m.Id,
                    m.SenderId,
                    m.SenderName,
                    m.Message,
                    m.MessageType,
                    m.FileName,
                    m.FileUrl,
                    m.FileMimeType,
                    m.SentAt
                })
                .ToListAsync();

            return Ok(messages);
        }

        // =====================================================================
        // ✅ GET /api/Customer/chat/file/{messageId}
        // تحميل ملف مرفق برسالة معينة
        // =====================================================================
        [AllowAnonymous]
        [HttpGet("chat/file/{messageId}")]
        public async Task<IActionResult> DownloadFile(Guid messageId)
        {
            var msg = await _context.ChatMessages
                .FirstOrDefaultAsync(m => m.Id == messageId);

            if (msg == null || string.IsNullOrEmpty(msg.FileUrl))
                return NotFound(new { message = "File not found" });

            var filePath = Path.Combine(
                Directory.GetCurrentDirectory(), "wwwroot",
                msg.FileUrl.TrimStart('/')
            );

            if (!System.IO.File.Exists(filePath))
                return NotFound(new { message = "File not found on disk" });

            var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
            var mimeType = msg.FileMimeType ?? "application/octet-stream";

            return File(fileBytes, mimeType, msg.FileName ?? "file");
        }

        // =====================================================================
        // POST /api/Customer/submit
        // =====================================================================
        [Authorize(AuthenticationSchemes =
            Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("submit")]
        public async Task<IActionResult> Submit([FromForm] PaymentDto dto)
        {
            var userIdClaim = User.FindFirst("sub")?.Value
                              ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out Guid userId))
                return Unauthorized(new { message = "Token valid but User ID claim is missing." });

            var request = await _context.Requests
                .FirstOrDefaultAsync(r => r.Request_Id == dto.RequestId);
            if (request == null)
                return BadRequest(new { message = "Invalid request" });

            byte[]? fileData = null;
            if (dto.Image != null)
            {
                using var ms = new MemoryStream();
                await dto.Image.CopyToAsync(ms);
                fileData = ms.ToArray();
            }

            var payment = new Payment
            {
                Id = Guid.NewGuid(),
                RequestId = dto.RequestId,
                UserId = userId,
                Amount = dto.Amount,
                Method = dto.Method,
                SenderNumber = dto.SenderNumber,
                ScreenshotData = fileData,
                Status = "Pending"
            };

            _context.Payments.Add(payment);
            request.Status = "PaymentSubmitted";
            request.Updated_At = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            var adminUserIds = await _context.Admin
                .Select(a => a.UserId).ToListAsync();

            foreach (var adminId in adminUserIds)
            {
                await _notificationService.Send(
                    adminId,
                    "Payment Proof Uploaded",
                    $"Customer uploaded payment proof for Request {request.Request_Id}",
                    request.Request_Id
                );
            }

            return Ok(new { message = "Payment submitted successfully", status = payment.Status });
        }

        // =====================================================================
        // GET /api/Customer/offers-from-providers
        // =====================================================================
        [AllowAnonymous]
        [HttpGet("offers-from-providers")]
        public async Task<IActionResult> GetOffersFromProvider()
        {
            var offers = await _context.ProviderOffers.ToListAsync();
            return Ok(offers);
        }
    }
}