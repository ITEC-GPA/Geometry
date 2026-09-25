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
        public static readonly LongKeyComparer Instance = new LongKeyComparer();

        public bool Equals(long x, long y)
        {
            return x == y;
        }

        public int GetHashCode(long key)
        {
            return (int)(unchecked((ulong)key * 0x9E3779B97F4A7C15UL) >> 32);
        }
    }
}
