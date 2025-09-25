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
  networks: string;
}


export interface CustomClaim {
  Key: string;
  Value: boolean
}

export interface NetworkClaims {
  id: string;
  claims: CustomClaim[]
}


export interface AuthorizationCode {
  code: string;
  accessToken: string;
}
