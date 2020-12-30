using System.ComponentModel.DataAnnotations;

namespace Foundry.Web.Grains.Authentication
{
    public class LoginParameters
    {
        [Required]
        public string Username { get; set; } = null!;

        [Required]
        public string Password { get; set; } = null!;

        public bool RememberMe { get; set; }
    }
}
