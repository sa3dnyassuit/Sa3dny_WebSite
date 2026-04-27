using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sa3dny.Api.DTOs.Requests.Provider
{
    public class ProviderOfferDto
    {
        public Guid Id { get; set; }
        public decimal Price { get; set; }
        public string ProviderName { get; set; }
    }
}
