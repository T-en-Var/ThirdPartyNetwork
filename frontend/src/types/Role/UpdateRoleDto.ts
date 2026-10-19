import type { FeatureFlagDto } from '../FeatureFlag/FeatureFlagDto';

export interface UpdateRoleDto {
  name: string;
  description: string;
  isDefault: boolean;
  featureFlags: FeatureFlagDto;
}
