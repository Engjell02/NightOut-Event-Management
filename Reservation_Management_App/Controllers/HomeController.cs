using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Reservation_Management_App.Web.Models;
using Reservation_Management_App.Models;
using Reservation_Management_App.Service.Interface;

namespace Reservation_Management_App.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IEventService _eventService;
        private readonly ILocationService _locationService;
        private readonly IPerformerService _performerService;
        private readonly IReservationService _reservationService;

        public HomeController(
            ILogger<HomeController> logger,
            IEventService eventService,
            ILocationService locationService,
            IPerformerService performerService,
            IReservationService reservationService)
        {
            _logger = logger;
            _eventService = eventService;
            _locationService = locationService;
            _performerService = performerService;
            _reservationService = reservationService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Dashboard()
        {
            var reservations = _reservationService.GetAll();

            // Calculate revenue from approved reservations
            var approvedRevenue = reservations
                .Where(r => r.Status == Domain.DomainModels.Enums.ReservationStatus.Approved)
                .Sum(r => (r.Event?.PricePerPerson ?? 0) * r.NumberOfPeople);

            var model = new DashboardViewModel
            {
                TotalEvents = _eventService.GetAll().Count(),
                TotalLocations = _locationService.GetAll().Count(),
                TotalPerformers = _performerService.GetAll().Count(),
                PendingReservations = reservations
                    .Count(r => r.Status == Domain.DomainModels.Enums.ReservationStatus.Pending),
                ApprovedReservations = reservations
                    .Count(r => r.Status == Domain.DomainModels.Enums.ReservationStatus.Approved),
                TotalReservations = reservations.Count(),
                ApprovedRevenue = approvedRevenue
            };

            return View(model);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> ImportEvents()
        {
            try
            {
                var importService = HttpContext.RequestServices.GetRequiredService<IEventImportService>();
                await importService.ImportEventsFromApiAsync();

                TempData["Success"] = "Events successfully imported from API!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Failed to import events: {ex.Message}";
            }

            return RedirectToAction("Dashboard");
        }
    }
}