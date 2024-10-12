using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PadelAppointments.Entities;
using PadelAppointments.Models.Authentication;

namespace PadelAppointments
{
    public class ApplicationDbContext(DbContextOptions options) : IdentityDbContext<ApplicationUser>(options)
    {
        public DbSet<Organization> Organizations { get; set; } = default!;
        public DbSet<Agenda> Agendas { get; set; } = default!;
        public DbSet<Appointment> Appointments { get; set; } = default!;
        public DbSet<Schedule> Schedules { get; set; } = default!;
        public DbSet<ItemConsumed> ItemsConsumed { get; set; } = default!;
        public DbSet<Check> Checks { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.ApplyConfigurationsFromAssembly(GetType().Assembly);
        }
    }
}
