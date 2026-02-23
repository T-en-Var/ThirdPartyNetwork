import type { NetworkUser } from '@/types';

export function getNameDisplayNetworkUser(networkUser: NetworkUser) {
  const { firstName, lastName, username } = networkUser.userProxy;

  const fullName = [firstName, lastName].filter(Boolean).join(' ');

  if (fullName && username) {
    return `${fullName} (${username})`;
  }

  return fullName || username || 'Unknown User'; 
}