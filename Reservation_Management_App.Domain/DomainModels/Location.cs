using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reservation_Management_App.Domain.DomainModels
{
    public class Location : BaseEntity
    {
        [Required]
        public string Name { get; set; } = string.Empty;
        public string? City { get; set; }
        public string? Address { get; set; }
        public string? PhoneNumber { get; set; }
        public string? ImageUrl { get; set; }
        public string? Type { get; set; }
        public int? Capacity { get; set; }
        public bool ImportedFromApi { get; set; } = false;


        public virtual ICollection<Event> Events { get; set; } = new List<Event>();
    }
}