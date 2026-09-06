namespace thirdpartynetwork.Core.Helper;

/// <summary>
/// Validates that a [Flags] enum value doesn't set any bit above the highest currently-defined
/// member. Assumes flag bits are only ever appended in order (1&lt;&lt;0, 1&lt;&lt;1, ...) and never
/// removed or reordered, up to the 32-bit width of the enum's `int` backing type — under that
/// assumption "defined" reduces to a plain range check against the largest declared flag, doubled
/// minus one (e.g. highest flag 16 -&gt; valid range is 0..31, covering every combination of the
/// bits below it). The bound is computed via reflection (Enum.GetValues) once per closed TEnum,
/// cached in a generic static field, so a newly appended flag is picked up automatically.
/// </summary>
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
