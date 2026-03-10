import type { User, UserProxy } from '@/types';

export function getNameDisplayUserProxy(userProxy: UserProxy) {
  const { firstName, lastName, username } = userProxy;

  const fullName = [firstName, lastName].filter(Boolean).join(' ');

  if (fullName && username) {
    return `${fullName} (${username})`;
  }

  return fullName || username || 'Unknown User'; 
}

export const defaultProxy = (user: User) => {
  return user.userProxies.find(up => up.isDefault)!;
}
