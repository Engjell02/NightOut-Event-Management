using Microsoft.AspNetCore.Mvc;
using Reservation_Management_App.Domain.DomainModels;
using Reservation_Management_App.Service.Interface;

namespace Reservation_Management_App.Web.Controllers
{
    public class LocationsController : Controller
    {
        private readonly ILocationService _locationService;

        public LocationsController(ILocationService locationService)
        {
            _locationService = locationService;
        }

        public IActionResult Index() => View(_locationService.GetAll());

        public IActionResult Details(Guid? id)
        {
            if (id == null) return NotFound();
            var location = _locationService.GetById(id.Value);
            if (location == null) return NotFound();
            return View(location);
        }

        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create([Bind("Name,City,Address,PhoneNumber,ImageUrl,Type,Capacity")] Location location)
        {
            if (!ModelState.IsValid)
            {
                return View(location);
            }

            _locationService.Create(location);
            TempData["Success"] = "Location created successfully.";
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Edit(Guid? id)
        {
            if (id == null) return NotFound();
            var location = _locationService.GetById(id.Value);
            if (location == null) return NotFound();
            return View(location);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Guid id, [Bind("Id,Name,City,Address,PhoneNumber,ImageUrl,Type,Capacity")] Location location)
        {
            if (id != location.Id) return NotFound();

            if (!ModelState.IsValid)
            {
                return View(location);
            }

            _locationService.Update(location);
            TempData["Success"] = "Location updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Delete(Guid? id)
        {
            if (id == null) return NotFound();
            var location = _locationService.GetById(id.Value);
            if (location == null) return NotFound();
            return View(location);
        }

        // POST: Locations/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(Guid id)
        {
            try
            {
                _locationService.Delete(id);
                TempData["Success"] = "Location deleted successfully.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
