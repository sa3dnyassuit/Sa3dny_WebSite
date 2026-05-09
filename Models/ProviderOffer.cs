using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sa3dny.Data.Models
{
    public class ProviderOffer
    {
        [Key]
        public Guid offerId { get; set; } = Guid.NewGuid();

        [Required]
        public Guid RequestId { get; set; }

        [ForeignKey("RequestId")]
        public Requests Request { get; set; }

        [Required]
        public Guid ProviderId { get; set; }

        [ForeignKey("ProviderId")]
        public Provider Provider { get; set; }

        [Required]
        public decimal Price { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}