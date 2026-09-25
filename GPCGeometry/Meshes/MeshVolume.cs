using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;

namespace GPC.Geometry.Meshes
{
    /// <summary>
    /// A volume of a mesh: a triangular prism (6 vertices) or a quadrangular prism (8 vertices), as ids of the vertices of its two bases
    /// </summary>
    [DebuggerDisplay("{DebuggerDisplay(),nq}")]
    [Serializable]
    public class MeshVolume : MeshBase, ISerializable, IEquatable<MeshVolume>, ICloneable
    {

        /// <summary>
        /// The id of the first vertex
        /// </summary>
        private int _a;
        /// <summary>
        /// The id of the second vertex
        /// </summary>
        private int _b;
        /// <summary>
        /// The id of the third vertex
        /// </summary>
        private int _c;
        /// <summary>
        /// The id of the fourth vertex
        /// </summary>
        private int _d;
        /// <summary>
        /// The id of the fifth vertex
        /// </summary>
        private int _e;
        /// <summary>
        /// The id of the sixth vertex
        /// </summary>
        private int _f;
        /// <summary>
        /// The id of the seventh vertex; <see cref="MeshVertex.Unassigned"/> for a triangular prism
        /// </summary>
        private int _g;
        /// <summary>
        /// The id of the eighth vertex; <see cref="MeshVertex.Unassigned"/> for a triangular prism
        /// </summary>
        private int _h;


        /// <summary>
        /// The id of the first vertex
        /// </summary>
        public int A => _a;
        /// <summary>
        /// The id of the second vertex
        /// </summary>
        public int B => _b;
        /// <summary>
        /// The id of the third vertex
        /// </summary>
        public int C => _c;
        /// <summary>
        /// The id of the fourth vertex
        /// </summary>
        public int D => _d;
        /// <summary>
        /// The id of the fifth vertex
        /// </summary>
        public int E => _e;
        /// <summary>
        /// The id of the sixth vertex
        /// </summary>
        public int F => _f;
        /// <summary>
        /// The id of the seventh vertex; <see cref="MeshVertex.Unassigned"/> for a triangular prism
        /// </summary>
        public int G => _g;
        /// <summary>
        /// The id of the eighth vertex; <see cref="MeshVertex.Unassigned"/> for a triangular prism
        /// </summary>
        public int H => _h;

        /// <summary>
        /// True if the volume has 6 vertices
        /// </summary>
        public bool IsTriangularPrism => _h == -1 && _g == -1;

        /// <summary>
        /// True if the volume has 8 vertices
        /// </summary>
        public bool IsQuadrangularPrism => _h != -1 && _g != -1;


        /// <summary>
        /// Creates a volume from the ids of its vertices: 6 for a triangular prism, 8 for a quadrangular prism (see
        /// <see cref="MeshVolume(int, int, int, int, int, int, int, int, object)"/> for the order)
        /// </summary>
        /// <param name="vertices">The ids of the vertices</param>
        /// <param name="tag">The tag of the volume</param>
        /// <exception cref="ArgumentOutOfRangeException">if Vertices[] lenght is higher than 8 or lower than 6 or equal to 7</exception>
        public MeshVolume(int[] vertices, object tag = null)
        {
            if (vertices.Count() > 8 || vertices.Count() < 6)
                throw new ArgumentOutOfRangeException("Mesh Volume vertices count higher than 8 or lower than 6");
            if (vertices.Count() == 7)
                throw new ArgumentOutOfRangeException("Number of mesh volume vertices cannot be 7");

            _a = vertices[0];
            _b = vertices[1];
            _c = vertices[2];
            _d = vertices[3];
            _e = vertices[4];
            _f = vertices[5];

            if (vertices.Count() > 6)
            {
                _g = vertices[6];
                _h = vertices[7];
            }
            else
            {
                _g = MeshVertex.Unassigned;
                _h = MeshVertex.Unassigned;
            }

            Tag = tag;
        }




        /// <summary>
        /// Creates a triangular prism: a,b,c represent the vertex ids of the first face, d,e,f the vertex ids of the second face.
        /// The order of the vertex should be the same on both face.
        /// </summary>
        /// <param name="a">The id of the first vertex of the first base</param>
        /// <param name="b">The id of the second vertex of the first base</param>
        /// <param name="c">The id of the third vertex of the first base</param>
        /// <param name="d">The id of the first vertex of the second base</param>
        /// <param name="e">The id of the second vertex of the second base</param>
        /// <param name="f">The id of the third vertex of the second base</param>
        /// <exception cref="ArgumentException">If an id is negative</exception>
        public MeshVolume(int a, int b, int c, int d, int e, int f)
            : this(a, b, c, d, e, f, -1, -1)
        {
            _a = a;
            _b = b;
            _c = c;
            _d = d;
            _e = e;
            _f = f;

            _g = MeshVertex.Unassigned;
            _h = MeshVertex.Unassigned;

            if (a < 0 || b < 0 || c < 0 || d < 0 || e < 0 || f < 0)
            {
                throw new ArgumentException("Vertex from A to F cannot be negative");
            }

        }

        /// <summary>
        /// if the volume have a quadrangular base:
        ///     a,b,c,d represent the vertex id of first face.
        ///     e,f,g,h represent the vertex id of second face.
        /// if the volume have a triangular base:
        ///     a,b,c represent the vertex id of first face.
        ///     d,e,f represent the vertex id of second face (g and h are <see cref="MeshVertex.Unassigned"/>).
        /// The order of the vertex should be the same on both face.
        /// </summary>
        /// <param name="a">The id of the first vertex</param>
        /// <param name="b">The id of the second vertex</param>
        /// <param name="c">The id of the third vertex</param>
        /// <param name="d">The id of the fourth vertex</param>
        /// <param name="e">The id of the fifth vertex</param>
        /// <param name="f">The id of the sixth vertex</param>
        /// <param name="g">The id of the seventh vertex, negative for a triangular prism</param>
        /// <param name="h">The id of the eighth vertex, negative for a triangular prism</param>
        /// <param name="tag">The tag of the volume</param>
        /// <exception cref="ArgumentException">If an id from a to f is negative, or only one of g and h is negative</exception>
        public MeshVolume(int a, int b, int c, int d, int e, int f, int g, int h, object tag = null)
        {
            _a = a;
            _b = b;
            _c = c;
            _d = d;
            _e = e;
            _f = f;
            _g = g;
            _h = h;

            if (a < 0 || b < 0 || c < 0 || d < 0 || e < 0 || f < 0)
            {
                throw new ArgumentException("Vertex from A to F cannot be negative");
            }
            if ((g >= 0 && h < 0) || (g < 0 && h >= 0))
            {
                throw new ArgumentException();
            }
            base.Tag = tag;
        }


        /// <summary>
        /// Creates a copy of a volume: same vertices (the id and the tag are not copied)
        /// </summary>
        /// <param name="volume">The volume to copy</param>
        public MeshVolume(MeshVolume volume)
        {
            _a = volume.A;
            _b = volume.B;
            _c = volume.C;
            _d = volume.D;
            _e = volume.E;
            _f = volume.F;
            _g = volume.G;
            _h = volume.H;

        }

        /// <summary>
        /// Deserialization constructor: reads the id and the vertices
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        public MeshVolume(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            _a = info.GetInt32("A");
            _b = info.GetInt32("B");
            _c = info.GetInt32("C");
            _d = info.GetInt32("D");
            _e = info.GetInt32("E");
            _f = info.GetInt32("F");
            _g = info.GetInt32("G");
            _h = info.GetInt32("H");
        }


        /// <summary>
        /// Get the nodes ids
        /// </summary>
        /// <param name="reverse">Return the array inverted</param>
        /// <returns>The Ids array</returns>
        public int[] GetNodes(bool reverse = false)
        {
            int[] nodes;
            if (IsQuadrangularPrism)
                nodes = new int[] { A, B, C, D, E, F, G, H };
            else
                nodes = new int[] { A, B, C, D, E, F };
            if (reverse)
                return nodes.Reverse().ToArray();
            return nodes;
        }

        /// <summary>
        /// Creates a copy of the volume (see <see cref="MeshVolume(MeshVolume)"/>)
        /// </summary>
        /// <returns>The copy</returns>
        public object Clone()
        {
            return new MeshVolume(this);
        }

        /// <summary>
        /// Tells if the nodes of the smaller volume are consecutive nodes of the larger one, in the same or in the opposite order
        /// (the larger one closed once, as <see cref="MeshFace.IsMatch(int[])"/>)
        /// </summary>
        /// <param name="nodes">The nodes of the volumes to compare</param>
        /// <returns>True if the nodes matches</returns>
        public bool IsMatch(int[] nodes)
        {
            List<int> bigVolume;
            List<int> smallVolume;

            int[] tn = GetNodes();

            if (tn.Length >= nodes.Length)
            {
                bigVolume = tn.ToList();
                smallVolume = new List<int>(nodes);
            }
            else
            {
                bigVolume = new List<int>(nodes);
                smallVolume = tn.ToList();
            }

            bigVolume.Add(bigVolume[0]);
            List<int> reversed = new List<int>(smallVolume);
            reversed.Reverse();

            bool match = false;
            for (int i = 0; i < bigVolume.Count - smallVolume.Count + 1; i++)
            {
                List<int> sub = bigVolume.GetRange(i, smallVolume.Count);
                if (sub.SequenceEqual(smallVolume) || sub.SequenceEqual(reversed))
                {
                    match = true;
                    break;
                }
            }

            return match;

        }

        /// <summary>
        /// Equality of the vertices in the same order (the id is not compared)
        /// </summary>
        /// <param name="other">The volume to compare</param>
        /// <returns>True if the volumes have the same vertices</returns>
        public bool Equals(MeshVolume other)
        {
            return !(other is null) && _a == other._a
                                    && _b == other._b && _c == other._c && _d == other._d
                                    && _e == other._e && _f == other._f && _g == other._g && _h == other._h;
        }

        /// <summary>
        /// Equality of the vertices (see <see cref="MeshBase.HasSameContent(MeshBase)"/>)
        /// </summary>
        /// <param name="other">The element to compare</param>
        /// <returns>True if <paramref name="other"/> is a volume with the same vertices</returns>
        internal override bool HasSameContent(MeshBase other)
        {
            return other is MeshVolume volume && Equals(volume);
        }

        /// <summary>
        /// Equality with another object (see <see cref="Equals(MeshVolume)"/>)
        /// </summary>
        /// <param name="obj">The object to compare</param>
        /// <returns>True if <paramref name="obj"/> is an equal volume</returns>
        public override bool Equals(object obj)
        {
            if (obj is MeshVolume face)
            {
                return Equals(face);
            }
            return false; // it was Equals(obj): infinite recursion for an object of another type
        }

        /// <summary>
        /// Serializes the <see cref="BaseObject.Guid"/>, the id and the vertices
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("A", _a);
            info.AddValue("B", _b);
            info.AddValue("C", _c);
            info.AddValue("D", _d);
            info.AddValue("E", _e);
            info.AddValue("F", _f);
            info.AddValue("G", _g);
            info.AddValue("H", _h);
        }


        /// <summary>
        /// The hash code of the vertices (dependent on their order)
        /// </summary>
        /// <returns>The hash code</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = 23;
                hashCode = hashCode * -17 + base.GetHashCode();
                hashCode = hashCode * -17 + _a.GetHashCode();
                hashCode = hashCode * -17 + _b.GetHashCode();
                hashCode = hashCode * -17 + _c.GetHashCode();
                hashCode = hashCode * -17 + _d.GetHashCode();
                hashCode = hashCode * -17 + _e.GetHashCode();
                hashCode = hashCode * -17 + _f.GetHashCode();
                hashCode = hashCode * -17 + _g.GetHashCode();
                hashCode = hashCode * -17 + _h.GetHashCode();
                return hashCode;
            }
        }


        #region Operators overrides

        /// <summary>
        /// Equality operator (see <see cref="Equals(MeshVolume)"/>)
        /// </summary>
        /// <param name="vol1">The first volume (not null, unless both are null)</param>
        /// <param name="vol2">The second volume</param>
        /// <returns>True if the volumes are equal</returns>
        public static bool operator ==(MeshVolume vol1, MeshVolume vol2)
        {
            if (ReferenceEquals(vol1, vol2))
                return true;
            return vol1.Equals(vol2);
        }

        /// <summary>
        /// Inequality operator (see <see cref="Equals(MeshVolume)"/>)
        /// </summary>
        /// <param name="vol1">The first volume</param>
        /// <param name="vol2">The second volume</param>
        /// <returns>True if the volumes are different</returns>
        public static bool operator !=(MeshVolume vol1, MeshVolume vol2)
        {
            return !(vol1 == vol2);
        }

        #endregion

        /// <summary>
        /// The text shown by the debugger
        /// </summary>
        /// <returns>The id and the vertices</returns>
        private string DebuggerDisplay()
        {
            return $"Id:{Id}, A:{A}, B:{B}, C:{C}, D:{D}, E:{E}, F:{F}, G:{G}, H:{H}";
        }


    }
}
