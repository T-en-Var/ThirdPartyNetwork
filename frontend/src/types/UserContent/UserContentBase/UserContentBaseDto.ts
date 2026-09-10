import type { UserProxyLightShortProfileDto } from '../../UserProxy/UserProxyDto';
import type { OwnershipMode } from '../../Enums/OwnershipMode';

export interface UserContentBaseDto {
  id: string;
  networkId: string;
  authorUserProxyId: string | null;
  authorUserProxy: UserProxyLightShortProfileDto | null;
  publishedAt: string | null;
  ownershipMode: OwnershipMode;
  viewingUrl: string | null;
  createdOn: string;
  updatedAt: string;
}

export interface UserContentBaseLightDto {
  id: string;
  networkId: string;
  publishedAt: string | null;
  createdOn: string;
  updatedAt: string;
}
