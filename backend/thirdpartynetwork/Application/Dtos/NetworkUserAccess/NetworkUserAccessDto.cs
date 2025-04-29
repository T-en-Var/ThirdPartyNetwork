namespace thirdpartynetwork.Application.Dtos.NetworkUserAccess
{
    public class NetworkUserAccessDto
    {
        public required Guid NetworkUserId { get; set; }
        public required Guid AccessId { get; set; }
        public required bool IsAccepted { get; set; }
        public required NetworkUserDto NetworkUser { get; set; }
        public required AccessDto Access { get; set; }
    }
}
