using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using MathNet.Numerics.LinearAlgebra;

namespace GPC.Geometry
{
    /// <summary>
    /// A right handed orthonormal coordinate system: origin and unit axes V1 (X), V2 (Y), V3 (Z). It moves points, vectors, lines,
    /// polygons and shapes between the global system and the local one
    /// </summary>
    [Serializable]
    public class CoordinateSystem : GeometryBase, ISerializable, ICloneable, IEquatable<CoordinateSystem>
    {
        #region Variables

        /// <summary>
        /// The origin, in global coordinates
        /// </summary>
        protected Point3d _origin;
        /// <summary>
        /// The unit X axis, in global coordinates
        /// </summary>
        protected Vector3d _v1;
        /// <summary>
        /// The unit Y axis, in global coordinates
        /// </summary>
        protected Vector3d _v2;
        /// <summary>
        /// The unit Z axis, in global coordinates
        /// </summary>
        protected Vector3d _v3;
        /// <summary>
        /// The local coordinates of the global origin: minus the origin projected on the axes (the translation of <see cref="ToLocal(Point3d)"/>)
        /// </summary>
        protected Vector3d _InvOrigin;
        /// <summary>
        /// The transformation matrix from local to global coordinates (see <see cref="TrfMatrix"/>)
        /// </summary>
        protected Matrix<double> _trfMatrix;
        /// <summary>
        /// The name of the coordinate system
        /// </summary>
        protected string _name;

        /// <summary>
        /// The components of the axes (the transformations read these fields instead of the indexer of the MathNet matrix: about 10 times faster)
        /// </summary>
        [NonSerialized] private double _v1x, _v1y, _v1z, _v2x, _v2y, _v2z, _v3x, _v3y, _v3z;
        /// <summary>
        /// The components of the origin and of the inverse origin (see <see cref="_InvOrigin"/>)
        /// </summary>
        [NonSerialized] private double _ox, _oy, _oz, _ix, _iy, _iz;

        #endregion

        #region Properties

        /// <summary>
        /// The origin, in global coordinates
        /// </summary>
        public Point3d Origin => _origin;

        /// <summary>
        /// The unit X axis, in global coordinates
        /// </summary>
        public Vector3d V1 => _v1;

        /// <summary>
        /// The unit Y axis, in global coordinates
        /// </summary>
        public Vector3d V2 => _v2;

        /// <summary>
        /// The unit Z axis, in global coordinates
        /// </summary>
        public Vector3d V3 => _v3;

        /// <summary>
        /// Trasformation matrix from this coordinate system to global coordinate system.
        /// </summary>
        /// <remarks>Size of the matrix is 3 x 4. Where the 4th column rapresent the origin of the reference system </remarks>
        public Matrix<double> TrfMatrix => _trfMatrix;

        /// <summary>
        /// The name of the coordinate system
        /// </summary>
        public string Name => _name;

        /// <summary>
        /// A new global coordinate system: origin (0, 0, 0) and the axes X, Y, Z
        /// </summary>
        public static CoordinateSystem Global => new CoordinateSystem(Point3d.Origin, Vector3d.XAxis, Vector3d.YAxis, "Global coordinate system");

		#endregion

		#region Public Constructors

		/// <summary>
		/// Create a new right handed Coordinate System
		/// </summary>
		/// <param name="p1">The origin</param>
		/// <param name="p2">A point on the X axes</param>
		/// <param name="p3">A point on the Y axes</param>
		/// <param name="name">The name of the CS</param>
		public CoordinateSystem(Point3d p1, Point3d p2, Point3d p3, string name = "")
            : this(p1, p2, p3, name, Guid.Empty)
        {

        }

		/// <summary>
		/// Create a new right handed Coordinate System: X along <paramref name="v1"/>, Z along <paramref name="v1"/> × <paramref name="v2"/>
		/// </summary>
		/// <param name="origin">The origin point</param>
		/// <param name="v1">The X axis vector (copied, then unitized; the argument is not modified)</param>
		/// <param name="v2">The Y axis vector, orthogonal to <paramref name="v1"/> (copied, then unitized; the argument is not modified)</param>
		/// <param name="name">The name of the CS</param>
		/// <param name="tolerance">The angular tolerance of the orthogonality check</param>
		/// <exception cref="ArgumentException">If the vectors are not orthogonal</exception>
		public CoordinateSystem(Point3d origin, Vector3d v1, Vector3d v2, string name = "", double tolerance = GeometryBase.AngularTolerance)
            : this(origin, v1, v2, v1.CrossProduct(v2), name, Guid.Empty, tolerance)
        {

        }

        /// <summary>
        /// Create a new Coordinate System
        /// </summary>
        /// <param name="origin">The origin point</param>
        /// <param name="v1">The X axis vector (copied, then unitized; the argument is not modified)</param>
        /// <param name="v2">The Y axis vector (copied, then unitized; the argument is not modified)</param>
        /// <param name="v3">The Z axis vector (copied, then unitized; the argument is not modified)</param>
        /// <param name="name">The name of the CS</param>
        /// <param name="tolerance">The angular tolerance of the orthogonality check</param>
        /// <remarks>The 3 vector must be orthogonal (the check is on v1 - v2 and v1 - v3)</remarks>
        /// <exception cref="ArgumentException">If the vectors are not orthogonal</exception>
        public CoordinateSystem(Point3d origin, Vector3d v1, Vector3d v2, Vector3d v3, string name = "", double tolerance = GeometryBase.AngularTolerance)
            : this(origin, v1, v2, v3, name, Guid.Empty, tolerance)
        {

        }

        /// <summary>
        /// Creates a copy of a coordinate system (with a new <see cref="BaseObject.Guid"/>)
        /// </summary>
        /// <param name="coordinateSystem">The coordinate system to copy</param>
        public CoordinateSystem(CoordinateSystem coordinateSystem)
            :this(coordinateSystem.Origin, coordinateSystem.V1, coordinateSystem.V2, coordinateSystem.Name)
		{

		}

        /// <summary>
        /// Create a new right handed Coordinate System
        /// </summary>
        /// <param name="p1">The origin</param>
        /// <param name="p2">A point on the X axes</param>
        /// <param name="p3">A point on the Y axes</param>
        /// <param name="name">The name of the CS</param>
        /// <param name="guid">The Guid. <see cref="Guid.Empty"/>: generated when it is requested</param>
        protected CoordinateSystem(Point3d p1, Point3d p2, Point3d p3, string name, Guid guid)
        {
            _name = name;
            if (guid != Guid.Empty)
                SetGuid(guid);
            _trfMatrix = Matrix<double>.Build.Dense(3, 4, 0.0);

            SetOrigin(p1);

            Vector3d v1 = new Vector3d((p2.X - p1.X), (p2.Y - p1.Y), (p2.Z - p1.Z));
            v1.Unitize();
            Vector3d v2 = new Vector3d((p3.X - p1.X), (p3.Y - p1.Y), (p3.Z - p1.Z));
            v2.Unitize();
            v2 -= (v1 * v2) * v1;
            v2.Unitize();
            Vector3d v3 = v1 ^ v2;

            SetTransformationMatrix(v1, v2, v3);
        }

        /// <summary>
        /// Create a new right handed Coordinate System: X along <paramref name="v1"/>, Z along <paramref name="v1"/> × <paramref name="v2"/>
        /// </summary>
        /// <param name="origin">The origin point</param>
        /// <param name="v1">The X axis vector</param>
        /// <param name="v2">The Y axis vector, orthogonal to <paramref name="v1"/></param>
        /// <param name="name">The name of the CS</param>
        /// <param name="guid">The unique GUID. <see cref="Guid.Empty"/>: generated when it is requested</param>
        protected CoordinateSystem(Point3d origin, Vector3d v1, Vector3d v2, string name, Guid guid)
            : this(origin, v1, v2, v1.CrossProduct(v2), name, guid)
        {
        }

        /// <summary>
        /// Constructor for generic Coordinate System
        /// </summary>
        /// <param name="origin">The origin point</param>
        /// <param name="v1">The X axis vector (copied, then unitized; the argument is not modified)</param>
        /// <param name="v2">The Y axis vector (copied, then unitized; the argument is not modified)</param>
        /// <param name="v3">The Z axis vector (copied, then unitized; the argument is not modified)</param>
        /// <param name="name">The name of the CS</param>
        /// <param name="guid">The unique GUID. <see cref="Guid.Empty"/>: generated when it is requested</param>
        /// <param name="tolerance">The angular tolerance of the orthogonality check</param>
        /// <remarks>The 3 vector must be orthogonal (the check is on v1 - v2 and v1 - v3)</remarks>
        /// <exception cref="ArgumentException">If the vectors are not orthogonal</exception>
        protected CoordinateSystem(Point3d origin, Vector3d v1, Vector3d v2, Vector3d v3, string name, Guid guid, double tolerance = GeometryBase.AngularTolerance)
        {
            // Copies: the caller's vectors are neither modified nor shared (a later change of them would leave the cached
            // transformation inconsistent with V1, V2, V3).
            v1 = new Vector3d(v1.X, v1.Y, v1.Z);
            v2 = new Vector3d(v2.X, v2.Y, v2.Z);
            v3 = new Vector3d(v3.X, v3.Y, v3.Z);
            v1.Unitize();
            v2.Unitize();
            v3.Unitize();

            double tol = Math.Sqrt(Utilities.Maths.ErrorPropagation.ProductSquareTolerance(1, 1, tolerance, tolerance));

            if (Math.Abs((Math.Abs(v1.AngleTo(v2)) - Math.PI / 2.0)) > tol)
            {
                throw new ArgumentException("Base vectors are not ortogonals");
            }
            if (Math.Abs((Math.Abs(v1.AngleTo(v3)) - Math.PI / 2.0)) > tol)
            {
                throw new ArgumentException("Base vectors are not ortogonals");
            }

            _name = name;
            if (guid != Guid.Empty)
                SetGuid(guid);
            _trfMatrix = Matrix<double>.Build.Dense(3, 4, 0.0);
            SetOrigin(origin);
            SetTransformationMatrix(v1, v2, v3);
        }

        /// <summary>
        /// Deserialization constructor (the saved <see cref="BaseObject.Guid"/> is not read)
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        protected CoordinateSystem(SerializationInfo info, StreamingContext context)
            : base()
        {
            _origin = (Point3d)info.GetValue("Origin", typeof(Point3d));
            _v1 = (Vector3d)info.GetValue("V1Direction", typeof(Vector3d));
            _v2 = (Vector3d)info.GetValue("V2Direction", typeof(Vector3d));
            _v3 = (Vector3d)info.GetValue("V3Direction", typeof(Vector3d));
            _InvOrigin = (Vector3d)info.GetValue("InvariantOrigin", typeof(Vector3d));
            _trfMatrix = (Matrix<double>)info.GetValue("TransformationMatrix", typeof(Matrix<double>));
            _name = info.GetString("Name");
            UpdateComponents();
        }

        #endregion

        #region Public Methods Specific

        /// <summary>
        /// Sets the origin (a copy of <paramref name="origin"/>) and updates the transformation
        /// </summary>
        /// <param name="origin">The new origin, in global coordinates</param>
        public virtual void SetOrigin(Point3d origin)
        {
            _trfMatrix[0, 3] = origin.X;
            _trfMatrix[1, 3] = origin.Y;
            _trfMatrix[2, 3] = origin.Z;
            _origin = new Point3d(origin.X, origin.Y, origin.Z);

            // the inverse origin must follow the origin (before, it was updated only with the axes: ToLocal used the old origin)
            if (_v1 != null && _v2 != null && _v3 != null)
                SetInverseOrigin();
        }

        /// <summary>
        /// Computes <see cref="_InvOrigin"/> from the origin and the axes
        /// </summary>
        private void SetInverseOrigin()
        {
            _InvOrigin = new Vector3d(-(_trfMatrix[0, 0] * Origin.X + _trfMatrix[1, 0] * Origin.Y + _trfMatrix[2, 0] * Origin.Z),
                                      -(_trfMatrix[0, 1] * Origin.X + _trfMatrix[1, 1] * Origin.Y + _trfMatrix[2, 1] * Origin.Z),
                                      -(_trfMatrix[0, 2] * Origin.X + _trfMatrix[1, 2] * Origin.Y + _trfMatrix[2, 2] * Origin.Z));
            UpdateComponents();
        }

        /// <summary>
        /// Copy the axes, the origin and the inverse origin in the fields used by the transformations
        /// </summary>
        private void UpdateComponents()
        {
            if (_v1 == null || _v2 == null || _v3 == null || _origin == null || _InvOrigin == null)
                return;

            _v1x = _v1.X; _v1y = _v1.Y; _v1z = _v1.Z;
            _v2x = _v2.X; _v2y = _v2.Y; _v2z = _v2.Z;
            _v3x = _v3.X; _v3y = _v3.Y; _v3z = _v3.Z;
            _ox = _origin.X; _oy = _origin.Y; _oz = _origin.Z;
            _ix = _InvOrigin.X; _iy = _InvOrigin.Y; _iz = _InvOrigin.Z;
        }

        /// <summary>
        /// Set the transformation matrix from 3 points: X towards <paramref name="p2"/>, Y towards <paramref name="p3"/> (made orthogonal to X)
        /// </summary>
        /// <param name="p1">The origin</param>
        /// <param name="p2">A point on the X axes</param>
        /// <param name="p3">A point on the XY plane, on the side of the positive Y</param>
        protected virtual void SetTransformationMatrix(Point3d p1, Point3d p2, Point3d p3)
        {
            SetOrigin(p1);

            Vector3d v1 = new Vector3d((p2.X - p1.X), (p2.Y - p1.Y), (p2.Z - p1.Z));
            v1.Unitize();
            Vector3d v2 = new Vector3d((p3.X - p1.X), (p3.Y - p1.Y), (p3.Z - p1.Z));
            v2.Unitize();
            v2 -= (v1 * v2) * v1;
            v2.Unitize();
            Vector3d v3 = v1 ^ v2;
            v3.Unitize();

            SetTransformationMatrix(v1, v2, v3);
        }

        /// <summary>
        /// Set the transformation matrix from 3 unit orthogonal vectors (the instances are kept)
        /// </summary>
        /// <param name="v1">X axis</param>
        /// <param name="v2">Y axis</param>
        /// <param name="v3">Z axis</param>
        protected virtual void SetTransformationMatrix(Vector3d v1, Vector3d v2, Vector3d v3)
        {
            _v1 = v1;
            _v2 = v2;
            _v3 = v3;

            _trfMatrix[0, 0] = v1.X;
            _trfMatrix[1, 0] = v1.Y;
            _trfMatrix[2, 0] = v1.Z;

            _trfMatrix[0, 1] = v2.X;
            _trfMatrix[1, 1] = v2.Y;
            _trfMatrix[2, 1] = v2.Z;

            _trfMatrix[0, 2] = v3.X;
            _trfMatrix[1, 2] = v3.Y;
            _trfMatrix[2, 2] = v3.Z;

            SetInverseOrigin();
        }

        /// <summary>
        /// Move the given point from global to local system
        /// </summary>
        /// <param name="point">Point in global coordinate system</param>
        /// <returns>The point mooved in the local system</returns>
        public virtual Point3d ToLocal(Point3d point)
        {
            return new Point3d(_v1x * point.X + _v1y * point.Y + _v1z * point.Z + _ix,
                               _v2x * point.X + _v2y * point.Y + _v2z * point.Z + _iy,
                               _v3x * point.X + _v3y * point.Y + _v3z * point.Z + _iz
                              );
        }

        /// <summary>
        /// Move the given vector3d from global to local system
        /// </summary>
        /// <param name="vector">Vector in global coordinate system</param>
        /// <returns>The vector moved in the local system</returns>
        public virtual Vector3d ToLocal(Vector3d vector)
        {
            return new Vector3d(_v1x * vector.X + _v1y * vector.Y + _v1z * vector.Z,
                                _v2x * vector.X + _v2y * vector.Y + _v2z * vector.Z,
                                _v3x * vector.X + _v3y * vector.Y + _v3z * vector.Z
                               );
        }

        /// <summary>
        /// Move a line from global to local system
        /// </summary>
        /// <param name="line">The line in global system</param>
        /// <returns>A new line in local system</returns>
        public virtual Line3d ToLocal(Line3d line)
        {
            return new Line3d(ToLocal(line.Start), ToLocal(line.End));
        }

        /// <summary>
        /// Move a polygon from global to local system, on the local XY plane
        /// </summary>
        /// <param name="polygon">The polygon in global system</param>
        /// <returns>A new polygon with the local X and Y of the vertices (the local Z is dropped)</returns>
        public virtual Polygon2d ToLocal(Polygon3d polygon)
        {
            Polygon2d polygonLocal = new Polygon2d();
            for (int i = 0; i < polygon.Count; i++)
            {
                polygonLocal.Add(ToLocal(polygon[i]));
            }
            return polygonLocal;
        }

        /// <summary>
        /// Move a shape from global to local system, on the local XY plane
        /// </summary>
        /// <param name="shape">The shape in global system</param>
        /// <returns>A new shape with the local X and Y of the vertices (the local Z is dropped)</returns>
        public virtual Shape2d ToLocal(Shape shape)
        {
            Polygon2d fill = ToLocal(shape.Fill);

            Polygon2d[] holes = null;
            if (shape.HasHoles)
            {
                holes = new Polygon2d[shape.Holes.Length];
                for (int i = 0; i < shape.Holes.Length; i++)
                {
                    holes[i] = ToLocal(shape.Holes[i]);
                }
            }

            Shape2d[] childs = null;
            if (shape.HasChilds)
            {
                childs = new Shape2d[shape.Childs.Length];
                for (int i = 0; i < shape.Childs.Length; i++)
                {
                    childs[i] = ToLocal(shape.Childs[i]);
                }
            }

            return new Shape2d(fill, holes, childs);
        }

        /// <summary>
        /// Move the given point from local to global system
        /// </summary>
        /// <param name="point">The point in local system</param>
        /// <returns>A new point in global system</returns>
        public virtual Point3d ToGlobal(Point3d point)
        {
            // same operations, in the same order, of origin + x v1 + y v2 + z v3, without the intermediate vectors
            return new Point3d(_ox + point.X * _v1x + point.Y * _v2x + point.Z * _v3x,
                               _oy + point.X * _v1y + point.Y * _v2y + point.Z * _v3y,
                               _oz + point.X * _v1z + point.Y * _v2z + point.Z * _v3z);
        }

        /// <summary>
        /// Move the given vector from local to global system
        /// </summary>
        /// <param name="vector">The vector in local system</param>
        /// <returns>A new vector in global system</returns>
        public virtual Vector3d ToGlobal(Vector3d vector)
        {
            // x v1 + y v2 + z v3 (before, the transposed matrix was allocated at every call)
            return new Vector3d(_v1x * vector.X + _v2x * vector.Y + _v3x * vector.Z,
                                _v1y * vector.X + _v2y * vector.Y + _v3y * vector.Z,
                                _v1z * vector.X + _v2z * vector.Y + _v3z * vector.Z);
        }

        /// <summary>
        /// Move the given line from local to global system
        /// </summary>
        /// <param name="line">The line in local system</param>
        /// <returns>A new line in global system</returns>
        public virtual Line3d ToGlobal(Line3d line)
        {
            return new Line3d(ToGlobal(line.Start), ToGlobal(line.End));
        }

        /// <summary>
        /// Move the given polygon from local to global system
        /// </summary>
        /// <param name="poly">The polygon in local system</param>
        /// <returns>A new polygon in global system</returns>
        public virtual Polygon3d ToGlobal(Polygon3d poly)
        {
            Polygon3d polygonGlobal = new Polygon3d();
            for (int i = 0; i < poly.Count; i++)
            {
                polygonGlobal.Add(ToGlobal(poly[i]));
            }
            return polygonGlobal;
        }

        /// <summary>
        /// Move the given shape from local to global system
        /// </summary>
        /// <param name="shape">The shape in local system</param>
        /// <returns>A new shape in global system</returns>
        public virtual Shape ToGlobal(Shape shape)
        {
            Polygon3d fill = ToGlobal(shape.Fill);

            Polygon3d[] holes = null;
            if (shape.HasHoles)
            {
                holes = new Polygon3d[shape.Holes.Length];
                for (int i = 0; i < shape.Holes.Length; i++)
                {
                    holes[i] = ToGlobal(shape.Holes[i]);
                }
            }

            Shape[] childs = null;
            if (shape.HasChilds)
            {
                if (shape.Childs.Length > 0)
                {
                    childs = new Shape[shape.Childs.Length];
                    for (int i = 0; i < shape.Childs.Length; i++)
                    {
                        childs[i] = ToGlobal(shape.Childs[i]);
                    }
                }
            }

            return new Shape(fill, holes, childs);

        }

        /// <summary>
        /// Rotate the axes of the coordinate system around X global axis (the origin does not change)
        /// </summary>
        /// <param name="angle">The angle rotation in radians</param>
        public void RotateX(double angle)
        {
            double sin = Math.Sin(angle);
            double cos = Math.Cos(angle);
            sin = sin > 1.0 ? 1.0 : sin;
            sin = sin < -1.0 ? -1.0 : sin;
            cos = cos > 1.0 ? 1.0 : cos;
            cos = cos < -1.0 ? -1.0 : cos;

            var trfMatrix = Matrix<double>.Build.Dense(3, 3, 0.0);

            trfMatrix[0, 0] = 1;
            trfMatrix[1, 0] = 0;
            trfMatrix[2, 0] = 0;

            trfMatrix[0, 1] = 0;
            trfMatrix[1, 1] = cos;
            trfMatrix[2, 1] = sin;

            trfMatrix[0, 2] = 0;
            trfMatrix[1, 2] = -sin;
            trfMatrix[2, 2] = cos;

            Vector3d v1 = new Vector3d((trfMatrix[0, 0] * _v1.X + trfMatrix[0, 1] * _v1.Y + trfMatrix[0, 2] * _v1.Z),
                                       (trfMatrix[1, 0] * _v1.X + trfMatrix[1, 1] * _v1.Y + trfMatrix[1, 2] * _v1.Z),
                                       (trfMatrix[2, 0] * _v1.X + trfMatrix[2, 1] * _v1.Y + trfMatrix[2, 2] * _v1.Z));

            Vector3d v2 = new Vector3d((trfMatrix[0, 0] * _v2.X + trfMatrix[0, 1] * _v2.Y + trfMatrix[0, 2] * _v2.Z),
                                       (trfMatrix[1, 0] * _v2.X + trfMatrix[1, 1] * _v2.Y + trfMatrix[1, 2] * _v2.Z),
                                       (trfMatrix[2, 0] * _v2.X + trfMatrix[2, 1] * _v2.Y + trfMatrix[2, 2] * _v2.Z));

            Vector3d v3 = new Vector3d((trfMatrix[0, 0] * _v3.X + trfMatrix[0, 1] * _v3.Y + trfMatrix[0, 2] * _v3.Z),
                                       (trfMatrix[1, 0] * _v3.X + trfMatrix[1, 1] * _v3.Y + trfMatrix[1, 2] * _v3.Z),
                                       (trfMatrix[2, 0] * _v3.X + trfMatrix[2, 1] * _v3.Y + trfMatrix[2, 2] * _v3.Z));
            _v1 = v1;
            _v1.Unitize();
            _v2 = v2;
            _v2.Unitize();
            _v3 = v3;
            _v3.Unitize();
            
            SetTransformationMatrix(_v1, _v2, _v3);
        }

        /// <summary>
        /// Rotate the axes of the coordinate system around Y global axis (the origin does not change)
        /// </summary>
        /// <param name="angle">The angle rotation in radians</param>
        public void RotateY(double angle)
        {
            double sin = Math.Sin(angle);
            double cos = Math.Cos(angle);
            sin = sin > 1.0 ? 1.0 : sin;
            sin = sin < -1.0 ? -1.0 : sin;
            cos = cos > 1.0 ? 1.0 : cos;
            cos = cos < -1.0 ? -1.0 : cos;

            var trfMatrix = Matrix<double>.Build.Dense(3, 3, 0.0);

            trfMatrix[0, 0] = cos;
            trfMatrix[1, 0] = 0;
            trfMatrix[2, 0] = -sin;
            
            trfMatrix[0, 1] = 0;
            trfMatrix[1, 1] = 1;
            trfMatrix[2, 1] = 0;
            
            trfMatrix[0, 2] = sin;
            trfMatrix[1, 2] = 0;
            trfMatrix[2, 2] = cos;

            Vector3d v1 = new Vector3d((trfMatrix[0, 0] * _v1.X + trfMatrix[0, 1] * _v1.Y + trfMatrix[0, 2] * _v1.Z),
                                       (trfMatrix[1, 0] * _v1.X + trfMatrix[1, 1] * _v1.Y + trfMatrix[1, 2] * _v1.Z),
                                       (trfMatrix[2, 0] * _v1.X + trfMatrix[2, 1] * _v1.Y + trfMatrix[2, 2] * _v1.Z));

            Vector3d v2 = new Vector3d((trfMatrix[0, 0] * _v2.X + trfMatrix[0, 1] * _v2.Y + trfMatrix[0, 2] * _v2.Z),
                                       (trfMatrix[1, 0] * _v2.X + trfMatrix[1, 1] * _v2.Y + trfMatrix[1, 2] * _v2.Z),
                                       (trfMatrix[2, 0] * _v2.X + trfMatrix[2, 1] * _v2.Y + trfMatrix[2, 2] * _v2.Z));

            Vector3d v3 = new Vector3d((trfMatrix[0, 0] * _v3.X + trfMatrix[0, 1] * _v3.Y + trfMatrix[0, 2] * _v3.Z),
                                       (trfMatrix[1, 0] * _v3.X + trfMatrix[1, 1] * _v3.Y + trfMatrix[1, 2] * _v3.Z),
                                       (trfMatrix[2, 0] * _v3.X + trfMatrix[2, 1] * _v3.Y + trfMatrix[2, 2] * _v3.Z));
            _v1 = v1;
            _v1.Unitize();
            _v2 = v2;
            _v2.Unitize();
            _v3 = v3;
            _v3.Unitize();

            SetTransformationMatrix(_v1, _v2, _v3);
        }

        /// <summary>
        /// Rotate the axes of the coordinate system around Z global axis (the origin does not change)
        /// </summary>
        /// <param name="angle">The angle rotation in radians</param>
        public void RotateZ(double angle)
        {
            double sin = Math.Sin(angle);
            double cos = Math.Cos(angle);
            sin = sin > 1.0 ? 1.0 : sin;
            sin = sin < -1.0 ? -1.0 : sin;
            cos = cos > 1.0 ? 1.0 : cos;
            cos = cos < -1.0 ? -1.0 : cos;

            var trfMatrix = Matrix<double>.Build.Dense(3, 3, 0.0);

            trfMatrix[0, 0] = cos;
            trfMatrix[1, 0] = sin;
            //trfMatrix[2, 0] = 0;

            trfMatrix[0, 1] = -sin;
            trfMatrix[1, 1] = cos;
            //trfMatrix[2, 1] = 0;

            //trfMatrix[0, 2] = 0;
            //trfMatrix[1, 2] = 0;
            trfMatrix[2, 2] = 1;

            Vector3d v1 = new Vector3d((trfMatrix[0, 0] * _v1.X + trfMatrix[0, 1] * _v1.Y + trfMatrix[0, 2] * _v1.Z),
                                       (trfMatrix[1, 0] * _v1.X + trfMatrix[1, 1] * _v1.Y + trfMatrix[1, 2] * _v1.Z),
                                       (trfMatrix[2, 0] * _v1.X + trfMatrix[2, 1] * _v1.Y + trfMatrix[2, 2] * _v1.Z));

            Vector3d v2 = new Vector3d((trfMatrix[0, 0] * _v2.X + trfMatrix[0, 1] * _v2.Y + trfMatrix[0, 2] * _v2.Z),
                                       (trfMatrix[1, 0] * _v2.X + trfMatrix[1, 1] * _v2.Y + trfMatrix[1, 2] * _v2.Z),
                                       (trfMatrix[2, 0] * _v2.X + trfMatrix[2, 1] * _v2.Y + trfMatrix[2, 2] * _v2.Z));

            Vector3d v3 = new Vector3d((trfMatrix[0, 0] * _v3.X + trfMatrix[0, 1] * _v3.Y + trfMatrix[0, 2] * _v3.Z),
                                       (trfMatrix[1, 0] * _v3.X + trfMatrix[1, 1] * _v3.Y + trfMatrix[1, 2] * _v3.Z),
                                       (trfMatrix[2, 0] * _v3.X + trfMatrix[2, 1] * _v3.Y + trfMatrix[2, 2] * _v3.Z));
            _v1 = v1;
            _v1.Unitize();
            _v2 = v2;
            _v2.Unitize();
            _v3 = v3;
            _v3.Unitize();

            SetTransformationMatrix(_v1, _v2, _v3);
        }

        /// <summary>
        /// Rotate the axes of the coordinate system around the global axes: first X, then Y, then Z
        /// </summary>
        /// <param name="rotateX">The angle rotation  around X-axis in radians</param>
        /// <param name="rotateY">The angle rotation  around Y-axis in radians</param>
        /// <param name="rotateZ">The angle rotation  around Z-axis in radians</param>
        public void Rotate(double rotateX, double rotateY, double rotateZ)
        {
            RotateX(rotateX);
            RotateY(rotateY);
            RotateZ(rotateZ);
        }

        /// <summary>
        /// Rotate the coordinate system around V1 local axis.
        /// </summary>
        /// <param name="angle">The angle rotation in radians</param>
        public void RotateV1(double angle)
        {
            RotateAroundAxis(new Vector3d(_v1), angle);
        }

        /// <summary>
        /// Rotate the coordinate system around V2 local axis.
        /// </summary>
        /// <param name="angle">The angle rotation in radians</param>
        public void RotateV2(double angle)
        {
            RotateAroundAxis(new Vector3d(_v2), angle);
        }

        /// <summary>
        /// Rotate the coordinate system around V3 local axis.
        /// </summary>
        /// <param name="angle">The angle rotation in radians</param>
        public void RotateV3(double angle)
        {
            RotateAroundAxis(new Vector3d(_v3), angle);
        }

        /// <summary>
        /// Rotate the axes of the coordinate system around the given axis passing through the origin (Rodrigues' rotation formula)
        /// </summary>
        /// <param name="axis">The rotation axis in global coordinates</param>
        /// <param name="angle">The angle rotation in radians, counterclockwise looking from the tip of <paramref name="axis"/></param>
        private void RotateAroundAxis(Vector3d axis, double angle)
        {
            axis.Unitize();
            double cos = Math.Cos(angle);
            double sin = Math.Sin(angle);

            Vector3d Rotate(Vector3d v)
            {
                // v cos + (k x v) sin + k (k . v)(1 - cos)
                Vector3d rotated = v * cos + axis.CrossProduct(v) * sin + axis * (axis.DotProduct(v) * (1.0 - cos));
                rotated.Unitize();
                return rotated;
            }

            SetTransformationMatrix(Rotate(_v1), Rotate(_v2), Rotate(_v3));
        }

        /// <summary>
        /// Not implemented: use <see cref="SetOrigin"/>
        /// </summary>
        /// <param name="v1">The translation along X</param>
        /// <param name="v2">The translation along Y</param>
        /// <param name="v3">The translation along Z</param>
        /// <exception cref="NotImplementedException">Always</exception>
        public override void Move(double v1, double v2, double v3)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Not implemented: use <see cref="SetOrigin"/>
        /// </summary>
        /// <param name="vector">The translation</param>
        /// <exception cref="NotImplementedException">Always</exception>
        public override void Move(Vector3d vector)
        {
            throw new NotImplementedException();
        }

		/// <summary>
		/// Creates a copy of the coordinate system (see the copy constructor)
		/// </summary>
		/// <returns>The copy</returns>
		public override object Clone()
		{
            return new CoordinateSystem(this);
		}

        /// <summary>
        /// Serializes the coordinate system: the <see cref="BaseObject.Guid"/>, the origin, the axes, the matrix and the name
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("Origin", _origin);
            info.AddValue("V1Direction", _v1);
            info.AddValue("V2Direction", _v2);
            info.AddValue("V3Direction", _v3);
            info.AddValue("InvariantOrigin", _InvOrigin);
            info.AddValue("TransformationMatrix", _trfMatrix);
            info.AddValue("Name", _name);
        }

        /// <summary>
        /// Equality of origin, axes and name within the tolerance (<see cref="Equals(CoordinateSystem)"/> does not compare the name)
        /// </summary>
        /// <param name="obj">The object to compare</param>
        /// <returns>True if <paramref name="obj"/> is an equal coordinate system</returns>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
                return true;

            if (obj is null)
                return false;

            return obj is CoordinateSystem system &&
                   EqualityComparer<Point3d>.Default.Equals(_origin, system._origin) &&
                   EqualityComparer<Vector3d>.Default.Equals(_v1, system._v1) &&
                   EqualityComparer<Vector3d>.Default.Equals(_v2, system._v2) &&
                   EqualityComparer<Vector3d>.Default.Equals(_v3, system._v3) &&
                   EqualityComparer<Vector3d>.Default.Equals(_InvOrigin, system._InvOrigin) &&
                   _name == system._name;
        }

        /// <summary>
        /// The hash code of the exact origin, axes and name
        /// </summary>
        /// <returns>The hash code</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = -23;
                hashCode = hashCode * -17 + base.GetHashCode();
                hashCode = hashCode * -17 + EqualityComparer<Point3d>.Default.GetHashCode(_origin);
                hashCode = hashCode * -17 + EqualityComparer<Vector3d>.Default.GetHashCode(_v1);
                hashCode = hashCode * -17 + EqualityComparer<Vector3d>.Default.GetHashCode(_v2);
                hashCode = hashCode * -17 + EqualityComparer<Vector3d>.Default.GetHashCode(_v3);
                hashCode = hashCode * -17 + EqualityComparer<Vector3d>.Default.GetHashCode(_InvOrigin);
                // Typed equality ignores the name, so it cannot contribute to the hash.
                return hashCode;
            }
        }

		/// <summary>
		/// Equality of origin and axes within the tolerance (the name is not compared)
		/// </summary>
		/// <param name="other">The coordinate system to compare</param>
		/// <returns>True if the coordinate systems are equal</returns>
		public bool Equals(CoordinateSystem other)
		{
            if (ReferenceEquals(this, other))
                return true;

            return !(other is null) && other._origin.Equals(_origin) && other._v1.Equals(_v1) && other._v2.Equals(_v2) && other._v3.Equals(_v3);
        }

        /// <summary>
        /// Equality with another geometry (see <see cref="Equals(CoordinateSystem)"/>)
        /// </summary>
        /// <param name="geometryBase">The geometry to compare</param>
        /// <returns>True if <paramref name="geometryBase"/> is an equal coordinate system</returns>
        public override bool Equals(GeometryBase geometryBase)
        {
            if (geometryBase is CoordinateSystem cs)
                return Equals(cs);

            return false;
        }

        #endregion
    }
}
