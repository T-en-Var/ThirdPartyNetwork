export interface AccessLevel {
  value: number;
  label: string;
  description: string;
}

export const ACCESS_LEVELS: AccessLevel[] = [
  {
    value: 0,
    label: 'Private',
    description: 'Only you can access this',
  },
  {
    value: 1,
    label: 'Network Only',
    description: 'Only network members can access this',
  },
  {
    value: 2,
    label: 'Public',
    description: 'Everyone can access this',
  },
];

export function createAccessLevelMap(): Map<number, AccessLevel> {
  const map = new Map<number, AccessLevel>();
  ACCESS_LEVELS.forEach((level) => map.set(level.value, level));
  return map;
}

const accessLevelMap = createAccessLevelMap();

export function isValidAccessLevel(value: unknown): value is number {
  if (typeof value !== 'number') return false;
  return accessLevelMap.has(value);
}

export function getAccessLevel(value: number): AccessLevel {
  return accessLevelMap.get(value) || ACCESS_LEVELS[0];
}

export function addAccessLevel(level: AccessLevel): void {
  if (!accessLevelMap.has(level.value)) {
    ACCESS_LEVELS.push(level);
    accessLevelMap.set(level.value, level);
    ACCESS_LEVELS.sort((a, b) => a.value - b.value);
  }
}

export function getAllowedAccessLevels(): number[] {
  return ACCESS_LEVELS.map((level) => level.value);
}
