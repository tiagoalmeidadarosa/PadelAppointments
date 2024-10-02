﻿namespace PadelAppointments.Entities
{
    public class Organization
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = default!;

        public ICollection<Agenda> Agendas { get; set; } = [];
    }
}
