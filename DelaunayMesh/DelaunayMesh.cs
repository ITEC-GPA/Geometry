using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace GPC.Geometry.Meshes.DelaunayMesh
{
    /// <summary>
    /// Mesh of a planar shape (fill, holes and childs), with the size of the elements given by <see cref="Mesh.GenerateOptions.MeshSize"/>:
    /// quadrilaterals as square as possible (<see cref="Mesh.GenerateOptions.Recombine"/>, default) or triangles.
    /// See <see cref="ConstrainedDelaunay"/> and <see cref="QuadRecombination"/> for the algorithms
    /// </summary>
    public sealed class DelaunayMesh : Mesh
    {
        #region Properties

        /// <summary>
        /// The options used to generate the mesh (null if the mesh was not generated with options)
        /// </summary>
        public DelaunayGenerateOptions DelaunayMeshOptions => (DelaunayGenerateOptions)_options;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates an empty mesh
        /// </summary>
        public DelaunayMesh()
            : base()
        {
        }

        /// <summary>
        /// Deserialization constructor (see <see cref="Mesh"/>)
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        private DelaunayMesh(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        #endregion

        /// <summary>
        /// Generate the mesh of the shape
        /// </summary>
        /// <param name="shape">The shape in the XY plane</param>
        /// <param name="delaunayGenerateOptions">
        /// <para><see cref="Mesh.GenerateOptions.MeshSize"/>: maximum length of the boundary segments and size of the elements.
        /// Quadrilaterals: rectangles with the edges not longer than the mesh size. Triangles: the circumradius is at most 0.72 times the mesh size,
        /// i.e. an equilateral triangle with the edges 25% longer than the mesh size.
        /// With the default value (1E+22) only the vertices of the shape are used.</para>
        /// <para><see cref="Mesh.GenerateOptions.Recombine"/> (default true): quadrilaterals as square as possible. The points inside are the nodes of
        /// a lattice aligned with the prevailing direction of the edges and passing through the edges parallel to it, so the shapes made of such
        /// edges (rectangles, T, L, I, box sections...) are divided exactly in rectangles. Along the other edges some triangles can remain.</para>
        /// <para><see cref="DelaunayGenerateOptions.RecombineAll"/>: only quadrilaterals (with <see cref="Mesh.GenerateOptions.Recombine"/>): the mesh
        /// is generated with twice the mesh size, then every element is divided in quadrilaterals.</para>
        /// <para><see cref="DelaunayGenerateOptions.InitialMeshOnly"/>: only the points on the boundary (divided according to the mesh size), no points inside.</para>
        /// <para><see cref="DelaunayGenerateOptions.MinAngle"/>: minimum angle of the triangles (degrees), without <see cref="Mesh.GenerateOptions.Recombine"/>.
        /// Zero (default): only the size is checked, the mesh is as coarse as possible.</para>
        /// <para><see cref="Mesh.GenerateOptions.Refine"/>: the mesh is refined with <see cref="Mesh.Refine"/>.</para>
        /// </param>
        /// <param name="mesh">The mesh: elements counterclockwise, vertices of the shape with their original coordinates</param>
        /// <param name="generateMeshStatus">Null if the mesh is generated, otherwise it contains the error</param>
        /// <returns>True if the mesh is generated</returns>
        public static bool Generate(Shape2d shape, DelaunayGenerateOptions delaunayGenerateOptions, out Mesh mesh, out DelaunayGenerateMeshStatus generateMeshStatus)
        {
            mesh = null;
            generateMeshStatus = null;

            try
            {
                DelaunayGenerateOptions options = delaunayGenerateOptions ?? new DelaunayGenerateOptions();

                mesh = options.Recombine
                    ? ConstrainedDelaunay.Quadrangulate(shape, options.MeshSize, !options.InitialMeshOnly, options.RecombineAll)
                    : ConstrainedDelaunay.Triangulate(shape, options.MeshSize, !options.InitialMeshOnly, options.MinAngle);

                if (options.Refine)
                    mesh.Refine();

                return true;
            }
            catch (Exception e)
            {
                mesh = null;
                generateMeshStatus = new DelaunayGenerateMeshStatus();
                generateMeshStatus.AddException(e, $"Fail to generate the mesh: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Generate the meshes of shapes, one for each shape (see <see cref="Generate(Shape2d, DelaunayGenerateOptions, out Mesh, out DelaunayGenerateMeshStatus)"/>)
        /// </summary>
        /// <param name="shapes">The shapes in the XY plane</param>
        /// <param name="delaunayGenerateOptions">The options (null: default options)</param>
        /// <param name="meshes">The meshes, in the order of the shapes; if a mesh fails, the meshes generated before it</param>
        /// <param name="generateMeshStatus">Null if all the meshes are generated, otherwise the error of the first shape that failed</param>
        /// <returns>True if all the meshes are generated (the generation stops at the first failure)</returns>
        public static bool Generate(IEnumerable<Shape2d> shapes, DelaunayGenerateOptions delaunayGenerateOptions, out List<Mesh> meshes, out DelaunayGenerateMeshStatus generateMeshStatus)
        {
            meshes = new List<Mesh>();
            generateMeshStatus = null;

            foreach (Shape2d shape in shapes)
            {
                if (Generate(shape, delaunayGenerateOptions, out Mesh mesh, out generateMeshStatus))
                    meshes.Add(mesh);
                else
                    return false;
            }

            return true;
        }

        /// <summary>
        /// The options of <see cref="Generate(Shape2d, DelaunayGenerateOptions, out Mesh, out DelaunayGenerateMeshStatus)"/>
        /// </summary>
        [Serializable]
        public sealed class DelaunayGenerateOptions : GenerateOptions, ICloneable
        {
            /// <summary>
            /// Only the points on the boundary of the shape (divided according to the mesh size), no points inside
            /// </summary>
            public bool InitialMeshOnly;

            /// <summary>
            /// Minimum angle of the triangles, in degrees: the triangles with a smaller angle are refined, unless the angle is an angle of the shape.
            /// Zero (default): only the size of the triangles is checked. Values up to 30 degrees can be reached, the typical value is 20
            /// </summary>
            public double MinAngle;

            /// <summary>
            /// With <see cref="Mesh.GenerateOptions.Recombine"/>: the mesh is made only of quadrilaterals (default false: quadrilaterals and,
            /// where they can not be good, triangles)
            /// </summary>
            public bool RecombineAll;

            /// <summary>
            /// The default options: no size limit (only the vertices of the shape), quadrilaterals and triangles, no refinement, no minimum angle
            /// </summary>
            public DelaunayGenerateOptions()
            {
                MeshSize = 1E+22;
                Recombine = true;
                Refine = false;
                InitialMeshOnly = false;
                MinAngle = 0;
                RecombineAll = false;
            }

            /// <summary>
            /// Creates a copy of the options
            /// </summary>
            /// <returns>The copy</returns>
            public override object Clone()
            {
                DelaunayGenerateOptions clone = new DelaunayGenerateOptions
                {
                    MeshSize = MeshSize,
                    Recombine = Recombine,
                    Refine = Refine,
                    InitialMeshOnly = InitialMeshOnly,
                    MinAngle = MinAngle,
                    RecombineAll = RecombineAll,
                };

                return clone;
            }
        }

        /// <summary>
        /// The error of a failed generation
        /// </summary>
        [Serializable]
        public sealed class DelaunayGenerateMeshStatus : GenerateMeshStatus
        {
            /// <summary>
            /// Creates an empty status
            /// </summary>
            public DelaunayGenerateMeshStatus()
                : base()
            {

            }
        }
    }
}
