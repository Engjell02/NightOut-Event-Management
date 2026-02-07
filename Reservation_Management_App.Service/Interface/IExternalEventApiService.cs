using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Reservation_Management_App.Service.DTOs;
namespace Reservation_Management_App.Service.Interface
{
    public interface IExternalEventApiService
    {
        Task<ExternalEventDto?> GetByEventCodeAsync(string eventCode);
        Task<List<ExternalEventDto>> GetAllEventsAsync();
    }
}