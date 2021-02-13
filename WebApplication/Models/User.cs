using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace WebApplication.Models
{
    public class User : IdentityUser
    {
        public string Name { get; set; }
        public DateTime DateTimeRegistration { get; set; }
        public DateTime LastLogin { get; set; }
    }
}
