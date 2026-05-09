// FILE: Sa3dny.Data/Models/ChatMessage.cs
// CHANGES: أضفنا دعم رفع الملفات في الشات

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sa3dny.Data.Models
{
    public class ChatMessage
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid RequestId { get; set; }

        [ForeignKey("RequestId")]
        public Requests Request { get; set; }

        [Required]
        public string SenderId { get; set; }

        [Required]
        public string SenderName { get; set; }

        // ✅ نص الرسالة — nullable لأن ممكن تكون رسالة ملف بس
        public string? Message { get; set; }

        // ✅ نوع الرسالة: "text" أو "file"
        public string MessageType { get; set; } = "text";

        // ✅ اسم الملف المرفق (لو MessageType = "file")
        public string? FileName { get; set; }

        // ✅ مسار الملف على السيرفر للتحميل
        public string? FileUrl { get; set; }

        // ✅ نوع الـ MIME (image/jpeg, application/pdf, ...)
        public string? FileMimeType { get; set; }

        public DateTime SentAt { get; set; } = DateTime.UtcNow;
    }
}