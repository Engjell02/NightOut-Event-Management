using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace Reservation_Management_App.Domain.Identity
{
    public class Reservation_Management_AppUser: IdentityUser 
    {
        public string? Firstname { get; set; }
        public string? Lastname { get; set; }
        public string? Phone { get; set; }

    }
}
