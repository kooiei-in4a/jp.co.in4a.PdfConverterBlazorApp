namespace PdfConverterApi.Models
{
    public class RateLimitingSettings
    {
        public bool EnableRateLimiting { get; set; }
        public PolicySettings Policies { get; set; } = new();
    }

    public class PolicySettings
    {
        public RateLimitPolicySetting Minute { get; set; } = new();
        public RateLimitPolicySetting Hour { get; set; } = new();
        public RateLimitPolicySetting Day { get; set; } = new();
    }

    public class RateLimitPolicySetting
    {
        public int PermitLimit { get; set; }
        public int WindowInSeconds { get; set; }
    }
}
