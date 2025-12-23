/**
 * Access level definitions for network resources
 * Lower numbers are more restrictive, higher numbers are more open
 */
export interface AccessLevel {
  value: number;
  label: string;
  description: string;
}

export const ACCESS_LEVELS: AccessLevel[] = [
  {
    value: 0,
    label: 'Private',
    description: 'Only you can access this'
  },
  {
    value: 1,
    label: 'Network Only',
    description: 'Only network members can access this'
  },
  {
    value: 2,
    label: 'Public',
    description: 'Everyone can access this'
  }
];

/**
 * Create a lookup map for fast validation
 */
export function createAccessLevelMap(): Map<number, AccessLevel> {
  const map = new Map<number, AccessLevel>();
  ACCESS_LEVELS.forEach(level => map.set(level.value, level));
  return map;
}

const accessLevelMap = createAccessLevelMap();

/**
 * Validate that a given value is an allowed access level
 */
export function isValidAccessLevel(value: unknown): value is number {
  if (typeof value !== 'number') return false;
  return accessLevelMap.has(value);
}

/**
 * Get access level by value, return default (0) if invalid
 */
export function getAccessLevel(value: number): AccessLevel {
  return accessLevelMap.get(value) || ACCESS_LEVELS[0];
}

/**
 * Add a new access level (useful for dynamic levels)
 */
export function addAccessLevel(level: AccessLevel): void {
  if (!accessLevelMap.has(level.value)) {
    ACCESS_LEVELS.push(level);
    accessLevelMap.set(level.value, level);
    // Sort by value for consistency
    ACCESS_LEVELS.sort((a, b) => a.value - b.value);
  }
}

/**
 * Get all allowed access level values (for validation)
 */
export function getAllowedAccessLevels(): number[] {
  return ACCESS_LEVELS.map(level => level.value);
}
