using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reservation_Management_App.Domain.DomainModels
{
    public class Performer : BaseEntity
    {
        [Required]
        public string StageName { get; set; } = string.Empty;

        public string? Type { get; set; }
        public string? ImageUrl { get; set; }
        public bool ImportedFromApi { get; set; } = false;

        public virtual ICollection<Event> EventsAsMainAct { get; set; } = new List<Event>();
        public virtual ICollection<Event> EventsAsDj { get; set; } = new List<Event>();
    }
}
