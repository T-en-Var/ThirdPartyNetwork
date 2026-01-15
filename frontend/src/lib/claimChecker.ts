import type { Network, NetworkClaims } from "@/types";

export class ClaimChecker {
  private networkClaims: NetworkClaims;

  private Administrator = "Administrator";
  private ReadNetwork = "Read Network";
  private ManageNetwork = "Manage Network";
  private ReadAccess = "Read Access";
  private ManageAccess = "Manage Access";
  private ReadPermission = "Read Permission";
  private ManagePermission = "Manage Permission";
  private ReadRole = "Read Role";
  private ManageRole = "Manage Role";
  private IsUserOwner = "IsUserOwner";
  private IsNetworkUserOwner = "IsNetworkUserOwner";
  private IsUserProxyOwner = "IsUserProxyOwner";
  private ReadUser = "Read User";
  private ManageUser = "Manage User";
  private ReadCustomPage = "Read CustomPage";
  private ManageCustomPage = "Manage CustomPage";
  private ReadPageBlock = "Read PageBlock";
  private ManagePageBlock = "Manage PageBlock";
  private ReadFile = "Read File";
  private ManageFile = "Manage File";
  private ReadConfiguration = "Read Configuration";
  private ManageConfiguration = "Manage Configuration";
  private ReadBlog = "Read Blog";
  private ManageBlog = "Manage Blog";

  constructor(networkClaims: NetworkClaims) {
    this.networkClaims = networkClaims;
  }

  setNetworkClaims(networkClaims: NetworkClaims) {
    this.networkClaims = networkClaims;
  }

  canManageNetwork(network: Network) {
    const claimCollection = this.networkClaims[network.id];
    if (!claimCollection) return false;

    return claimCollection.includes(this.Administrator) || claimCollection.includes(this.ManageNetwork);
  }
}
