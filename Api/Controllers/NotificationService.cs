using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Sa3dny.Data;
using Sa3dny.Data.Models;
using Sa3dny.Models;

namespace Sa3dny.Api.Controllers
{
    public class NotificationService
    {
        private readonly AppDbContext _context;

        public NotificationService(AppDbContext context)
        {
            _context = context;
        }

        public async Task Send(string userId, string title, string message, Guid? requestId = null)
        {
            var notification = new Notification
            {
                UserId = userId,
                Title = title,
                Message = message,
                RequestId = requestId
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
        }
    }
}
