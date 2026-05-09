using System;

namespace Sa3dny.Data.Models
{
    public class Payment
    {
        public Guid Id { get; set; }
        public Guid RequestId { get; set; }
        public Requests? Request { get; set; }

        public Guid? UserId { get; set; }
        public ApplicationUser? User { get; set; }

        public decimal? Amount { get; set; }
        public string? Method { get; set; }

        public byte[]? ScreenshotData { get; set; }

        public string? SenderNumber { get; set; }
        public string? Status { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}