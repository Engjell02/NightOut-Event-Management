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
    public class EventService : IEventService
    {
        private readonly IRepository<Event> _eventRepo;
        public EventService(IRepository<Event> eventRepo)
        {
            _eventRepo = eventRepo;
        }

        public IEnumerable<Event> GetAll()
        {
            return _eventRepo.GetAll(
                null,
                q => q.OrderBy(e => e.StartDateTime),
                e => e.Location,
                e => e.MainAct,
                e => e.Dj
            );

        }

        public IEnumerable<Event> GetUpcoming()
        {
            var now = DateTime.UtcNow;

            return _eventRepo.GetAll(
                e => e.StartDateTime >= now,
                q => q.OrderBy(e => e.StartDateTime),
                e => e.Location,
                e => e.MainAct,
                e => e.Dj
            );
        }


        public Event? GetById(Guid id)
        {
            return _eventRepo
                .GetAll(
                    e => e.Id == id,
                    null,
                    e => e.Location,
                    e => e.MainAct,
                    e => e.Dj
                )
                .FirstOrDefault();
        }


        public Event Create(Event entity)
        {
            entity.Id = Guid.NewGuid();
            return _eventRepo.Insert(entity);
        }

        public Event Update(Event entity)
        {
            return _eventRepo.Update(entity);
        }

        public Event Delete(Guid id)
        {
            var entity = _eventRepo.Get(id) ?? throw new Exception("Event not found.");
            return _eventRepo.Delete(entity);
        }
    }
}
