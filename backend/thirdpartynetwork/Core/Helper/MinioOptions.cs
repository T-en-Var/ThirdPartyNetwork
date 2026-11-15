namespace thirdpartynetwork.Core.Helper;

public sealed class MinioOptions
{
    public string RootUser { get; set; } = default!;
    public string RootPassword { get; set; } = default!;

    public string InternalEndpoint { get; set; } = default!;
    public bool InternalUseSsl { get; set; } = false;

    public string ExternalEndpoint { get; set; } = default!;
    public bool ExternalUseSsl { get; set; } = true;

    public string PublicBucket { get; set; } = "public";
    public string PrivateBucket { get; set; } = "private";

    public int PresignExpirySecondsDefault { get; set; } = 300;
    public int PresignExpirySecondsStreaming { get; set; } = 21600;
}
