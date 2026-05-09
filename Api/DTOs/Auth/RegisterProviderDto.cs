using System.ComponentModel.DataAnnotations;

namespace Sa3dny.Api.DTOs.Auth
{
    public class RegisterProviderDto
    {
        [Required]
        public string Name { get; set; }

        [Required]
        [Phone]
        public string Phone { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [MinLength(6)]
        public string Password { get; set; }

        [Required]
        [Compare("Password")]
        public string ConfirmPassword { get; set; }

        [Required]
        [StringLength(14)]
        public string NationalId { get; set; }

      
        [Required]
        public string GovernorateName { get; set; }

        [Required]
        public string LocationName { get; set; }

        [Required]
        public string ServiceCategoryName { get; set; }

        [Required]
        public string ServiceName { get; set; }

        [Required]
        public string NationalIdImageBase64 { get; set; }

        
    }
}