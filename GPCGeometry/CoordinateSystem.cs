using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using MathNet.Numerics.LinearAlgebra;

namespace GPC.Geometry
{
    [Serializable]
    public class CoordinateSystem : GeometryBase, ISerializable, ICloneable, IEquatable<CoordinateSystem>
    {
        #region Variables

        protected Point3d _origin;
        protected Vector3d _v1;
        protected Vector3d _v2;
        protected Vector3d _v3;
        protected Vector3d _InvOrigin;
        protected Matrix<double> _trfMatrix;
        protected string _name;

        #endregion

        #region Properties

        public Point3d Origin => _origin;

        public Vector3d V1 => _v1;

        public Vector3d V2 => _v2;

        public Vector3d V3 => _v3;

        /// <summary>
        /// Trasformation matrix from this coordinate system to global coordinate system.
        /// </summary>
        /// <remarks>Size of the matrix is 3 x 4. Where the 4th column rapresent the origin of the reference system </remarks>
        public Matrix<double> TrfMatrix => _trfMatrix;

        public string Name => _name;

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
		/// Create a new right handed Coordinate System
		/// </summary>
		/// <param name="origin">The origin point</param>
		/// <param name="v1">The X axis vector</param>
		/// <param name="v2">The Y axis vector</param>
		/// <param name="name"></param>
		/// <param name="tolerance"></param>
		public CoordinateSystem(Point3d origin, Vector3d v1, Vector3d v2, string name = "", double tolerance = GeometryBase.AngularTolerance)
            : this(origin, v1, v2, v1.CrossProduct(v2), name, Guid.Empty, tolerance)
        {

        }

        /// <summary>
        /// Create a new Coordinate System
        /// </summary>
        /// <param name="origin">The origin point</param>
        /// <param name="v1">The X axis vector</param>
        /// <param name="v2">The Y axis vector</param>
        /// <param name="v3">The Z axis vector</param>
        /// <param name="name">The name of the CS</param>
        /// <param name="tolerance">The tolerance</param>
        /// <remarks>The 3 vector must be ortogonals</remarks>
        public CoordinateSystem(Point3d origin, Vector3d v1, Vector3d v2, Vector3d v3, string name = "", double tolerance = GeometryBase.AngularTolerance)
            : this(origin, v1, v2, v3, name, Guid.Empty, tolerance)
        {

        }

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
        /// Create a new right handed Coordinate System
        /// </summary>
        /// <param name="origin">The origin point</param>
        /// <param name="v1">The X axis vector</param>
        /// <param name="v2">The Y axis vector</param>
        /// <param name="name">The name of the CS</param>
        /// <param name="guid">The unique GUID</param>
        protected CoordinateSystem(Point3d origin, Vector3d v1, Vector3d v2, string name, Guid guid)
            : this(origin, v1, v2, v1.CrossProduct(v2), name, guid)
        {
        }

        /// <summary>
        /// Constructor for generic Coordinate System 
        /// </summary>
        /// <param name="origin">The origin point</param>
        /// <param name="v1">The X axis vector</param>
        /// <param name="v2">The Y axis vector</param>
        /// <param name="v3">The Z axis vector</param>
        /// <param name="name">The name of the CS</param>
        /// <param name="guid">The unique GUID. <see cref="Guid.Empty"/>: generated when it is requested</param>
        /// <param name="tolerance">The tolerance</param>
        /// <remarks>The 3 vector must be ortogonals</remarks>
        protected CoordinateSystem(Point3d origin, Vector3d v1, Vector3d v2, Vector3d v3, string name, Guid guid, double tolerance = GeometryBase.AngularTolerance)
        {
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
        }

        #endregion

        #region Public Methods Specific

        public virtual void SetOrigin(Point3d origin)
        {
            _trfMatrix[0, 3] = origin.X;
            _trfMatrix[1, 3] = origin.Y;
            _trfMatrix[2, 3] = origin.Z;
            _origin = new Point3d(origin.X, origin.Y, origin.Z);
        }

        /// <summary>
        /// Set the trasformation matrix from 3 points
        /// </summary>
        /// <param name="p1">The origin</param>
        /// <param name="p2">A point on the X axes</param>
        /// <param name="p3">A point on the Y axes</param>
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
        /// Set the trasformation matrix from 3 vectors
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

            _InvOrigin = new Vector3d(-(_trfMatrix[0, 0] * Origin.X + _trfMatrix[1, 0] * Origin.Y + _trfMatrix[2, 0] * Origin.Z),
                                      -(_trfMatrix[0, 1] * Origin.X + _trfMatrix[1, 1] * Origin.Y + _trfMatrix[2, 1] * Origin.Z),
                                      -(_trfMatrix[0, 2] * Origin.X + _trfMatrix[1, 2] * Origin.Y + _trfMatrix[2, 2] * Origin.Z));
        }

        /// <summary>
        /// Move the given point from global to local system
        /// </summary>
        /// <param name="point">Point in global coordinate system</param>
        /// <returns>The point mooved in the local system</returns>
        public virtual Point3d ToLocal(Point3d point)
        {
            return new Point3d(_trfMatrix[0, 0] * point.X + _trfMatrix[1, 0] * point.Y + _trfMatrix[2, 0] * point.Z + _InvOrigin.X,
                               _trfMatrix[0, 1] * point.X + _trfMatrix[1, 1] * point.Y + _trfMatrix[2, 1] * point.Z + _InvOrigin.Y,
                               _trfMatrix[0, 2] * point.X + _trfMatrix[1, 2] * point.Y + _trfMatrix[2, 2] * point.Z + _InvOrigin.Z
                              );
        }

        /// <summary>
        /// Move the given vector3d from global to local system
        /// </summary>
        /// <param name="vector">Vector in global coordinate system</param>
        /// <returns>The vector moved in the local system</returns>
        public virtual Vector3d ToLocal(Vector3d vector)
        {
            return new Vector3d(_trfMatrix[0, 0] * vector.X + _trfMatrix[1, 0] * vector.Y + _trfMatrix[2, 0] * vector.Z,
                                _trfMatrix[0, 1] * vector.X + _trfMatrix[1, 1] * vector.Y + _trfMatrix[2, 1] * vector.Z,
                                _trfMatrix[0, 2] * vector.X + _trfMatrix[1, 2] * vector.Y + _trfMatrix[2, 2] * vector.Z
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
        /// Move a polygon from global to local system
        /// </summary>
        /// <param name="polygon">The polygon in global system</param>
        /// <returns>The poligon in local system</returns>
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
        /// Move a shape from glocal to local system
        /// </summary>
        /// <param name="shape">The shape in global system</param>
        /// <returns>A new shape in local system</returns>
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
            return _origin + point.X * _v1 + point.Y * _v2 + point.Z * _v3;
        }

        /// <summary>
        /// Move the given vector from local to global system
        /// </summary>
        /// <param name="vector">The vector in local system</param>
        /// <returns>A new vector in global system</returns>
        public virtual Vector3d ToGlobal(Vector3d vector)
        {
            var transposed = _trfMatrix.Transpose();

            return new Vector3d(transposed[0, 0] * vector.X + transposed[1, 0] * vector.Y + transposed[2, 0] * vector.Z,
                                  transposed[0, 1] * vector.X + transposed[1, 1] * vector.Y + transposed[2, 1] * vector.Z,
                                  transposed[0, 2] * vector.X + transposed[1, 2] * vector.Y + transposed[2, 2] * vector.Z
                                  );

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
        /// Rotate the coordinate system around X global axis.
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
        /// Rotate the coordinate system around Y global axis.
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
        /// Rotate the coordinate system around Z global axis.
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
        /// Rotate the coordinate system 
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

        public override void Move(double v1, double v2, double v3)
        {
            throw new NotImplementedException();
        }

        public override void Move(Vector3d vector)
        {
            throw new NotImplementedException();
        }

		public override object Clone()
		{
            return new CoordinateSystem(this);
		}

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
                hashCode = hashCode * -17 + EqualityComparer<string>.Default.GetHashCode(_name);
                return hashCode;
            }
        }

		public bool Equals(CoordinateSystem other)
		{
            if (ReferenceEquals(this, other))
                return true;

            return !(other is null) && other._origin.Equals(_origin) && other._v1.Equals(_v1) && other._v2.Equals(_v2) && other._v3.Equals(_v3);
        }

        public override bool Equals(GeometryBase geometryBase)
        {
            if (geometryBase is CoordinateSystem cs)
                return Equals(cs);

            return false;
        }

        #endregion
    }
}
