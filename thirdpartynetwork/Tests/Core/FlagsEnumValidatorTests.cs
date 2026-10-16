using NUnit.Framework;
using thirdpartynetwork.Core.Enum;
using thirdpartynetwork.Core.Helper;

namespace thirdpartynetwork.Tests.Core;

[TestFixture]
public class FlagsEnumValidatorTests
{
    [Test]
    public void IsDefinedCombination_None_IsValid()
    {
        Assert.That(FlagsEnumValidator.IsDefinedCombination(ContentKindFlags.None), Is.True);
    }

    [Test]
    public void IsDefinedCombination_SingleDefinedFlag_IsValid()
    {
        Assert.That(FlagsEnumValidator.IsDefinedCombination(ContentKindFlags.File), Is.True);
    }

    [Test]
    public void IsDefinedCombination_CombinationOfDefinedFlags_IsValid()
    {
        // 11010 binary = Blog(00010) | CustomPage(01000) | PageBlock(10000)
        var combo = ContentKindFlags.Blog | ContentKindFlags.CustomPage | ContentKindFlags.PageBlock;
        Assert.That((int)combo, Is.EqualTo(0b11010));
        Assert.That(FlagsEnumValidator.IsDefinedCombination(combo), Is.True);
    }

    [Test]
    public void IsDefinedCombination_BitOutsideDefinedRange_IsInvalid()
    {
        // 1 << 5 is one bit past PageBlock (1 << 4) — no member defines it.
        Assert.That(FlagsEnumValidator.IsDefinedCombination((ContentKindFlags)(1 << 5)), Is.False);
    }

    [Test]
    public void IsDefinedCombination_MixOfDefinedAndUndefinedBits_IsInvalid()
    {
        var value = (ContentKindFlags)((int)ContentKindFlags.File | (1 << 10));
        Assert.That(FlagsEnumValidator.IsDefinedCombination(value), Is.False);
    }

    [Test]
    public void IsDefinedCombination_NegativeValue_IsInvalid()
    {
        Assert.That(FlagsEnumValidator.IsDefinedCombination((ContentKindFlags)(-1)), Is.False);
    }

    // Proves this isn't hand-wired to ContentKindFlags specifically — it re-derives the mask
    // from whichever [Flags] enum it's asked about, via reflection, every time.
    [Test]
    public void IsDefinedCombination_WorksForAnyFlagsEnum_NotJustContentKindFlags()
    {
        var combo = ContentCapabilities.View | ContentCapabilities.Share;
        Assert.That(FlagsEnumValidator.IsDefinedCombination(combo), Is.True);
        Assert.That(FlagsEnumValidator.IsDefinedCombination((ContentCapabilities)(1 << 10)), Is.False);
    }
}
