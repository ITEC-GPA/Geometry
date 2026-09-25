using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace GPC.Geometry.Meshes.DelaunayMesh
{
    /// <summary>
    /// The initial mesh of a shape: the triangulation of its vertices only
    /// </summary>
    public sealed class InitialMesh : Mesh
    {
        #region Constructors

        /// <summary>
        /// Creates an empty mesh
        /// </summary>
        public InitialMesh()
            : base()
        {
        }

        /// <summary>
        /// Deserialization constructor (see <see cref="Mesh"/>)
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        private InitialMesh(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

		#endregion

		/// <summary>
		/// Triangulation of the shape using only its vertices (constrained Delaunay triangulation, see <see cref="ConstrainedDelaunay"/>)
		/// </summary>
		/// <param name="shape">The shape in the XY plane</param>
		/// <param name="mesh">The triangles, counterclockwise; an empty mesh if the triangulation fails</param>
		/// <param name="generateMeshStatus">Always null (also when the triangulation fails)</param>
		/// <returns>True if the triangulation succeeded</returns>
		public static bool Generate(Shape2d shape, out Mesh mesh, out InitialGenerateMeshStatus generateMeshStatus)
		{
			mesh = new Mesh();
			generateMeshStatus = null;

			try
			{
				mesh = ConstrainedDelaunay.Triangulate(shape, double.PositiveInfinity, false);
				return true;
			}
			catch (Exception)
			{
				return false;
			}
		}


        /// <summary>
        /// The status of the generation (not used: <see cref="Generate"/> returns null)
        /// </summary>
        [Serializable]
        public sealed class InitialGenerateMeshStatus : GenerateMeshStatus
        {
            /// <summary>
            /// Creates an empty status
            /// </summary>
            public InitialGenerateMeshStatus()
                : base()
            {

            }
        }
    }
}
