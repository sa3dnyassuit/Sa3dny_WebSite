using System;
using System.ComponentModel.DataAnnotations;

namespace Sa3dny.Api.DTOs.Requests.Customer
{
    public class CreateRequestDto
    {
        // ✅ التعديل: إضافة الحقل ده عشان نبعته في الـ Swagger يدوي
        [Required]
        public Guid Customer_Id { get; set; }

        [Required]
        public Guid Service_Id { get; set; }

        [Required]
        public string Description_Req { get; set; }

        [Required]
        public string Address { get; set; }

        [Required]
        public string Phone { get; set; }
    }
}