using static PadelAppointments.Constants;

namespace PadelAppointments.Services
{
    public sealed class UserResolver
    {
        private readonly HttpContext? _httpContext;

        public UserResolver(IHttpContextAccessor httpContextAccessor)
        {
            _httpContext = httpContextAccessor?.HttpContext;
        }

        public Guid OrganizationId
        {
            get
            {
                if (_httpContext is not null && Guid.TryParse(_httpContext.User.FindFirst(x => x.Type == CustomClaimNames.OrganizationId)?.Value, out var value))
                {
                    return value;
                }
                return Guid.Empty;
            }
        }
    }
}
