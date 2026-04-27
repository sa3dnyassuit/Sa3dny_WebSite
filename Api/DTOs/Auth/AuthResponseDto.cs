using System;

namespace Sa3dny.Api.DTOs.Auth
{
    public class AuthResponseDto
    {
        public string Token { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public string Role { get; set; }

        // ✅ التعديل: تغيير النوع من int إلى string ليتوافق مع IdentityUser.Id
        public string UserId { get; set; }

        public DateTime ExpiresAt { get; set; }
    }
}