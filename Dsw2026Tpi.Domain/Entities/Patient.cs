using System;
using System.Collections.Generic;

namespace Dsw2026Tpi.Domain.Entities
{
    public class Patient : EntityBase
    {
        public int DNI { get; set; }
        public string? Name { get; set; }
        public double TelephoneNumber { get; set; }

        // Colección de citas inicializada
        public List<Appointment> Appointments { get; set; } = new();

        // Constructor vacío requerido por EF Core
        public Patient() { }

        public Patient(int dni, string? name, double telephoneNumber, Guid? id = null) : base(id)
        {
            DNI = dni;
            Name = name;
            TelephoneNumber = telephoneNumber;
        }
    }
}