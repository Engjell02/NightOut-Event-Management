using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Reservation_Management_App.Domain.DomainModels;

namespace Reservation_Management_App.Service.Interface
{
    public interface IReservationService
    {
        // Admin
        IEnumerable<Reservation> GetAll();

        // User
        IEnumerable<Reservation> GetByUser(string userId);

        // Actions
        Reservation CreateReservation(Guid eventId, string userId, string reservationName, int numberOfPeople, string phoneNumber);
        bool CancelReservation(Guid reservationId, string userId); // true if cancelled, false if not allowed

        Reservation Approve(Guid reservationId);
        Reservation Reject(Guid reservationId);
    }
}
