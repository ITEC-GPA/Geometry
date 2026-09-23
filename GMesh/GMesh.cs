using GmshNet;
using GPC.Utilities.Maths;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace GPC.Geometry.Meshes.GMesh
{
    public sealed class GMesh : Mesh
    {
        #region Properties

        public GMeshGenerateOptions GMeshOptions => (GMeshGenerateOptions)_options;

        #endregion

        #region Constructors

        public GMesh()
            : base()
        {
        }

        private GMesh(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        #endregion

        #region Generate mesh functions

        /// <summary>
        /// 
        /// </summary>
        /// <param name="shapes">Geometry to mesh</param>
        /// <param name="options">Generate mesh options</param>
        /// <param name="meshes"></param>
        /// <param name="generateMeshStatus"></param>
        /// <returns>True if the mesh is generate without error, false otherwise</returns>
        public static bool Generate(IEnumerable<Shape> shapes, GMeshGenerateOptions options, out List<Mesh> meshes, out GMeshGenerateMeshStatus generateMeshStatus)
        {
            var _ = new Dictionary<Mesh, Dictionary<GeometryBase, int[]>>();
            return Generate(shapes, null, null, options, out meshes, out generateMeshStatus);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="shapes"></param>
        /// <param name="embeddedGeometries">association one to many of geometries embedded in geometry. es points in surface. Keys must be contained in shapes </param>
        /// <param name="embeddedGeomMeshSize">Mesh size at specific embed geometry. Keys must be contained in <paramref name="embeddedGeometries"/></param>
        /// <param name="generateMeshStatus">Class that collect all the errors, warning and additional information related to geometry generation</param>
        /// <param name="options">Generate mesh options</param>
        /// <param name="meshes"></param>
        /// <returns>True if the mesh is generate without error, false otherwise</returns>
        public static bool Generate(IEnumerable<Shape> shapes, Dictionary<Shape, GeometryBase[]> embeddedGeometries, GMeshGenerateOptions options,
            out List<Mesh> meshes, out GMeshGenerateMeshStatus generateMeshStatus, Dictionary<GeometryBase, double> embeddedGeomMeshSize = null)
        {
            return Generate(shapes, embeddedGeometries, embeddedGeomMeshSize, options, out meshes, out generateMeshStatus);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="shapes"></param>
        /// <param name="embeddedGeometries">association one to many of geometries embedded in geometry. es points in surface. Keys must be contained in shapes </param>
        /// <param name="generateMeshStatus">Class that collect all the errors, warning and additional information related to geometry generation</param>
        /// <param name="options">Generate mesh options</param>
        /// <param name="meshes"></param>
        /// <returns>True if the mesh is generate without error, false otherwise</returns>
        public static bool Generate(IEnumerable<Shape> shapes, Dictionary<Shape, GeometryBase[]> embeddedGeometries, GMeshGenerateOptions options,
            out List<Mesh> meshes, out GMeshGenerateMeshStatus generateMeshStatus)
        {
            return Generate(shapes, embeddedGeometries, null, options, out meshes, out generateMeshStatus);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="shapesInput">Geometry to mesh</param>
        /// <param name="embeddedGeometriesInput">association one to many of geometries embedded in geometry. es points in surface. Keys must be contained in shapes </param>
        /// <param name="embeddedGeomMeshSize">Mesh size at specific embed geometry. Keys must be contained in <paramref name="embeddedGeometriesInput"/></param>
        /// <param name="meshes">The meshes generated</param>
        /// <param name="generateMeshStatus">Class that collect all the errors, warning and additional information related to geometry generation</param>
        /// <param name="options">Generate mesh options</param>
        /// <returns>True if the mesh is generate without error, false otherwise</returns>
        /// <remarks>The default geometry tolerance is 10E-4. If the user want to use a custom tolerance, he have to scale by a number lower than 1 the geometry 
        /// and then rescale with the reciprocal la mesh.</remarks>
        public static bool Generate(IEnumerable<Shape> shapesInput, Dictionary<Shape, GeometryBase[]> embeddedGeometriesInput, Dictionary<GeometryBase,
            double> embeddedGeomMeshSize, GMeshGenerateOptions options, out List<Mesh> meshes, out GMeshGenerateMeshStatus generateMeshStatus)
        {
            if (options is null)
                throw new ArgumentNullException(nameof(GenerateOptions));

            if (shapesInput is null)
                throw new ArgumentNullException(nameof(Shape));

            // DateTime dt = new DateTime();
            DateTime dt0 = DateTime.Now;

            Gmsh.Initialize();
            //Gmsh.Logger.Start();


            #region OPZIONI GMSH
            // OPZIONI
            // TOLERANCE 
            Gmsh.Option.SetNumber("Geometry.Tolerance", options.Tolerance);
            Gmsh.Option.SetNumber("Geometry.MatchGeomAndMesh", options.MatchGeomAndMesh);
            Gmsh.Option.SetNumber("Geometry.MatchMeshTolerance", options.MatchMeshTolerance);
            Gmsh.Option.SetNumber("Mesh.ToleranceInitialDelaunay", options.ToleranceInitialDelaunay);
            Gmsh.Option.SetNumber("Mesh.ToleranceEdgeLength", options.ToleranceEdgeLength);
            Gmsh.Option.SetNumber("Mesh.AngleToleranceFacetOverlap", options.AngleToleranceFacetOverlap);
            Gmsh.Option.SetNumber("Mesh.RandomFactor", options.RandomFactor);

            // ALTRE
            Gmsh.Option.SetNumber("Geometry.OCCUnionUnify", options.OCCUnionUnify);
            Gmsh.Option.SetNumber("Geometry.OCCSewFaces", options.SewFaces);
            Gmsh.Option.SetNumber("General.ExpertMode", 1);     //to disable all the messages meant for inexperienced users

            // MESH 
            Gmsh.Option.SetNumber("Mesh.Algorithm", (int)options.Algorithm);

            if (options.MeshSizeMax == 0)
                throw new ArgumentException("Max mesh size cannot be zero");

            Gmsh.Option.SetNumber("Mesh.MeshSizeMax", options.MeshSizeMax);
            Gmsh.Option.SetNumber("Mesh.MeshSizeMin", options.MeshSizeMin);

            Gmsh.Option.SetNumber("Mesh.MeshSizeExtendFromBoundary", 1);
            Gmsh.Option.SetNumber("Mesh.MeshSizeFromCurvature", 0);
            Gmsh.Option.SetNumber("Mesh.MeshSizeFromPoints", 1);
            Gmsh.Option.SetNumber("Mesh.FlexibleTransfinite", 1);
            Gmsh.Option.SetNumber("Mesh.MeshSizeFromParametricPoints", 0);

            Gmsh.Option.SetNumber("Mesh.RefineSteps", options.RefineSteps);

            Gmsh.Option.SetNumber("Mesh.RecombinationAlgorithm", (int)options.RecombinationAlgorithm);
            Gmsh.Option.SetNumber("Mesh.RecombineOptimizeTopology", (int)options.RecombineOptimizeTopology);

            Gmsh.Option.SetNumber("Mesh.HighOrderOptimize", 2);
            Gmsh.Option.SetNumber("Mesh.Optimize", (int)options.OptimizeIteration);
            Gmsh.Option.SetNumber("Mesh.OptimizeThreshold", options.MinQuality);
            Gmsh.Option.SetNumber("Mesh.OptimizeNetgen", options.OptimizeNetgen);
            Gmsh.Option.SetNumber("Mesh.Smoothing", options.Smoothing);

            Gmsh.Option.SetNumber("Mesh.SurfaceFaces", 1);         // mostra gli elementi della mesh in automatico nell'output di gmsh

            #endregion

            DateTime dt1 = DateTime.Now;

            #region TOLERANCE

            double tolerance = options.GeometryBaseScaleFactor * GeometryBase.Tolerance;
            double matching = 1;

            double toleranceIntersection = tolerance;
            toleranceIntersection = toleranceIntersection > 1 ? 1 : toleranceIntersection;
            toleranceIntersection = toleranceIntersection < 1E-7 ? 1E-7 : toleranceIntersection;
            double toleranceMatch = toleranceIntersection * matching;

            double preProctime = 0;
            double geomCADTime = 0;
            double rebuildObjectTagsTime = 0;
            double nodeTime = 0;
            double elementTime = 0;
            double embTime = 0;

            #endregion


            #region SCALA E PREPARAZIONE LISTE E DIZIONARI

            meshes = new List<Mesh>();                                  // lista di meshes che poi vengono date in out
            generateMeshStatus = new GMeshGenerateMeshStatus();
            OpenCascadeWrapper occw = new OpenCascadeWrapper();

            List<Shape> shapesList = new List<Shape>();

            // PhysicalGroupTag, geometryDim, surfaceTags
            List<(int physicalGroupTag, int dim, int[] surfacesTag, Shape shape)> physicalGroupTagSurfacesAssociation = new List<(int, int, int[], Shape)>();
            // tag physical group base / tag physical group embedded / tag della superficie 
            // associa physical group a mesh. serve per quando embeddi una shape. devi embeddarla con un altro physical group e poi andarla a ripescare in fondo
            List<(int physicalGroupTag, int embPhysicalGroupTag, int surfaceTag)> physicalGroupMeshTagAssociation = new List<(int, int, int)>();

            // Associazione fra la geometria embedded e il tag del oggetto
            Dictionary<int, Dictionary<GeometryBase, int[]>> embeddedGeometriesTagAssociation = new Dictionary<int, Dictionary<GeometryBase, int[]>>();
            // Associazione tra shape, geometria emb con meshSize modificata e mesh size
            Dictionary<Shape, Dictionary<GeometryBase, double>> embGeomAssociation = new Dictionary<Shape, Dictionary<GeometryBase, double>>();
            // Associazione tra shape e geometria emb 
            Dictionary<Shape, GeometryBase[]> embeddedGeometries = new Dictionary<Shape, GeometryBase[]>();
            // Associazione tra shape embedded non modificata e shape embedded modificata.
            Dictionary<Shape, Shape> shapeOutputAssociation = new Dictionary<Shape, Shape>();

            HashSet<GeometryBase> listOfAllEmbGeom = new HashSet<GeometryBase>();
            HashSet<GeometryBase> listOfAllEmbGeomForEmbShape = new HashSet<GeometryBase>();

            // prima di scalare tutto, creo un dizionario in cui ho la shape, il riferimento in memoria della geometria (scalata) e la size.
            // scalo tutto del fattore scalegeometryfactor            
            for (int s = 0; s < shapesInput.Count(); s++)
            {
                Shape shape = new Shape(shapesInput.ElementAt(s));
                shape = shape.Scale(options.GeometryBaseScaleFactor);
                shapesList.Add(shape);
                List<GeometryBase> geometryBases = new List<GeometryBase>();
                Dictionary<GeometryBase, double> geometryAndMeshSize = new Dictionary<GeometryBase, double>();

                if (embeddedGeometriesInput != null && embeddedGeometriesInput.ContainsKey(shapesInput.ElementAt(s)))
                {
                    for (int i = 0; i < embeddedGeometriesInput[shapesInput.ElementAt(s)].Length; i++)
                    {
                        if (embeddedGeometriesInput[shapesInput.ElementAt(s)][i] is Point3d point3d)
                        {
                            Point3d scaledPoint = point3d.Scale(options.GeometryBaseScaleFactor);
                            geometryBases.Add(scaledPoint);
                            if (embeddedGeomMeshSize != null)
                            {
                                if (embeddedGeomMeshSize.ContainsKey(embeddedGeometriesInput[shapesInput.ElementAt(s)][i]))
                                {
                                    geometryAndMeshSize.Add(scaledPoint,
                                        embeddedGeomMeshSize[embeddedGeometriesInput[shapesInput.ElementAt(s)][i]] * options.GeometryBaseScaleFactor);
                                }
                            }
                        }

                        else if (embeddedGeometriesInput[shapesInput.ElementAt(s)][i] is Point2d point2d)
                        {
                            Point2d scaledPoint = point2d.Scale(options.GeometryBaseScaleFactor);
                            geometryBases.Add(scaledPoint);
                            if (embeddedGeomMeshSize != null)
                            {
                                if (embeddedGeomMeshSize.ContainsKey(embeddedGeometriesInput[shapesInput.ElementAt(s)][i]))
                                {
                                    geometryAndMeshSize.Add(scaledPoint,
                                        embeddedGeomMeshSize[embeddedGeometriesInput[shapesInput.ElementAt(s)][i]] * options.GeometryBaseScaleFactor);
                                }
                            }
                        }

                        else if (embeddedGeometriesInput[shapesInput.ElementAt(s)][i] is Line3d line3d)
                        {
                            Line3d scaledLine = line3d.Scale(options.GeometryBaseScaleFactor);
                            geometryBases.Add(scaledLine);
                            if (embeddedGeomMeshSize != null)
                            {
                                if (embeddedGeomMeshSize.ContainsKey(embeddedGeometriesInput[shapesInput.ElementAt(s)][i]))
                                {
                                    geometryAndMeshSize.Add(scaledLine,
                                        embeddedGeomMeshSize[embeddedGeometriesInput[shapesInput.ElementAt(s)][i]] * options.GeometryBaseScaleFactor);
                                }
                            }
                        }

                        else if (embeddedGeometriesInput[shapesInput.ElementAt(s)][i] is Line2d line2d)
                        {
                            Line2d scaledLine = line2d.Scale(options.GeometryBaseScaleFactor);
                            geometryBases.Add(scaledLine);
                            if (embeddedGeomMeshSize != null)
                            {
                                if (embeddedGeomMeshSize.ContainsKey(embeddedGeometriesInput[shapesInput.ElementAt(s)][i]))
                                {
                                    geometryAndMeshSize.Add(scaledLine,
                                        embeddedGeomMeshSize[embeddedGeometriesInput[shapesInput.ElementAt(s)][i]] * options.GeometryBaseScaleFactor);
                                }
                            }
                        }

                        else if (embeddedGeometriesInput[shapesInput.ElementAt(s)][i] is Polygon3d polygon3d)
                        {
                            Polygon3d scaledPolygon = polygon3d.Scale(options.GeometryBaseScaleFactor);
                            geometryBases.Add(scaledPolygon);
                            if (embeddedGeomMeshSize != null)
                            {
                                if (embeddedGeomMeshSize.ContainsKey(embeddedGeometriesInput[shapesInput.ElementAt(s)][i]))
                                {
                                    geometryAndMeshSize.Add(scaledPolygon,
                                        embeddedGeomMeshSize[embeddedGeometriesInput[shapesInput.ElementAt(s)][i]] * options.GeometryBaseScaleFactor);
                                }
                            }
                        }

                        else if (embeddedGeometriesInput[shapesInput.ElementAt(s)][i] is Polygon2d polygon2d)
                        {
                            Polygon2d scaledPolygon = polygon2d.Scale(options.GeometryBaseScaleFactor);
                            geometryBases.Add(scaledPolygon);
                            if (embeddedGeomMeshSize != null)
                            {
                                if (embeddedGeomMeshSize.ContainsKey(embeddedGeometriesInput[shapesInput.ElementAt(s)][i]))
                                {
                                    geometryAndMeshSize.Add(scaledPolygon,
                                        embeddedGeomMeshSize[embeddedGeometriesInput[shapesInput.ElementAt(s)][i]] * options.GeometryBaseScaleFactor);
                                }
                            }
                        }

                        else if (embeddedGeometriesInput[shapesInput.ElementAt(s)][i] is Shape shapeEmb)
                        {
                            Shape scaledShape = shapeEmb.Scale(options.GeometryBaseScaleFactor);
                            geometryBases.Add(scaledShape);
                            if (embeddedGeomMeshSize != null)
                            {
                                if (embeddedGeomMeshSize.ContainsKey(embeddedGeometriesInput[shapesInput.ElementAt(s)][i]))
                                {
                                    geometryAndMeshSize.Add(scaledShape,
                                        embeddedGeomMeshSize[embeddedGeometriesInput[shapesInput.ElementAt(s)][i]] * options.GeometryBaseScaleFactor);
                                }
                            }
                        }

                        else
                            throw new ArgumentException($"Geom {embeddedGeometriesInput[shapesInput.ElementAt(s)][i].GetType()} not supported");
                    }
                    embeddedGeometries.Add(shape, geometryBases.ToArray());
                    embGeomAssociation.Add(shape, geometryAndMeshSize);

                    for (int g = 0; g < geometryBases.Count; g++)
                        listOfAllEmbGeom.Add(geometryBases[g]);
                }

                for (int i = 0; i < listOfAllEmbGeom.Count; i++)
                    listOfAllEmbGeomForEmbShape.Add(listOfAllEmbGeom.ElementAt(i));
                for (int i = 0; i < shapesInput.ElementAt(s).Fill.Count; i++)
                    listOfAllEmbGeomForEmbShape.Add(shapesInput.ElementAt(s).Fill[i]);
                if (shapesInput.ElementAt(s).HasHoles)
                    for (int i = 0; i < shapesInput.ElementAt(s).Holes.Count(); i++)
                        for (int p = 0; p < shapesInput.ElementAt(s).Holes[i].Count; p++)
                            listOfAllEmbGeomForEmbShape.Add(shapesInput.ElementAt(s).Holes[i][p]);
            }

            Shape[] shapes = shapesList.ToArray();

            #endregion

            DateTime dt2 = DateTime.Now;

            try
            {
                #region CREAZIONE CAD 

                bool embShapeCut = false;
                int physicalGroupTag = 0;
                try
                {
                    for (int c = 0; c < shapes.Count(); c++)
                    {
                        DateTime dt2_1 = DateTime.Now;

                        HashSet<GeometryBase> hashSet = new HashSet<GeometryBase>();
                        Shape clone = (Shape)shapes[c].Clone();
                        GeometryBase[] embGeomClone = null;
                        Dictionary<GeometryBase, double> embMeshSizeBuffer = null;

                        if (embeddedGeometries.ContainsKey(shapes[c]))
                        {
                            embGeomClone = embeddedGeometries[shapes[c]];
                            embMeshSizeBuffer = embGeomAssociation[shapes[c]];
                        }
                        bool addpoint = false;


                        #region PREPROCESSING DELLA GEOMETRIA

                        // Prima di tutto verifica se ci sono embeddedGeometries che hanno punti sul bordo della shape e, se così, aggiunge tali punti
                        // Questo è un workarround per ebitare un bug di Gmesh che si verifica in tale situazione
                        try
                        {
                            #region PREPROCESSING EMBEDDED SHAPE 

                            if (embeddedGeometries != null && embeddedGeometries.ContainsKey(shapes[c]))
                            {
                                try
                                {
                                    // per ogni shape embedded nella shape che stiamo considerando vado a cercare tutte le insersezioni
                                    // e aggiungo tutti i vertici necessari per evitare che gmsh vada in eccezione
                                    for (int i = 0; i < embeddedGeometries[shapes[c]].Length; i++)
                                    {
                                        GeometryBase geom = embeddedGeometries[shapes[c]][i];
                                        if (geom is Shape shapeToCut)
                                        {
                                            Shape embeddedShapeClone = (Shape)shapeToCut.Clone();
                                            embShapeCut = true;

                                            foreach (GeometryBase g in listOfAllEmbGeomForEmbShape)
                                            {
                                                if (g.GetHashCode() != geom.GetHashCode())
                                                {
                                                    for (int j = 0; j < shapeToCut.Fill.Explode().Length; j++)
                                                    {
                                                        Line3d[] perim = shapeToCut.Fill.Explode();

                                                        if (g is Point3d p3d || g is Point2d p2d)
                                                        {
                                                            Point3d point;
                                                            if (g is Point3d p3)
                                                                point = p3;
                                                            else
                                                                point = new Point3d((Point2d)g);

                                                            // Se il punto è sul lato è [i] e non è un vertica, allora aggiungilo
                                                            if (perim[j].IsPointOnLine(point, toleranceIntersection))
                                                            {
                                                                double dist1 = point.DistanceTo(perim[j].Start);
                                                                double dist2 = point.DistanceTo(perim[j].End);
                                                                double tol = ErrorPropagation.ProductTolerance(dist1, dist2, toleranceIntersection, toleranceIntersection);
                                                                if (Math.Abs(dist1) > tol && Math.Abs(dist2) > tol)
                                                                {
                                                                    addpoint = true;
                                                                    shapeToCut.Fill.Insert(j + 1, point);
                                                                    hashSet.Add(point);
                                                                    break;
                                                                }
                                                            }
                                                        }

                                                        else if (g is Line3d line3d || g is Line2d line2d)
                                                        {
                                                            Line3d line;
                                                            if (g is Line3d l)
                                                                line = l;
                                                            else
                                                                line = new Line3d((Line2d)g);

                                                            GenerateMeshGeometryPreProcessingAddLineToShape(shapeToCut.Fill, perim[j], line, j, toleranceIntersection, out int vertexAdded);
                                                            if (vertexAdded > 0)
                                                            {
                                                                addpoint = true;
                                                                hashSet.Add(line);
                                                                perim = shapeToCut.Fill.Explode();
                                                            }
                                                            if (vertexAdded == 2)
                                                            {
                                                                addpoint = true;
                                                                hashSet.Add(line);
                                                                break; // ho aggiunto tutti e due i vertici della linea
                                                            }
                                                            if (line.GetIntersection(perim[j], out Point3d p))
                                                            {
                                                                double dist1 = p.DistanceTo(perim[j].Start);
                                                                double dist2 = p.DistanceTo(perim[j].End);
                                                                double tol = ErrorPropagation.ProductTolerance(dist1, dist2, toleranceIntersection, toleranceIntersection);
                                                                if (Math.Abs(dist1) > tol && Math.Abs(dist2) > tol)
                                                                {
                                                                    addpoint = true;
                                                                    shapeToCut.Fill.Insert(j + 1, p);
                                                                    hashSet.Add(p);
                                                                    perim = shapeToCut.Fill.Explode();
                                                                }
                                                            }
                                                        }

                                                        else if (g is Polygon3d pg3d || g is Polygon2d pg2d)
                                                        {
                                                            Polygon3d polygon3d;
                                                            if (g is Polygon3d polyg)
                                                                polygon3d = polyg;
                                                            else
                                                                polygon3d = new Polygon3d((Polygon2d)g);

                                                            Line3d[] array = polygon3d.Explode();
                                                            for (int i1 = 0; i1 < array.Length; i1++)
                                                            {
                                                                Line3d line = array[i1];
                                                                GenerateMeshGeometryPreProcessingAddLineToShape(shapeToCut.Fill, perim[j], line, j, toleranceIntersection, out int vertexAdded);
                                                                if (vertexAdded > 0)
                                                                {
                                                                    addpoint = true;
                                                                    hashSet.Add(line);
                                                                    perim = shapeToCut.Fill.Explode();
                                                                }
                                                                if (vertexAdded == 2)
                                                                {
                                                                    addpoint = true;
                                                                    hashSet.Add(line);
                                                                    break; // ho aggiunto tutti e due i vertici della linea
                                                                }
                                                                if (line.GetIntersection(perim[j], out Point3d p))
                                                                {
                                                                    double dist1 = p.DistanceTo(perim[j].Start);
                                                                    double dist2 = p.DistanceTo(perim[j].End);
                                                                    double tol = ErrorPropagation.ProductTolerance(dist1, dist2, toleranceIntersection, toleranceIntersection);
                                                                    if (Math.Abs(dist1) > tol && Math.Abs(dist2) > tol)
                                                                    {
                                                                        addpoint = true;
                                                                        shapeToCut.Fill.Insert(j + 1, p);
                                                                        hashSet.Add(p);
                                                                        perim = shapeToCut.Fill.Explode();
                                                                    }
                                                                }
                                                            }
                                                        }

                                                        else if (g is Shape s)
                                                        {
                                                            if (!s.Equals(shapeToCut))
                                                            {
                                                                Line3d[] array = s.Fill.Explode();
                                                                for (int i1 = 0; i1 < array.Length; i1++)
                                                                {
                                                                    Line3d line = array[i1];
                                                                    GenerateMeshGeometryPreProcessingAddLineToShape(shapeToCut.Fill, perim[j], line, j, toleranceIntersection, out int vertexAdded);
                                                                    if (vertexAdded > 0)
                                                                    {
                                                                        addpoint = true;
                                                                        hashSet.Add(line);
                                                                        perim = shapeToCut.Fill.Explode();
                                                                    }
                                                                    if (vertexAdded == 2)
                                                                    {
                                                                        addpoint = true;
                                                                        hashSet.Add(line);
                                                                        break; // ho aggiunto tutti e due i vertici della linea
                                                                    }
                                                                    if (line.GetIntersection(perim[j], out Point3d p))
                                                                    {
                                                                        double dist1 = p.DistanceTo(perim[j].Start);
                                                                        double dist2 = p.DistanceTo(perim[j].End);
                                                                        double tol = ErrorPropagation.ProductTolerance(dist1, dist2, toleranceIntersection, toleranceIntersection);
                                                                        if (Math.Abs(dist1) > tol && Math.Abs(dist2) > tol)
                                                                        {
                                                                            addpoint = true;
                                                                            shapeToCut.Fill.Insert(j + 1, p);
                                                                            hashSet.Add(p);
                                                                            perim = shapeToCut.Fill.Explode();
                                                                        }
                                                                    }
                                                                }

                                                                if (s.HasHoles)
                                                                {
                                                                    for (int k = 0; k < s.Holes.Count(); k++)
                                                                    {
                                                                        Line3d[] arrayH = s.Holes[k].Explode();
                                                                        for (int i1 = 0; i1 < arrayH.Length; i1++)
                                                                        {
                                                                            Line3d line = arrayH[i1];
                                                                            GenerateMeshGeometryPreProcessingAddLineToShape(s.Holes[k], perim[j], line, j, toleranceIntersection, out int vertexAdded);
                                                                            if (vertexAdded > 0)
                                                                            {
                                                                                addpoint = true;
                                                                                hashSet.Add(line);
                                                                                perim = shapes[c].Fill.Explode();
                                                                            }
                                                                            if (vertexAdded == 2)
                                                                            {
                                                                                addpoint = true;
                                                                                hashSet.Add(line);
                                                                                break; // ho aggiunto tutti e due i vertici della linea
                                                                            }
                                                                        }
                                                                    }
                                                                }

                                                                if (s.HasChilds)
                                                                {
                                                                    throw new NotImplementedException();
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }

                                            // questo codice serve a uniformare la normale della geometria dei fori e delle shape embedded
                                            if (shapes[c].Fill.IsRightHandOrdered(toleranceIntersection))
                                            {
                                                if (embeddedGeometries.ContainsKey(shapes[c]))
                                                {
                                                    for (int m = 0; m < embeddedGeometries[shapes[c]].Length; m++)
                                                    {
                                                        GeometryBase geometry = embeddedGeometries[shapes[c]][m];
                                                        if (geometry.Equals(shapeToCut))
                                                            if (!shapeToCut.Fill.IsRightHandOrdered(toleranceIntersection))
                                                                shapeToCut.Reverse();
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                if (embeddedGeometries.ContainsKey(shapes[c]))
                                                {
                                                    for (int m = 0; m < embeddedGeometries[shapes[c]].Length; m++)
                                                    {
                                                        GeometryBase geometry = embeddedGeometries[shapes[c]][m];
                                                        if (geometry.Equals(shapeToCut))
                                                            if (shapeToCut.Fill.IsRightHandOrdered(toleranceIntersection))
                                                                shapeToCut.Reverse();
                                                    }
                                                }
                                            }

                                            // aggiungo key e value al dizionario di associazione tra shape emb modificata e non modificata
                                            if (!shapeOutputAssociation.ContainsKey(shapeToCut))
                                                shapeOutputAssociation.Add(shapeToCut, embeddedShapeClone);
                                        }
                                    }
                                }
                                catch (Exception e)
                                {
                                    generateMeshStatus.AddException(e, "Failed modify the embedded shape in order to add a vertex in correspondance of a embedded geometry vertex");
                                    Gmsh.Finalize();
                                    return false;
                                }
                            }

                            #endregion

                            #region PREPROCESSING SHAPE

                            if (embeddedGeometries != null)
                            {
                                // c è per tenere conto del fatto che quando aggiungi un punto su un lato lui debba ricontrollare il alto stesso
                                // per la embeddedgeometry successiva e però deve essere tolto dal totale dei cicli da fare
                                // Creo i lati della shape 
                                // per ogni geometria embedded

                                bool addHole = false;
                                List<Polygon3d> holesToAdd = new List<Polygon3d>();

                                for (int g = 0; g < listOfAllEmbGeom.Count; g++)
                                {
                                    Line3d[] perimeter = shapes[c].Fill.Explode();

                                    if (!hashSet.Contains(listOfAllEmbGeom.ElementAt(g)))
                                    {
                                        for (int i = 0; i < perimeter.Length; i++)
                                        {
                                            if (listOfAllEmbGeom.ElementAt(g) is Point3d pt3d || listOfAllEmbGeom.ElementAt(g) is Point2d pt2d)
                                            {
                                                Point3d point;
                                                if (listOfAllEmbGeom.ElementAt(g) is Point3d p3d)
                                                    point = p3d;
                                                else
                                                    point = new Point3d((Point2d)listOfAllEmbGeom.ElementAt(g));

                                                // Se il punto è sul lato è [i] e non è un vertica, allora aggiungilo
                                                if (perimeter[i].IsPointOnLine(point, toleranceIntersection) && !shapes[c].Fill.Contains(point))
                                                {
                                                    addpoint = true;
                                                    shapes[c].Fill.Insert(i + 1, point);
                                                    hashSet.Add(point);
                                                    break;
                                                }
                                            }
                                            else if (listOfAllEmbGeom.ElementAt(g) is Line3d line3d || listOfAllEmbGeom.ElementAt(g) is Line2d line2d)
                                            {
                                                Line3d line;
                                                if (listOfAllEmbGeom.ElementAt(g) is Line3d l)
                                                    line = l;
                                                else
                                                    line = new Line3d((Line2d)listOfAllEmbGeom.ElementAt(g));

                                                GenerateMeshGeometryPreProcessingAddLineToShape(shapes[c].Fill, perimeter[i], line, i, toleranceIntersection, out int vertexAdded);
                                                if (vertexAdded > 0)
                                                {
                                                    addpoint = true;
                                                    hashSet.Add(line);
                                                    perimeter = shapes[c].Fill.Explode();
                                                }
                                                if (vertexAdded == 2)
                                                {
                                                    addpoint = true;
                                                    hashSet.Add(line);
                                                    break; // ho aggiunto tutti e due i vertici della linea
                                                }
                                                if (line.GetIntersection(perimeter[i], out Point3d p, toleranceIntersection))
                                                {
                                                    double dist1 = p.DistanceTo(perimeter[i].Start);
                                                    double dist2 = p.DistanceTo(perimeter[i].End);
                                                    double tol = ErrorPropagation.ProductTolerance(dist1, dist2, toleranceIntersection, toleranceIntersection);
                                                    if (Math.Abs(dist1) > tol && Math.Abs(dist2) > tol)
                                                    {
                                                        addpoint = true;
                                                        shapes[c].Fill.Insert(i + 1, p);
                                                        hashSet.Add(p);
                                                        perimeter = shapes[c].Fill.Explode();
                                                    }
                                                }
                                            }
                                            else if (listOfAllEmbGeom.ElementAt(g) is Polygon3d p3d || listOfAllEmbGeom.ElementAt(g) is Polygon2d p2d)
                                            {
                                                Polygon3d polygon3d;
                                                if (listOfAllEmbGeom.ElementAt(g) is Polygon3d polyg)
                                                    polygon3d = polyg;
                                                else
                                                    polygon3d = new Polygon3d((Polygon2d)listOfAllEmbGeom.ElementAt(g));

                                                Line3d[] lines = polygon3d.Explode().ToArray();
                                                for (int l = 0; l < lines.Count(); l++)
                                                {
                                                    GenerateMeshGeometryPreProcessingAddLineToShape(shapes[c].Fill, perimeter[i], lines[l], i, toleranceIntersection, out int vertexAdded);
                                                    if (vertexAdded > 0)
                                                    {
                                                        addpoint = true;
                                                        hashSet.Add(lines[l]);
                                                        perimeter = shapes[c].Fill.Explode();
                                                    }
                                                    if (vertexAdded == 2)
                                                    {
                                                        addpoint = true;
                                                        hashSet.Add(lines[l]);
                                                        break; // ho aggiunto tutti e due i vertici della linea
                                                    }
                                                    if (lines[l].GetIntersection(perimeter[i], out Point3d p, toleranceIntersection))
                                                    {
                                                        double dist1 = p.DistanceTo(perimeter[i].Start);
                                                        double dist2 = p.DistanceTo(perimeter[i].End);
                                                        double tol = ErrorPropagation.ProductTolerance(dist1, dist2, toleranceIntersection, toleranceIntersection);
                                                        if (Math.Abs(dist1) > tol && Math.Abs(dist2) > tol)
                                                        {
                                                            addpoint = true;
                                                            shapes[c].Fill.Insert(i + 1, p);
                                                            hashSet.Add(p);
                                                            perimeter = shapes[c].Fill.Explode();
                                                        }
                                                    }
                                                }
                                            }
                                            else if (listOfAllEmbGeom.ElementAt(g) is Shape s)
                                            {
                                                Line3d[] shapeArray = s.Fill.Explode();
                                                for (int i1 = 0; i1 < shapeArray.Length; i1++)
                                                {
                                                    Line3d line = shapeArray[i1];
                                                    GenerateMeshGeometryPreProcessingAddLineToShape(shapes[c].Fill, perimeter[i], line, i, toleranceIntersection, out int vertexAdded);
                                                    if (vertexAdded > 0)
                                                    {
                                                        addpoint = true;
                                                        hashSet.Add(line);
                                                        perimeter = shapes[c].Fill.Explode();
                                                    }
                                                    if (vertexAdded == 2)
                                                    {
                                                        addpoint = true;
                                                        hashSet.Add(line);
                                                        break; // ho aggiunto tutti e due i vertici della linea
                                                    }
                                                    if (line.GetIntersection(perimeter[i], out Point3d p, toleranceIntersection))
                                                    {
                                                        double dist1 = p.DistanceTo(perimeter[i].Start);
                                                        double dist2 = p.DistanceTo(perimeter[i].End);
                                                        double tol = ErrorPropagation.ProductTolerance(dist1, dist2, toleranceIntersection, toleranceIntersection);
                                                        if (Math.Abs(dist1) > tol && Math.Abs(dist2) > tol)
                                                        {
                                                            addpoint = true;
                                                            shapes[c].Fill.Insert(i + 1, p);
                                                            hashSet.Add(p);
                                                            perimeter = shapes[c].Fill.Explode();
                                                        }
                                                    }
                                                }

                                                if (s.HasHoles)
                                                {
                                                    for (int j = 0; j < s.Holes.Count(); j++)
                                                    {
                                                        Line3d[] holeArray = s.Holes[j].Explode();
                                                        for (int i1 = 0; i1 < holeArray.Length; i1++)
                                                        {
                                                            Line3d line = holeArray[i1];
                                                            GenerateMeshGeometryPreProcessingAddLineToShape(s.Holes[j], perimeter[i], line, i, toleranceIntersection, out int vertexAdded);
                                                            if (vertexAdded > 0)
                                                            {
                                                                addpoint = true;
                                                                hashSet.Add(line);
                                                                perimeter = shapes[c].Fill.Explode();
                                                            }
                                                            if (vertexAdded == 2)
                                                            {
                                                                addpoint = true;
                                                                hashSet.Add(line);
                                                                break; // ho aggiunto tutti e due i vertici della linea
                                                            }
                                                        }
                                                    }
                                                }

                                                if (s.HasChilds)
                                                {
                                                    throw new NotImplementedException();
                                                }
                                            }
                                        }

                                        if (listOfAllEmbGeom.ElementAt(g) is Shape sh)
                                        {
                                            if (shapes[c].IsPolygonInside(sh.Fill))
                                            {
                                                holesToAdd.Add(sh.Fill);
                                                addHole = true;
                                                addpoint = true;
                                            }
                                        }
                                    }


                                    if (shapes[c].HasHoles)
                                    {
                                        embShapeCut = true;

                                        for (int k = 0; k < shapes[c].Holes.Count(); k++)
                                        {
                                            Line3d[] perimeterHole = shapes[c].Holes[k].Explode();

                                            for (int j = 0; j < perimeterHole.Length; j++)
                                            {
                                                if (listOfAllEmbGeom.ElementAt(g) is Point3d p3d || listOfAllEmbGeom.ElementAt(g) is Point2d p2d)
                                                {
                                                    Point3d point;
                                                    if (listOfAllEmbGeom.ElementAt(g) is Point3d p3)
                                                        point = p3;
                                                    else
                                                        point = new Point3d((Point2d)listOfAllEmbGeom.ElementAt(g));

                                                    // Se il punto è sul lato è [i] e non è un vertica, allora aggiungilo
                                                    if (perimeterHole[j].IsPointOnLine(point, toleranceIntersection))
                                                    {
                                                        addpoint = true;
                                                        shapes[c].Holes[k].Insert(j + 1, point);
                                                        hashSet.Add(point);
                                                        break;
                                                    }
                                                }
                                                else if (listOfAllEmbGeom.ElementAt(g) is Line3d line3d || listOfAllEmbGeom.ElementAt(g) is Line2d line2d)
                                                {
                                                    Line3d line;
                                                    if (listOfAllEmbGeom.ElementAt(g) is Line3d l)
                                                        line = l;
                                                    else
                                                        line = new Line3d((Line2d)listOfAllEmbGeom.ElementAt(g));

                                                    GenerateMeshGeometryPreProcessingAddLineToShape(shapes[c].Holes[k], perimeterHole[j], line, j, toleranceIntersection, out int vertexAdded);
                                                    if (vertexAdded > 0)
                                                    {
                                                        addpoint = true;
                                                        hashSet.Add(line);
                                                        perimeterHole = shapes[c].Holes[k].Explode();
                                                    }
                                                    if (vertexAdded == 2)
                                                    {
                                                        addpoint = true;
                                                        hashSet.Add(line);
                                                        break; // ho aggiunto tutti e due i vertici della linea
                                                    }
                                                    if (line.GetIntersection(perimeterHole[j], out Point3d p, toleranceIntersection))
                                                    {
                                                        if (Math.Abs(p.DistanceTo(perimeterHole[j].Start)) > toleranceIntersection && Math.Abs(p.DistanceTo(perimeterHole[j].End)) > toleranceIntersection)
                                                        {
                                                            addpoint = true;
                                                            shapes[c].Holes[k].Insert(j + 1, p);
                                                            hashSet.Add(p);
                                                            perimeterHole = shapes[c].Holes[k].Explode();
                                                        }
                                                    }
                                                }
                                                else if (listOfAllEmbGeom.ElementAt(g) is Polygon3d pg3d || listOfAllEmbGeom.ElementAt(g) is Polygon2d pg2d)
                                                {
                                                    Polygon3d polygon3d;
                                                    if (listOfAllEmbGeom.ElementAt(g) is Polygon3d polyg)
                                                        polygon3d = polyg;
                                                    else
                                                        polygon3d = new Polygon3d((Polygon2d)listOfAllEmbGeom.ElementAt(g));

                                                    Line3d[] lines = polygon3d.Explode().ToArray();
                                                    for (int l = 0; l < lines.Count(); l++)
                                                    {
                                                        GenerateMeshGeometryPreProcessingAddLineToShape(shapes[c].Holes[k], perimeterHole[j], lines[l], j, toleranceIntersection, out int vertexAdded);
                                                        if (vertexAdded > 0)
                                                        {
                                                            addpoint = true;
                                                            hashSet.Add(lines[l]);
                                                            perimeterHole = shapes[c].Holes[k].Explode();
                                                        }
                                                        if (vertexAdded == 2)
                                                        {
                                                            addpoint = true;
                                                            hashSet.Add(lines[l]);
                                                            break; // ho aggiunto tutti e due i vertici della linea
                                                        }
                                                        if (lines[l].GetIntersection(perimeterHole[j], out Point3d p, toleranceIntersection))
                                                        {
                                                            if (Math.Abs(p.DistanceTo(perimeterHole[j].Start)) > toleranceIntersection && Math.Abs(p.DistanceTo(perimeterHole[j].End)) > toleranceIntersection)
                                                            {
                                                                addpoint = true;
                                                                shapes[c].Holes[k].Insert(j + 1, p);
                                                                hashSet.Add(p);
                                                                perimeterHole = shapes[c].Holes[k].Explode();
                                                            }
                                                        }
                                                    }
                                                }
                                                else if (listOfAllEmbGeom.ElementAt(g) is Shape s)
                                                {
                                                    if (s.HasHoles)
                                                    {
                                                        throw new NotImplementedException();
                                                    }

                                                    if (s.HasChilds)
                                                    {
                                                        throw new NotImplementedException();
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }

                                // aggiungo i fori (shape emb) alla shape
                                if (addHole)
                                {
                                    for (int i = 0; i < holesToAdd.Count; i++)
                                    {
                                        shapes[c].AddHole(holesToAdd[i]);
                                    }
                                }

                                // serve per evitare che gmsh importi punti doppi => crash
                                shapes[c].Fill.RemoveDuplicatedPoints(toleranceIntersection);
                                if (shapes[c].HasHoles)
                                {
                                    for (int i = 0; i < shapes[c].Holes.Length; i++)
                                    {
                                        shapes[c].Holes[i].RemoveDuplicatedPoints(toleranceIntersection);
                                    }
                                }

                                // aggiorno i dizionari
                                if (addpoint && embGeomClone != null)
                                {
                                    if (!shapes[c].Equals(clone))
                                    {
                                        embeddedGeometries.Add(shapes[c], embGeomClone);
                                        embGeomAssociation.Add(shapes[c], embMeshSizeBuffer);
                                        embeddedGeometries.Remove(clone);
                                    }
                                }
                            }

                            #endregion
                        }
                        catch (Exception e)
                        {
                            generateMeshStatus.AddException(e, "Failed modify the shape in order to add a vertex in correspondance of a embedded geometry vertex");
                            Gmsh.Finalize();
                            return false;
                        }

                        #endregion

                        DateTime dt2_2 = DateTime.Now;
                        preProctime += (dt2_2 - dt2_1).TotalSeconds;

                        #region GENERAZIONE DELLA GEOMETRIA DENTRO OPENCASCADE

                        try
                        {
                            int geometryDim = 2;

                            Line3d[] lines = shapes[c].Fill.Explode();
                            var occLineTags = new int[shapes[c].Fill.Count];

                            // Inserisco un punto nel cad per ogni punto che prima ho inserito nella mia shape
                            // Creo SOLO i bordi della shape
                            for (int i = 0; i < lines.Length; i++)
                            {
                                int n1 = occw.AddPoint(lines[i].Start, tolerance);
                                int n2 = occw.AddPoint(lines[i].End, tolerance);
                                occLineTags[i] = Gmsh.Model.Occ.AddLine(n1, n2);
                            }

                            int wireTag = Gmsh.Model.Occ.AddWire(occLineTags);
                            int surfaceTag = Gmsh.Model.Occ.AddPlaneSurface(new int[1] { wireTag });

                            if (shapes[c].HasHoles)
                            {
                                (int, int)[] cuttedSurfaceTags = new (int, int)[1] { (2, surfaceTag) };
                                List<(int, int)> listOfHoles = new List<(int, int)>();

                                for (int h = 0; h < shapes[c].Holes.Count(); h++)
                                {
                                    Line3d[] holeLines = shapes[c].Holes[h].Explode();
                                    var holeLineTags = new int[shapes[c].Holes[h].Count];

                                    for (int i = 0; i < holeLines.Length; i++)
                                    {
                                        int n1 = occw.AddPoint(holeLines[i].Start, tolerance);
                                        int n2 = occw.AddPoint(holeLines[i].End, tolerance);
                                        holeLineTags[i] = Gmsh.Model.Occ.AddLine(n1, n2);
                                    }

                                    int holeWireTag = Gmsh.Model.Occ.AddWire(holeLineTags);
                                    int holeSurfaceTag = Gmsh.Model.Occ.AddPlaneSurface(new int[1] { holeWireTag });

                                    (int, int) holeDimTag = (2, holeSurfaceTag);
                                    listOfHoles.Add(holeDimTag);
                                }

                                Gmsh.Model.Occ.Synchronize();
                                Gmsh.Model.Occ.Cut(new (int, int)[] { (geometryDim, cuttedSurfaceTags[0].Item2) }, (listOfHoles.ToArray()),
                                    out (int, int)[] outCuttedSurfaceTags, out (int, int)[][] resultMap, -1, true, true);
                                Gmsh.Model.Occ.Synchronize();

                                for (int i = 0; i < outCuttedSurfaceTags.Length; i++)
                                    occw.RebuildObjectTags(outCuttedSurfaceTags[i].Item2);

                                physicalGroupTagSurfacesAssociation.Add((physicalGroupTag++, geometryDim, outCuttedSurfaceTags.Select(i => i.Item2).ToArray(), shapes[c]));
                            }

                            else
                            {
                                physicalGroupTagSurfacesAssociation.Add((physicalGroupTag++, geometryDim, new int[1] { surfaceTag }, shapes[c]));
                            }
                        }
                        catch (GmshException ge)
                        {
                            generateMeshStatus.AddException(ge, $"{GMeshGenerateMeshStatus.FailedToCreateTheShape}: {Array.IndexOf(shapes.ToArray(), shapes[c])} in CAD.");
                            return false;
                        }


                        #endregion

                        DateTime dt3 = DateTime.Now;
                        geomCADTime += (dt3 - dt2_2).TotalSeconds;

                    }

                    Gmsh.Model.Occ.Synchronize();

                    DateTime dt3_3 = DateTime.Now;

                    if (embShapeCut)
                        occw.RebuildObjectTagsAndCheck(toleranceIntersection);
                    else
                        occw.RebuildObjectTags();

                    DateTime dt3_4 = DateTime.Now;
                    rebuildObjectTagsTime += (dt3_4 - dt3_3).TotalSeconds;
                }
                catch (Exception e)
                {
                    generateMeshStatus.AddException(e, "Failed to add the shapes into CAD");
                    Gmsh.Finalize();
                    return false;
                }

                #endregion

                DateTime dt3_2 = DateTime.Now;

                #region FRAGMENT

                // Spezza superfici e garantisce la congruenza fra superfici adiacenti
                // Da ad ogni shape di partenza un physical group, poi le spezza mantenendo nelle proprietà il PG corretto ma creando più superfici
                // Gestisce n shape con n tagli.
                try
                {
                    if (physicalGroupTagSurfacesAssociation.Count > 1)
                    {
                        // creo un array (int,int) in cui associo ad ogni PhG la propria superficie (in questa fase sono associate 1 a 1). 
                        // poi faccio il fragment tra questo array e se stesso, in modo che trovi tutte le intersezioni e generi le geometrie di cui ha bisogno. 
                        (int, int)[] dimSurfaceTagBuffer = physicalGroupTagSurfacesAssociation.Select(i => new { i.dim, i.surfacesTag }).
                            Select(i => new ValueTuple<int, int>(i.dim, i.surfacesTag[0])).ToArray();
                        try
                        {
                            Gmsh.Model.Occ.Fragment(dimSurfaceTagBuffer, dimSurfaceTagBuffer, out (int, int)[] result, out (int, int)[][] resultMap, -1, true, true);
                            Gmsh.Model.Occ.Synchronize();

                            for (int i = 0; i < physicalGroupTagSurfacesAssociation.Count; i++)
                            {
                                int physicalTag = physicalGroupTagSurfacesAssociation[i].physicalGroupTag;
                                int dim = physicalGroupTagSurfacesAssociation[i].dim;
                                int surfaceTag = physicalGroupTagSurfacesAssociation[i].surfacesTag[0];

                                var newSurfaceTags = resultMap[i].Where(j => j.Item2 != 0).Select(j => j.Item2).ToArray();

                                int newPhysicalTag = Gmsh.Model.AddPhysicalGroup(dim, newSurfaceTags, physicalTag);

                                physicalGroupTagSurfacesAssociation[i] = (newPhysicalTag, dim, newSurfaceTags, physicalGroupTagSurfacesAssociation[i].shape);

                                generateMeshStatus.GeneratedSurfaces = i;
                            }
                        }
                        catch (GmshException e)
                        {
                            generateMeshStatus.AddException(e, "Failed to find shapes intersections");
                            Gmsh.Finalize();
                            return false;
                        }
                        catch (Exception e)
                        {
                            generateMeshStatus.AddException(e, "Failed to find shapes intersections");
                            Gmsh.Finalize();
                            return false;
                        }

                        DateTime dt3_3 = DateTime.Now;

                        occw.RebuildObjectTags();

                        DateTime dt3_4 = DateTime.Now;
                        rebuildObjectTagsTime += (dt3_4 - dt3_3).TotalSeconds;
                    }
                    else
                    {
                        Gmsh.Model.AddPhysicalGroup(physicalGroupTagSurfacesAssociation.First().dim, physicalGroupTagSurfacesAssociation.First().surfacesTag,
                            physicalGroupTagSurfacesAssociation.First().physicalGroupTag);
                        generateMeshStatus.GeneratedSurfaces = 1;
                    }

                    Gmsh.Model.Occ.Synchronize();
                }
                catch (Exception e)
                {
                    generateMeshStatus.AddException(e, "Failed split the shapes");
                    Gmsh.Finalize();
                    return false;
                }

                #endregion

                DateTime dt4 = DateTime.Now;

                #region TRANSFINITE BOUNDARY

                try
                {
                    if (options.Transfinite && !options.TransfiniteSurface)
                    {
                        for (int k = 0; k < shapes.Count(); k++)
                        {
                            int physicalTag = physicalGroupTagSurfacesAssociation.Where(i => i.shape == shapes[k]).First().physicalGroupTag;

                            for (int count = 0; count < physicalGroupTagSurfacesAssociation.Where(i => i.shape == shapes[k]).First().surfacesTag.Length; count++)
                            {
                                int surfaceTag = physicalGroupTagSurfacesAssociation.Where(i => i.shape == shapes[k]).First().surfacesTag[count];
                                var boundaryCurvesTag = Gmsh.Model.GetBoundary(new (int, int)[1] { (2, surfaceTag) }, false, false, false).Select(i => i.Item2).ToList();

                                for (int j = 0; j < boundaryCurvesTag.Count(); j++)
                                {
                                    try
                                    {
                                        TransfiniteLine(boundaryCurvesTag[j], options.MeshSize, options.TransfiniteLineType.ToString(), options.TransfiniteFactor);
                                    }
                                    catch (GmshException ge)
                                    {
                                        generateMeshStatus.AddException(ge, "Boundary transfinite failed");
                                        Gmsh.Finalize();
                                        return false;
                                    }
                                }
                            }
                        }
                    }

                    Gmsh.Model.Occ.Synchronize();
                }
                catch (Exception e)
                {
                    generateMeshStatus.AddException(e, "Boundary transfinite failed");
                    Gmsh.Finalize();
                    return false;
                }

                #endregion

                DateTime dt5 = DateTime.Now;

                #region GESTIONE DELLE GEOMETRIE EMBEDDED

                // Include geometrie in superfici es linea in superfice // punto in superficie // poligono in superficie // superdicie in superficie
                // riconosce a quale superficie (prima creata) la geometria appartiene e la associa a quel Physical group.

                #region SHAPE

                #region SPLIT SHAPE 

                // devo crearmi questa lista perchè la libreria di clipper rimuove i punti allineati e quindi perdo i punti precedentemente aggiunti alla shape
                // devo quindi ricontrollare che la shape abbia quei punti ed eventualmente riaggiungerli
                List<Point3d> shapePoints = new List<Point3d>();
                if (embeddedGeometries != null)
                {
                    for (int s = 0; s < shapes.Count(); s++)
                    {
                        if (embeddedGeometries.ContainsKey(shapes[s]))
                        {
                            for (int k = 0; k < embeddedGeometries[shapes[s]].Count(); k++)
                            {
                                if (embeddedGeometries[shapes[s]][k] is Shape embS)
                                {
                                    shapePoints.AddRange(embS.Fill);
                                }
                            }
                        }
                    }
                }

                // associa ogni shape con le relative shape divise (se ci sono) 
                Dictionary<Shape, Shape[]> shapeToShapeSplitted = new Dictionary<Shape, Shape[]>();
                try
                {
                    if (embeddedGeometries != null)
                    {
                        HashSet<Shape> hashSetIntersection = new HashSet<Shape>();

                        for (int s = 0; s < shapes.Count(); s++)
                        {
                            bool hasShapeEmb = false;

                            // lista di tutte le shape embeddate diverse dalla shape [shapes[s]]
                            List<Shape> listOfEmbShapes = new List<Shape>();

                            if (embeddedGeometries.ContainsKey(shapes[s]))
                            {
                                for (int i = 0; i < embeddedGeometries[shapes[s]].Length; i++)
                                {
                                    if (embeddedGeometries[shapes[s]][i] is Shape embS)
                                    {
                                        listOfEmbShapes.Add(embS);
                                        hasShapeEmb = true;
                                    }
                                }
                            }

                            if (hasShapeEmb)
                            {
                                for (int i = 0; i < listOfEmbShapes.Count; i++)
                                {
                                    List<Shape> listOfShapeSplitter = new List<Shape>();
                                    List<Shape> listOfShapeDiffAndIntersect = new List<Shape>();
                                    List<Shape> listOfShapeIntersectBuffer = new List<Shape>();

                                    for (int k = 0; k < listOfEmbShapes.Count; k++)
                                    {
                                        Shape shapeSplitter = listOfEmbShapes[k];
                                        if (!shapeSplitter.Equals(listOfEmbShapes[i]))
                                        {
                                            listOfShapeSplitter.Add(shapeSplitter);
                                        }
                                    }

                                    if (listOfShapeSplitter.Count() != 0)
                                    {
                                        // questo procedimento serve a gestire tutti i casi possibili. crea tutte le intersezioni con il metodo di intersection
                                        // e poi crea l'associazione tra la shape e le rispettive parti aggiungendole al dizionario shapeToShapeSplitted

                                        for (int k = 0; k < listOfShapeSplitter.Count; k++)
                                        {
                                            Shape shapeBuffer = listOfShapeSplitter[k];
                                            if (Shape.Intersection(listOfEmbShapes[i], shapeBuffer, out Shape[] intersectShape))
                                                listOfShapeIntersectBuffer.AddRange(intersectShape.ToList());     // aggiungo shape intersection
                                        }

                                        if (listOfShapeIntersectBuffer.Count > 1)
                                        {
                                            for (int k = 0; k < listOfShapeIntersectBuffer.Count; k++)
                                            {
                                                for (int v = 0; v < listOfShapeIntersectBuffer.Count; v++)
                                                {
                                                    if (v > k)
                                                    {
                                                        if (Shape.Intersection(listOfShapeIntersectBuffer[v], listOfShapeIntersectBuffer[k], out Shape[] sh))
                                                        {
                                                            listOfShapeDiffAndIntersect.AddRange(sh.ToList());

                                                            if (Shape.Difference(listOfShapeIntersectBuffer[v], listOfShapeIntersectBuffer[k], out Shape[] shDiffA))
                                                                listOfShapeDiffAndIntersect.AddRange(shDiffA.ToList());
                                                            if (Shape.Difference(listOfShapeIntersectBuffer[k], listOfShapeIntersectBuffer[v], out Shape[] shDiffB))
                                                                listOfShapeDiffAndIntersect.AddRange(shDiffB.ToList());
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            listOfShapeDiffAndIntersect.AddRange(listOfShapeIntersectBuffer.ToList());     // aggiungo shape intersection    
                                        }

                                        if (listOfShapeIntersectBuffer.Count > 0)
                                            if (Shape.Union(listOfShapeIntersectBuffer.ToArray(), listOfShapeIntersectBuffer.ToArray(), out Shape[] union))
                                                listOfShapeIntersectBuffer = union.ToList();

                                        if (Shape.Difference(new Shape[] { listOfEmbShapes[i] }, listOfShapeIntersectBuffer.ToArray(), out Shape[] diffShape))
                                        {
                                            listOfShapeDiffAndIntersect.AddRange(diffShape.ToList());     // aggiungo shape difference                                              

                                            // questo metodo riaggiunge i vertici alle shape spezzate che clipper ha tolto
                                            if (listOfShapeDiffAndIntersect.Count() > 1)
                                            {
                                                for (int k = 0; k < listOfShapeDiffAndIntersect.Count(); k++)
                                                {
                                                    for (int p = 0; p < listOfShapeDiffAndIntersect[k].Fill.Explode().Count(); p++)
                                                    {
                                                        for (int j = 0; j < shapePoints.Count; j++)
                                                        {
                                                            Line3d[] perim = listOfShapeDiffAndIntersect[k].Fill.Explode();

                                                            // Se il punto è sul lato è [i] e non è un vertica, allora aggiungilo
                                                            if (perim[p].IsPointOnLine(shapePoints[j], toleranceIntersection))
                                                            {
                                                                double dist1 = shapePoints[j].DistanceTo(perim[p].Start);
                                                                double dist2 = shapePoints[j].DistanceTo(perim[p].End);
                                                                double tol = ErrorPropagation.ProductTolerance(dist1, dist2, toleranceIntersection, toleranceIntersection);
                                                                if (Math.Abs(dist1) > tol && Math.Abs(dist2) > tol)
                                                                {
                                                                    listOfShapeDiffAndIntersect[k].Fill.Insert(p + 1, shapePoints[j]);
                                                                    break;
                                                                }
                                                            }
                                                        }
                                                    }
                                                }

                                                shapeToShapeSplitted.Add(listOfEmbShapes[i], listOfShapeDiffAndIntersect.ToArray());      //associo shape con le relative sottoshape 
                                            }
                                            else
                                                shapeToShapeSplitted.Add(listOfEmbShapes[i], new Shape[] { listOfEmbShapes[i] });
                                        }
                                        else
                                            shapeToShapeSplitted.Add(listOfEmbShapes[i], new Shape[] { listOfEmbShapes[i] });
                                    }
                                    else
                                        shapeToShapeSplitted.Add(listOfEmbShapes[i], new Shape[] { listOfEmbShapes[i] });
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    generateMeshStatus.AddException(e, $"Failed to split the embedded shape");
                    Gmsh.Finalize();
                    return false;
                }

                #endregion

                #region EMBED SHAPE

                try
                {
                    if (embeddedGeometries != null)
                    {
                        foreach (Shape shape in shapes)
                        {
                            if (embeddedGeometries.ContainsKey(shape))
                            {
                                int physicalTag = physicalGroupTagSurfacesAssociation.Where(i => i.shape == shape).First().physicalGroupTag;
                                embeddedGeometriesTagAssociation[physicalTag] = new Dictionary<GeometryBase, int[]>();

                                // aggiungo a questa lista una shape ogni volta che la embeddo
                                List<Shape> listEmbShape = new List<Shape>();
                                // dizionario di associazione Shape e tag surface
                                Dictionary<Shape, int> shapeSurfaceTagAssociation = new Dictionary<Shape, int>();

                                for (int j = 0; j < embeddedGeometries[shape].Length; j++)
                                {
                                    GeometryBase geometry = embeddedGeometries[shape][j];
                                    List<int> surfTagBuffer = new List<int>();
                                    if (geometry is Shape sh)
                                    {
                                        for (int k = 0; k < shapeToShapeSplitted[sh].Length; k++)
                                        {
                                            Shape embS = shapeToShapeSplitted[sh][k];
                                            for (int count = 0; count < physicalGroupTagSurfacesAssociation.Where(i => i.shape == shape).First().surfacesTag.Length; count++)
                                            {
                                                int[] SurfaceTagArray = physicalGroupTagSurfacesAssociation.Where(i => i.shape == shape).First().surfacesTag.ToArray();
                                                bool match = false;

                                                // controllo che non sia già stata aggiunta.
                                                // se è stata aggiunta, mi prendo il tag, altrimenti vado avanti
                                                for (int i = 0; i < listEmbShape.Count; i++)
                                                {
                                                    Shape s = listEmbShape[i];
                                                    if (embS.EqualsShifted(s))
                                                    {
                                                        shapeSurfaceTagAssociation.TryGetValue(s, out int tg);
                                                        surfTagBuffer.Add(tg);
                                                        match = true;
                                                        break;
                                                    }
                                                }

                                                if (!match)
                                                {
                                                    try
                                                    {
                                                        // vado a creare la superficie
                                                        Line3d[] lines = embS.Fill.Explode();
                                                        int[] occLineTags = new int[embS.Fill.Count];

                                                        for (int i = 0; i < lines.Length; i++)
                                                        {
                                                            occLineTags[i] = occw.AddLineAndSync(lines[i], tolerance);
                                                        }

                                                        int wireTag = Gmsh.Model.Occ.AddWire(occLineTags);
                                                        int EmbSurfaceTag = Gmsh.Model.Occ.AddPlaneSurface(new int[1] { wireTag });
                                                        Gmsh.Model.Occ.Synchronize();

                                                        if (options.Transfinite)
                                                        {
                                                            for (int i = 0; i < occLineTags.Length; i++)
                                                            {
                                                                int tag = occLineTags[i];
                                                                try
                                                                {
                                                                    if (embeddedGeomMeshSize != null && embeddedGeomMeshSize.ContainsKey(embS))
                                                                        TransfiniteLine(tag, embeddedGeomMeshSize[embS], options.TransfiniteLineType.ToString(), options.TransfiniteFactor);

                                                                    else
                                                                        TransfiniteLine(tag, options.MeshSize, options.TransfiniteLineType.ToString(), options.TransfiniteFactor);
                                                                }
                                                                catch (Exception e)
                                                                {
                                                                    generateMeshStatus.AddException(e, $"{GMeshGenerateMeshStatus.FailedToTransfinite} for curveTag:{tag}. Line Start:{lines[tag].Start * options.MeshScalingFactor} Line End{lines[tag].End * options.MeshScalingFactor}");
                                                                    return false;
                                                                }
                                                            }
                                                        }

                                                        // per ora embedda la shape nella superficie dov'è il primo vertice
                                                        int surfaceWhereEmbedShape = -1;
                                                        for (int i = 0; i < SurfaceTagArray.Length; i++)
                                                        {
                                                            int newSurfaceTag = SurfaceTagArray[i];
                                                            int t = IsInside(lines[0].Start, newSurfaceTag);
                                                            if (t != -1)
                                                            {
                                                                surfaceWhereEmbedShape = t;
                                                                break;
                                                            }
                                                        }

                                                        if (embS.HasHoles)
                                                        {
                                                            (int, int)[] outCuttedSurfaceTags = new (int, int)[1] { (2, EmbSurfaceTag) };

                                                            for (int h = 0; h < embS.Holes.Length; h++)
                                                            {
                                                                Polygon3d hole = embS.Holes[h];
                                                                Line3d[] holeLines = hole.Explode();
                                                                int[] holeLineTags = new int[hole.Count];

                                                                for (int i = 0; i < holeLines.Length; i++)
                                                                {
                                                                    holeLineTags[i] = occw.AddLineAndSync(holeLines[i], tolerance);
                                                                    if (options.Transfinite)
                                                                    {
                                                                        try
                                                                        {
                                                                            if (embeddedGeomMeshSize != null && embeddedGeomMeshSize.ContainsKey(embS))
                                                                                TransfiniteLine(holeLineTags[i], embeddedGeomMeshSize[embS], options.TransfiniteLineType.ToString(), options.TransfiniteFactor);

                                                                            else
                                                                                TransfiniteLine(holeLineTags[i], options.MeshSize, options.TransfiniteLineType.ToString(), options.TransfiniteFactor);
                                                                        }
                                                                        catch (Exception e)
                                                                        {
                                                                            generateMeshStatus.AddException(e, $"{GMeshGenerateMeshStatus.FailedToTransfinite} for curveTag:{holeLineTags[i]}. " +
                                                                                $"Line Start:{holeLines[i].Start * options.MeshScalingFactor} Line End{holeLines[i].End * options.MeshScalingFactor}");
                                                                            return false;
                                                                        }
                                                                    }
                                                                }

                                                                int holeWireTag = Gmsh.Model.Occ.AddWire(holeLineTags);
                                                                int holeSurfaceTag = Gmsh.Model.Occ.AddPlaneSurface(new int[1] { holeWireTag });
                                                                Gmsh.Model.Occ.Synchronize();
                                                                Gmsh.Model.Occ.Cut(new (int, int)[] { (2, outCuttedSurfaceTags[0].Item2) }, new (int, int)[] { (2, holeSurfaceTag) },
                                                                                    out outCuttedSurfaceTags, out (int, int)[][] resultMap, -1, true, true);
                                                            }
                                                        }

                                                        surfTagBuffer.Add(EmbSurfaceTag);

                                                        generateMeshStatus.EmbeddedShapes += 1;
                                                        listEmbShape.Add(embS);

                                                        int phgTag = Gmsh.Model.AddPhysicalGroup(2, new int[] { EmbSurfaceTag });

                                                        // aggiungo il tag ai relativi dizionari
                                                        shapeSurfaceTagAssociation.Add(embS, EmbSurfaceTag);
                                                        physicalGroupMeshTagAssociation.Add((physicalTag, phgTag, EmbSurfaceTag));

                                                        break;
                                                    }
                                                    catch (Exception e)
                                                    {
                                                        generateMeshStatus.AddException(e, $"{GMeshGenerateMeshStatus.FailedToEmbedTheShape} {geometry}");
                                                        Gmsh.Finalize();
                                                        return false;
                                                    }
                                                }
                                            }
                                        }
                                        embeddedGeometriesTagAssociation[physicalTag].Add(geometry, surfTagBuffer.ToArray());
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    generateMeshStatus.AddException(e, $"Failed to embed the shape");
                    Gmsh.Finalize();
                    return false;
                }

                #endregion

                #endregion

                //Gmsh.Fltk.Run();

                #region POINT, LINE, POLYGON              

                try
                {
                    if (embeddedGeometries != null)
                    {
                        for (int t = 0; t < shapes.Count(); t++)
                        {
                            // Creo una lista splitLine di tutte le linee (linee2d/3d polygon2d/3d) che andrò ad embeddare
                            // ne cerco le intersezioni e le inserisco nella lista splitPoints

                            // lista di tutte le linee emb
                            HashSet<Line3d> splitLine = new HashSet<Line3d>();
                            // associo le linee spezzate alla geometria originale
                            Dictionary<Line3d, Line3d[]> splitLines = new Dictionary<Line3d, Line3d[]>();
                            // lista dei punti di intersezione
                            HashSet<Point3d> splitPoints = new HashSet<Point3d>();
                            // hashset per controllare di non controllare la stessa geometria 2 volte
                            HashSet<GeometryBase> hashSet = new HashSet<GeometryBase>();

                            if (embeddedGeometries.ContainsKey(shapes[t]))
                            {
                                #region INTERSEZIONI TRA GEOMETRIE EMBEDDED

                                // cerco le intersezioni con le altre embedded geometry (se ho almeno 2 geometrie emb)
                                if (embeddedGeometries[shapes[t]].Count() > 0)
                                {
                                    for (int k = 0; k < shapes.Length; k++)
                                        for (int j = 0; j < shapes[k].Fill.Count; j++)
                                            splitPoints.Add(shapes[k].Fill[j]);

                                    if (shapes[t].HasHoles)
                                        for (int h = 0; h < shapes[t].Holes.Length; h++)
                                            for (int j = 0; j < shapes[t].Holes[h].Count; j++)
                                                splitPoints.Add(shapes[t].Holes[h][j]);

                                    for (int i = 0; i < embeddedGeometries[shapes[t]].Count(); i++)
                                    {
                                        Line3d lineToTest;
                                        if (embeddedGeometries[shapes[t]][i] is Point3d p3d)
                                        {
                                            splitPoints.Add(p3d);
                                        }
                                        else if (embeddedGeometries[shapes[t]][i] is Point2d p2d)
                                        {
                                            Point3d pointToTest = new Point3d((Point2d)embeddedGeometries[shapes[t]][i]);
                                            splitPoints.Add(pointToTest);
                                        }
                                        else if (embeddedGeometries[shapes[t]][i] is Line3d l3d)
                                        {
                                            splitLine.Add(l3d);
                                        }
                                        else if (embeddedGeometries[shapes[t]][i] is Line2d l2d)
                                        {
                                            lineToTest = new Line3d((Line2d)embeddedGeometries[shapes[t]][i]);
                                            splitLine.Add(lineToTest);
                                        }
                                        else if (embeddedGeometries[shapes[t]][i] is Polygon3d poly3d)
                                        {
                                            Line3d[] poly3dArray = poly3d.Explode();
                                            for (int i1 = 0; i1 < poly3dArray.Length; i1++)
                                            {
                                                splitLine.Add(poly3dArray[i1]);
                                            }
                                        }
                                        else if (embeddedGeometries[shapes[t]][i] is Polygon2d poly2d)
                                        {
                                            Line2d[] poly2dArray = poly2d.Explode();
                                            for (int i1 = 0; i1 < poly2dArray.Length; i1++)
                                            {
                                                lineToTest = new Line3d(poly2dArray[i1]);
                                                splitLine.Add(lineToTest);
                                            }
                                        }
                                        else if (embeddedGeometries[shapes[t]][i] is Shape s)
                                        {
                                            Line3d[] sFillArray = s.Fill.Explode();
                                            for (int i1 = 0; i1 < sFillArray.Length; i1++)
                                            {
                                                splitPoints.Add(sFillArray[i1].Start);
                                            }
                                            if (s.HasHoles)
                                            {
                                                for (int j = 0; j < s.Holes.Count(); j++)
                                                {
                                                    Line3d[] sHolesArray = s.Holes[j].Explode();
                                                    for (int i1 = 0; i1 < sHolesArray.Length; i1++)
                                                    {
                                                        splitLine.Add(sHolesArray[i1]);
                                                    }
                                                }
                                            }
                                            if (s.HasChilds)
                                                throw new NotImplementedException("Child in shape not supported");
                                        }

                                        else
                                            throw new ArgumentException($"Geom {embeddedGeometries[shapes[t]][i].GetType()} not supported");
                                    }

                                    Line3d[] splitLineArray = splitLine.ToArray();

                                    for (int i = 0; i < splitLineArray.Length; i++)
                                    {
                                        for (int j = 0; j < splitLineArray.Count(); j++)
                                        {
                                            if (j >= i)
                                            {
                                                if (i != j)
                                                {
                                                    splitLineArray[i].GetIntersection(splitLineArray[j], out Point3d splitPoint, toleranceIntersection);
                                                    if (splitPoint != null)
                                                    {
                                                        if (Math.Abs(splitPoint.DistanceTo(splitLineArray[i].Start)) > toleranceIntersection &&
                                                            Math.Abs(splitPoint.DistanceTo(splitLineArray[i].End)) > toleranceIntersection)
                                                        {
                                                            splitPoints.Add(splitPoint);
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    // indici uguali => intersezione con se stessa
                                                }
                                            }
                                        }
                                    }

                                    Point3d[] splitPointsArray = splitPoints.ToArray();

                                    for (int i = 0; i < splitLineArray.Length; i++)
                                    {
                                        Line3d[] lineSplit = splitLineArray[i].Split(splitPoints.ToArray(), toleranceIntersection);        // splitto le linee in base ai punti di intersezione trovati
                                        if (lineSplit[0] == splitLineArray[i])                                                                // associo la geometria originale alle nuove linee spezzate
                                        {
                                            Line3d[] lineToSplitArray = new Line3d[1] { splitLineArray[i] };
                                            if (!splitLines.ContainsKey(splitLineArray[i]))
                                                splitLines.Add(splitLineArray[i], lineToSplitArray);
                                        }
                                        else
                                        {
                                            if (!splitLines.ContainsKey(splitLineArray[i]))
                                                splitLines.Add(splitLineArray[i], lineSplit);
                                        }
                                    }
                                }

                                #endregion

                                int physicalTag = physicalGroupTagSurfacesAssociation.Where(i => i.shape == shapes[t]).First().physicalGroupTag;

                                for (int s = 0; s < embeddedGeometries[shapes[t]].Count(); s++)
                                {
                                    if (!hashSet.Contains(embeddedGeometries[shapes[t]][s]))
                                    {
                                        for (int count = 0; count < physicalGroupTagSurfacesAssociation.Where(i => i.shape == shapes[t]).First().surfacesTag.Length; count++)
                                        {
                                            int surfaceTag = physicalGroupTagSurfacesAssociation.Where(i => i.shape == shapes[t]).First().surfacesTag[count];
                                            var surfTagArray = physicalGroupTagSurfacesAssociation.Where(i => i.shape == shapes[t]);
                                            var surftagArray2 = physicalGroupMeshTagAssociation.Where(i => i.physicalGroupTag == physicalTag);
                                            List<int> surfaceTagArray = new List<int>();

                                            foreach (var sT in surfTagArray)
                                                foreach (int tt in sT.surfacesTag)
                                                    surfaceTagArray.Add(tt);
                                            foreach (var ST in surftagArray2)
                                                surfaceTagArray.Add(ST.surfaceTag);

                                            #region POINT

                                            if (embeddedGeometries[shapes[t]][s] is Point3d || embeddedGeometries[shapes[t]][s] is Point2d)
                                            {
                                                Point3d point;
                                                if (embeddedGeometries[shapes[t]][s] is Point3d p)
                                                    point = p;
                                                else
                                                    point = new Point3d((Point2d)embeddedGeometries[shapes[t]][s]);
                                                try
                                                {
                                                    int surfaceWhereEmbedPoint = -1;
                                                    for (int i = 0; i < surfaceTagArray.Count(); i++)
                                                    {
                                                        int tt = IsInside(point, surfaceTagArray[i]);
                                                        if (tt != -1)
                                                        {
                                                            surfaceWhereEmbedPoint = tt;
                                                            break;
                                                        }
                                                    }

                                                    bool pointExist = false;

                                                    // Controllo che il punto non sia un vertice
                                                    int tag;
                                                    tag = occw.GetPointTag(point, toleranceMatch);
                                                    if (tag != -1)
                                                    {
                                                        pointExist = true;
                                                        embeddedGeometriesTagAssociation[physicalTag].Add(embeddedGeometries[shapes[t]][s], new int[1] { tag });

                                                        //Gmsh.Model.Occ.Synchronize();
                                                        generateMeshStatus.EmbeddedPoints += 1;
                                                        hashSet.Add(point);
                                                        break;
                                                    }


                                                    if (!pointExist && surfaceWhereEmbedPoint == -1)
                                                    {
                                                        // il punto è esterno ad ogni shape. non lo creiamo.
                                                    }

                                                    // Controllo che il punto sia interno
                                                    if (!pointExist && surfaceWhereEmbedPoint != -1)
                                                    {
                                                        int p1 = occw.AddPointAndSync(point);
                                                        Gmsh.Model.Mesh.Embed(0, new int[1] { p1 }, 2, surfaceWhereEmbedPoint);
                                                        //Gmsh.Model.Occ.Synchronize();
                                                        generateMeshStatus.EmbeddedPoints += 1;

                                                        embeddedGeometriesTagAssociation[physicalTag].Add(embeddedGeometries[shapes[t]][s], new int[1] { p1 });
                                                        hashSet.Add(point);
                                                        break;
                                                    }
                                                }
                                                catch (Exception e)
                                                {
                                                    generateMeshStatus.AddException(e, $"{GMeshGenerateMeshStatus.FailedToEmbedThePoint}{point * options.MeshScalingFactor}");
                                                    return false;
                                                }
                                            }

                                            #endregion

                                            #region LINE

                                            else if (embeddedGeometries[shapes[t]][s] is Line3d || embeddedGeometries[shapes[t]][s] is Line2d)
                                            {
                                                try
                                                {
                                                    Line3d l;
                                                    if (embeddedGeometries[shapes[t]][s] is Line3d l3d)
                                                        l = l3d;
                                                    else
                                                        l = new Line3d((Line2d)embeddedGeometries[shapes[t]][s]);

                                                    bool isAdded = true;

                                                    if (splitLines.ContainsKey(l) && !embeddedGeometriesTagAssociation[physicalTag].ContainsKey(l))
                                                    {
                                                        List<int> lineTagToEmbed = new List<int>();

                                                        for (int c = 0; c < splitLines[l].Count(); c++)
                                                        {
                                                            Line3d line = splitLines[l][c];

                                                            int surfaceWhereEmbedStart = -1;
                                                            int surfaceWhereEmbedEnd = -1;
                                                            int surfaceWhereEmbedMid = -1;
                                                            bool curveOnBoundary = false;
                                                            bool isInternal = true;

                                                            var geomCoord = new double[6] { line.Start.X, line.Start.Y, line.Start.Z, line.End.X, line.End.Y, line.End.Z };

                                                            int startPointTag = occw.AddPointAndSync(line.Start, toleranceMatch);
                                                            int endPointTag = occw.AddPointAndSync(line.End, toleranceMatch);

                                                            double[] startGmshCoord = new double[3] { line.Start.X, line.Start.Y, line.Start.Z };
                                                            double[] endGmshCoord = new double[3] { line.End.X, line.End.Y, line.End.Z };

                                                            Point3d start = new Point3d(line.Start.X, line.Start.Y, line.Start.Z);
                                                            Point3d end = new Point3d(line.End.X, line.End.Y, line.End.Z);

                                                            List<int> boundaryCurvesTag = new List<int>();

                                                            for (int k = 0; k < surfaceTagArray.Count; k++)
                                                            {
                                                                var boundaryCurvesTagBuffer = Gmsh.Model.GetBoundary(new (int, int)[1] { (2, surfaceTagArray[k]) }, false, false, false).Select(i => i.Item2).ToList();

                                                                // Se la curva è su un bordo
                                                                for (int j = 0; j < boundaryCurvesTagBuffer.Count; j++)
                                                                {
                                                                    var boundaryPointTag = Gmsh.Model.GetBoundary(new (int, int)[1] { (1, boundaryCurvesTagBuffer[j]) }, false, false, true).Select(i => i.Item2).ToList();

                                                                    if ((boundaryPointTag[0] == startPointTag && boundaryPointTag[1] == endPointTag) || (boundaryPointTag[1] == startPointTag && boundaryPointTag[0] == endPointTag))
                                                                    {
                                                                        // la linea coincide con un bordo
                                                                        int[] curveBoundaryTag = new int[1] { boundaryCurvesTagBuffer[j] };
                                                                        lineTagToEmbed.Add(boundaryCurvesTagBuffer[j]);                             // Imposto il tag del bordo come una geometria embd in modo da ottnere la mappatura dei nodi
                                                                        curveOnBoundary = true;
                                                                        hashSet.Add(line);
                                                                        break;
                                                                    }
                                                                }

                                                                boundaryCurvesTag.AddRange(boundaryCurvesTagBuffer);
                                                            }

                                                            if (curveOnBoundary == false)
                                                            {
                                                                // trovo le superfici di Start e End 
                                                                // trovo la superficie dov'è il punto medio della linea. questo lo faccio nel caso di linee embedded dentro shape embedded
                                                                for (int i = 0; i < surfaceTagArray.Count(); i++)
                                                                {
                                                                    if (surfaceWhereEmbedStart == -1)
                                                                    {
                                                                        int startSurfaceTag = IsInside(line.Start, surfaceTagArray[i]);
                                                                        if (startSurfaceTag != -1)
                                                                        {
                                                                            surfaceWhereEmbedStart = startSurfaceTag;
                                                                        }
                                                                    }

                                                                    if (surfaceWhereEmbedEnd == -1)
                                                                    {
                                                                        int endSurfaceTag = IsInside(line.End, surfaceTagArray[i]);
                                                                        if (endSurfaceTag != -1)
                                                                        {
                                                                            surfaceWhereEmbedEnd = endSurfaceTag;
                                                                        }
                                                                    }

                                                                    if (surfaceWhereEmbedEnd != -1 && surfaceWhereEmbedStart != -1)
                                                                    {
                                                                        break;
                                                                    }
                                                                }

                                                                for (int i = 0; i < surfaceTagArray.Count(); i++)
                                                                {
                                                                    if (surfaceWhereEmbedMid == -1)
                                                                    {
                                                                        int midSurfaceTag = IsInside((line.End + line.Start) / 2, surfaceTagArray[i]);
                                                                        if (midSurfaceTag != -1)
                                                                        {
                                                                            surfaceWhereEmbedMid = midSurfaceTag;
                                                                        }
                                                                    }

                                                                    if ((surfaceWhereEmbedMid != surfaceWhereEmbedStart && surfaceWhereEmbedMid != surfaceWhereEmbedEnd && surfaceWhereEmbedMid != -1) ||
                                                                        (surfaceWhereEmbedStart == surfaceWhereEmbedMid && surfaceWhereEmbedMid == surfaceWhereEmbedEnd))

                                                                    {
                                                                        break;
                                                                    }
                                                                }

                                                                // caso 0 LINEA COMPLETAMENTE ESTERNA
                                                                if (surfaceWhereEmbedMid == -1 && (surfaceWhereEmbedEnd == -1 && surfaceWhereEmbedStart == -1))
                                                                {
                                                                    isAdded = false;
                                                                }


                                                                // caso 1 TRATTO DI LINEA ESTERNO
                                                                if (surfaceWhereEmbedMid == -1 && (surfaceWhereEmbedEnd != -1 || surfaceWhereEmbedStart != -1))
                                                                {
                                                                    isInternal = false;
                                                                }

                                                                if (isInternal)
                                                                {
                                                                    Point3d intersectionPoint = null;

                                                                    // cerco le intersezioni con i bordi => intersectionPoint
                                                                    for (int j = 0; j < boundaryCurvesTag.Count(); j++)
                                                                    {
                                                                        var boundaryPointTag = Gmsh.Model.GetBoundary(new (int, int)[1] { (1, boundaryCurvesTag[j]) }, false, false, false).Select(i => i.Item2).ToList();

                                                                        Gmsh.Model.Occ.GetBoundingBox(0, boundaryPointTag[0], out double x1, out double y1, out double z1, out var _, out var _, out var _);
                                                                        Gmsh.Model.Occ.GetBoundingBox(0, boundaryPointTag[1], out double x2, out double y2, out double z2, out var _, out var _, out var _);

                                                                        Line3d edgeLine = new Line3d(new Point3d(x1, y1, z1), new Point3d(x2, y2, z2));

                                                                        edgeLine.GetIntersection(line, out Point3d intPoint, toleranceIntersection);

                                                                        if (intPoint != null)
                                                                        {
                                                                            double a = intPoint.DistanceTo(line.End);
                                                                            double b = intPoint.DistanceTo(line.Start);
                                                                            double tol = ErrorPropagation.ProductTolerance(Math.Pow(a, 2), Math.Max(b, 2), 2.82 * tolerance, 2.82 * tolerance);
                                                                            tol = tol < 1E-13 ? 1E-13 : tol;

                                                                            if (Math.Abs(a) > tol && Math.Abs(b) > tol)
                                                                            {
                                                                                intersectionPoint = intPoint;
                                                                                break;
                                                                            }
                                                                            else
                                                                                intersectionPoint = null;
                                                                        }
                                                                    }


                                                                    // caso 2 linea su una sola superficie. sia nel caso tocchi un bordo che nel caso non lo tocchi
                                                                    if (intersectionPoint == null && surfaceWhereEmbedMid != -1)
                                                                    {
                                                                        int l1 = occw.AddLineAndSync(startPointTag, endPointTag);

                                                                        int surf = -1;

                                                                        if (surfaceWhereEmbedStart != -1 && surfaceWhereEmbedEnd != -1)
                                                                            surf = surfaceWhereEmbedMid;

                                                                        Gmsh.Model.Mesh.Embed(1, new int[1] { l1 }, 2, surf);

                                                                        // TRANSFINITE THE EMBEDDED LINE
                                                                        if (options.Transfinite)
                                                                        {
                                                                            try
                                                                            {
                                                                                TransfiniteLine(l1, options.MeshSize, options.TransfiniteLineType.ToString(), options.TransfiniteFactor);
                                                                            }
                                                                            catch (GmshException e)
                                                                            {
                                                                                generateMeshStatus.AddException(e, $"{GMeshGenerateMeshStatus.FailedToTransfinite} for curveTag:{l1}. Line Start:{line.Start * options.MeshScalingFactor} Line End{line.End * options.MeshScalingFactor}");
                                                                                Gmsh.Finalize();
                                                                                return false;
                                                                            }
                                                                        }

                                                                        lineTagToEmbed.Add(l1);
                                                                        hashSet.Add(line);
                                                                    }


                                                                    // caso 3 linea su 2 superfici
                                                                    if (intersectionPoint != null && surfaceWhereEmbedStart != surfaceWhereEmbedEnd)
                                                                    {
                                                                        // la retta interseca uno dei bordi e va in un'altra superficie. Spezza la linea e associa ogni parte alla giusta superficie
                                                                        // aggiungo 1 punto  e creo le 2 linee embeddate nella giusta superficie (stessa procedura del caso semplice)

                                                                        int intersectPoint = occw.AddPointAndSync(intersectionPoint, toleranceMatch);
                                                                        int l1 = occw.AddLineAndSync(startPointTag, intersectPoint);
                                                                        int l2 = occw.AddLineAndSync(intersectPoint, endPointTag);

                                                                        Gmsh.Model.Mesh.Embed(1, new int[1] { l1 }, 2, surfaceWhereEmbedStart);
                                                                        Gmsh.Model.Mesh.Embed(1, new int[1] { l2 }, 2, surfaceWhereEmbedEnd);

                                                                        // TRANSFINITE THE EMBEDDED LINES
                                                                        if (options.Transfinite)
                                                                        {
                                                                            try
                                                                            {
                                                                                TransfiniteLine(l1, options.MeshSize, options.TransfiniteLineType.ToString(), options.TransfiniteFactor);
                                                                                TransfiniteLine(l2, options.MeshSize, options.TransfiniteLineType.ToString(), options.TransfiniteFactor);
                                                                            }
                                                                            catch (GmshException e)
                                                                            {
                                                                                generateMeshStatus.AddException(e, $"{GMeshGenerateMeshStatus.FailedToTransfinite} for curveTag:{l1} and {l2}. Line Start:{line.Start * options.MeshScalingFactor} Line End{line.End * options.MeshScalingFactor}");
                                                                                return false;
                                                                            }
                                                                        }

                                                                        lineTagToEmbed.Add(l1);
                                                                        lineTagToEmbed.Add(l2);
                                                                        hashSet.Add(line);
                                                                    }
                                                                }
                                                            }

                                                            //Gmsh.Fltk.Run();
                                                        }
                                                        if (isAdded)
                                                        {
                                                            embeddedGeometriesTagAssociation[physicalTag].Add(embeddedGeometries[shapes[t]][s], lineTagToEmbed.ToArray());
                                                            generateMeshStatus.EmbeddedLines += 1;
                                                        }
                                                    }
                                                }
                                                catch (Exception e)
                                                {
                                                    generateMeshStatus.AddException(e, $"{GMeshGenerateMeshStatus.FailedToEmbedTheLine} {embeddedGeometries[shapes[t]][s]}");
                                                    Gmsh.Finalize();
                                                    return false;
                                                }

                                            }

                                            #endregion

                                            #region POLYGON

                                            else if (embeddedGeometries[shapes[t]][s] is Polygon3d || embeddedGeometries[shapes[t]][s] is Polygon2d)
                                            {
                                                try
                                                {
                                                    Polygon3d polygon;
                                                    if (embeddedGeometries[shapes[t]][s] is Polygon3d p)
                                                        polygon = p;
                                                    else
                                                        polygon = new Polygon3d((Polygon2d)embeddedGeometries[shapes[t]][s]);

                                                    List<int> embeddedPolygonTagAssociation = new List<int>();
                                                    bool isAdded = true;

                                                    Line3d[] lines = polygon.Explode().ToArray();
                                                    for (int c = 0; c < lines.Count(); c++)
                                                    {
                                                        if (splitLines.ContainsKey(lines[c]) && !embeddedGeometriesTagAssociation[physicalTag].ContainsKey(lines[c]))
                                                        {
                                                            List<int> lineTagToEmbed = new List<int>();

                                                            for (int m = 0; m < splitLines[lines[c]].Count(); m++)
                                                            {
                                                                Line3d line = splitLines[lines[c]][m];

                                                                int surfaceWhereEmbedStart = -1;
                                                                int surfaceWhereEmbedEnd = -1;
                                                                int surfaceWhereEmbedMid = -1;
                                                                bool curveOnBoundary = false;
                                                                bool isInternal = true;

                                                                var geomCoord = new double[6] { line.Start.X, line.Start.Y, line.Start.Z, line.End.X, line.End.Y, line.End.Z };

                                                                int startPointTag = occw.AddPointAndSync(line.Start, toleranceMatch);
                                                                int endPointTag = occw.AddPointAndSync(line.End, toleranceMatch);

                                                                double[] startGmshCoord = new double[3] { line.Start.X, line.Start.Y, line.Start.Z };
                                                                double[] endGmshCoord = new double[3] { line.End.X, line.End.Y, line.End.Z };

                                                                Point3d start = new Point3d(line.Start.X, line.Start.Y, line.Start.Z);
                                                                Point3d end = new Point3d(line.End.X, line.End.Y, line.End.Z);

                                                                List<int> boundaryCurvesTag = new List<int>();

                                                                for (int k = 0; k < surfaceTagArray.Count; k++)
                                                                {
                                                                    var boundaryCurvesTagBuffer = Gmsh.Model.GetBoundary(new (int, int)[1] { (2, surfaceTagArray[k]) }, false, false, false).Select(i => i.Item2).ToList();

                                                                    // Se la curva è su un bordo
                                                                    for (int j = 0; j < boundaryCurvesTagBuffer.Count; j++)
                                                                    {
                                                                        var boundaryPointTag = Gmsh.Model.GetBoundary(new (int, int)[1] { (1, boundaryCurvesTagBuffer[j]) }, false, false, true).Select(i => i.Item2).ToList();

                                                                        if ((boundaryPointTag[0] == startPointTag && boundaryPointTag[1] == endPointTag) || (boundaryPointTag[1] == startPointTag && boundaryPointTag[0] == endPointTag))
                                                                        {
                                                                            // la linea coincide con un bordo
                                                                            int[] curveBoundaryTag = new int[1] { boundaryCurvesTagBuffer[j] };
                                                                            lineTagToEmbed.Add(boundaryCurvesTagBuffer[j]);                             // Imposto il tag del bordo come una geometria embd in modo da ottnere la mappatura dei nodi
                                                                            curveOnBoundary = true;
                                                                            hashSet.Add(line);
                                                                            break;
                                                                        }
                                                                    }

                                                                    boundaryCurvesTag.AddRange(boundaryCurvesTagBuffer);
                                                                }

                                                                if (curveOnBoundary == false)
                                                                {
                                                                    // trovo le superfici di Start e End 
                                                                    for (int i = 0; i < surfaceTagArray.Count(); i++)
                                                                    {
                                                                        if (surfaceWhereEmbedStart == -1)
                                                                        {
                                                                            int startSurfaceTag = IsInside(line.Start, surfaceTagArray[i]);
                                                                            if (startSurfaceTag != -1)
                                                                            {
                                                                                surfaceWhereEmbedStart = startSurfaceTag;
                                                                            }
                                                                        }

                                                                        if (surfaceWhereEmbedEnd == -1)
                                                                        {
                                                                            int endSurfaceTag = IsInside(line.End, surfaceTagArray[i]);
                                                                            if (endSurfaceTag != -1)
                                                                            {
                                                                                surfaceWhereEmbedEnd = endSurfaceTag;
                                                                            }
                                                                        }

                                                                        if (surfaceWhereEmbedEnd != -1 && surfaceWhereEmbedStart != -1)
                                                                        {
                                                                            break;
                                                                        }
                                                                    }

                                                                    for (int i = 0; i < surfaceTagArray.Count(); i++)
                                                                    {
                                                                        if (surfaceWhereEmbedMid == -1)
                                                                        {
                                                                            int midSurfaceTag = IsInside((line.End + line.Start) / 2, surfaceTagArray[i]);
                                                                            if (midSurfaceTag != -1)
                                                                            {
                                                                                surfaceWhereEmbedMid = midSurfaceTag;
                                                                            }
                                                                        }

                                                                        if ((surfaceWhereEmbedMid != surfaceWhereEmbedStart && surfaceWhereEmbedMid != surfaceWhereEmbedEnd && surfaceWhereEmbedMid != -1) ||
                                                                            (surfaceWhereEmbedStart == surfaceWhereEmbedMid && surfaceWhereEmbedMid == surfaceWhereEmbedEnd))

                                                                        {
                                                                            break;
                                                                        }
                                                                    }


                                                                    // caso 0 LINEA COMPLETAMENTE ESTERNA
                                                                    if (surfaceWhereEmbedMid == -1 && (surfaceWhereEmbedEnd == -1 && surfaceWhereEmbedStart == -1))
                                                                    {
                                                                        isAdded = false;
                                                                    }

                                                                    // caso 1 LINEA ESTERNA
                                                                    if (surfaceWhereEmbedStart == -1 || surfaceWhereEmbedMid == -1 || surfaceWhereEmbedEnd == -1)
                                                                    {
                                                                        //Gmsh.Model.Occ.Remove(new (int, int)[] { (0, startPointTag), (0, endPointTag) });
                                                                        //Gmsh.Model.Occ.Synchronize();
                                                                        isInternal = false;
                                                                    }

                                                                    if (isInternal)
                                                                    {
                                                                        Point3d intersectionPoint = null;

                                                                        // cerco le intersezioni con i bordi => intersectionPoint
                                                                        for (int j = 0; j < boundaryCurvesTag.Count(); j++)
                                                                        {
                                                                            var boundaryPointTag = Gmsh.Model.GetBoundary(new (int, int)[1] { (1, boundaryCurvesTag[j]) }, false, false, false).Select(i => i.Item2).ToList();

                                                                            Gmsh.Model.Occ.GetBoundingBox(0, boundaryPointTag[0], out double x1, out double y1, out double z1, out var _, out var _, out var _);
                                                                            Gmsh.Model.Occ.GetBoundingBox(0, boundaryPointTag[1], out double x2, out double y2, out double z2, out var _, out var _, out var _);

                                                                            Line3d edgeLine = new Line3d(new Point3d(x1, y1, z1), new Point3d(x2, y2, z2));

                                                                            edgeLine.GetIntersection(line, out Point3d intPoint, toleranceIntersection);

                                                                            if (intPoint != null)
                                                                            {
                                                                                double a = intPoint.DistanceTo(line.End);
                                                                                double b = intPoint.DistanceTo(line.Start);
                                                                                double tol = ErrorPropagation.ProductTolerance(Math.Pow(a, 2), Math.Max(b, 2), 2.82 * tolerance, 2.82 * tolerance);
                                                                                tol = tol < 1E-13 ? 1E-13 : tol;

                                                                                if (Math.Abs(a) > tol && Math.Abs(b) > tol)
                                                                                {
                                                                                    intersectionPoint = intPoint;
                                                                                    break;
                                                                                }
                                                                                else
                                                                                    intersectionPoint = null;
                                                                            }
                                                                        }


                                                                        // caso 2 linea su una sola superficie. sia nel caso tocchi un bordo che nel caso non lo tocchi
                                                                        if (intersectionPoint == null && surfaceWhereEmbedMid != -1)
                                                                        {
                                                                            int l1 = occw.AddLineAndSync(startPointTag, endPointTag);

                                                                            int surf = -1;

                                                                            if (surfaceWhereEmbedStart != -1 && surfaceWhereEmbedEnd != -1)
                                                                                surf = surfaceWhereEmbedMid;

                                                                            Gmsh.Model.Mesh.Embed(1, new int[1] { l1 }, 2, surf);

                                                                            // TRANSFINITE THE EMBEDDED LINE
                                                                            if (options.Transfinite)
                                                                            {
                                                                                try
                                                                                {
                                                                                    TransfiniteLine(l1, options.MeshSize, options.TransfiniteLineType.ToString(), options.TransfiniteFactor);
                                                                                }
                                                                                catch (GmshException e)
                                                                                {
                                                                                    generateMeshStatus.AddException(e, $"{GMeshGenerateMeshStatus.FailedToTransfinite} for curveTag:{l1}. Line Start:{line.Start * options.MeshScalingFactor} Line End{line.End * options.MeshScalingFactor}");
                                                                                    Gmsh.Finalize();
                                                                                    return false;
                                                                                }
                                                                            }

                                                                            embeddedPolygonTagAssociation.Add(l1);
                                                                        }


                                                                        // caso 3 linea su 2 superfici
                                                                        if (intersectionPoint != null && surfaceWhereEmbedStart != surfaceWhereEmbedEnd)
                                                                        {
                                                                            // la retta interseca uno dei bordi e va in un'altra superficie. Spezza la linea e associa ogni parte alla giusta superficie
                                                                            // aggiungo 1 punto  e creo le 2 linee embeddate nella giusta superficie (stessa procedura del caso semplice)

                                                                            int intersectPoint = occw.AddPointAndSync(intersectionPoint, tolerance);
                                                                            int l1 = occw.AddLineAndSync(startPointTag, intersectPoint);
                                                                            int l2 = occw.AddLineAndSync(intersectPoint, endPointTag);

                                                                            Gmsh.Model.Mesh.Embed(1, new int[1] { l1 }, 2, surfaceWhereEmbedStart);
                                                                            Gmsh.Model.Mesh.Embed(1, new int[1] { l2 }, 2, surfaceWhereEmbedEnd);

                                                                            // TRANSFINITE THE EMBEDDED LINES
                                                                            if (options.Transfinite)
                                                                            {
                                                                                try
                                                                                {
                                                                                    TransfiniteLine(l1, options.MeshSize, options.TransfiniteLineType.ToString(), options.TransfiniteFactor);
                                                                                    TransfiniteLine(l2, options.MeshSize, options.TransfiniteLineType.ToString(), options.TransfiniteFactor);
                                                                                }
                                                                                catch (GmshException e)
                                                                                {
                                                                                    generateMeshStatus.AddException(e, $"{GMeshGenerateMeshStatus.FailedToTransfinite} for curveTag:{l1} and {l2}. Line Start:{line.Start * options.MeshScalingFactor} Line End{line.End * options.MeshScalingFactor}");
                                                                                    return false;
                                                                                }
                                                                            }

                                                                            embeddedPolygonTagAssociation.Add(l1);
                                                                            embeddedPolygonTagAssociation.Add(l2);
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                    if (isAdded)
                                                    {
                                                        embeddedGeometriesTagAssociation[physicalTag].Add(polygon, embeddedPolygonTagAssociation.ToArray().ToArray());
                                                        generateMeshStatus.EmbeddedPolygons += 1;
                                                    }
                                                    hashSet.Add(polygon);
                                                }
                                                catch (Exception e)
                                                {
                                                    generateMeshStatus.AddException(e, $"{GMeshGenerateMeshStatus.FailedToEmbedThePolygon} {embeddedGeometries[shapes[t]][s]}");
                                                    Gmsh.Finalize();
                                                    return false;
                                                }
                                            }

                                            #endregion

                                            else if (embeddedGeometries[shapes[t]][s] is Shape sh)
                                            {

                                            }

                                            else
                                            {
                                                throw new NotSupportedException($"Embedded geometry of type {embeddedGeometries[shapes[t]][s].GetType()} is not supported.");
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    generateMeshStatus.AddException(e, $"Failed to embed the geometries");
                    Gmsh.Finalize();
                    return false;
                }

                #endregion

                #endregion 

                DateTime dt6 = DateTime.Now;

                //Gmsh.Fltk.Run();

                #region SETUP OPZIONI 

                // HEAL SHAPE
                try
                {
                    if (options.HealShapes)
                        Gmsh.Model.Occ.HealShapes(null, tolerance, true, true, true, true, true);
                }
                catch (GmshException)
                {
                    string warning = "Warning: fail to heal the shapes";
                    generateMeshStatus.AddWarning(warning);
                }

                // POINT MESH REFINEMENT SIZE 
                try
                {
                    (int, int)[] dimTags = Gmsh.Model.GetEntities(0);
                    Gmsh.Model.Mesh.SetSize(dimTags, options.MeshSize);
                    Gmsh.Option.SetNumber("Mesh.MeshSizeFromPoints", 1);
                }
                catch (GmshException gm)
                {
                    generateMeshStatus.AddException(gm, "Failed to set the mesh size at points");
                    Gmsh.Finalize();
                    return false;
                }

                // TRANFINITE LINE / POLYGON / POINT MESH REFINEMENT SIZE 
                try
                {
                    if (options.Transfinite && embeddedGeomMeshSize != null)
                    {
                        for (int s = 0; s < shapes.Length; s++)
                        {
                            int physicalTag = physicalGroupTagSurfacesAssociation.Where(i => i.shape == shapes[s]).First().physicalGroupTag;
                            if (embGeomAssociation.ContainsKey(shapes[s]))
                            {
                                foreach (GeometryBase geometry in embGeomAssociation[shapes[s]].Keys)
                                {
                                    if (geometry is Point3d || geometry is Point2d)
                                    {
                                        Point3d point;
                                        if (geometry is Point3d p)
                                            point = p;
                                        else
                                            point = new Point3d((Point2d)geometry);

                                        try
                                        {
                                            int[] tags = embeddedGeometriesTagAssociation[physicalTag][geometry];
                                            for (int j = 0; j < embeddedGeometriesTagAssociation[physicalTag][geometry].Count(); j++)
                                            {
                                                int tg = tags[j];
                                                double m = embGeomAssociation[shapes[s]][geometry];
                                                Gmsh.Model.Mesh.SetSize(new (int, int)[1] { (0, tags[j]) }, embGeomAssociation[shapes[s]][geometry]);
                                            }
                                        }
                                        catch (GmshException)
                                        {
                                            string warning = "Warning: fail to set the size in the embed point";
                                            generateMeshStatus.AddWarning(warning);
                                        }
                                    }

                                    else if (geometry is Line3d || geometry is Line2d)
                                    {
                                        Line3d line;
                                        if (geometry is Line3d l)
                                            line = l;
                                        else
                                            line = new Line3d((Line2d)geometry);

                                        try
                                        {
                                            double meshSize = embGeomAssociation[shapes[s]][geometry];
                                            int[] tags;
                                            tags = embeddedGeometriesTagAssociation[physicalTag][geometry];

                                            for (int j = 0; j < embeddedGeometriesTagAssociation[physicalTag][geometry].Count(); j++)
                                            {
                                                TransfiniteLine(tags[j], meshSize, options.TransfiniteLineType.ToString(), options.TransfiniteFactor);
                                            }
                                        }
                                        catch (GmshException)
                                        {
                                            string warning = "Warning: fail to set the size in the embed line";
                                            generateMeshStatus.AddWarning(warning);
                                        }
                                    }

                                    else if (geometry is Polygon3d || geometry is Polygon2d)
                                    {
                                        Polygon3d poly;
                                        if (geometry is Polygon3d p)
                                            poly = p;
                                        else
                                            poly = new Polygon3d((Polygon2d)geometry);

                                        try
                                        {
                                            double meshSize = embGeomAssociation[shapes[s]][geometry];
                                            int[] tags;
                                            tags = embeddedGeometriesTagAssociation[physicalTag][geometry];

                                            for (int j = 0; j < embeddedGeometriesTagAssociation[physicalTag][geometry].Count(); j++)
                                            {
                                                TransfiniteLine(tags[j], meshSize, options.TransfiniteLineType.ToString(), options.TransfiniteFactor);
                                            }
                                        }
                                        catch (GmshException)
                                        {
                                            string warning = "Warning: fail to set the size in the embed polygon";
                                            generateMeshStatus.AddWarning(warning);
                                        }
                                    }

                                    else if (geometry is Shape)
                                    {

                                    }

                                    else
                                        throw new ArgumentException($"Geom {geometry.GetType()} not supported");
                                }
                            }
                        }
                    }
                }
                catch (SystemException)
                {
                    string warning = "Warning: fail to set the size in the embed geometry";
                    generateMeshStatus.AddWarning(warning);
                }

                //Gmsh.Fltk.Run();

                // TRANSFINITE SURFACE QUAD
                try
                {
                    if (options.TransfiniteSurface)
                    {
                        for (int s = 0; s < shapes.Length; s++)
                        {
                            Shape shape = shapes[s];
                            if (!embeddedGeometries.ContainsKey(shape))
                            {
                                for (int j = 0; j < physicalGroupTagSurfacesAssociation.Where(i => i.shape == shape).First().surfacesTag.Length; j++)
                                {
                                    int surfaceTag = physicalGroupTagSurfacesAssociation.Where(i => i.shape == shape).First().surfacesTag[j];
                                    var boundaryPointTag = Gmsh.Model.GetBoundary(new (int, int)[1] { (2, surfaceTag) }, true, true, true).Select(i => i.Item2).ToList();

                                    if (boundaryPointTag.Count == 4)
                                    {
                                        try
                                        {
                                            for (int i = 0; i < 2; i++)
                                            {
                                                Gmsh.Model.Occ.GetBoundingBox(1, boundaryPointTag[i], out double x1min1, out double y1min1, out double z1min1, out double x1max1, out double y1max1, out double z1max1);
                                                Point3d start1 = new Point3d(x1min1, y1min1, z1min1);
                                                Point3d end1 = new Point3d(x1max1, y1max1, z1max1);
                                                double distance1 = start1.DistanceTo(end1);
                                                int geometryNumNode1 = (int)Math.Round((distance1 / options.MeshSize), 0, MidpointRounding.AwayFromZero) + 1;

                                                Gmsh.Model.Occ.GetBoundingBox(1, boundaryPointTag[i + 2], out double x1min2, out double y1min2, out double z1min2, out double x1max2, out double y1max2, out double z1max2);
                                                Point3d start2 = new Point3d(x1min2, y1min2, z1min2);
                                                Point3d end2 = new Point3d(x1max2, y1max2, z1max2);
                                                double distance2 = start2.DistanceTo(end2);
                                                int geometryNumNode2 = (int)Math.Round((distance2 / options.MeshSize), 0, MidpointRounding.AwayFromZero) + 1;

                                                int node = Math.Min(geometryNumNode1, geometryNumNode2);

                                                Gmsh.Model.Mesh.SetTransfiniteCurve(boundaryPointTag[i], node, options.TransfiniteLineType.ToString(), options.TransfiniteFactor);
                                                Gmsh.Model.Mesh.SetTransfiniteCurve(boundaryPointTag[i + 2], node, options.TransfiniteLineType.ToString(), options.TransfiniteFactor);
                                            }
                                        }
                                        catch (GmshException ge)
                                        {
                                            generateMeshStatus.AddException(ge, "Boundary transfinite failed");
                                            Gmsh.Finalize();
                                            return false;
                                        }
                                        Gmsh.Model.Mesh.SetTransfiniteSurface(surfaceTag);

                                    }
                                    else
                                    {
                                        generateMeshStatus.AddWarning("Failed to transfinite the surface. Surface must have 4 vertices");
                                    }
                                }
                            }
                        }
                    }
                }
                catch (GmshException)
                {
                    generateMeshStatus.AddWarning("Failed to transfinite the surface");
                }

                // MESH GENERATE
                try
                {
                    // MESHATURA  
                    Gmsh.Model.Mesh.Generate(1);
                    Gmsh.Model.Mesh.Generate(2);

                    //Gmsh.Model.Mesh.RemoveDuplicateNodes(); // NON USARE: crash in caso di recombine
                }
                catch (GmshException gm)
                {
                    string singularMatrix = "singular matrix";          // error: singular matrix 3x3
                    string unableToRecover = "Unable to recover";       // GmshNet.Gmsh+Model+Mesh.Generate Unable to recover the edge
                    string identicalPoints = "Identical points in triangulation";

                    if (gm.ToString().Contains(singularMatrix))
                        generateMeshStatus.AddException(gm, $"{GMeshGenerateMeshStatus.SingularMatrix}: Failed to generate the mesh");

                    else if (gm.ToString().Contains(unableToRecover))
                        generateMeshStatus.AddException(gm, $"{GMeshGenerateMeshStatus.UnableToRecover}: Failed to generate the mesh");

                    else if (gm.ToString().Contains(identicalPoints))
                        generateMeshStatus.AddException(gm, $"{GMeshGenerateMeshStatus.IdenticalPoints}: Failed to generate the mesh. " +
                            $"Probably there are overlapping points. If tolerance is set, try to reduce to default value");

                    else
                        generateMeshStatus.AddException(gm, "Failed to generate the mesh");

                    Gmsh.Finalize();
                    return false;
                }
                catch (Exception ex)
                {
                    generateMeshStatus.AddException(ex, "Failed to generate the mesh");
                }

                // REFINE
                try
                {
                    if (options.Refine)
                        Gmsh.Model.Mesh.Refine();
                }
                catch (GmshException)
                {
                    generateMeshStatus.AddWarning("Failed to refine the mesh");
                }

                // RENUMBER
                try
                {
                    if (options.Renumber)
                    {
                        Gmsh.Model.Mesh.RenumberNodes();
                        Gmsh.Model.Mesh.RenumberElements();
                    }
                }
                catch (GmshException gm)
                {
                    generateMeshStatus.AddException(gm, "Failed to renumber the nodes or elements");
                    Gmsh.Finalize();
                    return false;
                }

                // RECOMBINE
                try
                {
                    if (options.Recombine)
                    {
                        Gmsh.Model.Mesh.SplitQuadrangles(options.MinQuality, -1);
                        Gmsh.Model.Mesh.Recombine();
                    }
                }
                catch (GmshException e)
                {
                    generateMeshStatus.AddException(e, "Failed to recombine the mesh, try to change the recombine algorithm");
                    Gmsh.Finalize();
                    return false;
                }
                catch (Exception e)
                {
                    generateMeshStatus.AddException(e, "Failed to recombine the mesh");
                    Gmsh.Finalize();
                    return false;
                }

                // OPTIMIZE
                try
                {
                    if (options.Optimize)
                    {
                        Gmsh.Model.Mesh.Optimize(options.OptimizeAlgorithm.ToString(), false, options.OptimizeIteration, null);
                    }
                }
                catch (Exception)
                {
                    generateMeshStatus.AddWarning("Failed to optimize the mesh");
                }

                #endregion

                DateTime dt7 = DateTime.Now;

                //Gmsh.Fltk.Run();

                #region MMesh

                // LETTURA MESH GMESH E CREAZIONE MESH FACE / VERTEX / EDGES

                List<GMeshElementParameters> meshElementParameters = new List<GMeshElementParameters>
                {
                    new GMeshElementParameters() { ElementName = "Tri3", ElementType = 2, Dimension = 2, Order = 1, NodesNumber = 3, PrimaryNodesNumber = 3 },
                    new GMeshElementParameters() { ElementName = "Quad4", ElementType = 3, Dimension = 2, Order = 1, NodesNumber = 4, PrimaryNodesNumber = 4 },
                    new GMeshElementParameters() { ElementName = "Point", ElementType = 15, Dimension = 0, Order = 0, NodesNumber = 1, PrimaryNodesNumber = 1 },
                    new GMeshElementParameters() { ElementName = "Line2", ElementType = 1, Dimension = 1, Order = 1, NodesNumber = 2, PrimaryNodesNumber = 2 }
                };

                //Gmsh.Model.Mesh.GetElements(out elementTypes, out elementTags, out elementNodeTags);
                Gmsh.Model.Mesh.GetElements(out int[] elementTypes, out long[][] elementTags, out long[][] elementNodeTags);

                int progressVertexId = 0;
                int progressPlateId = 0;
                int progressEdgeId = 0;

                // dizionario di associazione tra meshVertex della nostra mesh e id del vertice di gmsh
                Dictionary<MeshVertex, int> pointIdAssociation = new Dictionary<MeshVertex, int>();
                // dizionario di associazione tra id della faccia di gmsh e id della nostra faccia
                Dictionary<int, int> faceIdAssociation = new Dictionary<int, int>();

                int[] phGTags = physicalGroupTagSurfacesAssociation.Select(i => i.physicalGroupTag).Distinct().ToArray();
                for (int p = 0; p < phGTags.Count(); p++)
                {
                    Dictionary<int, int> vertexIdAssociation = new Dictionary<int, int>(); // Associazione fra vertex tag gmesh e tag progressivo                    
                    HashSet<int> nodeTagHashSet = new HashSet<int>();

                    if (!options.UseGlobalProgressID)
                    {
                        progressVertexId = 0;
                        progressPlateId = 0;
                        progressEdgeId = 0;
                    }

                    GMesh meshBuffer = new GMesh();

                    DateTime dtV1 = DateTime.Now;

                    // tag del gruppo fisico
                    (int physicalGroupTag, int dim, int[] surfacesTag, Shape shape)[] pairs = physicalGroupTagSurfacesAssociation.Where(i => i.physicalGroupTag == phGTags[p]).ToArray();
                    for (int m = 0; m < pairs.Length; m++)
                    {
                        Gmsh.Model.Mesh.GetNodesForPhysicalGroup(pairs.ElementAt(m).dim, phGTags[p], out long[] nodeTags, out double[] nodeTagsCoord);
                        Dictionary<MeshVertex, int> pointIdAssociationBuffer = new Dictionary<MeshVertex, int>();

                        try
                        {
                            #region VERTICI                             

                            for (int j = 0; j < nodeTags.Count(); j++)
                            {
                                Point3d pt = new Point3d(nodeTagsCoord[j * 3], nodeTagsCoord[j * 3 + 1], nodeTagsCoord[j * 3 + 2]);
                                Point3d point = pt.Scale(options.MeshScalingFactor);

                                // Uso id vertice comune ad altre mesh se esiste, altrimenti un progressivo
                                bool commonPoint = false;
                                int vertexId = -1;
                                MeshVertex mv = new MeshVertex(point);

                                // controllo che il vertice non sia stato aggiunto. nel caso, mi faccio restituire l'id
                                if (pointIdAssociation.ContainsKey(mv))
                                {
                                    vertexId = pointIdAssociation[mv];
                                    vertexId = meshBuffer._vertices.Add(mv, vertexId);
                                    commonPoint = true;
                                    break;
                                }

                                // se non è stato già aggiunto, lo aggiungo con un nuovo id
                                if (!commonPoint)
                                {
                                    if (!pointIdAssociationBuffer.ContainsKey(mv) && !options.UseGlobalProgressID)
                                    {
                                        vertexId = meshBuffer._vertices.Add(mv);
                                    }
                                    else if (!pointIdAssociationBuffer.ContainsKey(mv) && options.UseGlobalProgressID)
                                    {
                                        ++progressVertexId;
                                        vertexId = meshBuffer._vertices.Add(mv, progressVertexId);
                                    }
                                    else
                                    {
                                        vertexId = pointIdAssociationBuffer[mv];
                                        commonPoint = true;
                                    }
                                }

                                if (vertexId == -1)
                                {
                                    throw new NotSupportedException("Vertex id not assigned");
                                }

                                vertexIdAssociation[(int)nodeTags[j]] = vertexId;
                                nodeTagHashSet.Add((int)nodeTags[j]);
                                if (!commonPoint)
                                {
                                    pointIdAssociationBuffer.Add(mv, vertexId);
                                }
                            }

                            #endregion
                        }
                        catch (Exception e)
                        {
                            generateMeshStatus.AddException(e, "Failed to postprocess the mesh vertices");
                            Gmsh.Finalize();
                            return false;
                        }

                        // se ho una shape embeddata
                        foreach ((int physicalGroupTag, int embPhysicalGroupTag, int surfaceTag) pgTag in physicalGroupMeshTagAssociation.Where(i => i.physicalGroupTag == phGTags[p]))
                        {
                            Gmsh.Model.Mesh.GetNodesForPhysicalGroup(2, pgTag.embPhysicalGroupTag, out nodeTags, out nodeTagsCoord);

                            try
                            {
                                #region VERTICI EVENTUALI SHAPES EMBEDDED

                                for (int j = 0; j < nodeTags.Count(); j++)
                                {
                                    var pt = new Point3d(nodeTagsCoord[j * 3], nodeTagsCoord[j * 3 + 1], nodeTagsCoord[j * 3 + 2]);
                                    var point = pt.Scale(options.MeshScalingFactor);

                                    // Uso id vertice comune ad altre mesh se esiste, altrimenti un progressivo
                                    bool commonPoint = false;
                                    int vertexId = -1;
                                    MeshVertex mv = new MeshVertex(point);

                                    if (pointIdAssociation.ContainsKey(mv))
                                    {
                                        vertexId = pointIdAssociation[mv];
                                        vertexId = meshBuffer._vertices.Add(mv, vertexId);
                                        commonPoint = true;
                                        break;
                                    }

                                    if (!commonPoint)
                                    {
                                        if (!pointIdAssociationBuffer.ContainsKey(mv) && !options.UseGlobalProgressID)
                                        {
                                            vertexId = meshBuffer._vertices.Add(mv);
                                        }
                                        else if (!pointIdAssociationBuffer.ContainsKey(mv) && options.UseGlobalProgressID)
                                        {
                                            ++progressVertexId;
                                            vertexId = meshBuffer._vertices.Add(mv, progressVertexId);
                                        }
                                        else
                                        {
                                            vertexId = pointIdAssociationBuffer[mv];
                                            commonPoint = true;
                                        }
                                    }

                                    if (vertexId == -1)
                                    {
                                        throw new NotSupportedException("Vertex id not assigned");
                                    }

                                    vertexIdAssociation[(int)nodeTags[j]] = vertexId;
                                    nodeTagHashSet.Add((int)nodeTags[j]);
                                    if (!commonPoint)
                                    {
                                        pointIdAssociationBuffer.Add(mv, vertexId);
                                    }
                                }

                                #endregion
                            }
                            catch (Exception e)
                            {
                                generateMeshStatus.AddException(e, "Failed to postprocess the mesh vertices of embedded shape");
                                Gmsh.Finalize();
                                return false;
                            }
                        }

                        DateTime dtV2 = DateTime.Now;
                        nodeTime += (dtV2 - dtV1).TotalSeconds;

                        try
                        {
                            #region ELEMENTI - PUNTI, LINEE, PLATE

                            for (int count = 0; count < meshElementParameters.Count; count++)
                            {
                                if (meshElementParameters[count].ElementType == 2 || meshElementParameters[count].ElementType == 3)
                                {
                                    int elementTypeIndex = Array.IndexOf(elementTypes, meshElementParameters[count].ElementType);

                                    if (elementTypeIndex != -1)
                                    {
                                        for (int i = 0; i < elementTags[elementTypeIndex].Length; i++)
                                        {
                                            // tag singolo elemento in gmsh, non so in che gruppo sia
                                            long elTag = elementTags[elementTypeIndex][i];
                                            long[] elNodeTags = null;

                                            if (meshElementParameters[count].PrimaryNodesNumber == 4)
                                                elNodeTags = new long[] {
                                                    elementNodeTags[elementTypeIndex][meshElementParameters[count].PrimaryNodesNumber * i],
                                                    elementNodeTags[elementTypeIndex][meshElementParameters[count].PrimaryNodesNumber*i + 1],
                                                    elementNodeTags[elementTypeIndex][meshElementParameters[count].PrimaryNodesNumber*i + 2],
                                                    elementNodeTags[elementTypeIndex][meshElementParameters[count].PrimaryNodesNumber*i + 3] };

                                            else if (meshElementParameters[count].PrimaryNodesNumber == 3)
                                                elNodeTags = new long[] {
                                                    elementNodeTags[elementTypeIndex][meshElementParameters[count].PrimaryNodesNumber*i],
                                                    elementNodeTags[elementTypeIndex][meshElementParameters[count].PrimaryNodesNumber*i + 1],
                                                    elementNodeTags[elementTypeIndex][meshElementParameters[count].PrimaryNodesNumber*i + 2] };

                                            // Controllo che faccia parte del gruppo fisico in analisi
                                            if (elNodeTags != null)
                                            {
                                                bool isInGroup = true;
                                                for (int item = 0; item < elNodeTags.Length; item++)
                                                {
                                                    if (!nodeTagHashSet.Contains((int)elNodeTags[item]))
                                                    {
                                                        isInGroup = false;
                                                        break;
                                                    }
                                                }

                                                // creo una faccia e i rispettivi bordi
                                                if (isInGroup)
                                                {
                                                    if (meshElementParameters[count].ElementType == 2 || meshElementParameters[count].ElementType == 3) // TRI 3 o QUAD4
                                                    {
                                                        try
                                                        {
                                                            faceIdAssociation.Add((int)elTag, progressPlateId);
                                                        }
                                                        catch (Exception)
                                                        {
                                                            continue;
                                                        }

                                                        if (meshElementParameters[count].PrimaryNodesNumber == 3)
                                                        {
                                                            meshBuffer._faces.Add(new MeshFace(
                                                                vertexIdAssociation[(int)elNodeTags[0]],
                                                                vertexIdAssociation[(int)elNodeTags[1]],
                                                                vertexIdAssociation[(int)elNodeTags[2]]),
                                                                ++progressPlateId);

                                                            meshBuffer._edges.Add(new MeshEdge(
                                                                vertexIdAssociation[(int)elNodeTags[0]],
                                                                vertexIdAssociation[(int)elNodeTags[1]]),
                                                                ++progressEdgeId);
                                                            meshBuffer._edges.Add(new MeshEdge(
                                                                vertexIdAssociation[(int)elNodeTags[1]],
                                                                vertexIdAssociation[(int)elNodeTags[2]]),
                                                                ++progressEdgeId);
                                                            meshBuffer._edges.Add(new MeshEdge(
                                                                vertexIdAssociation[(int)elNodeTags[2]],
                                                                vertexIdAssociation[(int)elNodeTags[0]]),
                                                                ++progressEdgeId);
                                                        }

                                                        else if (meshElementParameters[count].PrimaryNodesNumber == 4)
                                                        {
                                                            meshBuffer._faces.Add(new MeshFace(
                                                                vertexIdAssociation[(int)elNodeTags[0]],
                                                                vertexIdAssociation[(int)elNodeTags[1]],
                                                                vertexIdAssociation[(int)elNodeTags[2]],
                                                                vertexIdAssociation[(int)elNodeTags[3]]),
                                                                ++progressPlateId);

                                                            meshBuffer._edges.Add(new MeshEdge(
                                                                vertexIdAssociation[(int)elNodeTags[0]],
                                                                vertexIdAssociation[(int)elNodeTags[1]]),
                                                                ++progressEdgeId);
                                                            meshBuffer._edges.Add(new MeshEdge(
                                                                vertexIdAssociation[(int)elNodeTags[1]],
                                                                vertexIdAssociation[(int)elNodeTags[2]]),
                                                                ++progressEdgeId);
                                                            meshBuffer._edges.Add(new MeshEdge(
                                                                vertexIdAssociation[(int)elNodeTags[2]],
                                                                vertexIdAssociation[(int)elNodeTags[3]]),
                                                                ++progressEdgeId);
                                                            meshBuffer._edges.Add(new MeshEdge(
                                                                vertexIdAssociation[(int)elNodeTags[3]],
                                                                vertexIdAssociation[(int)elNodeTags[0]]),
                                                                ++progressEdgeId);
                                                        }

                                                        else
                                                        {
                                                            throw new NotSupportedException("Node number not supported");
                                                        }


                                                    }

                                                    else if (meshElementParameters[count].ElementType == 1 || meshElementParameters[count].ElementType == 15) // linea2 o punto
                                                    {
                                                        continue;
                                                    }

                                                    else
                                                    {
                                                        throw new NotSupportedException("Element type not supported");
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }

                            meshBuffer.Clean(tolerance, tolerance);

                            #endregion
                        }
                        catch (Exception e)
                        {
                            generateMeshStatus.AddException(e, "Failed to postprocess the mesh elements");
                            Gmsh.Finalize();
                            return false;
                        }

                        DateTime dtE2 = DateTime.Now;
                        elementTime += (dtE2 - dtV2).TotalSeconds;

                        try
                        {
                            #region PER OGNI GEOMETRIA EMBEDD mappo i nodi o le facce

                            DateTime dtEmb1 = DateTime.Now;

                            if (embeddedGeometriesTagAssociation.Count > 0 && embeddedGeometriesTagAssociation.ContainsKey(phGTags[p]))
                            {
                                try
                                {
                                    Dictionary<GeometryBase, int[]> embeddedGeometriesSingleMesh = new Dictionary<GeometryBase, int[]>();

                                    for (int k = 0; k < embeddedGeometriesTagAssociation[phGTags[p]].Count; k++)
                                    {
                                        GeometryBase geometry = embeddedGeometriesTagAssociation[phGTags[p]].ElementAt(k).Key;
                                        int[] geometryTag = embeddedGeometriesTagAssociation[phGTags[p]].ElementAt(k).Value;

                                        List<int> nodesBuffer = new List<int>();            // lista temporanea
                                        List<int> nodesBufferOut = new List<int>();         // lista che va in output con i vertici corretti nel giusto ordine

                                        for (int j = 0; j < geometryTag.Length; j++)
                                        {
                                            if (geometry is Line3d || geometry is Line2d)
                                            {
                                                // Gmsh.Model.Mesh.GetNodes(out long[] nTags, out double[] coord, out double[] parametricCoord, 1, geometryTag, true, false);
                                                // Gabriele: messo parametro includeBoundsry a true. restituisce i tag degli estremi però li mette in fondo all'array.
                                                // l'array è ordinato così: tag 1 - 2 - 3 - .... - start - end. lo riscrivo ordinato prima di aggiungerlo alla mappa dei nodi
                                                // problema: lo restituisce secondo l'ordine interno di gmsh e non quello mio di inserimento. devo flipparlo se nel verso opposto. 
                                                // faccio il controllo prima sullo start della linea e poi sull'ultimo tag della lista
                                                Gmsh.Model.Mesh.GetNodes(out long[] nTags, out double[] coord, out double[] parametricCoord, 1, geometryTag[j], true, false);

                                                int[] nodes = new int[nTags.Length];

                                                //if(vertexIdAssociation[(int)nTags[nTags.Length - 2]] != null)
                                                nodesBuffer.Add(vertexIdAssociation[(int)nTags[nTags.Length - 2]]);

                                                for (int i = 0; i < nTags.Length; i++)
                                                {
                                                    if (i == nTags.Length - 2)
                                                    {
                                                    }
                                                    else if (vertexIdAssociation.ContainsKey((int)nTags[i]))
                                                        nodesBuffer.Add(vertexIdAssociation[(int)nTags[i]]);
                                                }

                                                Line3d l;
                                                if (geometry is Line3d l3d)
                                                    l = l3d;
                                                else
                                                    l = new Line3d((Line2d)geometry);

                                                int startTag;
                                                double[] startCoord;
                                                if (nodesBufferOut.Count == 0)
                                                {
                                                    startTag = geometryTag[j];
                                                    startCoord = Gmsh.Model.GetValue(1, startTag, new double[] { 0 });

                                                    if (!(Math.Abs(l.Start.X - startCoord[0]) < tolerance && Math.Abs(l.Start.Y - startCoord[1]) < tolerance && Math.Abs(l.Start.Z - startCoord[2]) < tolerance))
                                                    {
                                                        // significa che lo start della mia linea NON coincide con lo start della linea di gmsh => è al contrario. la rigiro
                                                        nodesBuffer.Reverse();
                                                    }
                                                }
                                                else
                                                {
                                                    startTag = nodesBufferOut.LastOrDefault();
                                                    if (startTag != nodesBuffer[0])
                                                        // significa che la lista che ha tirato fuori gmsh è in ordine opposto a quella che vorrei => è al contrario. la rigiro
                                                        nodesBuffer.Reverse();
                                                }

                                                for (int i = 0; i < nodesBuffer.Count(); i++)
                                                {
                                                    if (!nodesBufferOut.Contains(nodesBuffer[i]))
                                                        nodesBufferOut.Add(nodesBuffer[i]);
                                                }
                                                nodesBuffer.Clear();            // lo pulisco almeno evito calcoli inutili
                                            }

                                            else if (geometry is Point3d || geometry is Point2d)
                                            {
                                                //Gmsh.Model.Mesh.GetNodes(out long[] nTags, out double[] coord, out double[] parametricCoord, 0, geometryTag, true, false);
                                                // Giorgio: messo parametro includeBoundsry a fase altrimenti restituiva tag inesistenti
                                                Gmsh.Model.Mesh.GetNodes(out long[] nTags, out double[] coord, out double[] parametricCoord, 0, geometryTag[j], false, false);

                                                int[] nodes = new int[nTags.Length];
                                                for (int i = 0; i < nTags.Length; i++)
                                                {
                                                    nodesBufferOut.Add(vertexIdAssociation[(int)nTags[i]]);
                                                }
                                            }

                                            else if (geometry is Polygon2d || geometry is Polygon3d)
                                            {
                                                Gmsh.Model.Mesh.GetNodes(out long[] nTags, out double[] coord, out double[] parametricCoord, 1, geometryTag[j], true, false);

                                                int[] nodes = new int[nTags.Length];

                                                nodesBuffer.Add(vertexIdAssociation[(int)nTags[nTags.Length - 2]]);

                                                for (int i = 0; i < nTags.Length; i++)
                                                {
                                                    if (i == nTags.Length - 2)
                                                    {
                                                    }
                                                    else if (vertexIdAssociation.ContainsKey((int)nTags[i]))
                                                        nodesBuffer.Add(vertexIdAssociation[(int)nTags[i]]);
                                                }

                                                Polygon3d poly = (Polygon3d)geometry;

                                                int startTag;
                                                double[] startCoord;
                                                if (nodesBufferOut.Count == 0)
                                                {
                                                    startTag = geometryTag[j];
                                                    startCoord = Gmsh.Model.GetValue(1, startTag, new double[] { 0 });

                                                    if (!(Math.Abs(poly.Explode()[0].Start.X - startCoord[0]) < tolerance && Math.Abs(poly.Explode()[0].Start.Y - startCoord[1]) < tolerance && Math.Abs(poly.Explode()[0].Start.Z - startCoord[2]) < tolerance))
                                                    {
                                                        // significa che lo start della mia linea NON coincide con lo start della linea di gmsh => è al contrario. la rigiro
                                                        nodesBuffer.Reverse();
                                                    }
                                                }
                                                else
                                                {
                                                    startTag = nodesBufferOut.LastOrDefault();
                                                    if (startTag != nodesBuffer[0])
                                                        // significa che la lista che ha tirato fuori gmsh è in ordine opposto a quella che vorrei => è al contrario. la rigiro
                                                        nodesBuffer.Reverse();
                                                }


                                                for (int i = 0; i < nodesBuffer.Count(); i++)
                                                {
                                                    if (!nodesBufferOut.Contains(nodesBuffer[i]))
                                                        nodesBufferOut.Add(nodesBuffer[i]);
                                                }
                                                nodesBuffer.Clear();            // lo pulisco almeno evito calcoli inutili
                                            }

                                            else if (geometry is Shape)
                                            {
                                                Gmsh.Model.Mesh.GetElements(out int[] elementType, out long[][] elementTag, out long[][] nTags, 2, geometryTag[j]);
                                                for (int i = 0; i < elementTag.Length; i++)
                                                {
                                                    for (int h = 0; h < elementTag[i].Length; h++)
                                                    {
                                                        if (!nodesBufferOut.Contains(faceIdAssociation[Convert.ToInt32(elementTag[i][h])]))
                                                            nodesBufferOut.Add(faceIdAssociation[Convert.ToInt32(elementTag[i][h])]);

                                                        // elementTag[0] => facce triangolari
                                                        // elementTag[1] => facce quadrangolari
                                                    }
                                                }
                                            }

                                            else
                                            {
                                                throw new NotSupportedException($"Geometry of type {geometry.GetType()} is not supported.");
                                            }
                                        }

                                        if (geometry is Shape shape)
                                        {
                                            embeddedGeometriesSingleMesh.Add(shapeOutputAssociation[shape], nodesBufferOut.ToArray());
                                            // associa la shape modificata a quella di input per essere ripescata dal dizionario
                                        }

                                        else
                                            embeddedGeometriesSingleMesh.Add(geometry, nodesBufferOut.ToArray());
                                    }

                                    generateMeshStatus.AddEmbeddedGeometries(meshBuffer, embeddedGeometriesSingleMesh);
                                }
                                catch (Exception)
                                {
                                    generateMeshStatus.AddWarning("Failed to recover the embedded geometries nodes");
                                }
                            }

                            #endregion
                        }
                        catch (Exception e)
                        {
                            generateMeshStatus.AddException(e, "Failed to postprocess the embedded geometries");
                            Gmsh.Finalize();
                            return false;
                        }

                        pointIdAssociation.Concat(pointIdAssociationBuffer);
                        meshes.Add(meshBuffer);

                        DateTime dtEmb2 = DateTime.Now;
                        embTime += (dtEmb2 - dtE2).TotalSeconds;
                    }
                }

                #endregion

                DateTime dt8 = DateTime.Now;

                #region EXECUTION TIME

                generateMeshStatus.AddExecutionTimeMessage("Gmsh inizialize", (dt1 - dt0).TotalSeconds);
                generateMeshStatus.AddExecutionTimeMessage("Geometry init and scale", (dt2 - dt1).TotalSeconds);
                generateMeshStatus.AddExecutionTimeMessage("PreProcessing", preProctime);
                generateMeshStatus.AddExecutionTimeMessage("Geometry CAD", geomCADTime);
                generateMeshStatus.AddExecutionTimeMessage("RebuildObjectTags", rebuildObjectTagsTime);
                generateMeshStatus.AddExecutionTimeMessage("Total CAD time", (dt3_2 - dt2).TotalSeconds);
                generateMeshStatus.AddExecutionTimeMessage("Geometry Fragment", (dt4 - dt3_2).TotalSeconds);
                generateMeshStatus.AddExecutionTimeMessage("Geometry Transfinite boundary", (dt5 - dt4).TotalSeconds);
                generateMeshStatus.AddExecutionTimeMessage("Geometry Embedded", (dt6 - dt5).TotalSeconds);
                generateMeshStatus.AddExecutionTimeMessage("Mesh generate and optimization", (dt7 - dt6).TotalSeconds);
                generateMeshStatus.AddExecutionTimeMessage("Mesh postprocessing Total", (dt8 - dt7).TotalSeconds);
                generateMeshStatus.AddExecutionTimeMessage("Mesh postprocessing: node", nodeTime);
                generateMeshStatus.AddExecutionTimeMessage("Mesh postprocessing: element", elementTime);
                generateMeshStatus.AddExecutionTimeMessage("Mesh postprocessing: embedded geometries", embTime);

                #endregion

                System.Diagnostics.Debug.Write(Gmsh.Logger.GetLastError());
            }
            catch (Exception e)
            {
                generateMeshStatus.AddException(e, "Generic exception");
                Gmsh.Finalize();
                return false;
            }

            //Gmsh.Logger.Stop();
            Gmsh.Finalize();

            generateMeshStatus.AddExecutionTimeMessage(GMeshGenerateMeshStatus.TotalTime, (DateTime.Now - dt0).TotalSeconds);


            return true;
        }

        #region Private generate mesh methods

        /// <summary>
        /// 
        /// </summary>
        /// <returns>True if the line has been added to shape fill</returns>
        private static bool GenerateMeshGeometryPreProcessingAddLineToShape(Polygon3d polygonBondary, Line3d perimeterLine, Line3d lineToAdd, int index, double tolerance, out int vertexAdded)
        {
            vertexAdded = 0;
            if (perimeterLine.IsPointOnLine(lineToAdd.Start, tolerance) && perimeterLine.IsPointOnLine(lineToAdd.End, tolerance))
            {
                double tol = ErrorPropagation.ProductTolerance(perimeterLine.GetLength(), lineToAdd.GetLength(), tolerance, tolerance);
                if (((lineToAdd.Start.DistanceTo(perimeterLine.Start) > tol && lineToAdd.End.DistanceTo(perimeterLine.End) > tol) &&
                    (lineToAdd.Start.DistanceTo(perimeterLine.End) > tol && lineToAdd.End.DistanceTo(perimeterLine.Start) > tol)))
                {
                    // se sia Start che End sono sulla linea, aggiungili entrambi alla shape
                    // Altrimenti se solo Start oppure End sono sulla linea e NON sono un vertice, allora aggiungili
                    // Il controllo se non sono un vertice lo facciamo semplicemente sulle coordinate, tanto alla fine serve fare un removeduplicates

                    if (lineToAdd.Start.DistanceTo(perimeterLine.Start) < lineToAdd.End.DistanceTo(perimeterLine.Start))
                    {
                        polygonBondary.Insert(index + 1, lineToAdd.Start);
                        polygonBondary.Insert(index + 2, lineToAdd.End);
                    }
                    else
                    {
                        polygonBondary.Insert(index + 1, lineToAdd.End);
                        polygonBondary.Insert(index + 2, lineToAdd.Start);
                    }
                    vertexAdded = 2;
                    return true;
                }
            }
            else if (perimeterLine.IsPointOnLine(lineToAdd.Start, tolerance) && !polygonBondary.Contains(lineToAdd.Start))
            {
                polygonBondary.Insert(index + 1, lineToAdd.Start);
                vertexAdded++;
            }
            else if (perimeterLine.IsPointOnLine(lineToAdd.End, tolerance) && !polygonBondary.Contains(lineToAdd.End))
            {
                polygonBondary.Insert(index + 1, lineToAdd.End);
                vertexAdded++;
            }

            if (vertexAdded > 0)
                return false;
            else
                return true;
        }

        /// <summary>
        /// Check if the input point is inside the input surface
        /// </summary>
        /// <param name="point">Point to test</param>
        /// <param name="surfaceTag">The tag of the surface</param>
        /// <returns>The surface tag if the point is inside, -1 if it's outside</returns>
        private static int IsInside(Point3d point, int surfaceTag)
        {
            try
            {
                double[] startParametricCoord = Gmsh.Model.GetParametrization(2, surfaceTag, new double[3] { point.X, point.Y, point.Z });

                if (startParametricCoord != null)
                {
                    int isInsideStart = Gmsh.Model.IsInside(2, surfaceTag, startParametricCoord, true);

                    if (isInsideStart != 0) // significa che abbiamo beccato la superficie dove sta il punto 
                    {
                        return surfaceTag;
                    }
                }
            }
            catch (ArgumentNullException)
            {
                // vuol dire che non è dentro newSurfaceTag. il try/catch è stato messo perchè  GetParametrization non vuole punti esterni alla superficie. noi vogliamo però usarlo per capire se il punto è sulla superficie,
                // quindi non deve andare in eccezione se l'argomento è nullo. Lo gestiamo mandandolo nel caso di punto esterno
            }
            return -1;
        }

        /// <summary>
        /// Transfinite the input line
        /// </summary>
        /// <param name="lineTag">The tag of the line</param>
        /// <param name="meshSize">Meshsize of refinement</param>
        /// <param name="transfiniteLineType">Type of transfinite algorithm</param>
        /// <param name="transfiniteFactor">Transfinite algorithm factor</param>
        private static void TransfiniteLine(int lineTag, double meshSize, string transfiniteLineType, double transfiniteFactor)
        {
            Gmsh.Model.GetBoundingBox(1, lineTag, out double x1, out double y1, out double z1, out double x2, out double y2, out double z2);
            Point3d p1 = new Point3d(x1, y1, z1);
            Point3d p2 = new Point3d(x2, y2, z2);
            double l1lenght = p1.DistanceTo(p2);
            int geometryNumNodel1 = (int)Math.Round((l1lenght / meshSize), 0, MidpointRounding.AwayFromZero) + 1;

            Gmsh.Model.Mesh.SetTransfiniteCurve(lineTag, geometryNumNodel1, transfiniteLineType, transfiniteFactor);
        }

        #endregion

        #endregion

        #region Nested classes

        /// <summary>
        /// Struct to collect the Gmesh element parameters
        /// </summary>
        private struct GMeshElementParameters
        {
            public string ElementName;
            public int ElementType;
            public int NodesNumber;
            public int PrimaryNodesNumber;
            public int Dimension;
            public int Order;
        }

        [Serializable]
        public sealed class GMeshGenerateOptions : GenerateOptions, ICloneable
        {
            #region MeshOptions
            /// <summary>
            /// 2D mesh algorithm.
            /// </summary>
            public MeshAlgorithm Algorithm;

            /// <summary>
            /// Recombine the current mesh into quadrangles. Best algorithm: BlossomFullQuad, more robust: Simple.
            /// </summary>
            public RecombinationMeshAlgorithm RecombinationAlgorithm;

            /// <summary>
            /// Number of topological optimization passes (removal of diamonds, ...) of recombined surface meshes. Gmsh default value: '5'
            /// </summary>
            public int RecombineOptimizeTopology;

            /// <summary>
            /// Renumbers the node tags and the element tags in the current mesh in a contiunous sequence.
            /// </summary>
            public bool Renumber;

            /// <summary>
            /// Number of smoothing steps applied to the final mesh. Gmsh default value: '1'.
            /// </summary>
            public int Smoothing;

            /// <summary>
            /// Maximum mesh element size. It is recommended not to set it or to set it to a value bigger than MeshSize. Gmsh default value: '1E+22'
            /// </summary>
            public double MeshSizeMax;

            /// <summary>
            /// Minimum mesh element size. It is recommended not to set it or to set it to a value smaller than MeshSize. Gmsh default value: '0'.
            /// </summary>
            public double MeshSizeMin;

            /// <summary>
            /// Elements globally order from 1 to n between each mesh in meshes, if false elements are ordered only inside the single mesh
            /// </summary>
            public bool UseGlobalProgressID;

            /// <summary>
            /// Set a transfinite meshing constraint on the curve tag with numNodes nodes distributed according to meshType and coef.
            /// </summary>
            public bool Transfinite;

            /// <summary>
            /// Types of transfinite metod. Currently supported types are "Progression" (geometrical progression with power coef) and "Bump" (refinement toward both extremities of the curve)
            /// </summary>
            public TransfiniteType TransfiniteLineType;

            /// <summary>
            /// Transfinite factor. Gmsh default value: "1".
            /// </summary>
            public double TransfiniteFactor;

            /// <summary>
            /// Optimize the mesh to improve the quality of tetrahedral elements. Optimizes the current mesh with the given algorithm. If force is set apply the optimization also to discrete entities. If dimTags is given, only apply the optimizer to the given entities. Gmsh default value: '1'
            /// </summary>
            public bool Optimize;

            /// <summary>
            /// Optimize algorithm. Gmsh default: Netgen.
            /// </summary>
            public MeshOptimize OptimizeAlgorithm;

            /// <summary>
            /// Number of Optimize Iteration. Gmsh default value: '1'
            /// </summary>
            public int OptimizeIteration;

            /// <summary>
            /// Optimize the mesh using Netgen to improve the quality of tetrahedral elements. Gmsh default value: '0'
            /// </summary>
            public double OptimizeNetgen;

            /// <summary>
            /// Number of refinement steps in the MeshAdapt-based 2D algorithms. Gmsh default value: '10'
            /// </summary>
            public int RefineSteps;

            /// <summary>
            /// Optimize tetrahedra that have a quality below MinQuality value. Gmsh default value: '0.3'
            /// </summary>
            public double MinQuality;

            /// <summary>
            /// Random factor used in the 2D meshing algorithm (should be increased if RandomFactor * size(triangle)/size(model) approaches machine accuracy). Gmsh default value: '1E-9'
            /// </summary>
            public double RandomFactor;

            /// <summary>
            /// Use the Transfinite Surface algorithm to mesh the surface. It's valid only on 4 edge geometries without embedded geometries. MeshSize is the smallest size in the mesh. It overrides any other mesh settings. Default valure: "false".
            /// </summary>
            public bool TransfiniteSurface;

            #endregion

            #region GeometryOptions

            /// <summary>
            /// Apply various healing procedures to all the entities in the model in the OpenCASCADE CAD representation
            /// </summary>
            public bool HealShapes;

            /// <summary>
            /// Matches geometries and meshes. Gmsh default value: '0'
            /// </summary>
            public double MatchGeomAndMesh;

            /// <summary>
            /// Sew faces with the OpenCASCADE kernel. Gmsh default value: '0'
            /// </summary>
            public double SewFaces;

            /// <summary>
            /// Try to unify faces and edges (remove internal seams) which lie on the same geometry after performing a boolean union with the OpenCASCADE kernel. Gmsh default value: '1'
            /// </summary>
            public int OCCUnionUnify;

            #endregion

            #region ToleranceOptions

            /// <summary>
            /// Geometrical tolerance. Gmsh default value: '1e-08'
            /// </summary>
            public double Tolerance;

            /// <summary>
            /// Tolerance for matching mesh and geometry. Gmsh default value: '1e-06'
            /// </summary>
            public double MatchMeshTolerance;

            /// <summary>
            /// Tolerance for initial 3D Delaunay mesher. Gmsh default value: '1e-08'
            /// </summary>
            public double ToleranceInitialDelaunay;

            /// <summary>
            /// Skip a model edge in mesh generation if its length is less than user's defined tolerance. Gmsh default value: '0'
            /// </summary>
            public double ToleranceEdgeLength;

            /// <summary>
            /// Consider connected facets as overlapping when the dihedral angle between the facets is smaller than the user's defined tolerance (in degrees). Gmsh default value: '0.1'
            /// </summary>
            public double AngleToleranceFacetOverlap;

            #endregion

            #region ScalingOptions

            /// <summary>
            /// Initial scaling factor for the mesh to correspond to size of the geometry. Gmsh default value: '1'
            /// </summary>
            public double GeometryBaseScaleFactor;

            /// <summary>
            /// Rescaling factor for the mesh to correspond to size of the geometry. Gmsh default value: '1'
            /// </summary>
            public double MeshScalingFactor;

            #endregion

            #region Public enums
            public enum MeshAlgorithm
            {
                MeshAdapt = 1,
                Automatic = 2,
                InitialMeshOnly = 3,
                Delaunay = 5,
                [Description("Frontal-Delaunay")]
                FrontalDelaunay = 6,
                BAMG = 7,
                [Description("Frontal-Delaunay for Quads")]
                FrontalDelaunayForQuads = 8,
                [Description("Packing of Parallelograms")]
                PackingOfParallelograms = 9,
                [Description("Quasi-structured Quad")]
                QuasiStructuredQuad = 11,
            }

            public enum RecombinationMeshAlgorithm
            {
                Simple = 0,
                Blossom = 1,
                [Description("Simple Full-Quad")]
                SimpleFullQuad = 2,
                [Description("Blossom Full-Quad")]
                BlossomFullQuad = 3,
            }

            public enum TransfiniteType
            {
                [Description("Progression")]            // geometrical progression with power coef (ogni curva ha il numero delle suddivisioni della precedente moltiplicato per coef)
                Progression = 0,
                [Description("Bump")]                   // refinement toward both extremities of the curve
                Bump = 1,
            }

            public enum MeshOptimize
            {
                [Description("")]
                Tetrahedral = 0,
                [Description("Netgen")]
                Netgen = 1,
                [Description("HighOrder")]
                HighOrder = 2,
                [Description("HighOrderElastic")]
                HighOrderElastic = 3,
                [Description("HighOrderFastCurving")]
                HighOrderFastCurving = 4,
                [Description("Relocate2D")]
                Relocate2D = 5,
                [Description("Relocate3D")]
                Relocate3D = 6,
            }

            #endregion

            public GMeshGenerateOptions()
            {
                // Mesh
                Algorithm = MeshAlgorithm.FrontalDelaunayForQuads;
                Recombine = true;
                RecombinationAlgorithm = RecombinationMeshAlgorithm.SimpleFullQuad;
                RecombineOptimizeTopology = 5;
                Renumber = true;
                Smoothing = 1;
                Refine = false;
                MeshSize = 1E+22;
                MeshSizeMax = 1E+22;
                MeshSizeMin = 0;
                UseGlobalProgressID = true;
                Transfinite = true;
                TransfiniteLineType = TransfiniteType.Progression;
                TransfiniteFactor = 1;
                Optimize = false;
                OptimizeAlgorithm = MeshOptimize.Netgen;
                OptimizeIteration = 1;
                OptimizeNetgen = 0;
                RefineSteps = 10;
                MinQuality = 0.3;
                RandomFactor = 1E-9;
                TransfiniteSurface = false;

                // Geometry
                HealShapes = true;
                MatchGeomAndMesh = 0;
                SewFaces = 0;
                OCCUnionUnify = 1;

                // Tolleranza
                Tolerance = 1E-8;
                MatchMeshTolerance = 1E-6;
                ToleranceInitialDelaunay = 1E-8;
                ToleranceEdgeLength = 0;
                AngleToleranceFacetOverlap = 0.1;

                // ScalingOptions
                GeometryBaseScaleFactor = 1;
                MeshScalingFactor = 1;

            }

            public override object Clone()
            {
                GMeshGenerateOptions clone = new GMeshGenerateOptions
                {
                    Algorithm = Algorithm,
                    Recombine = Recombine,
                    RecombinationAlgorithm = RecombinationAlgorithm,
                    RecombineOptimizeTopology = RecombineOptimizeTopology,
                    Renumber = Renumber,
                    Smoothing = Smoothing,
                    Refine = Refine,
                    MeshSize = MeshSize,
                    MeshSizeMax = MeshSizeMax,
                    MeshSizeMin = MeshSizeMin,
                    UseGlobalProgressID = UseGlobalProgressID,
                    Transfinite = Transfinite,
                    TransfiniteLineType = TransfiniteLineType,
                    TransfiniteFactor = TransfiniteFactor,
                    Optimize = Optimize,
                    OptimizeAlgorithm = OptimizeAlgorithm,
                    OptimizeIteration = OptimizeIteration,
                    OptimizeNetgen = OptimizeNetgen,
                    RefineSteps = RefineSteps,
                    MinQuality = MinQuality,
                    RandomFactor = RandomFactor,

                    HealShapes = HealShapes,
                    MatchGeomAndMesh = MatchGeomAndMesh,
                    SewFaces = SewFaces,
                    OCCUnionUnify = OCCUnionUnify,

                    Tolerance = Tolerance,
                    MatchMeshTolerance = MatchMeshTolerance,
                    ToleranceInitialDelaunay = ToleranceInitialDelaunay,
                    ToleranceEdgeLength = ToleranceEdgeLength,
                    AngleToleranceFacetOverlap = AngleToleranceFacetOverlap,

                    GeometryBaseScaleFactor = GeometryBaseScaleFactor,
                    MeshScalingFactor = MeshScalingFactor
                };

                return clone;
            }
        }

        [Serializable]
        public sealed class GMeshGenerateMeshStatus : GenerateMeshStatus
        {
            public const string TotalTime = "Total time";
            public const string FailedToTransfinite = "GmshException: Failed to set transfinite";
            public const string FailedToCreateTheShape = "Failed to create the shape";
            public const string FailedToEmbedTheLine = "Failed to embed the line";
            public const string FailedToEmbedThePoint = "Failed to embed the point";
            public const string FailedToEmbedThePolygon = "Failed to embed the polygon";
            public const string FailedToEmbedTheShape = "Failed to embed the shape";
            public const string UnableToRecover = "GmshException: Mesh.Generate Unable to recover the edge on curve";
            public const string IdenticalPoints = "GmshException: Identical points in triangulation";
            public const string SingularMatrix = "GmshException: Singular matrix 3x3";

            private readonly Dictionary<Mesh, Dictionary<GeometryBase, int[]>> _embeddedGeometriesVertexMap;

            private int _embeddedPoints;
            private int _embeddedLines;
            private int _embeddedPolygons;
            private int _embeddedShapes;

            public Dictionary<Mesh, Dictionary<GeometryBase, int[]>> EmbeddedGeometriesVertexMap => _embeddedGeometriesVertexMap;

            public int EmbeddedPoints { get => _embeddedPoints; set => _embeddedPoints = value; }

            public int EmbeddedLines { get => _embeddedLines; set => _embeddedLines = value; }

            public int EmbeddedPolygons { get => _embeddedPolygons; set => _embeddedPolygons = value; }

            public int EmbeddedShapes { get => _embeddedShapes; set => _embeddedShapes = value; }

            public GMeshGenerateMeshStatus()
                : base()
            {
                _embeddedGeometriesVertexMap = new Dictionary<Mesh, Dictionary<GeometryBase, int[]>>();
            }

            public void AddEmbeddedGeometries(Mesh mesh, Dictionary<GeometryBase, int[]> embeddedGeometries)
            {
                _embeddedGeometriesVertexMap[mesh] = embeddedGeometries;
            }
        }

        private sealed class OpenCascadeWrapper
        {
            private Dictionary<Point3d, int> _pointTag;
            private Dictionary<Line3d, int> _lineTag;


            /// <summary>
            /// This Class use <see cref="Gmsh"/> then <see cref="Gmsh.Initialize(char[], bool)"/> needs to be called before calling any of its methods
            /// </summary>
            public OpenCascadeWrapper()
            {
                _pointTag = new Dictionary<Point3d, int>();
                _lineTag = new Dictionary<Line3d, int>();
            }

            /// <summary>
            /// Add a point to OCC and return the tag. If point already exist in OCC return the tag of the point already present
            /// </summary>
            /// <param name="point">Point to Add</param>
            /// <returns>Point tag inside OCC</returns>
            /// <remarks>This method use <see cref="Gmsh"/> then <see cref="Gmsh.Initialize(char[], bool)"/> needs to be called before calling this method </remarks>
            public int AddPoint(Point3d point)
            {
                if (_pointTag.ContainsKey(point))
                {
                    return _pointTag[point];
                }

                int t = Gmsh.Model.Occ.AddPoint(point.X, point.Y, point.Z);
                return _pointTag[point] = t;
            }

            /// <summary>
            /// Add a point to OCC and return the tag. If point already exist in OCC return the tag of the point already present
            /// </summary>
            /// <param name="point">Point to Add</param>
            /// <returns>Point tag inside OCC</returns>
            /// <remarks>This method use <see cref="Gmsh"/> then <see cref="Gmsh.Initialize(char[], bool)"/> needs to be called before calling this method </remarks>
            public int AddPointAndSync(Point3d point)
            {
                if (_pointTag.ContainsKey(point))
                {
                    return _pointTag[point];
                }

                int t = Gmsh.Model.Occ.AddPoint(point.X, point.Y, point.Z);
                Gmsh.Model.Occ.Synchronize();
                return _pointTag[point] = t;
            }

            /// <summary>
            /// Add a point to OCC and return the tag. If point already exist in OCC return the tag of the point already present
            /// </summary>
            /// <param name="point">Point to Add</param>
            /// <param name="tolerance">Tolerance</param>
            /// <returns>Point tag inside OCC</returns>
            /// <remarks>This method use <see cref="Gmsh"/> then <see cref="Gmsh.Initialize(char[], bool)"/> needs to be called before calling this method </remarks>
            public int AddPoint(Point3d point, double tolerance)
            {
                int tag = GetPointTag(point, tolerance);

                if (tag != -1)
                    return tag;
                else
                {
                    int t = Gmsh.Model.Occ.AddPoint(point.X, point.Y, point.Z);
                    return _pointTag[point] = t;
                }
            }

            /// <summary>
            /// Add a point to OCC and return the tag. If point already exist in OCC return the tag of the point already present
            /// </summary>
            /// <param name="point">Point to Add</param>
            /// <param name="tolerance">Tolerance</param>
            /// <returns>Point tag inside OCC</returns>
            /// <remarks>This method use <see cref="Gmsh"/> then <see cref="Gmsh.Initialize(char[], bool)"/> needs to be called before calling this method </remarks>
            public int AddPointAndSync(Point3d point, double tolerance)
            {
                int tag = GetPointTag(point, tolerance);

                if (tag != -1)
                    return tag;
                else
                {
                    int t = Gmsh.Model.Occ.AddPoint(point.X, point.Y, point.Z);
                    Gmsh.Model.Occ.Synchronize();
                    return _pointTag[point] = t;
                }
            }

            /// <summary>
            /// Add a line to OCC and return the tag. If start or end points exist, use the existing point tags
            /// </summary>
            /// <param name="line">Line to add</param>
            /// <param name="tolerance">Tolerance of matching points</param>
            /// <returns>The tag of the line</returns>
            public int AddLineAndSync(Line3d line, double tolerance)
            {
                int startTag;
                int endTag;

                startTag = AddPoint(line.Start, tolerance);
                endTag = AddPoint(line.End, tolerance);

                int t = Gmsh.Model.Occ.AddLine(startTag, endTag);
                Gmsh.Model.Occ.Synchronize();
                return _lineTag[line] = t;
            }

            /// <summary>
            /// Add a line to OCC and return the tag. If start or end points exist, use the existing point tags
            /// </summary>
            /// <param name="startTag">Start line gmsh tag</param>
            /// <param name="endTag">End line gmsh tag</param>
            /// <returns>The tag of the line</returns>
            public int AddLineAndSync(int startTag, int endTag)
            {
                int t = Gmsh.Model.Occ.AddLine(startTag, endTag);
                Gmsh.Model.Occ.Synchronize();

                Gmsh.Model.GetBoundingBox(1, t, out double x1, out double y1, out double z1, out double x2, out double y2, out double z2);
                Line3d line = new Line3d(new Point3d(x1, y1, z1), new Point3d(x2, y2, z2));
                return _lineTag[line] = t;
            }

            /// <summary>
            /// Get the point tag
            /// </summary>
            /// <param name="point">Point to Add</param>
            /// <returns>Point tag inside OCC if already added. If the point does not exist return -1</returns>
            public int GetPointTag(Point3d point)
            {
                if (_pointTag.ContainsKey(point))
                {
                    return _pointTag[point];
                }
                else
                {
                    return -1;
                }
            }

            /// <summary>
            /// Get the point tag
            /// </summary>
            /// <param name="point">Point to Add</param>
            /// <param name="tolerance">The tolerance of matching</param>
            /// <returns>Point tag inside OCC if already added. If the point does not exist return -1</returns>
            public int GetPointTag(Point3d point, double tolerance)
            {
                int tag = -1;

                if (_pointTag.ContainsKey(point))
                {
                    return _pointTag[point];
                }

                Parallel.For(0, _pointTag.Keys.Count, (i, state) =>
                {
                    double dist = Math.Sqrt(point.DistanceTo(_pointTag.ElementAt((int)i).Key));
                    double tol = Math.Sqrt(ErrorPropagation.ProductTolerance(dist, dist, tolerance, tolerance));

                    if (Math.Abs(dist) < tol)
                    {
                        tag = _pointTag[_pointTag.ElementAt((int)i).Key];
                        state.Break();
                    }
                });

                return tag;
            }

            /// <summary>
            /// Rebuild the objects tag associations inside OpenCascadeWrapper
            /// </summary>
            /// <param name="surfaceTag">If is set, rebuilt only the points of the selected surface (the input is the surfaceTag). Otherwise rebuilt all the points of the model</param>
            /// <remarks>This method use <see cref="Gmsh"/> then <see cref="Gmsh.Initialize( char[], bool)"/> needs to be called before calling this method </remarks>
            public void RebuildObjectTags(int surfaceTag = -1)
            {
                Dictionary<Point3d, int> pointsTagCopy = _pointTag;
                _pointTag = new Dictionary<Point3d, int>(); // Reset della mappa
                (int, int)[] tags;

                if (surfaceTag == -1)
                {
                    tags = Gmsh.Model.Occ.GetEntities(0); // Recupero tutte le entità

                    for (int i = 0; i < pointsTagCopy.Keys.Count; i++)
                    {
                        int indexMin = -1;
                        double dMin = Double.MaxValue;
                        for (int j = 0; j < tags.Length; j++)
                        {
                            Gmsh.Model.Occ.GetBoundingBox(0, tags.ElementAt(j).Item2, out double x, out double y, out double z, out _, out _, out _);
                            Point3d pointToTest = new Point3d(x, y, z);
                            double squareDistance = pointToTest.SquareDistanceTo(pointsTagCopy.ElementAt(i).Key);

                            if (Math.Abs(squareDistance) < dMin)
                            {
                                indexMin = tags.ElementAt(j).Item2;
                                dMin = squareDistance;
                            }
                        }

                        try
                        {
                            _pointTag.Add(pointsTagCopy.ElementAt(i).Key, indexMin); // Ricostruisco mappa 
                        }
                        catch (Exception) { }
                    }
                }
                else
                {
                    tags = Gmsh.Model.GetBoundary(new (int, int)[] { (2, surfaceTag) }, false, false, true);

                    for (int i = 0; i < tags.Length; i++)
                    {
                        int indexMin = -1;
                        double dMin = Double.MaxValue;
                        Point3d pointMatched = null;
                        Gmsh.Model.Occ.GetBoundingBox(0, tags.ElementAt(i).Item2, out double x, out double y, out double z, out _, out _, out _);
                        Point3d pointToMatch = new Point3d(x, y, z);

                        for (int j = 0; j < pointsTagCopy.Keys.Count; j++)
                        {
                            double squareDistance = pointsTagCopy.ElementAt(j).Key.SquareDistanceTo(pointToMatch);

                            if (Math.Abs(squareDistance) < dMin)
                            {
                                indexMin = tags.ElementAt(i).Item2;
                                dMin = squareDistance;
                                pointMatched = pointsTagCopy.ElementAt(j).Key;
                            }
                        }
                        try
                        {
                            _pointTag.Add(pointMatched, indexMin); // Ricostruisco mappa
                        }
                        catch (Exception) { }
                    }

                    for (int p = 0; p < pointsTagCopy.Keys.Count; p++)
                    {
                        if (!_pointTag.ContainsKey(pointsTagCopy.ElementAt(p).Key))
                        {
                            _pointTag.Add(pointsTagCopy.ElementAt(p).Key, pointsTagCopy[pointsTagCopy.ElementAt(p).Key]);
                        }
                    }
                }
            }

            /// <summary>
            /// Rebuild the objects tag associations inside OpenCascadeWrapper and check if each point exist
            /// </summary>
            /// <remarks>This method use <see cref="Gmsh"/> then <see cref="Gmsh.Initialize(char[], bool)"/> needs to be called before calling this method </remarks>
            public void RebuildObjectTagsAndCheck(double tolerance)
            {
                Dictionary<Point3d, int> pointsTagCopy = _pointTag;
                _pointTag = new Dictionary<Point3d, int>(); // Reset della mappa

                double toll = ErrorPropagation.DefaultProductTolerance(2.82 * tolerance);

                (int, int)[] tags = Gmsh.Model.Occ.GetEntities(0); // Recupero tutte le entità

                for (int i = 0; i < pointsTagCopy.Keys.Count; i++)
                {
                    int indexMin = -1;
                    double dMin = Double.MaxValue;

                    for (int j = 0; j < tags.Length; j++)
                    {
                        Gmsh.Model.Occ.GetBoundingBox(0, tags.ElementAt(j).Item2, out double x, out double y, out double z, out _, out _, out _);
                        Point3d pointToTest = new Point3d(x, y, z);
                        double squareDistance = pointToTest.DistanceTo(pointsTagCopy.ElementAt(i).Key);
                        double distance = Math.Sqrt(squareDistance);

                        toll = ErrorPropagation.ProductTolerance(distance, distance, tolerance, tolerance);
                        toll = toll < 1E-13 ? 1E-13 : toll;

                        if (Math.Abs(squareDistance) < dMin)
                        {
                            indexMin = tags.ElementAt(j).Item2;
                            dMin = squareDistance;
                        }
                    }

                    if (dMin < toll)
                    {
                        try
                        {
                            _pointTag.Add(pointsTagCopy.ElementAt(i).Key, indexMin); // Ricostruisco mappa
                        }
                        catch { }
                    }

                    else
                    {
                        _pointTag.Remove(pointsTagCopy.ElementAt(i).Key);
                        int t = Gmsh.Model.Occ.AddPoint(pointsTagCopy.ElementAt(i).Key.X, pointsTagCopy.ElementAt(i).Key.Y, pointsTagCopy.ElementAt(i).Key.Z);
                        _pointTag.Add(pointsTagCopy.ElementAt(i).Key, t);
                    }
                }
            }
        }

        #endregion
    }
}
