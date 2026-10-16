using thirdpartynetwork.Core.Exceptions.HttpExceptions;
using thirdpartynetwork.Core.Models;

public class NetworkEntitlementValidator :  INetworkEntitlementValidator
{
    public void ValidateNetworkEntitlement(Network? network)
    {
        if (network == null)
        {
            throw new NotFoundHttpException("Network not found");
        }
    }

    public void ValidateNetworkUser(NetworkUser? networkUser)
    {
        if (networkUser == null)
        {
            throw new NotFoundHttpException("Networkuser not found");
        }
    }
    public void CheckNetworkPropertyIsAllowed(bool allowed)
    {
        if (!allowed)
        {
            throw new Exception("Operation not allowed");
        }
    }

    public void CheckNetworkFileCount(int count, int fileCountLimit)
    {
        if (count >= fileCountLimit)
        {
            throw new Exception("FileCountLimit reached");
        }
    }

    public void CheckNetworkFileSizeLimit(IFormFile file, int fileSizeLimit)
    {
        if (file.Length > fileSizeLimit * 1024.0) // FileSizeLimit is in KB
        {
            throw new Exception("FileSizeLimit reached");
        }
    }

    public void CheckNetworkFileStorageReached(ICollection<thirdpartynetwork.Core.Models.File> files, IFormFile file, int fileStorageLimit)
    {
        var totalNetworkFileStorageSize = files.Sum(networkFile => networkFile.SizeBytes);
        if (totalNetworkFileStorageSize + file.Length > fileStorageLimit * 1024.0) // FileStorageLimit is in KB
        {
            throw new Exception("FileStorageLimit reached");
        }
    }
}