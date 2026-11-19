namespace thirdpartynetwork.Application.Dtos.NetworkUserAccess
{
    public class NetworkUserAccessDto
    {
        public required Guid NetworkUserId { get; set; }
        public required Guid AccessId { get; set; }
        public required bool IsAccepted { get; set; }
        public required NetworkUserLightDto NetworkUser { get; set; }
        public required AccessLightDto Access { get; set; }
    }
    
    public class NetworkUserAccessNtADto
    {
        public required bool IsAccepted { get; set; }
        public required AccessLightDto Access { get; set; }
    }
    
    public class NetworkUserAccessAtNDto
    {
        public required bool IsAccepted { get; set; }
        public required NetworkUserLightDto NetworkUser { get; set; }
    }
}
