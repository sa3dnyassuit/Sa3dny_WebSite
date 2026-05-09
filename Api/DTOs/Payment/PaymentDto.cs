using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sa3dny.Api.DTOs.Payment
{
    public class PaymentDto
    {
        public Guid RequestId { get; set; }

        public string Method { get; set; }

        public string SenderNumber { get; set; }

        public decimal Amount { get; set; }

        public IFormFile Image { get; set; }
    }
}
