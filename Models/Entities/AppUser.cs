using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace DuploAuth.Models.Entities
{
    public class AppUser : IdentityUser
    {
        // Extended Properties
        public string Name { get; set; }

        public string Role { get; set; }

        [EmailAddress]
        public override string Email { get => base.Email; set => base.Email = value; }
    }
}