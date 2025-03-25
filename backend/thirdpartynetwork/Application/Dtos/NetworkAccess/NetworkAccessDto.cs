namespace thirdpartynetwork.Application.Dtos.NetworkAccess
{
    public class NetworkAccessDto
    {
        public Guid NetworkId { get; set; }
        public Guid AccessId { get; set; }
        public required bool IsRequired { get; set; }
        public required AccessDto Access { get; set; }
        public required NetworkDto Network { get; set; }
    }
}
