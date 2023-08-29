using System.Text.Json.Serialization;

namespace PadelAppointments.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum RecurrenceType
    {
        Yearly,
	}
}
