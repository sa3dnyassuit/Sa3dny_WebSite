using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sa3dny.Api.DTOs.Requests.Provider;
using Sa3dny.Data;
using Sa3dny.Data.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Sa3dny.Api.Controllers
{
    [Route("api/provider-requests")]
    [ApiController]
    public class ProviderRequestsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly NotificationService _notificationService;


        public ProviderRequestsController(AppDbContext context, NotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        [AllowAnonymous]
        [HttpGet("requests")]
        public async Task<IActionResult> GetRequests()
        {
            var requests = await _context.Requests
                .Where(r => r.Status == "Pending")
                .Select(r => new
                {
                    r.Request_Id,
                    r.Service_Id,
                    r.Description_Req,
                    r.Address,
                    r.Phone,
                    r.Status,
                    r.Time
                })
                .ToListAsync();

            return Ok(requests);
        }

        [AllowAnonymous]
        [HttpPost("offer")]
        public async Task<IActionResult> SendOffer([FromBody] SendOfferDto dto)
        {
            var provider = await _context.Providers.FirstOrDefaultAsync();

            if (provider == null)
                return BadRequest(new { message = "No providers found in database" });

            var request = await _context.Requests.FindAsync(dto.RequestId);

            if (request == null)
                return NotFound(new { message = "Request not found" });

            var offer = new ProviderOffer
            {
                offerId = Guid.NewGuid(),
                RequestId = dto.RequestId,
                ProviderId = provider.provider_id,
                Price = dto.Price
            };

            _context.ProviderOffers.Add(offer);
            request.Status = "HasOffers";

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Offer sent successfully",
                OfferId = offer.offerId
            });
            var customerUserId = await _context.Customers
             .Where(c => c.Id_Customer == request.Customer_Id)
            .Select(c => c.UserId)
            .FirstOrDefaultAsync();

            await _notificationService.Send(
                customerUserId,
                "عرض جديد 💰",
                $"في بروفايدر بعتلك سعر: {dto.Price}",
                request.Request_Id
            );
        }

        [AllowAnonymous]
        [HttpPost("complete/{requestId}")]
        public async Task<IActionResult> Complete(Guid requestId)
        {
            var request = await _context.Requests
                .FirstOrDefaultAsync(r => r.Request_Id == requestId);

            if (request == null)
                return NotFound(new { message = "Request not found" });

            request.Status = "Completed";
            await _context.SaveChangesAsync();

            return Ok(new { message = "Request completed successfully" });
        }
    }
}