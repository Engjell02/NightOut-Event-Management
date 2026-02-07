using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Reservation_Management_App.Domain.Identity;
using Reservation_Management_App.Service.Interface;

namespace Reservation_Management_App.Web.Controllers
{
    [Authorize]
    public class ReservationsController : Controller
    {
        private readonly IReservationService _reservationService;
        private readonly UserManager<Reservation_Management_AppUser> _userManager;

        public ReservationsController(
            IReservationService reservationService,
            UserManager<Reservation_Management_AppUser> userManager)
        {
            _reservationService = reservationService;
            _userManager = userManager;
        }

        // ✅ USER: My reservations
        [HttpGet]
        public IActionResult My()
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrWhiteSpace(userId)) return Challenge();

            var reservations = _reservationService.GetByUser(userId);
            return View(reservations);
        }

        // ✅ USER: Reserve form (from Events)
        [HttpGet]
        public IActionResult Reserve(Guid eventId)
        {
            if (eventId == Guid.Empty) return BadRequest();

            ViewBag.EventId = eventId;

            // show possible previous error messages
            ViewBag.Error = TempData["Error"];
            return View();
        }

        // ✅ USER: Reserve submit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Reserve(Guid eventId, string reservationName, int numberOfPeople, string phoneNumber)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrWhiteSpace(userId)) return Challenge();

            if (eventId == Guid.Empty)
                ModelState.AddModelError("", "Invalid event.");

            if (string.IsNullOrWhiteSpace(reservationName))
                ModelState.AddModelError(nameof(reservationName), "Reservation name is required.");

            if (numberOfPeople < 2 || numberOfPeople > 6)
                ModelState.AddModelError(nameof(numberOfPeople), "Number of people must be between 2 and 6.");

            if (string.IsNullOrWhiteSpace(phoneNumber))
                ModelState.AddModelError(nameof(phoneNumber), "Phone number is required.");

            if (!ModelState.IsValid)
            {
                ViewBag.EventId = eventId;
                return View();
            }

            try
            {
                _reservationService.CreateReservation(eventId, userId, reservationName.Trim(), numberOfPeople, phoneNumber.Trim());
                TempData["Success"] = "Reservation created! Status: Pending (waiting for admin approval).";
                return RedirectToAction(nameof(My));
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Reserve), new { eventId });
            }
        }

        // ✅ USER: Cancel (24h rule enforced in service)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Cancel(Guid id)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrWhiteSpace(userId)) return Challenge();

            try
            {
                var ok = _reservationService.CancelReservation(id, userId);

                if (!ok)
                    TempData["Error"] = "You can cancel only 24 hours or more before the event.";
                else
                    TempData["Success"] = "Reservation cancelled successfully.";

                return RedirectToAction(nameof(My));
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(My));
            }
        }

        // ✅ ADMIN: All reservations with optional event filter
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult Index(Guid? eventId)
        {
            var reservations = _reservationService.GetAll();

            // Filter by event if specified
            if (eventId.HasValue && eventId.Value != Guid.Empty)
            {
                reservations = reservations.Where(r => r.EventId == eventId.Value);
            }

            // Get all events for dropdown
            var events = reservations.Select(r => r.Event).Distinct().ToList();
            ViewBag.Events = events;
            ViewBag.SelectedEventId = eventId;

            return View(reservations);
        }

        // ✅ ADMIN: Approve
        // ✅ ADMIN: Approve
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Approve(Guid id)
        {
            try
            {
                _reservationService.Approve(id);
                TempData["Success"] = "Reservation approved successfully.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        // ✅ ADMIN: Reject
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Reject(Guid id)
        {
            try
            {
                _reservationService.Reject(id);
                TempData["Success"] = "Reservation rejected successfully.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
