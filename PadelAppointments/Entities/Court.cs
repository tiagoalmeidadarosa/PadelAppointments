﻿namespace PadelAppointments.Entities
{
    public class Court
    {
        public int Id { get; set; }
        public string? Name { get; set; }

        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
        public ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();
    }
}
