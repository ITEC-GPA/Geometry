using System.Collections.Generic;

namespace GPC.Geometry
{
    /// <summary>
    /// Comparer for the keys made of two integers ((long)a &lt;&lt; 32 | b): the default hash of a long is a ^ b, which gives a lot of
    /// collisions for the ids of the vertices of a mesh (every operation of a HashSet or Dictionary about 100 times slower).
    /// The key is mixed with the Fibonacci multiplicative hash
    /// </summary>
    internal sealed class LongKeyComparer : IEqualityComparer<long>
    {
        /// <summary>
        /// The shared instance (the comparer has no state)
        /// </summary>
        public static readonly LongKeyComparer Instance = new LongKeyComparer();

        /// <summary>
        /// Equality of two keys
        /// </summary>
        /// <param name="x">The first key</param>
        /// <param name="y">The second key</param>
        /// <returns>True if the keys are equal</returns>
        public bool Equals(long x, long y)
        {
            return x == y;
        }

        /// <summary>
        /// The hash code of a key: the high 32 bits of the key multiplied by 2^64 / golden ratio
        /// </summary>
        /// <param name="key">The key</param>
        /// <returns>The hash code</returns>
        public int GetHashCode(long key)
        {
            return (int)(unchecked((ulong)key * 0x9E3779B97F4A7C15UL) >> 32);
        }
    }
}
