using Microsoft.AspNetCore.Identity;

namespace universite_platform.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? FullName { get; set; }
        public string? StudentNumber { get; set; }
    }
}
