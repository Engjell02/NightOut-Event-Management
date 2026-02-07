using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Reservation_Management_App.Domain.DomainModels;
using Reservation_Management_App.Repository;
using Reservation_Management_App.Service.Interface;

namespace Reservation_Management_App.Service.Implementation
{
    public class PerformerService : IPerformerService
    {
        private readonly IRepository<Performer> _performerRepo;

        public PerformerService(IRepository<Performer> performerRepo)
        {
            _performerRepo = performerRepo;
        }

        public IEnumerable<Performer> GetAll()
        {
            return _performerRepo.GetAll(orderBy: q => q.OrderBy(p => p.StageName));
        }

        public Performer? GetById(Guid id)
        {
            return _performerRepo.Get(id);
        }

        public Performer Create(Performer entity)
        {
            entity.Id = Guid.NewGuid();
            return _performerRepo.Insert(entity);
        }

        public Performer Update(Performer entity)
        {
            return _performerRepo.Update(entity);
        }

        public Performer Delete(Guid id)
        {
            // Load performer with their event relationships
            var performer = _performerRepo.GetWithIncludes(
                id,
                p => p.EventsAsMainAct,
                p => p.EventsAsDj
            );

            if (performer == null)
                throw new Exception("Performer not found.");

            // Check if performer is used in any events
            var eventsAsMainAct = performer.EventsAsMainAct?.Count ?? 0;
            var eventsAsDj = performer.EventsAsDj?.Count ?? 0;
            var totalEvents = eventsAsMainAct + eventsAsDj;

            if (totalEvents > 0)
            {
                throw new Exception($"Cannot delete '{performer.StageName}'. This performer is assigned to {totalEvents} event(s). Please remove them from all events first.");
            }

            return _performerRepo.Delete(performer);
        }
    }
}
