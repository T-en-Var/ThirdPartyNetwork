using thirdpartynetwork.Core.Models;

public interface INetworkEntitlementValidator
{
    void ValidateNetworkEntitlement(Network? network);
    void ValidateNetworkUser(NetworkUser? networkUser);
    void CheckNetworkPropertyIsAllowed(bool allowed);
    void CheckNetworkFileCount(int count, int fileCountLimit);
    void CheckNetworkFileSizeLimit(IFormFile file, int fileSizeLimit);
    void CheckNetworkFileStorageReached(ICollection<thirdpartynetwork.Core.Models.File> files, IFormFile file, int fileStorageLimit);
}