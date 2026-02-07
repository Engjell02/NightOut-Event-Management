using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Reservation_Management_App.Domain.DomainModels;

namespace Reservation_Management_App.Service.Interface
{
    public interface IPerformerService
    {
        IEnumerable<Performer> GetAll();
        Performer? GetById(Guid id);

        Performer Create(Performer entity);
        Performer Update(Performer entity);
        Performer Delete(Guid id);
    }
}
