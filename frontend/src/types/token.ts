export interface TokenPair {
    accessToken: string
    refreshToken: string
}


export interface accessTokenClaims {
    aud: string;
    exp: number;
    iat: number;
    iss: string;
    jti: string;
    name: string;
    nbf: number;
    uid: string;
}
