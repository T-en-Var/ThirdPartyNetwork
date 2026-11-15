interface UserProxyNameFields {
  firstName: string | null;
  lastName: string | null;
  username: string | null;
}

export function getNameDisplayUserProxy(userProxy: UserProxyNameFields) {
  const { firstName, lastName, username } = userProxy;

  const fullName = [firstName, lastName].filter(Boolean).join(' ');

  if (fullName && username) {
    return `${fullName} (${username})`;
  }

  return fullName || username || 'Unknown User';
}

export function defaultProxy<T extends { userProxies: { isDefault: boolean }[] }>(
  user: T,
): T['userProxies'][number] {
  return user.userProxies.find((up) => up.isDefault)!;
}
