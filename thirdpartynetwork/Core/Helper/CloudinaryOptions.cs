namespace thirdpartynetwork.Core.Helper;

public sealed class CloudinaryOptions
{
    public string CloudName { get; set; } = default!;
    public string ApiKey { get; set; } = default!;
    public string ApiSecret { get; set; } = default!;
    public string DefaultFolder { get; set; } = "uploads";
}