// FILE: Sa3dny.Api/DTOs/Requests/Customer/SendMessageDto.cs
// CHANGES: أضفنا IFormFile للـ file messages

using Microsoft.AspNetCore.Http;
using System;

namespace Sa3dny.Api.DTOs.Requests.Customer
{
    public class SendMessageDto
    {
        public Guid RequestId { get; set; }
        public string SenderId { get; set; }
        public string SenderName { get; set; }

        // نص الرسالة — اختياري لو بيبعت ملف
        public string? Message { get; set; }
    }

    // DTO منفصل للـ FormData (نص + ملف)
    public class SendMessageWithFileDto
    {
        public Guid RequestId { get; set; }
        public string SenderId { get; set; }
        public string SenderName { get; set; }
        public string? Message { get; set; }

        // ✅ الملف المرفق — اختياري
        public IFormFile? File { get; set; }
    }
}