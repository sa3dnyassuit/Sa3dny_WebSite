// FILE: Sa3dny.Data/Models/Customer.cs
// ACTION: Add Points property to the existing Customer model

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sa3dny.Data.Models
{
    public class Customer
    {
        [Key]
        public Guid Id_Customer { get; set; } = Guid.NewGuid();

        [Required]
        public string UserId { get; set; }

        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        [StringLength(11)]
        public string Phone { get; set; }

        [Required]
        public string LocationName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        // ✅ NEW: Points column — default 0, increments +10 per accepted offer
        public int Points { get; set; } = 0;

        public DateTime created_at { get; set; } = DateTime.UtcNow;

        public ICollection<Requests> requests { get; set; }
        public ICollection<Review> reviews { get; set; }
    }
}
