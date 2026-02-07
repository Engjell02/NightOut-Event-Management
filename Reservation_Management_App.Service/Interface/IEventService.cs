using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Reservation_Management_App.Domain.DomainModels;

namespace Reservation_Management_App.Service.Interface
{
    public interface IEventService
    {
        IEnumerable<Event> GetAll();
        IEnumerable<Event> GetUpcoming();
        Event? GetById(Guid id);

        Event Create(Event entity);
        Event Update(Event entity);
        Event Delete(Guid id);
    }
}

