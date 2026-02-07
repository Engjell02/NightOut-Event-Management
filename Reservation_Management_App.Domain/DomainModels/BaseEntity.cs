using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace Reservation_Management_App.Domain.DomainModels
{
    public abstract class BaseEntity
    {
        [Key]
        public Guid Id { get; set; }
    }
}