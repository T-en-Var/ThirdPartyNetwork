namespace thirdpartynetwork.Application.Dtos.Network
{
    public class NetworkEntitlementDto
    {
        public Guid NetworkId { get; set; }
        
        public bool AllowFiles { get; set; }
        public int FileCountLimit { get; set; } = 0;
        public int FileSizeLimit { get; set; } = 0;
        public int FileStorageLimit { get; set; } = 0; // in KB
    
        public bool AllowBlogs { get; set; }
        public int BlogCountLimit { get; set; } = 0;
    
        public bool AllowConfigurations { get; set; }
        public int ConfigurationCountLimit { get; set; } = 0;
    
        public bool AllowCustomPages { get; set; }
        public int CustomPageCountLimit { get; set; }
    }
}
