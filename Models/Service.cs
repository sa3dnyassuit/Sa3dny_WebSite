using Sa3dny.Data.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Sa3dny.Data.Models
{
    public class Service
    {
        [Key]
        public Guid service_id { get; set; } = Guid.NewGuid();

        [Required]
        [DisplayName("Service Name")]
        public string service_name { get; set; }

        [Required]
        [DisplayName("Service Description")]
        public string Description { get; set; }

        [Required]
        [DisplayName("Minimum Price")]
        public decimal Min_price { get; set; }

        public ICollection<Provider_Service> provider_services { get; set; }
        public ICollection<Requests> requests { get; set; }
    }
}