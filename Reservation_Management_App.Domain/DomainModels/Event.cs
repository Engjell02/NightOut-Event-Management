using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Reservation_Management_App.Domain.DomainModels
{
    public class Event : BaseEntity
    {
        [Required]
        public string Title { get; set; } = string.Empty;

        [Required]
        public DateTime StartDateTime { get; set; }

        [Required]
        public decimal PricePerPerson { get; set; }

        [Required]
        public int AvailableSpots { get; set; }

        public string? PosterImageUrl { get; set; }

        public string? ExternalEventCode { get; set; }

        public bool ImportedFromApi { get; set; } = false;

        public Guid LocationId { get; set; }
        public Guid? MainActId { get; set; }
        public Guid? DjId { get; set; }

        public virtual Location? Location { get; set; }
        public virtual Performer? MainAct { get; set; }
        public virtual Performer? Dj { get; set; }
        public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    }
}