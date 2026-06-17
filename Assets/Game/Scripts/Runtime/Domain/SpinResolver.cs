using VertigoWheel.Config;
using VertigoWheel.Core;

namespace VertigoWheel.Domain
{
    /// <summary>Result of a spin: which chamber the wheel stopped on and whether it was the bomb.</summary>
    public readonly struct SpinOutcome
    {
        public readonly int SliceIndex;
        public readonly WheelSlice Slice;

        public SpinOutcome(int sliceIndex, WheelSlice slice)
        {
            SliceIndex = sliceIndex;
            Slice = slice;
        }

        public bool IsBomb => Slice != null && Slice.IsBomb;
    }

    public interface ISpinResolver
    {
        SpinOutcome Resolve(WheelDefinition wheel);
    }

    /// <summary>
    /// Picks the landing chamber for a spin. Uniform across slices and driven by an injected
    /// <see cref="IRandomProvider"/> so outcomes are reproducible in tests. Weighted odds could be
    /// added here without touching any caller (Open/Closed).
    /// </summary>
    public sealed class SpinResolver : ISpinResolver
    {
        private readonly IRandomProvider _random;

        public SpinResolver(IRandomProvider random) => _random = random;

        public SpinOutcome Resolve(WheelDefinition wheel)
        {
            int index = _random.Range(0, wheel.Count);
            return new SpinOutcome(index, wheel.GetSlice(index));
        }
    }
}
