using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sa3dny.Api.DTOs.Requests.Provider;
using Sa3dny.Data;
using Sa3dny.Data.Models;

namespace Sa3dny.Api.Controllers
{
    [Route("api/provider-requests")]
    [ApiController]
    public class ProviderRequestsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProviderRequestsController(AppDbContext context)
        {
            _context = context;
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
                Id = Guid.NewGuid(),
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
                OfferId = offer.Id
            });
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