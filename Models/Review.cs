using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sa3dny.Data.Models
{
    public class Review
    {
        [Key]
        public Guid Review_Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid Provider_Id { get; set; }
        [ForeignKey("Provider_Id")]
        public Provider provider { get; set; }

        [Required]
        public Guid Request_Id { get; set; }
        [ForeignKey("Request_Id")]
        public Requests requests { get; set; }

        [Required]
        public Guid Customer_Id { get; set; }
        [ForeignKey("Customer_Id")]
        public Customer customer { get; set; }

        [Range(1, 5)]
        public int Rate { get; set; }

        public string Comment { get; set; }

        public DateTime Date { get; set; } = DateTime.UtcNow;
    }
}