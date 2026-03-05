namespace thirdpartynetwork.Application.Dtos.Network
{
    public class CreateNetworkEntitlementDto
    {
        public bool AllowFiles { get; set; } = false;
        public bool AllowBlogs { get; set; } = false;
        public bool AllowConfigurations { get; set; } = false;
        public bool AllowCustomPages { get; set; } = false;
    }
}
