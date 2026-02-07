using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Reservation_Management_App.Domain.DomainModels;
using Reservation_Management_App.Service.Interface;
using Reservation_Management_App.Web.ViewModels;

namespace Reservation_Management_App.Web.Controllers
{
    public class EventsController : Controller
    {
        private readonly IEventService _eventService;
        private readonly ILocationService _locationService;
        private readonly IPerformerService _performerService;
        private readonly IExternalEventApiService _externalApi;



        public EventsController(IEventService eventService, ILocationService locationService, IPerformerService performerService,
             IExternalEventApiService externalApi)
        {
            _eventService = eventService;
            _locationService = locationService;
            _performerService = performerService;
            _externalApi = externalApi;

        }

        // GET: Events
        public IActionResult Index()
        {
            var events = _eventService.GetAll();

            // If admin, calculate approved people count per event
            if (User.IsInRole("Admin"))
            {
                var reservationService = HttpContext.RequestServices.GetRequiredService<IReservationService>();
                var allReservations = reservationService.GetAll();

                // Create dictionary of event ID -> total approved people
                var approvedPeopleByEvent = allReservations
                    .Where(r => r.Status == Reservation_Management_App.Domain.DomainModels.Enums.ReservationStatus.Approved)
                    .GroupBy(r => r.EventId)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Sum(r => r.NumberOfPeople)
                    );

                ViewBag.ApprovedPeopleByEvent = approvedPeopleByEvent;
            }

            return View(events);
        }

        // GET: Events/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null) return NotFound();

            var ev = _eventService.GetById(id.Value);
            if (ev == null) return NotFound();

            var externalVm = new EventExternalInfoViewModel { HasExternalData = false };

            if (!string.IsNullOrWhiteSpace(ev.ExternalEventCode))
            {
                try
                {
                    var ext = await _externalApi.GetByEventCodeAsync(ev.ExternalEventCode);

                    if (ext != null)
                    {
                        var dj = ext.Dj?.Price ?? 0m;
                        var main = ext.MainAct?.Price ?? 0m;
                        var venue = ext.Venue?.BaseFee ?? 0m;

                        externalVm = new EventExternalInfoViewModel
                        {
                            HasExternalData = true,
                            DjCost = dj,
                            MainActCost = main,
                            VenueFee = venue,
                            TotalNightCost = dj + main + venue,
                            Timeline = ext.Schedule == null
                                ? "Timeline unavailable"
                                : $"Doors {ext.Schedule.DoorsOpen} → DJ {ext.Schedule.DjStart} → Main Act {ext.Schedule.MainActStart}"
                        };
                    }
                }
                catch { }
            }

            ViewBag.ExternalInfo = externalVm;
            return View(ev);
        }



        // GET: Events/Create
        public IActionResult Create()
        {
            PopulateDropdowns();
            return View();
        }

        // POST: Events/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create([Bind("Title,StartDateTime,PricePerPerson,AvailableSpots,PosterImageUrl,LocationId,MainActId,DjId")] Event ev)
        {
            if (!ModelState.IsValid)
            {
                PopulateDropdowns(ev);
                return View(ev);
            }

            _eventService.Create(ev);
            return RedirectToAction(nameof(Index));
        }

        // GET: Events/Edit/5
        public IActionResult Edit(Guid? id)
        {
            if (id == null) return NotFound();

            var ev = _eventService.GetById(id.Value);
            if (ev == null) return NotFound();

            PopulateDropdowns(ev);
            return View(ev);
        }

        // POST: Events/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Guid id, [Bind("Id,Title,StartDateTime,PricePerPerson,AvailableSpots,PosterImageUrl,LocationId,MainActId,DjId")] Event ev)
        {
            if (id != ev.Id) return NotFound();

            if (!ModelState.IsValid)
            {
                PopulateDropdowns(ev);
                return View(ev);
            }

            _eventService.Update(ev);
            return RedirectToAction(nameof(Index));
        }

        // GET: Events/Delete/5
        public IActionResult Delete(Guid? id)
        {
            if (id == null) return NotFound();

            var ev = _eventService.GetById(id.Value);
            if (ev == null) return NotFound();

            return View(ev);
        }

        // POST: Events/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(Guid id)
        {
            _eventService.Delete(id);
            return RedirectToAction(nameof(Index));
        }

        private void PopulateDropdowns(Event? ev = null)
        {
            var allPerformers = _performerService.GetAll();

            // Filter DJs (only show performers with Type = "DJ")
            var djs = allPerformers.Where(p => p.Type?.ToLower() == "dj").ToList();

            // Filter Main Acts (Bands and Singers only)
            var mainActs = allPerformers.Where(p =>
                p.Type?.ToLower() == "band" ||
                p.Type?.ToLower() == "singer"
            ).ToList();

            var locations = _locationService.GetAll();

            // DJ dropdown - only DJs
            ViewData["DjId"] = new SelectList(djs, "Id", "StageName", ev?.DjId);

            // Main Act dropdown - only Bands and Singers
            ViewData["MainActId"] = new SelectList(mainActs, "Id", "StageName", ev?.MainActId);

            // Location dropdown - all locations
            ViewData["LocationId"] = new SelectList(locations, "Id", "Name", ev?.LocationId);
        }
    }
}
