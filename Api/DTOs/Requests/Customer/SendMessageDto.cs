using System;

namespace Sa3dny.Api.DTOs.Requests.Customer
{
    public class SendMessageDto
    {
        public Guid RequestId { get; set; }
        public string SenderId { get; set; }
        public string SenderName { get; set; }
        public string Message { get; set; }
    }
}