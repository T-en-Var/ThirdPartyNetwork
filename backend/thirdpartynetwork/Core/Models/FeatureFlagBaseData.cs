namespace thirdpartynetwork.Core.Models;
public class FeatureFlagBaseModel : BaseModel
{
    public bool? AllowFiles { get; set; }
    public int? FileCountLimit { get; set; }
    public int? FileSizeLimit { get; set; }
    public int? FileStorageLimit { get; set; }
    
    public bool? AllowBlogs { get; set; }
    public int? BlogCountLimit { get; set; }
    
    public bool? AllowConfigurations { get; set; }
    public int? ConfigurationCountLimit { get; set; }
    
    public bool? AllowCustomPages { get; set; }
    public int? CustomPageCountLimit { get; set; }
    public int? CustomPageBlockCountLimit { get; set; }
    public int? CustomPageBlockSizeLimit { get; set; }  // Character count
}

public class NetworkUserFeatureFlag : FeatureFlagBaseModel
{
    public Guid NetworkUserId { get; set; }
}

public class RoleFeatureFlag : FeatureFlagBaseModel
{
    public Guid RoleId { get; set; }
}