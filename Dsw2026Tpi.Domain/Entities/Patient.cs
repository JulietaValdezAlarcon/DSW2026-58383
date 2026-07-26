using System;
using System.Collections.Generic;
using System.Text;

namespace Dsw2026Tpi.Domain.Entities;

public class Patient : EntityBase
{
    public int DNI { get; set; }
    public string? Name { get; set; }
    public int TelephoneNumber { get; set; }
    public List<Appointment>? appointments { get; set; }

}
