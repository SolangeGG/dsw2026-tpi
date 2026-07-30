namespace Dsw2026Tpi.Api.Configurations;

    public class RateLimitingOptions
    {
        public const string SectionName = "RateLimiting";

        public RateLimitPolicySettings Global { get; set; } = new();
        public RateLimitPolicySettings AdminLogin { get; set; } = new();
        public RateLimitPolicySettings PatientLogin { get; set; } = new();
        public RateLimitPolicySettings AppointmentBooking { get; set; } = new();
    }

public class RateLimitPolicySettings
{
    public int PermitLimit { get; set; }
    public int WindowSeconds { get; set; }
}
