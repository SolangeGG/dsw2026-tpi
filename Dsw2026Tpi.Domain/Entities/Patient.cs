using System;
using System.Collections.Generic;
using System.Text;

namespace Dsw2026Tpi.Domain.Entities
{
    public class Patient : EntityBase
    {
        public string Email { get; init; }
        public long Dni { get; init; }
        public string Name { get; private set; }
        public string? Phone { get; private set; }


        #region Constructor for EF
#pragma warning disable CS8618
        private Patient()
        {
        }
#pragma warning restore CS8618
        #endregion

        public Patient(string email, long dni, string name, string? phone = null, Guid? id = null) : base(id)
        {
            Email = email;
            Dni = dni;
            Name = name;
            Phone = phone;
        }

        public void Update(string name, string? phone)
        {
            Name = name;
            Phone = phone;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
