using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sa3dny.Api.DTOs.Requests.Customer;
using Sa3dny.Data;
using Sa3dny.Data.Models;
using Sa3dny.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Sa3dny.Api.Controllers.Requestsall
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CustomerController(AppDbContext context)
        {
            _context = context;
        }

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
                Time = DateTime.UtcNow
            };

            _context.Requests.Add(request);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                Message = "Request created successfully",
                RequestId = request.Request_Id,
                Status = request.Status
            });
        }

        [HttpGet("{requestId}/offers")]
        public async Task<IActionResult> GetOffers(Guid requestId)
        {
            var offers = await _context.ProviderOffers
                .Where(o => o.RequestId == requestId)
                .Select(o => new
                {
                    o.Id,
                    o.Price,
                    o.ProviderId
                })
                .ToListAsync();

            return Ok(offers);
        }

        [AllowAnonymous]
        [HttpPost("customer-approve")]
        public async Task<IActionResult> Approve(ApproveOfferDto dto)
        {
            var offer = await _context.ProviderOffers
                .Include(o => o.Request)
                .FirstOrDefaultAsync(o => o.Id == dto.OfferId);

            if (offer == null)
                return NotFound(new { message = "Offer not found" });

            offer.Request.AcceptedOfferId = offer.Id;
            offer.Request.Status = "Approved";

            await _context.SaveChangesAsync();

            return Ok(new { message = "Offer approved successfully" });
        }

        [AllowAnonymous]
        [HttpPost("customer-pay")]
        public async Task<IActionResult> Pay(PayRequestDto dto)
        {
            var request = await _context.Requests
                .Include(r => r.AcceptedOffer)
                .FirstOrDefaultAsync(r => r.Request_Id == dto.RequestId);

            if (request == null || request.AcceptedOffer == null)
                return BadRequest("Invalid request or no accepted offer");

            request.Status = "Paid";

            await _context.SaveChangesAsync();

            return Ok(new { message = "Payment successful" });
        }

        [AllowAnonymous]
        [HttpGet("all-requests")]
        public async Task<IActionResult> GetAllRequests()
        {
            var allRequests = await (from r in _context.Requests
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
                                         // ✅ السعر من الـ AcceptedOffer
                                         Price = _context.ProviderOffers
                                                 .Where(o => o.Id == r.AcceptedOfferId)
                                                 .Select(o => (decimal?)o.Price)
                                                 .FirstOrDefault(),
                                         // ✅ أول عرض موجود لو status = HasOffers
                                         OfferId = _context.ProviderOffers
                                                   .Where(o => o.RequestId == r.Request_Id)
                                                   .Select(o => (Guid?)o.Id)
                                                   .FirstOrDefault()
                                     }).ToListAsync();

            return Ok(allRequests);
        }

        [AllowAnonymous]
        [HttpGet("customer/{customerId}/requests")]
        public async Task<IActionResult> GetRequestsByCustomer(Guid customerId)
        {
            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.Id_Customer == customerId);

            if (customer == null)
                return BadRequest(new { message = "Customer not found" });

            var requests = await (from r in _context.Requests
                                  join s in _context.Services on r.Service_Id equals s.service_id
                                  where r.Customer_Id == customerId
                                  select new
                                  {
                                      r.Request_Id,
                                      r.Description_Req,
                                      r.Status,
                                      r.Time,
                                      r.Address,
                                      r.Phone,
                                      ServiceName = s.service_name,
                                      Price = _context.ProviderOffers
                                              .Where(o => o.Id == r.AcceptedOfferId)
                                              .Select(o => (decimal?)o.Price)
                                              .FirstOrDefault(),
                                      OfferId = _context.ProviderOffers
                                                .Where(o => o.RequestId == r.Request_Id)
                                                .Select(o => (Guid?)o.Id)
                                                .FirstOrDefault()
                                  }).ToListAsync();

            return Ok(requests);
        }

        // ✅ Chat APIs
        [AllowAnonymous]
        [HttpPost("chat/send")]
        public async Task<IActionResult> SendMessage([FromBody] SendMessageDto dto)
        {
            var request = await _context.Requests
                .FirstOrDefaultAsync(r => r.Request_Id == dto.RequestId);

            if (request == null)
                return NotFound(new { message = "Request not found" });

            var message = new ChatMessage
            {
                Id = Guid.NewGuid(),
                RequestId = dto.RequestId,
                SenderId = dto.SenderId,
                SenderName = dto.SenderName,
                Message = dto.Message,
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
                message.SentAt
            });
        }

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
                    m.SentAt
                })
                .ToListAsync();

            return Ok(messages);
        }
    }
}