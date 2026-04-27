using System;
using System.ComponentModel.DataAnnotations;

namespace Sa3dny.Api.DTOs.Requests.Provider
{
    public class SendOfferDto
    {
        [Required]
        public Guid RequestId { get; set; }

        [Required]
        public decimal Price { get; set; }
    }
}