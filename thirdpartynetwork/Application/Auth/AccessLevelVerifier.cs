using thirdpartynetwork.Core.Enum;
using thirdpartynetwork.Core.Interfaces.Auth;
using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Application.Auth;

public class AccessLevelVerifier(
    IFileRepository fileRepository,
    ICustomPageRepository customPageRepository,
    IPageBlockRepository pageBlockRepository
) : IAccessLevelVerifier
{
    public bool CheckFileAccess(Guid fileId, UserProxy userProxy, Guid networkId)
    {
        var file = fileRepository.GetById(fileId);
        return file != null && CheckAccess(file, userProxy, networkId);
    }
    
    public bool CheckCustomPageAccess(Guid customPageId, UserProxy userProxy, Guid networkId)
    {
        
        var customPage = customPageRepository.GetById(customPageId);
        return customPage != null && CheckAccess(customPage, userProxy, networkId);
    }
    
    public bool CheckPageBlockAccess(Guid pageBlockId, UserProxy userProxy, Guid networkId)
    {
        var pageBlock = pageBlockRepository.GetById(pageBlockId);
        return pageBlock != null && CheckAccess(pageBlock, userProxy, networkId);
    }

    private static bool CheckAccess(UserContentBaseModel model, UserProxy userProxy, Guid networkId)
    {
        switch (model.AccessLevel)
        {
            case AccessLevel.Public:
                return true;
            
            case AccessLevel.PublicToNetwork when model.NetworkId != networkId:
                return false;
            
            case AccessLevel.PublicToNetwork:
                var networkUser = userProxy.NetworkUsers.FirstOrDefault(u => u.NetworkId == networkId);
                return networkUser != null;
                
            case AccessLevel.Private:
                return model.AuthorId == userProxy.Id;
            
            default:
                return false;
        }
    }
}