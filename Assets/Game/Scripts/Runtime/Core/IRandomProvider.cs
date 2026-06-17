namespace VertigoWheel.Core
{
    /// <summary>
    /// Abstraction over a random number source so gameplay logic can be unit tested
    /// deterministically (Dependency Inversion). Production code uses <see cref="UnityRandomProvider"/>,
    /// tests inject <see cref="SeededRandomProvider"/>.
    /// </summary>
    public interface IRandomProvider
    {
        /// <summary>Returns an integer in the range [minInclusive, maxExclusive).</summary>
        int Range(int minInclusive, int maxExclusive);

        /// <summary>Returns a float in the range [0, 1).</summary>
        float Value01();
    }
}
