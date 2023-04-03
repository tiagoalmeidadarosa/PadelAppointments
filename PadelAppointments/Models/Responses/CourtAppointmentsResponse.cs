namespace PadelAppointments.Models.Responses
{
    public class CourtAppointmentsResponse
    {
        public int Id { get; set; }
        public List<AppointmentResponse> Appointments { get; set; } = new List<AppointmentResponse>();
    }
}
