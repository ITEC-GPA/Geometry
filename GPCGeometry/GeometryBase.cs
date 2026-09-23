using System;
using System.Runtime.Serialization;

namespace GPC.Geometry
{
    [Serializable]
    public abstract class GeometryBase : BaseObject, ISerializable, ICloneable, IEquatable<GeometryBase>
    {
        public const double Tolerance = 1E-4;

        public const double AngularTolerance = 1E-4;

		#region Constructor 

		protected GeometryBase()
            : base(Guid.NewGuid())
        {

        }

        protected GeometryBase(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {

        }

		#endregion

		#region Methods

		public abstract void Move(double v1, double v2, double v3);

        public abstract void Move(Vector3d vector);

        public abstract object Clone();

        /// <returns>The default tolerance: <see cref="Tolerance"/></returns>
        public static double GetDefaultTolerance()
        {
            return Tolerance;
        }

        /// <returns>The default angular tolerance: <see cref="AngularTolerance"/></returns>
        public static double GetDefaultAngularTolerance()
        {
            return AngularTolerance;
        }

        #endregion

        public abstract override bool Equals(object obj);

        public override int GetHashCode()
        {
            return -23; 
        }

        public abstract bool Equals(GeometryBase other);

		public static bool operator ==(GeometryBase obj1, GeometryBase obj2)
        {
            if (ReferenceEquals(obj1, obj2))
                return true;

            if (obj1 is null || obj2 is null)
                return false;

            return obj1.Equals(obj2);
        }

        public static bool operator !=(GeometryBase obj1, GeometryBase obj2)
        {
            return !(obj1 == obj2);
        }
    }
}