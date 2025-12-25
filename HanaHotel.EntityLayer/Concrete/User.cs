using Microsoft.AspNetCore.Identity;
using System;

namespace HanaHotel.EntityLayer.Concrete
{
    public class User : IdentityUser<int>
    {
        public string Name { get; set; }
        public DateTime DateOfBirth { get; set; }
        public GenderType Gender { get; set; }
        public string? Address { get; set; }

        // Do NOT redeclare Email or PhoneNumber here.
        public int RoleId { get; set; }
    }

    public enum GenderType
    {
        Male = 0,
        Female = 1,
    }
}