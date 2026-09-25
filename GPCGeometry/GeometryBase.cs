using System;
using System.Runtime.Serialization;

namespace GPC.Geometry
{
    /// <summary>
    /// Base class of the geometries: default tolerances, translation, copy and equality within the tolerance
    /// </summary>
    [Serializable]
    public abstract class GeometryBase : BaseObject, ISerializable, ICloneable, IEquatable<GeometryBase>
    {
        /// <summary>
        /// The default tolerance on the distances (model units)
        /// </summary>
        public const double Tolerance = 1E-4;

        /// <summary>
        /// The default angular tolerance (radians)
        /// </summary>
        public const double AngularTolerance = 1E-4;

		#region Constructor 

		/// <summary>
		/// The Guid is generated only when it is requested (see <see cref="BaseObject.Guid"/>)
		/// </summary>
		protected GeometryBase()
            : base()
        {

        }

        /// <summary>
        /// Deserialization constructor
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        protected GeometryBase(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {

        }

		#endregion

		#region Methods

		/// <summary>
		/// Translates the geometry in place
		/// </summary>
		/// <param name="v1">The translation along X</param>
		/// <param name="v2">The translation along Y</param>
		/// <param name="v3">The translation along Z</param>
		public abstract void Move(double v1, double v2, double v3);

        /// <summary>
        /// Translates the geometry in place
        /// </summary>
        /// <param name="vector">The translation vector</param>
        public abstract void Move(Vector3d vector);

        /// <summary>
        /// Creates a copy of the geometry
        /// </summary>
        /// <returns>The copy</returns>
        public abstract object Clone();

        /// <summary>
        /// The default tolerance on the distances
        /// </summary>
        /// <returns><see cref="Tolerance"/></returns>
        public static double GetDefaultTolerance()
        {
            return Tolerance;
        }

        /// <summary>
        /// The default angular tolerance
        /// </summary>
        /// <returns><see cref="AngularTolerance"/></returns>
        public static double GetDefaultAngularTolerance()
        {
            return AngularTolerance;
        }

        #endregion

        /// <summary>
        /// Equality with another object: the derived geometries compare their coordinates within the tolerance
        /// </summary>
        /// <param name="obj">The object to compare</param>
        /// <returns>True if <paramref name="obj"/> is an equal geometry</returns>
        public abstract override bool Equals(object obj);

        /// <summary>
        /// A constant hash code: the equality is within a tolerance, so equal geometries must have the same hash code
        /// </summary>
        /// <returns>The hash code</returns>
        public override int GetHashCode()
        {
            return -23; 
        }

        /// <summary>
        /// Equality with another geometry (the derived geometries compare their coordinates within the tolerance)
        /// </summary>
        /// <param name="other">The geometry to compare</param>
        /// <returns>True if <paramref name="other"/> is equal to this geometry</returns>
        public abstract bool Equals(GeometryBase other);

		/// <summary>
		/// Equality of two geometries: true if they are the same object, both null or equal (<see cref="Equals(GeometryBase)"/>)
		/// </summary>
		/// <param name="obj1">The first geometry</param>
		/// <param name="obj2">The second geometry</param>
		/// <returns>True if the geometries are equal</returns>
		public static bool operator ==(GeometryBase obj1, GeometryBase obj2)
        {
            if (ReferenceEquals(obj1, obj2))
                return true;

            if (obj1 is null || obj2 is null)
                return false;

            return obj1.Equals(obj2);
        }

        /// <summary>
        /// Inequality of two geometries (see the equality operator)
        /// </summary>
        /// <param name="obj1">The first geometry</param>
        /// <param name="obj2">The second geometry</param>
        /// <returns>True if the geometries are different</returns>
        public static bool operator !=(GeometryBase obj1, GeometryBase obj2)
        {
            return !(obj1 == obj2);
        }
    }
}