using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Reservation_Management_App.Domain.DomainModels.Enums;

namespace Reservation_Management_App.Domain.DomainModels
{
    public class Reservation : BaseEntity
    {
        [Required]
        public string ReservationName { get; set; } = string.Empty;

        [Range(2, 6)]
        public int NumberOfPeople { get; set; }

        [Required]
        public string PhoneNumber { get; set; } = string.Empty;

        public ReservationStatus Status { get; set; } = ReservationStatus.Pending;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Guid EventId { get; set; }
        public virtual Event? Event { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;
    }
}
