using Microsoft.AspNetCore.Identity;

namespace TaRge25Shop.Core.Domain
{
    public class ApplicationUser : IdentityUser
    {
        public string Name { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
    }
}
