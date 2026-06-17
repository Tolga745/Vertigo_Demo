using System;

namespace VertigoWheel.Core
{
    /// <summary>Default provider backed by UnityEngine.Random.</summary>
    public sealed class UnityRandomProvider : IRandomProvider
    {
        public int Range(int minInclusive, int maxExclusive)
            => UnityEngine.Random.Range(minInclusive, maxExclusive);

        public float Value01() => UnityEngine.Random.value;
    }

    /// <summary>
    /// Deterministic provider backed by <see cref="System.Random"/>. Used by tests and by anyone
    /// who needs reproducible spins.
    /// </summary>
    public sealed class SeededRandomProvider : IRandomProvider
    {
        private readonly Random _random;

        public SeededRandomProvider(int seed) => _random = new Random(seed);

        public int Range(int minInclusive, int maxExclusive)
            => _random.Next(minInclusive, maxExclusive);

        public float Value01() => (float)_random.NextDouble();
    }
}
