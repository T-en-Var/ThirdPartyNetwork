import type { PermissionShort } from './permission';

export interface TokenPair {
  accessToken: string;
  refreshToken: string;
}

export interface AccessTokenClaims {
  aud: string;
  exp: number;
  iat: number;
  iss: string;
  jti: string;
  name: string;
  nbf: number;
  uid: string;
  AccessIncomplete?: string;
}

export interface NetworkPermissionCollection {
  id: string;
  name: string;
  permissions: PermissionShort[];
}

export interface AuthorizationCode {
  code: string;
  accessToken: string;
}
