using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace TaskBud.Business.Data
{
    public class ApiToken
    {
        [Key]
        public string Token { get; set; }

        public string UserId { get; set; }

        public IdentityUser User { get; set; }
    }
}
