using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sa3dny.Api.DTOs.Requests.Provider
{
    public class ProviderRequestDto
    {
        public Guid Id { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
    }
}
