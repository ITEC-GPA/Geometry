using System;

namespace GPC.Geometry
{
    /// <summary>A finite, strictly increasing parameter interval. The default struct value is invalid.</summary>
    [Serializable]
    public readonly struct CurveInterval : IEquatable<CurveInterval>
    {
        public double Start { get; }
        public double End { get; }
        public double Length => End - Start;
        public bool IsValid => CurveMath.IsFinite(Start) && CurveMath.IsFinite(End)
            && End > Start && CurveMath.IsFinite(Length);

        public CurveInterval(double start, double end)
        {
            Start = start;
            End = end;
            if (!IsValid) throw new ArgumentException("A curve domain must be finite and strictly increasing.");
        }

        public double ParameterAt(double fraction)
        {
            if (!IsValid) throw new InvalidOperationException("Invalid curve domain.");
            CurveMath.InRange(fraction, 0, 1, nameof(fraction));
            return fraction == 1 ? End : Start + fraction * Length;
        }

        public double Normalize(double parameter)
        {
            if (!IsValid) throw new InvalidOperationException("Invalid curve domain.");
            CurveMath.InRange(parameter, Start, End, nameof(parameter));
            return (parameter - Start) / Length;
        }

        public bool Equals(CurveInterval other) => Start.Equals(other.Start) && End.Equals(other.End);
        public override bool Equals(object obj) => obj is CurveInterval other && Equals(other);
        public override int GetHashCode() => unchecked(Start.GetHashCode() * 397 ^ End.GetHashCode());
    }
}
