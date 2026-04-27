using Microsoft.AspNetCore.Identity;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Sa3dny.Data.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [DisplayName("Name")]
        public string Name { get; set; }

        [Required]
        [DisplayName("Location")]
        public string LocationName { get; set; }

        public DateTime? created_at { get; set; } = DateTime.UtcNow;
    }
}