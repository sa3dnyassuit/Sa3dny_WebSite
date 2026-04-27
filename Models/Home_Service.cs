using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Sa3dny.Data.Models
{
    public class Home_Service
    {
        [Key]
        [ForeignKey(nameof(service))]
        // ✅ التعديل: تغيير النوع من int إلى Guid ليتوافق مع جدول Service
        public Guid Service_Id { get; set; }

        [Required]
        public string Type_service { get; set; }

        public string? Equipments { get; set; }

        [Required]
        public string Category_name { get; set; }

        public Service service { get; set; }
    }
}