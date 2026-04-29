namespace ZenlyAPI.Domain.Config
{
    public class ZenlyConfig
    {
        public string ConnectionString { get; set; } = default!;
        public bool IsProduction { get; set; } = default!;
        public string ApiKey { get; set; } = default!;
        public SerilogConfig SerilogConfig { get; set; } = default!;
        public JwtConfig JwtConfig { get; set; } = default!;
        public CloudinaryConfig CloudinaryConfig { get; set; } = default!;
    }
}
