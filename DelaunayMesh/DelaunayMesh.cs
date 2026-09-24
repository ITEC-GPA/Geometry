using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace GPC.Geometry.Meshes.DelaunayMesh
{
    /// <summary>
    /// Triangular mesh of a planar shape (fill, holes and childs), with the size of the elements given by <see cref="Mesh.GenerateOptions.MeshSize"/>.
    /// See <see cref="ConstrainedDelaunay"/> for the algorithm
    /// </summary>
    public sealed class DelaunayMesh : Mesh
    {
        #region Properties

        public DelaunayGenerateOptions DelaunayMeshOptions => (DelaunayGenerateOptions)_options;

        #endregion

        #region Constructors

        public DelaunayMesh()
            : base()
        {
        }

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
        /// <para><see cref="Mesh.GenerateOptions.MeshSize"/>: maximum length of the boundary segments and size of the triangles
        /// (the circumradius of a triangle is at most 0.72 times the mesh size, i.e. an equilateral triangle with the edges 25% longer than the mesh size).
        /// With the default value (1E+22) only the vertices of the shape are used.</para>
        /// <para><see cref="DelaunayGenerateOptions.InitialMeshOnly"/>: only the points on the boundary (divided according to the mesh size), no points inside.</para>
        /// <para><see cref="DelaunayGenerateOptions.MinAngle"/>: minimum angle of the triangles (degrees). Zero (default): only the size is checked, the mesh is as coarse as possible.</para>
        /// <para><see cref="Mesh.GenerateOptions.Refine"/>: the mesh is refined with <see cref="Mesh.Refine"/>.</para>
        /// <para><see cref="Mesh.GenerateOptions.Recombine"/> is not used: the mesh is made only of triangles.</para>
        /// </param>
        /// <param name="mesh">The mesh: triangles counterclockwise, vertices of the shape with their original coordinates</param>
        /// <param name="generateMeshStatus">Null if the mesh is generated, otherwise it contains the error</param>
        /// <returns>True if the mesh is generated</returns>
        public static bool Generate(Shape2d shape, DelaunayGenerateOptions delaunayGenerateOptions, out Mesh mesh, out DelaunayGenerateMeshStatus generateMeshStatus)
        {
            mesh = null;
            generateMeshStatus = null;

            try
            {
                DelaunayGenerateOptions options = delaunayGenerateOptions ?? new DelaunayGenerateOptions();

                mesh = ConstrainedDelaunay.Triangulate(shape, options.MeshSize, !options.InitialMeshOnly, options.MinAngle);

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

            public DelaunayGenerateOptions()
            {
                MeshSize = 1E+22;
                Recombine = true;
                Refine = false;
                InitialMeshOnly = false;
                MinAngle = 0;
            }

            public override object Clone()
            {
                DelaunayGenerateOptions clone = new DelaunayGenerateOptions
                {
                    MeshSize = MeshSize,
                    Recombine = Recombine,
                    Refine = Refine,
                    InitialMeshOnly = InitialMeshOnly,
                    MinAngle = MinAngle,
                };

                return clone;
            }
        }

        [Serializable]
        public sealed class DelaunayGenerateMeshStatus : GenerateMeshStatus
        {
            public DelaunayGenerateMeshStatus()
                : base()
            {

            }
        }
    }
}
