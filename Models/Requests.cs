using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sa3dny.Data.Models
{
    public class Requests
    {
        [Key]
        public Guid Request_Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid Customer_Id { get; set; }

        [ForeignKey("Customer_Id")]
        public Customer customer { get; set; }

        public Guid? Provider_Id { get; set; }

        [ForeignKey("Provider_Id")]
        public Provider provider { get; set; }

        [Required]
        public Guid Service_Id { get; set; }

        [ForeignKey("Service_Id")]
        public Service service { get; set; }

        [Required]
        public string Description_Req { get; set; }

        [Required]
        public string Address { get; set; }

        [Required]
        public string Phone { get; set; }

        public string Status { get; set; } = "Pending";

        public decimal? Total_Price { get; set; }

        public Guid? AcceptedOfferId { get; set; }

        [ForeignKey("AcceptedOfferId")]
        public ProviderOffer AcceptedOffer { get; set; }

        public DateTime? Time { get; set; }
        public DateTime? Created_At { get; set; }
        public DateTime? Updated_At { get; set; }

        public ICollection<Review> reviews { get; set; }
        public ICollection<ProviderOffer> Offers { get; set; }
    }
}