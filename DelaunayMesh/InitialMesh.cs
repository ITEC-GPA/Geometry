using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace GPC.Geometry.Meshes.DelaunayMesh
{
    public sealed class InitialMesh : Mesh
    {
        #region Constructors

        public InitialMesh()
            : base()
        {
        }

        private InitialMesh(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

		#endregion

		public static bool Generate(Shape2d shape, out Mesh mesh, out InitialGenerateMeshStatus generateMeshStatus)
		{
			mesh = new Mesh();
			generateMeshStatus = null;

			try
			{
				Helper.InitialTriangulationHelper triangulation = new Helper.InitialTriangulationHelper(shape);
				List<Helper.Triangle> triangles = triangulation.Triangulate();

				mesh = Helper.BuildMesh(triangles);

				return true;
			}
			catch (Exception)
			{
				return false;
			}
		}


        [Serializable]
        public sealed class InitialGenerateMeshStatus : GenerateMeshStatus
        {
            public InitialGenerateMeshStatus()
                : base()
            {

            }
        }
    }
}
