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
    public class LocationService : ILocationService
    {
        private readonly IRepository<Location> _locationRepo;

        public LocationService(IRepository<Location> locationRepo)
        {
            _locationRepo = locationRepo;
        }

        public IEnumerable<Location> GetAll()
        {
            return _locationRepo.GetAll(orderBy: q => q.OrderBy(l => l.Name));
        }

        public Location? GetById(Guid id)
        {
            return _locationRepo.Get(id);
        }

        public Location Create(Location entity)
        {
            entity.Id = Guid.NewGuid();
            return _locationRepo.Insert(entity);
        }

        public Location Update(Location entity)
        {
            return _locationRepo.Update(entity);
        }

        public Location Delete(Guid id)
        {
            var location = _locationRepo.GetWithIncludes(id, l => l.Events);

            if (location == null)
                throw new Exception("Location not found.");

            var eventCount = location.Events?.Count ?? 0;

            if (eventCount > 0)
            {
                throw new Exception($"Cannot delete '{location.Name}'. This location has {eventCount} event(s). Please delete those events first.");
            }

            return _locationRepo.Delete(location);
        }
    }
}
