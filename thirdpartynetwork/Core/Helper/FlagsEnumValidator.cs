namespace thirdpartynetwork.Core.Helper;

/// <summary>
/// Validates that a [Flags] enum value only sets bits belonging to a currently-defined member.
/// The "known bits" mask is computed via reflection (Enum.GetValues), once per closed TEnum,
/// cached in a generic static field — so adding/removing a member on the enum is picked up
/// automatically the next time it runs; nothing here needs to change alongside the enum.
/// Only correct for enums backed by an integral type no larger than 64 bits signed (i.e. every
/// [Flags] enum in this codebase today — the implicit `int` backing type).
/// </summary>
public static class FlagsEnumValidator
{
    public static bool IsDefinedCombination<TEnum>(TEnum value) where TEnum : struct, Enum
    {
        var bits = Convert.ToInt64(value);
        return (bits & ~KnownBits<TEnum>.Mask) == 0;
    }

    private static class KnownBits<TEnum> where TEnum : struct, Enum
    {
        public static readonly long Mask = Enum.GetValues<TEnum>()
            .Aggregate(0L, (mask, member) => mask | Convert.ToInt64(member));
    }
}
