export function networkKey(networkId: string, ...rest: (string | number)[]): string {
  return ['networks', networkId, ...rest].join('_');
}

export function userProxyKey(
  userId: string,
  userProxyId: string,
  ...rest: (string | number)[]
): string {
  return ['users', userId, 'proxies', userProxyId, ...rest].join('_');
}
