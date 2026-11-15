import type { FeatureFlagDto } from '../FeatureFlag/FeatureFlagDto';

export interface CreateRoleDto {
  name: string;
  description: string;
  isDefault: boolean;
  featureFlags: FeatureFlagDto;
}
