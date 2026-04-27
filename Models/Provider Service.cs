using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sa3dny.Data.Models
{
    public class Provider_Service
    {
        // تم التغيير لـ Guid لتوافق الموديلات الأصلية
        public Guid ProviderId { get; set; }
        public Guid ServiceId { get; set; }

        [ForeignKey("ProviderId")]
        public Provider Provider { get; set; }

        [ForeignKey("ServiceId")]
        public Service Service { get; set; }
    }
}