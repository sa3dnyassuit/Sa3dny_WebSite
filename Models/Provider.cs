using Sa3dny.Data.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sa3dny.Data.Models
{
    public class Provider
    {
        [Key]
        public Guid provider_id { get; set; } = Guid.NewGuid();

        [Required]
        public string UserId { get; set; } // تم تغييره لـ string

        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; }

        [Required]
        [StringLength(14)]
        public string national_id_Provider { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string LocationName { get; set; }

        public DateTime created_at { get; set; } = DateTime.UtcNow;

        public double? rate_Provider { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [StringLength(11)]
        public string Phone { get; set; }

        [Required]
        public int GovernorateId { get; set; }
        public Governorate Governorate { get; set; }

        [Required]
        public int ServiceCategoryId { get; set; }
        public ServiceCategory ServiceCategory { get; set; }

        [Required]
        public Guid ServiceId { get; set; }
        public Service Service { get; set; }

        [Required]
        public string NationalIdImagePath { get; set; }

        [Required]
        public string ProfessionalLicensePath { get; set; }

        public ICollection<Review> reviews { get; set; }
        public ICollection<Provider_Service> provider_Services { get; set; }
        public ICollection<Requests> requests { get; set; }
    }
}