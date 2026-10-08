using System;
using System.Collections.Generic;

namespace GPC.Geometry
{
    internal sealed class ExactCoordinateComparer<T> : IEqualityComparer<T> where T : class
    {
        private readonly Func<T, (double X, double Y, double Z)> _coordinates;

        internal ExactCoordinateComparer(Func<T, (double X, double Y, double Z)> coordinates)
        {
            _coordinates = coordinates;
        }

        public bool Equals(T x, T y) => ReferenceEquals(x, y) ||
            (!(x is null) && !(y is null) && _coordinates(x).Equals(_coordinates(y)));

        public int GetHashCode(T obj) => obj is null ? 0 : _coordinates(obj).GetHashCode();
    }
}
