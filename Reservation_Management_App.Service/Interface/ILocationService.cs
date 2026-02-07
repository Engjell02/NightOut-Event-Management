using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Reservation_Management_App.Domain.DomainModels;

namespace Reservation_Management_App.Service.Interface
{
    public interface ILocationService
    {
        IEnumerable<Location> GetAll();
        Location? GetById(Guid id);

        Location Create(Location entity);
        Location Update(Location entity);
        Location Delete(Guid id);
    }
}