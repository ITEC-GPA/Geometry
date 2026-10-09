using System;

namespace GPC.Geometry
{
    /// <summary>Shared parameter lookup for piecewise curves. All lookups are logarithmic.</summary>
    internal static class CurvePartition
    {
        internal static double[] Uniform(int count)
        {
            var knots = new double[count + 1];
            for (int i = 0; i <= count; i++) knots[i] = (double)i / count;
            return knots;
        }
        internal static double[] CopyKnots(double[] knots, int segmentCount)
        {
            if (knots == null || knots.Length != segmentCount + 1 || knots[0] != 0 || knots[segmentCount] != 1)
                throw new ArgumentException("Invalid curve parameter partition.", nameof(knots));
            for (int i = 1; i < knots.Length; i++)
                if (!CurveMath.IsFinite(knots[i]) || knots[i] <= knots[i - 1])
                    throw new ArgumentException("Curve parameters must increase strictly.", nameof(knots));
            return (double[])knots.Clone();
        }
        internal static int Find(double[] breaks, double value, CurveEvaluationSide side = CurveEvaluationSide.Automatic)
        {
            CurveMath.Side(side);
            int index = Array.BinarySearch(breaks, value);
            if (index < 0) index = ~index - 1;
            else if (side == CurveEvaluationSide.Below) index--;
            return Math.Max(0, Math.Min(breaks.Length - 2, index));
        }
        internal static double Fraction(double[] knots, int index, double value)
            => Math.Max(0, Math.Min(1, (value - knots[index]) / (knots[index + 1] - knots[index])));
        internal static double[] Reverse(double[] knots)
        {
            var result = new double[knots.Length];
            for (int i = 0; i < result.Length; i++) result[i] = 1 - knots[result.Length - 1 - i];
            return result;
        }
    }
}
