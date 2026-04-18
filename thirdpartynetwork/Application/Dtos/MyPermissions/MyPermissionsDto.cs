namespace thirdpartynetwork.Application.Dtos.MyPermissions
{
    public class NetworkPermissionsCollectionDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public ICollection<PermissionShortDto> Permissions { get; set; }
    }
}
