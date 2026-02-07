using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reservation_Management_App.Service.Interface
{
    public interface IEventImportService
    {
        Task<int> ImportEventsFromApiAsync();
    }
}