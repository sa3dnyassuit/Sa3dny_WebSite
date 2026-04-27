using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sa3dny.Api.DTOs.Auth;
using Sa3dny.Api.Services;
using Sa3dny.Data;
using Sa3dny.Data.Models;

namespace Sa3dny.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AppDbContext _context;
        private readonly JwtService _jwtService;

        public AuthController(
            UserManager<ApplicationUser> userManager,
            AppDbContext context,
            JwtService jwtService)
        {
            _userManager = userManager;
            _context = context;
            _jwtService = jwtService;
        }
        // ✅ جلب كل الكاستومر (العملاء)
        [HttpGet("customers")]
        public async Task<IActionResult> GetAllCustomers()
        {
            var customers = await _context.Customers
                .OrderByDescending(c => c.created_at)
                .Select(c => new
                {
                    c.Id_Customer,
                    c.UserId,
                    c.Name,
                    c.Email,
                    c.Phone,
                    c.LocationName,
                    c.created_at
                })
                .ToListAsync();

            return Ok(customers);
        }

        // ✅ جلب كل البروفايدر (الفنيين)
        [HttpGet("providers")]
        public async Task<IActionResult> GetAllProviders()
        {
            var providers = await _context.Providers
                .OrderByDescending(p => p.created_at)
                .Select(p => new
                {
                    p.provider_id,
                    p.UserId,
                    p.Name,
                    p.Email,
                    p.Phone,
                    p.national_id_Provider,
                    p.LocationName,
                    // بنجيب اسم المحافظة والخدمة بدل الـ ID عشان تظهر صح في الفرونت
                    Governorate = _context.Governorates
                        .Where(g => g.Id_Governorate == p.GovernorateId)
                        .Select(g => g.Name_Governorate)
                        .FirstOrDefault(),
                    Service = _context.Services
                        .Where(s => s.service_id == p.ServiceId)
                        .Select(s => s.service_name)
                        .FirstOrDefault(),
                    p.NationalIdImagePath,
                    p.ProfessionalLicensePath,
                    p.created_at
                })
                .ToListAsync();

            return Ok(providers);
        }

        [HttpPost("register/customer")]
        public async Task<IActionResult> RegisterCustomer([FromBody] RegisterCustomerDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existingUser = await _userManager.FindByEmailAsync(dto.Email);
            if (existingUser != null)
                return BadRequest(new { message = "Email already exists" });

            var location = _context.Locations.FirstOrDefault(l => l.Name_Location == dto.LocationName);
            if (location == null)
                return BadRequest(new { message = "Location not found" });

            var identityUser = new ApplicationUser
            {
                UserName = dto.Email,
                Email = dto.Email,
                PhoneNumber = dto.Phone,
                Name = dto.Name,
                LocationName = dto.LocationName
            };

            var result = await _userManager.CreateAsync(identityUser, dto.Password);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            await _userManager.AddToRoleAsync(identityUser, "Customer");

            var customer = new Customer
            {
                UserId = identityUser.Id,
                Name = dto.Name,
                Phone = dto.Phone,
                Email = dto.Email,
                LocationName = dto.LocationName,
                created_at = DateTime.UtcNow
            };

            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();

            var token = _jwtService.GenerateToken(identityUser.Id, dto.Email, dto.Name, "Customer");

            return Ok(new AuthResponseDto
            {
                Token = token,
                Email = dto.Email,
                Name = dto.Name,
                Role = "Customer",
                UserId = customer.Id_Customer.ToString(),
                ExpiresAt = DateTime.UtcNow.AddDays(7)
            });
        }

        [HttpPost("register/provider")]
        public async Task<IActionResult> RegisterProvider([FromBody] RegisterProviderDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existingUser = await _userManager.FindByEmailAsync(dto.Email);
            if (existingUser != null)
                return BadRequest(new { message = "Email already exists" });

            var governorate = _context.Governorates.FirstOrDefault(g => g.Name_Governorate == dto.GovernorateName);
            if (governorate == null)
                return BadRequest(new { message = "Governorate not found" });

            var location = _context.Locations.FirstOrDefault(l => l.Name_Location == dto.LocationName);
            if (location == null)
                return BadRequest(new { message = "Location not found" });

            var serviceCategory = _context.ServiceCategories.FirstOrDefault(sc => sc.Name_Category == dto.ServiceCategoryName);
            if (serviceCategory == null)
                return BadRequest(new { message = "Service category not found" });

            var service = _context.Services.FirstOrDefault(s => s.service_name == dto.ServiceName);
            if (service == null)
                return BadRequest(new { message = "Service not found" });

            var nationalIdPath = SaveBase64File(dto.NationalIdImageBase64, "national_ids");
            var licensePath = SaveBase64File(dto.ProfessionalLicenseBase64, "licenses");

            var identityUser = new ApplicationUser
            {
                UserName = dto.Email,
                Email = dto.Email,
                PhoneNumber = dto.Phone,
                Name = dto.Name,
                LocationName = dto.LocationName
            };

            var result = await _userManager.CreateAsync(identityUser, dto.Password);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            await _userManager.AddToRoleAsync(identityUser, "Provider");

            var provider = new Provider
            {
                UserId = identityUser.Id,
                Name = dto.Name,
                Phone = dto.Phone,
                Email = dto.Email,
                GovernorateId = governorate.Id_Governorate,
                LocationName = dto.LocationName,
                national_id_Provider = dto.NationalId,
                ServiceCategoryId = serviceCategory.Id_Category,
                ServiceId = service.service_id,
                NationalIdImagePath = nationalIdPath,
                ProfessionalLicensePath = licensePath,
                created_at = DateTime.UtcNow
            };

            _context.Providers.Add(provider);
            await _context.SaveChangesAsync();

            var token = _jwtService.GenerateToken(identityUser.Id, dto.Email, dto.Name, "Provider");

            return Ok(new AuthResponseDto
            {
                Token = token,
                Email = dto.Email,
                Name = dto.Name,
                Role = "Provider",
                // ✅ استخدام المسمى الصحيح للمفتاح provider_id
                UserId = provider.provider_id.ToString(),
                ExpiresAt = DateTime.UtcNow.AddDays(7)
            });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null || !await _userManager.CheckPasswordAsync(user, dto.Password))
                return Unauthorized(new { message = "Invalid email or password" });

            var roles = await _userManager.GetRolesAsync(user);
            string role = roles.FirstOrDefault() ?? "";
            string name = user.Name;
            string realUserId = user.Id;

            if (role == "Customer")
            {
                var customer = _context.Customers.FirstOrDefault(c => c.UserId == user.Id);
                if (customer != null) realUserId = customer.Id_Customer.ToString();
            }
            else if (role == "Provider")
            {
                var provider = _context.Providers.FirstOrDefault(p => p.UserId == user.Id);
                // ✅ استخدام المسمى الصحيح للمفتاح provider_id
                if (provider != null) realUserId = provider.provider_id.ToString();
            }

            var token = _jwtService.GenerateToken(user.Id, dto.Email, name, role);

            return Ok(new AuthResponseDto
            {
                Token = token,
                Email = dto.Email,
                Name = name,
                Role = role,
                UserId = realUserId,
                ExpiresAt = DateTime.UtcNow.AddDays(7)
            });
        }
        private string SaveBase64File(string base64, string folder)
        {
            if (string.IsNullOrEmpty(base64)) return null;
            var bytes = Convert.FromBase64String(base64);
            var fileName = $"{Guid.NewGuid()}.jpg";
            var folderPath = Path.Combine("wwwroot", "uploads", folder);
            Directory.CreateDirectory(folderPath);
            var filePath = Path.Combine(folderPath, fileName);
            System.IO.File.WriteAllBytes(filePath, bytes);
            return $"/uploads/{folder}/{fileName}";
        }
    }
}