using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace GPC.Geometry.Meshes
{
    [Serializable]
    [DebuggerDisplay("{" + nameof(GetDebuggerDisplay) + "(),nq}")]
    public sealed class MeshVertex : MeshBase, ISerializable, IEquatable<MeshVertex>, ICloneable
    {
        #region Variables

        private Point3d _point;
        public const int Unassigned = -1;

        #endregion

        #region Properties

        public Point3d Point => _point;

        #endregion

        #region Public Constructors

        public MeshVertex(Point3d point, object tag = null)
            : base()
        {
            _point = new Point3d(point);
            Tag = tag;
        }


        public MeshVertex(MeshVertex vertex)
        {
            _point = (Point3d)vertex.Point.Clone();
            Id = vertex.Id;
            Tag = vertex.Tag;
        }


        private MeshVertex(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            _point = (Point3d)info.GetValue("Point", typeof(Point3d));
        }


        #endregion

        #region Public Methods Specific

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("Point", _point, typeof(Point3d));
        }

        public object Clone()
        {
            return new MeshVertex(this);
        }

        #endregion

        #region Operators overrides

        public static bool operator ==(MeshVertex vertex1, MeshVertex vertex2)
        {
            if (ReferenceEquals(vertex1, vertex2))
                return true;
            return vertex1.Equals(vertex2);
        }

        public static bool operator !=(MeshVertex vertex1, MeshVertex vertex2)
        {
            return !(vertex1 == vertex2);
        }

        #endregion

        #region Public Methods Override

        public bool Equals(MeshVertex other)
        {
            return base.Equals(other) && _point == other._point;
        }

        public bool EqualsWithoutId(MeshVertex other)
        {
            return _point == other._point;
        }

        public override bool Equals(object obj)
        {
            if (obj is MeshVertex vertex)
            {
                return Equals(vertex);
            }
            return Equals(obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = 23;
                hashCode = hashCode * -17 + EqualityComparer<Point3d>.Default.GetHashCode(_point);
                return hashCode;
            }
        }

        public override string ToString()
        {
            return $"Id {Id} {_point}";
        }

        private string GetDebuggerDisplay()
        {
            return ToString();
        }
        #endregion
    }
}