﻿namespace PadelAppointments.Entities
{
    public class Appointment
    {
        public int Id { get; set; }
        public DateOnly Date { get; set; }
        public string? CustomerName { get; set; }
        public string? CustomerPhoneNumber { get; set; }
        public double Price { get; set; }
        public bool HasRecurrence { get; set; }

        public ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();
        public ICollection<Check> Checks { get; set; } = new List<Check>();
    }
}
