namespace thirdpartynetwork.Application.Dtos.NetworkUser
{
    public class CreateNetworkUserDto
    {
        public required Guid NetworkId { get; set; }
        public required Guid UserProxyId { get; set; }
    }
}
