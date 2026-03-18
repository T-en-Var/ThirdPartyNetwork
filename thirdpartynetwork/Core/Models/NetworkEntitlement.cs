namespace thirdpartynetwork.Core.Models;

public class NetworkEntitlement : BaseModel
{
    public Guid NetworkId { get; set; }
    public bool AllowFiles { get; set; } = false;
    public int FileCountLimit { get; set; } = 0;
    public int FileSizeLimit { get; set; } = 0;
    public int FileStorageLimit { get; set; } = 0; // in KB
    
    public bool AllowBlogs { get; set; } = false;
    public int BlogCountLimit { get; set; } = 0;
    
    public bool AllowConfigurations { get; set; } = false;
    public int ConfigurationCountLimit { get; set; } = 0;
    
    public bool AllowCustomPages { get; set; } = false;
    public int CustomPageCountLimit { get; set; } = 0;
    public int CustomPageBlockCountLimit { get; set; } = 0;
    public int CustomPageBlockSizeLimit { get; set; } = 0;  // Character count
}