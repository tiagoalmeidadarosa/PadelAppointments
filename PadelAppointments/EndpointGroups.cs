using PadelAppointments.Endpoints;

namespace PadelAppointments
{
    public static class EndpointGroups
    {
        public static void MapEndpoints(this WebApplication app)
        {
            app.MapGroup("auth")
                .MapAuthGroup();

            app.MapGroup("courts")
                .MapCourtsGroup()
                .RequireAuthorization();

            app.MapGroup("appointments")
                .MapAppointmentsGroup()
                .RequireAuthorization();

            app.MapGroup("checks")
                .MapChecksGroup()
                .RequireAuthorization();
        }
    }
}
