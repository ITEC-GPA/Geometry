using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace GPC.Geometry.Meshes
{
    /// <summary>
    /// A vertex of a mesh: a point with an id
    /// </summary>
    [Serializable]
    [DebuggerDisplay("{" + nameof(GetDebuggerDisplay) + "(),nq}")]
    public sealed class MeshVertex : MeshBase, ISerializable, IEquatable<MeshVertex>, ICloneable
    {
        #region Variables

        /// <summary>
        /// The position of the vertex
        /// </summary>
        private Point3d _point;
        /// <summary>
        /// The value of a node not assigned (e.g. the fourth node of a triangular face)
        /// </summary>
        public const int Unassigned = -1;

        #endregion

        #region Properties

        /// <summary>
        /// The position of the vertex (the instance of the vertex)
        /// </summary>
        public Point3d Point => _point;

        #endregion

        #region Public Constructors

        /// <summary>
        /// Creates a vertex with a copy of a point
        /// </summary>
        /// <param name="point">The position</param>
        /// <param name="tag">The tag of the vertex</param>
        public MeshVertex(Point3d point, object tag = null)
            : base()
        {
            _point = new Point3d(point);
            Tag = tag;
        }


        /// <summary>
        /// Creates a copy of a vertex: copy of the point, same id and tag
        /// </summary>
        /// <param name="vertex">The vertex to copy</param>
        public MeshVertex(MeshVertex vertex)
        {
            _point = (Point3d)vertex.Point.Clone();
            Id = vertex.Id;
            Tag = vertex.Tag;
        }


        /// <summary>
        /// Deserialization constructor: reads the id and the point
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        private MeshVertex(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            _point = (Point3d)info.GetValue("Point", typeof(Point3d));
        }


        #endregion

        #region Public Methods Specific

        /// <summary>
        /// Serializes the <see cref="BaseObject.Guid"/>, the id and the point
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("Point", _point, typeof(Point3d));
        }

        /// <summary>
        /// Creates a copy of the vertex (see <see cref="MeshVertex(MeshVertex)"/>)
        /// </summary>
        /// <returns>The copy</returns>
        public object Clone()
        {
            return new MeshVertex(this);
        }

        #endregion

        #region Operators overrides

        /// <summary>
        /// Equality operator (see <see cref="Equals(MeshVertex)"/>); two null vertices are equal
        /// </summary>
        /// <param name="vertex1">The first vertex</param>
        /// <param name="vertex2">The second vertex</param>
        /// <returns>True if the vertices are equal</returns>
        public static bool operator ==(MeshVertex vertex1, MeshVertex vertex2)
        {
            if (ReferenceEquals(vertex1, vertex2))
                return true;
            if (vertex1 is null || vertex2 is null)
                return false;
            return vertex1.Equals(vertex2);
        }

        /// <summary>
        /// Inequality operator (see <see cref="Equals(MeshVertex)"/>)
        /// </summary>
        /// <param name="vertex1">The first vertex</param>
        /// <param name="vertex2">The second vertex</param>
        /// <returns>True if the vertices are different</returns>
        public static bool operator !=(MeshVertex vertex1, MeshVertex vertex2)
        {
            return !(vertex1 == vertex2);
        }

        #endregion

        #region Public Methods Override

        /// <summary>
        /// Equality of the id and of the point (within the tolerance of <see cref="Point3d"/>)
        /// </summary>
        /// <param name="other">The vertex to compare</param>
        /// <returns>True if the vertices are equal</returns>
        public bool Equals(MeshVertex other)
        {
            return !(other is null) && base.Equals(other) && _point == other._point;
        }

        /// <summary>
        /// Equality of the point (within the tolerance of <see cref="Point3d"/>), whatever the id
        /// </summary>
        /// <param name="other">The vertex to compare (not null)</param>
        /// <returns>True if the points are equal</returns>
        public bool EqualsWithoutId(MeshVertex other)
        {
            return _point == other._point;
        }

        /// <summary>
        /// Equality of the points (see <see cref="MeshBase.HasSameContent(MeshBase)"/>)
        /// </summary>
        /// <param name="other">The element to compare</param>
        /// <returns>True if <paramref name="other"/> is a vertex with an equal point</returns>
        internal override bool HasSameContent(MeshBase other)
        {
            return other is MeshVertex vertex && _point == vertex._point;
        }

        /// <summary>
        /// Equality with another object (see <see cref="Equals(MeshVertex)"/>)
        /// </summary>
        /// <param name="obj">The object to compare</param>
        /// <returns>True if <paramref name="obj"/> is an equal vertex</returns>
        public override bool Equals(object obj)
        {
            if (obj is MeshVertex vertex)
            {
                return Equals(vertex);
            }
            return false;
        }

        /// <summary>
        /// Hashes the ID and the tolerance-compatible point hash; content deduplication is independent of the ID.
        /// </summary>
        /// <returns>The hash code</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = 23;
                hashCode = hashCode * -17 + Id.GetHashCode();
                hashCode = hashCode * -17 + EqualityComparer<Point3d>.Default.GetHashCode(_point);
                return hashCode;
            }
        }

        /// <summary>
        /// The id and the point
        /// </summary>
        /// <returns>The description of the vertex</returns>
        public override string ToString()
        {
            return $"Id {Id} {_point}";
        }

        /// <summary>
        /// The text shown by the debugger
        /// </summary>
        /// <returns>See <see cref="ToString"/></returns>
        private string GetDebuggerDisplay()
        {
            return ToString();
        }
        #endregion
    }
}
