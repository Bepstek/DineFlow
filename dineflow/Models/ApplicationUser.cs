using Microsoft.AspNetCore.Identity;

namespace dineflow.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public string Status { get; set; }

       
    }
}
