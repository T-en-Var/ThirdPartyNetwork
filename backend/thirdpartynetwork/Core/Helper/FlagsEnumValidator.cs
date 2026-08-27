namespace thirdpartynetwork.Core.Helper;

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
