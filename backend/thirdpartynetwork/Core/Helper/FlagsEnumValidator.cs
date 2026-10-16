namespace thirdpartynetwork.Core.Helper;


public static class FlagsEnumValidator
{
    public static bool IsDefinedCombination<TEnum>(TEnum value) where TEnum : struct, System.Enum
    {
        var bits = Convert.ToInt64(value);
        return bits >= 0 && bits <= UpperBound<TEnum>.Value;
    }

    private static class UpperBound<TEnum> where TEnum : struct, System.Enum
    {
        public static readonly long Value = Math.Max(
            System.Enum.GetValues<TEnum>().Select(member => Convert.ToInt64(member)).DefaultIfEmpty(0).Max() * 2 - 1,
            0);
    }
}
