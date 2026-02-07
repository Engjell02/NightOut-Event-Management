using Microsoft.AspNetCore.Mvc;
using Reservation_Management_App.Domain.DomainModels;
using Reservation_Management_App.Service.Interface;

namespace Reservation_Management_App.Web.Controllers
{
    public class PerformersController : Controller
    {
        private readonly IPerformerService _performerService;

        public PerformersController(IPerformerService performerService)
        {
            _performerService = performerService;
        }

        public IActionResult Index() => View(_performerService.GetAll());

        public IActionResult Details(Guid? id)
        {
            if (id == null) return NotFound();
            var performer = _performerService.GetById(id.Value);
            if (performer == null) return NotFound();
            return View(performer);
        }

        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create([Bind("StageName,Type,ImageUrl")] Performer performer)
        {
            if (!ModelState.IsValid) return View(performer);

            _performerService.Create(performer);
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Edit(Guid? id)
        {
            if (id == null) return NotFound();
            var performer = _performerService.GetById(id.Value);
            if (performer == null) return NotFound();
            return View(performer);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Guid id, [Bind("Id,StageName,Type,ImageUrl")] Performer performer)
        {
            if (id != performer.Id) return NotFound();
            if (!ModelState.IsValid) return View(performer);

            _performerService.Update(performer);
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Delete(Guid? id)
        {
            if (id == null) return NotFound();
            var performer = _performerService.GetById(id.Value);
            if (performer == null) return NotFound();
            return View(performer);
        }

        // POST: Performers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(Guid id)
        {
            try
            {
                _performerService.Delete(id);
                TempData["Success"] = "Performer deleted successfully.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
