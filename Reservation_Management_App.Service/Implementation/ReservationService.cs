using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Reservation_Management_App.Domain.DomainModels;
using Reservation_Management_App.Domain.DomainModels.Enums;
using Reservation_Management_App.Repository;
using Reservation_Management_App.Service.Interface;

namespace Reservation_Management_App.Service.Implementation
{
    public class ReservationService : IReservationService
    {
        private readonly IRepository<Reservation> _reservationRepo;
        private readonly IRepository<Event> _eventRepo;

        public ReservationService(IRepository<Reservation> reservationRepo, IRepository<Event> eventRepo)
        {
            _reservationRepo = reservationRepo;
            _eventRepo = eventRepo;
        }

        // Admin list
        public IEnumerable<Reservation> GetAll()
        {
            return _reservationRepo.GetAll(
                null,
                q => q.OrderByDescending(r => r.CreatedAt),
                r => r.Event,
                r => r.Event.Location,
                r => r.Event.MainAct,
                r => r.Event.Dj
            );
        }


        // My reservations list
        public IEnumerable<Reservation> GetByUser(string userId)
        {
            return _reservationRepo.GetAll(
                r => r.UserId == userId,
                q => q.OrderByDescending(r => r.CreatedAt),
                r => r.Event,
                r => r.Event.Location,
                r => r.Event.MainAct,
                r => r.Event.Dj

            );
        }


        public Reservation CreateReservation(Guid eventId, string userId, string reservationName, int numberOfPeople, string phoneNumber)
        {
            var ev = _eventRepo.GetWithIncludes(eventId, e => e.Location);
            if (ev == null)
                throw new Exception("Event not found.");

            // Check if event has available tables
            if (ev.AvailableSpots <= 0)
                throw new Exception("No tables available for this event.");

            // Validate group size (2-6 people per table)
            if (numberOfPeople < 2 || numberOfPeople > 6)
                throw new Exception("Group size must be between 2 and 6 people.");

            var reservation = new Reservation
            {
                Id = Guid.NewGuid(),
                EventId = eventId,
                UserId = userId,
                ReservationName = reservationName,
                NumberOfPeople = numberOfPeople,
                PhoneNumber = phoneNumber,
                Status = ReservationStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            _reservationRepo.Insert(reservation);

            // Decrease available tables by 1 (one reservation = one table)
            ev.AvailableSpots -= 1; // ✅ FIXED - decrease by 1 table, not by number of people
            _eventRepo.Update(ev);

            return reservation;
        }

        public bool CancelReservation(Guid reservationId, string userId)
        {
            var reservation = _reservationRepo.GetWithIncludes(
                reservationId,
                r => r.Event
            );

            if (reservation == null || reservation.UserId != userId)
                throw new Exception("Reservation not found or you don't have permission.");

            if (reservation.Status == ReservationStatus.Rejected || reservation.Status == ReservationStatus.Cancelled)
                throw new Exception("This reservation is already cancelled or rejected.");

            var now = DateTime.UtcNow;
            if (reservation.Event.StartDateTime <= now.AddHours(24))
                return false; // Cannot cancel within 24h

            reservation.Status = ReservationStatus.Cancelled;
            _reservationRepo.Update(reservation);

            // Restore 1 table (not the number of people)
            reservation.Event.AvailableSpots += 1; // ✅ FIXED - restore 1 table
            _eventRepo.Update(reservation.Event);

            return true;
        }

        public Reservation Approve(Guid id)
        {
            var reservation = _reservationRepo.GetWithIncludes(id, r => r.Event);
            if (reservation == null)
                throw new Exception("Reservation not found.");

            // If currently rejected, restore the table first (since reject removed it)
            if (reservation.Status == ReservationStatus.Rejected)
            {
                reservation.Event.AvailableSpots -= 1; // Take the table back
                _eventRepo.Update(reservation.Event);
            }

            reservation.Status = ReservationStatus.Approved;
            return _reservationRepo.Update(reservation);
        }


        public Reservation Reject(Guid id)
        {
            var reservation = _reservationRepo.GetWithIncludes(id, r => r.Event);
            if (reservation == null)
                throw new Exception("Reservation not found.");

            // If currently approved, restore the table (approving doesn't remove it, but we need consistency)
            // If currently pending, restore the table (pending already removed it on creation)
            if (reservation.Status == ReservationStatus.Pending || reservation.Status == ReservationStatus.Approved)
            {
                reservation.Event.AvailableSpots += 1; // Restore 1 table
                _eventRepo.Update(reservation.Event);
            }

            reservation.Status = ReservationStatus.Rejected;
            return _reservationRepo.Update(reservation);
        }

    }
}
