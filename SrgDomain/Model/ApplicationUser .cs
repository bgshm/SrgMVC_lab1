using Microsoft.AspNetCore.Identity;

namespace SrgDomain.Model
{
    public class ApplicationUser : IdentityUser
    {
        public int? DepartmentId { get; set; }
    }
}
