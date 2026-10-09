using GmshNet;
using GPC.Utilities.Maths;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace GPC.Geometry.Meshes.GMesh
{
    /// <summary>
    /// Mesh of planar shapes generated with Gmsh (OpenCASCADE kernel): triangles or quadrilaterals (recombination), with points, lines,
    /// polygons and shapes embedded in the surfaces and optional transfinite meshes. Gmsh has a global state: the generations are serialized
    /// </summary>
    public sealed class GMesh : Mesh
    {
        #region Properties

        /// <summary>
        /// The options used to generate the mesh (null if the mesh was not generated with options)
        /// </summary>
        public GMeshGenerateOptions GMeshOptions => (GMeshGenerateOptions)_options;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates an empty mesh
        /// </summary>
        public GMesh()
            : base()
        {
        }

        /// <summary>
        /// Deserialization constructor (see <see cref="Mesh"/>)
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        private GMesh(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        #endregion

        #region Generate mesh functions

        /// <summary>
        /// Generate the meshes of shapes, one mesh for each shape, without embedded geometries
        /// (see <see cref="Generate(IEnumerable{Shape}, Dictionary{Shape, GeometryBase[]}, Dictionary{GeometryBase, double}, GMeshGenerateOptions, out List{Mesh}, out GMeshGenerateMeshStatus)"/>)
        /// </summary>
        /// <param name="shapes">Geometry to mesh</param>
        /// <param name="options">Generate mesh options</param>
        /// <param name="meshes">The meshes generated, one for each shape</param>
        /// <param name="generateMeshStatus">The errors, the warnings and the additional information of the generation</param>
        /// <returns>True if the mesh is generate without error, false otherwise</returns>
        public static bool Generate(IEnumerable<Shape> shapes, GMeshGenerateOptions options, out List<Mesh> meshes, out GMeshGenerateMeshStatus generateMeshStatus)
        {
            var _ = new Dictionary<Mesh, Dictionary<GeometryBase, int[]>>(ReferenceComparer<Mesh>.Instance);
            return Generate(shapes, null, null, options, out meshes, out generateMeshStatus);
        }

        /// <summary>
        /// Generate the meshes of shapes with embedded geometries and their mesh sizes
        /// (see <see cref="Generate(IEnumerable{Shape}, Dictionary{Shape, GeometryBase[]}, Dictionary{GeometryBase, double}, GMeshGenerateOptions, out List{Mesh}, out GMeshGenerateMeshStatus)"/>)
        /// </summary>
        /// <param name="shapes">Geometry to mesh</param>
        /// <param name="embeddedGeometries">association one to many of geometries embedded in geometry. es points in surface. Keys must be contained in shapes </param>
        /// <param name="generateMeshStatus">Class that collect all the errors, warning and additional information related to geometry generation</param>
        /// <param name="options">Generate mesh options</param>
        /// <param name="meshes">The meshes generated, one for each shape</param>
        /// <param name="embeddedGeomMeshSize">Mesh size at specific embed geometry. Keys must be contained in <paramref name="embeddedGeometries"/></param>
        /// <returns>True if the mesh is generate without error, false otherwise</returns>
        public static bool Generate(IEnumerable<Shape> shapes, Dictionary<Shape, GeometryBase[]> embeddedGeometries, GMeshGenerateOptions options,
            out List<Mesh> meshes, out GMeshGenerateMeshStatus generateMeshStatus, Dictionary<GeometryBase, double> embeddedGeomMeshSize = null)
        {
            return Generate(shapes, embeddedGeometries, embeddedGeomMeshSize, options, out meshes, out generateMeshStatus);
        }

        /// <summary>
        /// Generate the meshes of shapes with embedded geometries
        /// (see <see cref="Generate(IEnumerable{Shape}, Dictionary{Shape, GeometryBase[]}, Dictionary{GeometryBase, double}, GMeshGenerateOptions, out List{Mesh}, out GMeshGenerateMeshStatus)"/>)
        /// </summary>
        /// <param name="shapes">Geometry to mesh</param>
        /// <param name="embeddedGeometries">association one to many of geometries embedded in geometry. es points in surface. Keys must be contained in shapes </param>
        /// <param name="generateMeshStatus">Class that collect all the errors, warning and additional information related to geometry generation</param>
        /// <param name="options">Generate mesh options</param>
        /// <param name="meshes">The meshes generated, one for each shape</param>
        /// <returns>True if the mesh is generate without error, false otherwise</returns>
        public static bool Generate(IEnumerable<Shape> shapes, Dictionary<Shape, GeometryBase[]> embeddedGeometries, GMeshGenerateOptions options,
            out List<Mesh> meshes, out GMeshGenerateMeshStatus generateMeshStatus)
        {
            return Generate(shapes, embeddedGeometries, null, options, out meshes, out generateMeshStatus);
        }

        /// <summary>
        /// Generate the meshes of shapes with Gmsh, one mesh for each shape: the shapes are built with OpenCASCADE, the embedded geometries
        /// (points, lines, polygons, shapes) are embedded in the surfaces, then the surfaces are meshed with the options
        /// </summary>
        /// <param name="shapesInput">Geometry to mesh</param>
        /// <param name="embeddedGeometriesInput">association one to many of geometries embedded in geometry. es points in surface. Keys must be contained in shapes </param>
        /// <param name="embeddedGeomMeshSize">Mesh size at specific embed geometry. Keys must be contained in <paramref name="embeddedGeometriesInput"/></param>
        /// <param name="meshes">The meshes generated</param>
        /// <param name="generateMeshStatus">Class that collect all the errors, warning and additional information related to geometry generation</param>
        /// <param name="options">Generate mesh options</param>
        /// <returns>True if the mesh is generate without error, false otherwise</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="options"/> or <paramref name="shapesInput"/> is null</exception>
        /// <exception cref="ArgumentException">If the mesh sizes are not finite and positive (0 &lt;= minimum &lt;= maximum)</exception>
        /// <remarks>The geometric tolerance of the points is <see cref="GeometryBase.Tolerance"/> (1E-4) times <see cref="GMeshGenerateOptions.GeometryBaseScaleFactor"/>:
        /// for a smaller tolerance scale the geometry by a number lower than 1 and then rescale the mesh with the reciprocal.
        /// Gmsh has a global state: the calls are serialized (lock) and Gmsh is always finalized, also when an exception is thrown
        /// or the generation fails (before, many error paths returned without Gmsh.Finalize and the next call found the old model)</remarks>
        public static bool Generate(IEnumerable<Shape> shapesInput, Dictionary<Shape, GeometryBase[]> embeddedGeometriesInput, Dictionary<GeometryBase,
            double> embeddedGeomMeshSize, GMeshGenerateOptions options, out List<Mesh> meshes, out GMeshGenerateMeshStatus generateMeshStatus)
        {
            if (options is null)
                throw new ArgumentNullException(nameof(GenerateOptions));

            if (shapesInput is null)
                throw new ArgumentNullException(nameof(Shape));

            if (!IsFinitePositive(options.MeshSize) || !IsFinitePositive(options.MeshSizeMax)
                || double.IsNaN(options.MeshSizeMin) || double.IsInfinity(options.MeshSizeMin)
                || options.MeshSizeMin < 0 || options.MeshSizeMin > options.MeshSizeMax)
                throw new ArgumentException("Mesh sizes must be finite, with positive target/maximum and 0 <= minimum <= maximum.", nameof(options));

            if (embeddedGeomMeshSize != null && embeddedGeomMeshSize.Values.Any(size => !IsFinitePositive(size)))
                throw new ArgumentException("Embedded mesh sizes must be finite and positive.", nameof(embeddedGeomMeshSize));

            // the input is enumerated once (before, Count() and ElementAt(s) at every step)
            IList<Shape> shapesList = shapesInput as IList<Shape> ?? shapesInput.ToList();

            var curveSegments = ExpandEmbeddedCurves(shapesList, ref embeddedGeometriesInput, ref embeddedGeomMeshSize, options);

            lock (GmshSync)
            {
                // Personal Gmsh configuration must not override this library's meshing options.
                Gmsh.Initialize(readConfigFiles: false);
                try
                {
                    bool success = GenerateCore(shapesList, embeddedGeometriesInput, embeddedGeomMeshSize, options, out meshes, out generateMeshStatus);
                    AddCurveVertexMaps(curveSegments, options, generateMeshStatus);
                    return success;
                }
                finally
                {
                    Gmsh.Finalize();
                }
            }
        }

        /// <summary>
        /// Gmsh is a global state: one generation at a time
        /// </summary>
        private static readonly object GmshSync = new object();

        /// <summary>
        /// Tell if a value is finite and positive
        /// </summary>
        /// <param name="value">The value</param>
        /// <returns>True if the value is positive and not infinite (false for NaN)</returns>
        private static bool IsFinitePositive(double value) => value > 0 && !double.IsInfinity(value);

        // Expand only the new curve types; existing inputs continue through the identical generation path.
        private static Dictionary<Curve3d, Line3d[]> ExpandEmbeddedCurves(IList<Shape> shapes,
            ref Dictionary<Shape, GeometryBase[]> embedded, ref Dictionary<GeometryBase, double> sizes, GMeshGenerateOptions options)
        {
            var curves = new Dictionary<Curve3d, Line3d[]>(ReferenceComparer<Curve3d>.Instance);
            if (embedded == null) return curves;
            foreach (var shape in shapes)
                if (embedded.TryGetValue(shape, out var geometries) && geometries != null)
                    foreach (var curve in geometries.OfType<Curve3d>())
                        if (!curves.ContainsKey(curve))
                            curves.Add(curve, curve.ToLineSegments(options.CurveChordTolerance, options.CurveMaxSegmentLength));
            if (curves.Count == 0) return curves;
            if (!IsFinitePositive(options.GeometryBaseScaleFactor) || !IsFinitePositive(options.MeshScalingFactor))
                throw new ArgumentException("Curve meshing requires finite positive geometry and mesh scale factors.", nameof(options));

            var expanded = new Dictionary<Shape, GeometryBase[]>(embedded.Comparer);
            var expandedSizes = sizes == null ? null : new Dictionary<GeometryBase, double>(sizes, GeometryKeyComparer.Instance);
            foreach (var entry in embedded)
            {
                var geometries = new List<GeometryBase>();
                foreach (var geometry in entry.Value ?? Array.Empty<GeometryBase>())
                {
                    if (geometry is Curve3d curve && curves.TryGetValue(curve, out var lines))
                    {
                        geometries.AddRange(lines);
                        if (sizes != null && sizes.TryGetValue(curve, out double size))
                            foreach (var line in lines)
                            {
                                // Overlapping constraints use the finer of the requested sizes.
                                if (expandedSizes.TryGetValue(line, out double previous)) expandedSizes[line] = Math.Min(previous, size);
                                else expandedSizes.Add(line, size);
                            }
                    }
                    else geometries.Add(geometry);
                }
                expanded.Add(entry.Key, geometries.Distinct(GeometryKeyComparer.Instance).ToArray());
            }
            embedded = expanded; sizes = expandedSizes; return curves;
        }

        private static void AddCurveVertexMaps(Dictionary<Curve3d, Line3d[]> curves, GMeshGenerateOptions options, GMeshGenerateMeshStatus status)
        {
            if (curves.Count == 0 || status == null) return;
            foreach (var meshMap in status.EmbeddedGeometriesVertexMap)
                foreach (var curve in curves)
                {
                    var nodes = new List<int>(); var seen = new HashSet<int>();
                    foreach (var segment in curve.Value)
                    {
                        var scaled = segment.Scale(options.GeometryBaseScaleFactor);
                        if (!meshMap.Value.TryGetValue(scaled, out var ids)) continue;
                        // Shared legacy line keys are direction-independent; restore the curve's traversal order.
                        Point3d first = scaled.Start.Scale(options.MeshScalingFactor);
                        IEnumerable<int> ordered = ids;
                        if (ids.Length > 1 && meshMap.Key.GetVertex(ids[0]).Point.DistanceTo(first)
                            > meshMap.Key.GetVertex(ids[ids.Length - 1]).Point.DistanceTo(first)) ordered = ids.Reverse();
                        foreach (int id in ordered) if (seen.Add(id)) nodes.Add(id);
                    }
                    if (nodes.Count > 0) meshMap.Value[curve.Key] = nodes.ToArray();
                }
        }

        /// <summary>
        /// The generation, with Gmsh initialized and locked (see <see cref="Generate(IEnumerable{Shape}, Dictionary{Shape, GeometryBase[]}, Dictionary{GeometryBase, double}, GMeshGenerateOptions, out List{Mesh}, out GMeshGenerateMeshStatus)"/>)
        /// </summary>
        /// <param name="shapesInput">Geometry to mesh</param>
        /// <param name="embeddedGeometriesInput">The geometries embedded in each shape</param>
        /// <param name="embeddedGeomMeshSize">Mesh size at specific embed geometry</param>
        /// <param name="options">Generate mesh options</param>
        /// <param name="meshes">The meshes generated</param>
        /// <param name="generateMeshStatus">The errors, the warnings and the additional information of the generation</param>
        /// <returns>True if the mesh is generate without error, false otherwise</returns>
        private static bool GenerateCore(IList<Shape> shapesInput, Dictionary<Shape, GeometryBase[]> embeddedGeometriesInput, Dictionary<GeometryBase,
            double> embeddedGeomMeshSize, GMeshGenerateOptions options, out List<Mesh> meshes, out GMeshGenerateMeshStatus generateMeshStatus)
        {
            Stopwatch clock = Stopwatch.StartNew();
            TimeSpan dt0 = clock.Elapsed;


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
            // Gmsh 4.15.2's algorithm 9 unconditionally invokes UntangleTris, even
            // with Optimize=false. Embedded surfaces can cause native heap corruption
            // there, which cannot be caught as a GmshException. Keep the requested
            // options intact and report the safe algorithm actually used below.
            bool packingWorkaround = options.Algorithm == GMeshGenerateOptions.MeshAlgorithm.PackingOfParallelograms
                && Gmsh.Option.GetString("General.Version") == "4.15.2";
            Gmsh.Option.SetNumber("Mesh.Algorithm", (int)(packingWorkaround
                ? GMeshGenerateOptions.MeshAlgorithm.FrontalDelaunayForQuads : options.Algorithm));

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

            TimeSpan dt1 = clock.Elapsed;

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
            if (packingWorkaround)
                generateMeshStatus.AddWarning("Gmsh 4.15.2 PackingOfParallelograms can crash in native UntangleTris; using FrontalDelaunayForQuads with the requested recombination instead.");
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
            Dictionary<Shape, Dictionary<GeometryBase, double>> embGeomAssociation = new Dictionary<Shape, Dictionary<GeometryBase, double>>(ReferenceComparer<Shape>.Instance);
            // Associazione tra shape e geometria emb 
            Dictionary<Shape, GeometryBase[]> embeddedGeometries = new Dictionary<Shape, GeometryBase[]>(ReferenceComparer<Shape>.Instance);
            // Associazione tra shape embedded non modificata e shape embedded modificata.
            Dictionary<Shape, Shape> shapeOutputAssociation = new Dictionary<Shape, Shape>(ReferenceComparer<Shape>.Instance);

            HashSet<GeometryBase> listOfAllEmbGeom = new HashSet<GeometryBase>(GeometryKeyComparer.Instance);
            HashSet<GeometryBase> listOfAllEmbGeomForEmbShape = new HashSet<GeometryBase>(GeometryKeyComparer.Instance);

            // prima di scalare tutto, creo un dizionario in cui ho la shape, il riferimento in memoria della geometria (scalata) e la size.
            // scalo tutto del fattore scalegeometryfactor            
            for (int s = 0; s < shapesInput.Count; s++)
            {
                Shape shape = new Shape(shapesInput[s]);
                shape = shape.Scale(options.GeometryBaseScaleFactor);
                shapesList.Add(shape);
                List<GeometryBase> geometryBases = new List<GeometryBase>();
                Dictionary<GeometryBase, double> geometryAndMeshSize = new Dictionary<GeometryBase, double>(GeometryKeyComparer.Instance);

                if (embeddedGeometriesInput != null && embeddedGeometriesInput.ContainsKey(shapesInput[s]))
                {
                    for (int i = 0; i < embeddedGeometriesInput[shapesInput[s]].Length; i++)
                    {
                        if (embeddedGeometriesInput[shapesInput[s]][i] is Point3d point3d)
                        {
                            Point3d scaledPoint = point3d.Scale(options.GeometryBaseScaleFactor);
                            geometryBases.Add(scaledPoint);
                            if (embeddedGeomMeshSize != null)
                            {
                                if (embeddedGeomMeshSize.ContainsKey(embeddedGeometriesInput[shapesInput[s]][i]))
                                {
                                    geometryAndMeshSize.Add(scaledPoint,
                                        embeddedGeomMeshSize[embeddedGeometriesInput[shapesInput[s]][i]] * options.GeometryBaseScaleFactor);
                                }
                            }
                        }

                        else if (embeddedGeometriesInput[shapesInput[s]][i] is Point2d point2d)
                        {
                            Point2d scaledPoint = point2d.Scale(options.GeometryBaseScaleFactor);
                            geometryBases.Add(scaledPoint);
                            if (embeddedGeomMeshSize != null)
                            {
                                if (embeddedGeomMeshSize.ContainsKey(embeddedGeometriesInput[shapesInput[s]][i]))
                                {
                                    geometryAndMeshSize.Add(scaledPoint,
                                        embeddedGeomMeshSize[embeddedGeometriesInput[shapesInput[s]][i]] * options.GeometryBaseScaleFactor);
                                }
                            }
                        }

                        else if (embeddedGeometriesInput[shapesInput[s]][i] is Line3d line3d)
                        {
                            Line3d scaledLine = line3d.Scale(options.GeometryBaseScaleFactor);
                            geometryBases.Add(scaledLine);
                            if (embeddedGeomMeshSize != null)
                            {
                                if (embeddedGeomMeshSize.ContainsKey(embeddedGeometriesInput[shapesInput[s]][i]))
                                {
                                    geometryAndMeshSize.Add(scaledLine,
                                        embeddedGeomMeshSize[embeddedGeometriesInput[shapesInput[s]][i]] * options.GeometryBaseScaleFactor);
                                }
                            }
                        }

                        else if (embeddedGeometriesInput[shapesInput[s]][i] is Line2d line2d)
                        {
                            Line2d scaledLine = line2d.Scale(options.GeometryBaseScaleFactor);
                            geometryBases.Add(scaledLine);
                            if (embeddedGeomMeshSize != null)
                            {
                                if (embeddedGeomMeshSize.ContainsKey(embeddedGeometriesInput[shapesInput[s]][i]))
                                {
                                    geometryAndMeshSize.Add(scaledLine,
                                        embeddedGeomMeshSize[embeddedGeometriesInput[shapesInput[s]][i]] * options.GeometryBaseScaleFactor);
                                }
                            }
                        }

                        else if (embeddedGeometriesInput[shapesInput[s]][i] is Polygon3d polygon3d)
                        {
                            Polygon3d scaledPolygon = polygon3d.Scale(options.GeometryBaseScaleFactor);
                            geometryBases.Add(scaledPolygon);
                            if (embeddedGeomMeshSize != null)
                            {
                                if (embeddedGeomMeshSize.ContainsKey(embeddedGeometriesInput[shapesInput[s]][i]))
                                {
                                    geometryAndMeshSize.Add(scaledPolygon,
                                        embeddedGeomMeshSize[embeddedGeometriesInput[shapesInput[s]][i]] * options.GeometryBaseScaleFactor);
                                }
                            }
                        }

                        else if (embeddedGeometriesInput[shapesInput[s]][i] is Polygon2d polygon2d)
                        {
                            Polygon2d scaledPolygon = polygon2d.Scale(options.GeometryBaseScaleFactor);
                            geometryBases.Add(scaledPolygon);
                            if (embeddedGeomMeshSize != null)
                            {
                                if (embeddedGeomMeshSize.ContainsKey(embeddedGeometriesInput[shapesInput[s]][i]))
                                {
                                    geometryAndMeshSize.Add(scaledPolygon,
                                        embeddedGeomMeshSize[embeddedGeometriesInput[shapesInput[s]][i]] * options.GeometryBaseScaleFactor);
                                }
                            }
                        }

                        else if (embeddedGeometriesInput[shapesInput[s]][i] is Shape shapeEmb)
                        {
                            Shape scaledShape = shapeEmb.Scale(options.GeometryBaseScaleFactor);
                            geometryBases.Add(scaledShape);
                            if (embeddedGeomMeshSize != null)
                            {
                                if (embeddedGeomMeshSize.ContainsKey(embeddedGeometriesInput[shapesInput[s]][i]))
                                {
                                    geometryAndMeshSize.Add(scaledShape,
                                        embeddedGeomMeshSize[embeddedGeometriesInput[shapesInput[s]][i]] * options.GeometryBaseScaleFactor);
                                }
                            }
                        }

                        else
                            throw new ArgumentException($"Geom {embeddedGeometriesInput[shapesInput[s]][i].GetType()} not supported");
                    }
                    embeddedGeometries.Add(shape, geometryBases.ToArray());
                    embGeomAssociation.Add(shape, geometryAndMeshSize);

                    for (int g = 0; g < geometryBases.Count; g++)
                        listOfAllEmbGeom.Add(geometryBases[g]);
                }

                listOfAllEmbGeomForEmbShape.UnionWith(listOfAllEmbGeom); // before, ElementAt(i) on the HashSet: O(n^2)
                for (int i = 0; i < shapesInput[s].Fill.Count; i++)
                    listOfAllEmbGeomForEmbShape.Add(shapesInput[s].Fill[i]);
                if (shapesInput[s].HasHoles)
                    for (int i = 0; i < shapesInput[s].Holes.Count(); i++)
                        for (int p = 0; p < shapesInput[s].Holes[i].Count; p++)
                            listOfAllEmbGeomForEmbShape.Add(shapesInput[s].Holes[i][p]);
            }

            Shape[] shapes = shapesList.ToArray();

            // the embedded geometries in an array (before, ElementAt(g) on the HashSet in the loops: O(n^2))
            GeometryBase[] allEmbGeom = listOfAllEmbGeom.ToArray();

            #endregion

            TimeSpan dt2 = clock.Elapsed;

            try
            {
                #region CREAZIONE CAD 

                bool embShapeCut = false;
                int physicalGroupTag = 0;
                try
                {
                    for (int c = 0; c < shapes.Count(); c++)
                    {
                        TimeSpan dt2_1 = clock.Elapsed;

                        HashSet<GeometryBase> hashSet = new HashSet<GeometryBase>(GeometryKeyComparer.Instance);


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
                                                // identity by reference (before, by hash code: the hash of the shape changes when a point is inserted)
                                                if (!ReferenceEquals(g, geom))
                                                {
                                                    for (int j = 0; j < shapeToCut.Fill.Count; j++) // Count = number of sides (before, Explode() at every iteration)
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
                                                                hashSet.Add(line);
                                                                perim = shapeToCut.Fill.Explode();
                                                            }
                                                            if (vertexAdded == 2)
                                                            {
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
                                                                    hashSet.Add(line);
                                                                    perim = shapeToCut.Fill.Explode();
                                                                }
                                                                if (vertexAdded == 2)
                                                                {
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
                                                                        hashSet.Add(line);
                                                                        perim = shapeToCut.Fill.Explode();
                                                                    }
                                                                    if (vertexAdded == 2)
                                                                    {
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
                                                                            shapeToCut.Fill.Insert(j + 1, p);
                                                                            hashSet.Add(p);
                                                                            perim = shapeToCut.Fill.Explode();
                                                                        }
                                                                    }
                                                                }

                                                                if (s.HasHoles)
                                                                {
                                                                    // the vertices of the holes of s on the side j are added to the embedded shape, as the ones of the fill
                                                                    // (before, they were inserted in the hole of s at the index j of the other shape, and perim was
                                                                    // rebuilt from the host shape)
                                                                    for (int k = 0; k < s.Holes.Count(); k++)
                                                                    {
                                                                        Line3d[] arrayH = s.Holes[k].Explode();
                                                                        for (int i1 = 0; i1 < arrayH.Length; i1++)
                                                                        {
                                                                            Line3d line = arrayH[i1];
                                                                            GenerateMeshGeometryPreProcessingAddLineToShape(shapeToCut.Fill, perim[j], line, j, toleranceIntersection, out int vertexAdded);
                                                                            if (vertexAdded > 0)
                                                                            {
                                                                                hashSet.Add(line);
                                                                                perim = shapeToCut.Fill.Explode();
                                                                            }
                                                                            if (vertexAdded == 2)
                                                                            {
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
                                                                                    shapeToCut.Fill.Insert(j + 1, p);
                                                                                    hashSet.Add(p);
                                                                                    perim = shapeToCut.Fill.Explode();
                                                                                }
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

                                for (int g = 0; g < allEmbGeom.Length; g++)
                                {
                                    Line3d[] perimeter = shapes[c].Fill.Explode();

                                    if (!hashSet.Contains(allEmbGeom[g]))
                                    {
                                        for (int i = 0; i < perimeter.Length; i++)
                                        {
                                            if (allEmbGeom[g] is Point3d pt3d || allEmbGeom[g] is Point2d pt2d)
                                            {
                                                Point3d point;
                                                if (allEmbGeom[g] is Point3d p3d)
                                                    point = p3d;
                                                else
                                                    point = new Point3d((Point2d)allEmbGeom[g]);

                                                // Se il punto è sul lato è [i] e non è un vertica, allora aggiungilo
                                                if (perimeter[i].IsPointOnLine(point, toleranceIntersection) && !shapes[c].Fill.Contains(point))
                                                {
                                                    shapes[c].Fill.Insert(i + 1, point);
                                                    hashSet.Add(point);
                                                    break;
                                                }
                                            }
                                            else if (allEmbGeom[g] is Line3d line3d || allEmbGeom[g] is Line2d line2d)
                                            {
                                                Line3d line;
                                                if (allEmbGeom[g] is Line3d l)
                                                    line = l;
                                                else
                                                    line = new Line3d((Line2d)allEmbGeom[g]);

                                                GenerateMeshGeometryPreProcessingAddLineToShape(shapes[c].Fill, perimeter[i], line, i, toleranceIntersection, out int vertexAdded);
                                                if (vertexAdded > 0)
                                                {
                                                    hashSet.Add(line);
                                                    perimeter = shapes[c].Fill.Explode();
                                                }
                                                if (vertexAdded == 2)
                                                {
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
                                                        shapes[c].Fill.Insert(i + 1, p);
                                                        hashSet.Add(p);
                                                        perimeter = shapes[c].Fill.Explode();
                                                    }
                                                }
                                            }
                                            else if (allEmbGeom[g] is Polygon3d p3d || allEmbGeom[g] is Polygon2d p2d)
                                            {
                                                Polygon3d polygon3d;
                                                if (allEmbGeom[g] is Polygon3d polyg)
                                                    polygon3d = polyg;
                                                else
                                                    polygon3d = new Polygon3d((Polygon2d)allEmbGeom[g]);

                                                Line3d[] lines = polygon3d.Explode().ToArray();
                                                for (int l = 0; l < lines.Count(); l++)
                                                {
                                                    GenerateMeshGeometryPreProcessingAddLineToShape(shapes[c].Fill, perimeter[i], lines[l], i, toleranceIntersection, out int vertexAdded);
                                                    if (vertexAdded > 0)
                                                    {
                                                        hashSet.Add(lines[l]);
                                                        perimeter = shapes[c].Fill.Explode();
                                                    }
                                                    if (vertexAdded == 2)
                                                    {
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
                                                            shapes[c].Fill.Insert(i + 1, p);
                                                            hashSet.Add(p);
                                                            perimeter = shapes[c].Fill.Explode();
                                                        }
                                                    }
                                                }
                                            }
                                            else if (allEmbGeom[g] is Shape s)
                                            {
                                                Line3d[] shapeArray = s.Fill.Explode();
                                                for (int i1 = 0; i1 < shapeArray.Length; i1++)
                                                {
                                                    Line3d line = shapeArray[i1];
                                                    GenerateMeshGeometryPreProcessingAddLineToShape(shapes[c].Fill, perimeter[i], line, i, toleranceIntersection, out int vertexAdded);
                                                    if (vertexAdded > 0)
                                                    {
                                                        hashSet.Add(line);
                                                        perimeter = shapes[c].Fill.Explode();
                                                    }
                                                    if (vertexAdded == 2)
                                                    {
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
                                                            shapes[c].Fill.Insert(i + 1, p);
                                                            hashSet.Add(p);
                                                            perimeter = shapes[c].Fill.Explode();
                                                        }
                                                    }
                                                }

                                                if (s.HasHoles)
                                                {
                                                    // the vertices of the holes of s on the side i are added to the shape, as the ones of the fill
                                                    // (before, they were inserted in the hole of s at the index i of the shape)
                                                    for (int j = 0; j < s.Holes.Count(); j++)
                                                    {
                                                        Line3d[] holeArray = s.Holes[j].Explode();
                                                        for (int i1 = 0; i1 < holeArray.Length; i1++)
                                                        {
                                                            Line3d line = holeArray[i1];
                                                            GenerateMeshGeometryPreProcessingAddLineToShape(shapes[c].Fill, perimeter[i], line, i, toleranceIntersection, out int vertexAdded);
                                                            if (vertexAdded > 0)
                                                            {
                                                                hashSet.Add(line);
                                                                perimeter = shapes[c].Fill.Explode();
                                                            }
                                                            if (vertexAdded == 2)
                                                            {
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
                                                                    shapes[c].Fill.Insert(i + 1, p);
                                                                    hashSet.Add(p);
                                                                    perimeter = shapes[c].Fill.Explode();
                                                                }
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

                                        if (allEmbGeom[g] is Shape sh)
                                        {
                                            if (shapes[c].IsPolygonInside(sh.Fill))
                                            {
                                                holesToAdd.Add(sh.Fill);
                                                addHole = true;
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
                                                if (allEmbGeom[g] is Point3d p3d || allEmbGeom[g] is Point2d p2d)
                                                {
                                                    Point3d point;
                                                    if (allEmbGeom[g] is Point3d p3)
                                                        point = p3;
                                                    else
                                                        point = new Point3d((Point2d)allEmbGeom[g]);

                                                    // Se il punto è sul lato è [i] e non è un vertica, allora aggiungilo
                                                    if (perimeterHole[j].IsPointOnLine(point, toleranceIntersection))
                                                    {
                                                        shapes[c].Holes[k].Insert(j + 1, point);
                                                        hashSet.Add(point);
                                                        break;
                                                    }
                                                }
                                                else if (allEmbGeom[g] is Line3d line3d || allEmbGeom[g] is Line2d line2d)
                                                {
                                                    Line3d line;
                                                    if (allEmbGeom[g] is Line3d l)
                                                        line = l;
                                                    else
                                                        line = new Line3d((Line2d)allEmbGeom[g]);

                                                    GenerateMeshGeometryPreProcessingAddLineToShape(shapes[c].Holes[k], perimeterHole[j], line, j, toleranceIntersection, out int vertexAdded);
                                                    if (vertexAdded > 0)
                                                    {
                                                        hashSet.Add(line);
                                                        perimeterHole = shapes[c].Holes[k].Explode();
                                                    }
                                                    if (vertexAdded == 2)
                                                    {
                                                        hashSet.Add(line);
                                                        break; // ho aggiunto tutti e due i vertici della linea
                                                    }
                                                    if (line.GetIntersection(perimeterHole[j], out Point3d p, toleranceIntersection))
                                                    {
                                                        if (Math.Abs(p.DistanceTo(perimeterHole[j].Start)) > toleranceIntersection && Math.Abs(p.DistanceTo(perimeterHole[j].End)) > toleranceIntersection)
                                                        {
                                                            shapes[c].Holes[k].Insert(j + 1, p);
                                                            hashSet.Add(p);
                                                            perimeterHole = shapes[c].Holes[k].Explode();
                                                        }
                                                    }
                                                }
                                                else if (allEmbGeom[g] is Polygon3d pg3d || allEmbGeom[g] is Polygon2d pg2d)
                                                {
                                                    Polygon3d polygon3d;
                                                    if (allEmbGeom[g] is Polygon3d polyg)
                                                        polygon3d = polyg;
                                                    else
                                                        polygon3d = new Polygon3d((Polygon2d)allEmbGeom[g]);

                                                    Line3d[] lines = polygon3d.Explode().ToArray();
                                                    for (int l = 0; l < lines.Count(); l++)
                                                    {
                                                        GenerateMeshGeometryPreProcessingAddLineToShape(shapes[c].Holes[k], perimeterHole[j], lines[l], j, toleranceIntersection, out int vertexAdded);
                                                        if (vertexAdded > 0)
                                                        {
                                                            hashSet.Add(lines[l]);
                                                            perimeterHole = shapes[c].Holes[k].Explode();
                                                        }
                                                        if (vertexAdded == 2)
                                                        {
                                                            hashSet.Add(lines[l]);
                                                            break; // ho aggiunto tutti e due i vertici della linea
                                                        }
                                                        if (lines[l].GetIntersection(perimeterHole[j], out Point3d p, toleranceIntersection))
                                                        {
                                                            if (Math.Abs(p.DistanceTo(perimeterHole[j].Start)) > toleranceIntersection && Math.Abs(p.DistanceTo(perimeterHole[j].End)) > toleranceIntersection)
                                                            {
                                                                shapes[c].Holes[k].Insert(j + 1, p);
                                                                hashSet.Add(p);
                                                                perimeterHole = shapes[c].Holes[k].Explode();
                                                            }
                                                        }
                                                    }
                                                }
                                                else if (allEmbGeom[g] is Shape s)
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

                                // The dictionaries use the reference of the shape as key: the shape can be modified here without updating them
                                // (before, the modified shape was added again and the old key was not removed; a shape changed only by
                                // RemoveDuplicatedPoints was no more found and its embedded geometries were ignored)
                            }

                            #endregion
                        }
                        catch (Exception e)
                        {
                            generateMeshStatus.AddException(e, "Failed modify the shape in order to add a vertex in correspondance of a embedded geometry vertex");
                            return false;
                        }

                        #endregion

                        TimeSpan dt2_2 = clock.Elapsed;
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
                            generateMeshStatus.AddException(ge, $"{GMeshGenerateMeshStatus.FailedToCreateTheShape}: {c} in CAD.");
                            return false;
                        }


                        #endregion

                        TimeSpan dt3 = clock.Elapsed;
                        geomCADTime += (dt3 - dt2_2).TotalSeconds;

                    }

                    Gmsh.Model.Occ.Synchronize();

                    TimeSpan dt3_3 = clock.Elapsed;

                    if (embShapeCut)
                        occw.RebuildObjectTagsAndCheck(toleranceIntersection);
                    else
                        occw.RebuildObjectTags();

                    TimeSpan dt3_4 = clock.Elapsed;
                    rebuildObjectTagsTime += (dt3_4 - dt3_3).TotalSeconds;
                }
                catch (Exception e)
                {
                    generateMeshStatus.AddException(e, "Failed to add the shapes into CAD");
                    return false;
                }

                #endregion

                TimeSpan dt3_2 = clock.Elapsed;

                #region FRAGMENT

                // Spezza superfici e garantisce la congruenza fra superfici adiacenti
                // Da ad ogni shape di partenza un physical group, poi le spezza mantenendo nelle proprietà il PG corretto ma creando più superfici
                // Gestisce n shape con n tagli.
                try
                {
                    if (physicalGroupTagSurfacesAssociation.Count > 1)
                    {
                        // creo un array (int,int) con tutte le superfici di ogni PhG (una shape con fori può essere divisa in più superfici dal Cut).
                        // poi faccio il fragment tra questo array e se stesso, in modo che trovi tutte le intersezioni e generi le geometrie di cui ha bisogno.
                        // (before, only the first surface of each shape was fragmented: the other ones lost the congruence and the physical group)
                        var surfaceOwner = new List<int>();
                        var dimSurfaceTagList = new List<(int, int)>();
                        for (int i = 0; i < physicalGroupTagSurfacesAssociation.Count; i++)
                        {
                            foreach (int surfaceTag in physicalGroupTagSurfacesAssociation[i].surfacesTag)
                            {
                                dimSurfaceTagList.Add((physicalGroupTagSurfacesAssociation[i].dim, surfaceTag));
                                surfaceOwner.Add(i);
                            }
                        }
                        (int, int)[] dimSurfaceTagBuffer = dimSurfaceTagList.ToArray();
                        try
                        {
                            Gmsh.Model.Occ.Fragment(dimSurfaceTagBuffer, dimSurfaceTagBuffer, out (int, int)[] result, out (int, int)[][] resultMap, -1, true, true);
                            Gmsh.Model.Occ.Synchronize();

                            for (int i = 0; i < physicalGroupTagSurfacesAssociation.Count; i++)
                            {
                                int physicalTag = physicalGroupTagSurfacesAssociation[i].physicalGroupTag;
                                int dim = physicalGroupTagSurfacesAssociation[i].dim;

                                // resultMap: the new entities of every input object (the objects come first)
                                var newSurfaceTags = new List<int>();
                                for (int k = 0; k < surfaceOwner.Count; k++)
                                {
                                    if (surfaceOwner[k] != i)
                                        continue;
                                    foreach ((int, int) dimTag in resultMap[k] ?? new (int, int)[0])
                                    {
                                        if (dimTag.Item2 != 0 && !newSurfaceTags.Contains(dimTag.Item2))
                                            newSurfaceTags.Add(dimTag.Item2);
                                    }
                                }

                                int newPhysicalTag = Gmsh.Model.AddPhysicalGroup(dim, newSurfaceTags.ToArray(), physicalTag);

                                physicalGroupTagSurfacesAssociation[i] = (newPhysicalTag, dim, newSurfaceTags.ToArray(), physicalGroupTagSurfacesAssociation[i].shape);
                            }

                            generateMeshStatus.GeneratedSurfaces = physicalGroupTagSurfacesAssociation.Count; // it was the index of the last one
                        }
                        catch (GmshException e)
                        {
                            generateMeshStatus.AddException(e, "Failed to find shapes intersections");
                            return false;
                        }
                        catch (Exception e)
                        {
                            generateMeshStatus.AddException(e, "Failed to find shapes intersections");
                            return false;
                        }

                        TimeSpan dt3_3 = clock.Elapsed;

                        occw.RebuildObjectTags();

                        TimeSpan dt3_4 = clock.Elapsed;
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
                    return false;
                }

                #endregion

                TimeSpan dt4 = clock.Elapsed;

                #region TRANSFINITE BOUNDARY

                try
                {
                    if (options.Transfinite && !options.TransfiniteSurface)
                    {
                        // the association k is the one of the shape k (before, searched with == at every iteration: equal shapes were
                        // confused); a curve shared by two surfaces is set once
                        var transfiniteCurves = new HashSet<int>();
                        for (int k = 0; k < shapes.Length; k++)
                        {
                            int[] surfacesTag = physicalGroupTagSurfacesAssociation[k].surfacesTag;

                            for (int count = 0; count < surfacesTag.Length; count++)
                            {
                                int surfaceTag = surfacesTag[count];
                                var boundaryCurvesTag = Gmsh.Model.GetBoundary(new (int, int)[1] { (2, surfaceTag) }, false, false, false).Select(i => i.Item2).ToList();

                                for (int j = 0; j < boundaryCurvesTag.Count; j++)
                                {
                                    if (!transfiniteCurves.Add(Math.Abs(boundaryCurvesTag[j])))
                                        continue;

                                    try
                                    {
                                        TransfiniteLine(boundaryCurvesTag[j], options.MeshSize, options.TransfiniteLineType.ToString(), options.TransfiniteFactor);
                                    }
                                    catch (GmshException ge)
                                    {
                                        generateMeshStatus.AddException(ge, "Boundary transfinite failed");
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
                    return false;
                }

                #endregion

                TimeSpan dt5 = clock.Elapsed;

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
                Dictionary<Shape, Shape[]> shapeToShapeSplitted = new Dictionary<Shape, Shape[]>(ReferenceComparer<Shape>.Instance);
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
                                                for (int k = 0; k < listOfShapeDiffAndIntersect.Count; k++)
                                                {
                                                    Polygon3d fill = listOfShapeDiffAndIntersect[k].Fill;
                                                    for (int p = 0; p < fill.Count; p++)
                                                    {
                                                        // the side p (before, the whole polygon was exploded for every point and every side)
                                                        Line3d side = new Line3d(fill[p], fill[(p + 1) % fill.Count]);

                                                        for (int j = 0; j < shapePoints.Count; j++)
                                                        {
                                                            // Se il punto è sul lato è [i] e non è un vertica, allora aggiungilo
                                                            if (side.IsPointOnLine(shapePoints[j], toleranceIntersection))
                                                            {
                                                                double dist1 = shapePoints[j].DistanceTo(side.Start);
                                                                double dist2 = shapePoints[j].DistanceTo(side.End);
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
                    return false;
                }

                #endregion

                #region EMBED SHAPE

                try
                {
                    if (embeddedGeometries != null)
                    {
                        for (int si = 0; si < shapes.Length; si++)
                        {
                            Shape shape = shapes[si];
                            if (embeddedGeometries.ContainsKey(shape))
                            {
                                int physicalTag = physicalGroupTagSurfacesAssociation[si].physicalGroupTag;
                                embeddedGeometriesTagAssociation[physicalTag] = new Dictionary<GeometryBase, int[]>(GeometryKeyComparer.Instance);

                                // aggiungo a questa lista una shape ogni volta che la embeddo
                                List<Shape> listEmbShape = new List<Shape>();
                                // dizionario di associazione Shape e tag delle superfici (più di una se i fori la dividono)
                                Dictionary<Shape, int[]> shapeSurfaceTagAssociation = new Dictionary<Shape, int[]>(ReferenceComparer<Shape>.Instance);

                                for (int j = 0; j < embeddedGeometries[shape].Length; j++)
                                {
                                    GeometryBase geometry = embeddedGeometries[shape][j];
                                    List<int> surfTagBuffer = new List<int>();
                                    if (geometry is Shape sh)
                                    {
                                        for (int k = 0; k < shapeToShapeSplitted[sh].Length; k++)
                                        {
                                            Shape embS = shapeToShapeSplitted[sh][k];

                                            // before, this was repeated for every surface of the host shape (the same work, and the tag of an
                                            // existing shape was added once per surface)
                                            {
                                                bool match = false;

                                                // controllo che non sia già stata aggiunta.
                                                // se è stata aggiunta, mi prendo il tag, altrimenti vado avanti
                                                for (int i = 0; i < listEmbShape.Count; i++)
                                                {
                                                    Shape s = listEmbShape[i];
                                                    if (embS.EqualsShifted(s))
                                                    {
                                                        surfTagBuffer.AddRange(shapeSurfaceTagAssociation[s]);
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
                                                                    // lines[i]: before lines[tag], the Gmsh tag used as index (another exception in the catch)
                                                                    generateMeshStatus.AddException(e, $"{GMeshGenerateMeshStatus.FailedToTransfinite} for curveTag:{tag}. Line Start:{lines[i].Start * options.MeshScalingFactor} Line End{lines[i].End * options.MeshScalingFactor}");
                                                                    return false;
                                                                }
                                                            }
                                                        }

                                                        // (the surface where the first vertex is was computed here and never used: removed)

                                                        // the surfaces of the embedded shape: the ones given by the Cut of the holes (before, the tag of the surface
                                                        // before the Cut was used also after it, when the Cut had removed it)
                                                        int[] embSurfaceTags = new int[] { EmbSurfaceTag };

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
                                                                // every hole cuts all the surfaces left by the previous holes (before, only the first one)
                                                                Gmsh.Model.Occ.Cut(outCuttedSurfaceTags, new (int, int)[] { (2, holeSurfaceTag) },
                                                                                    out outCuttedSurfaceTags, out (int, int)[][] resultMap, -1, true, true);
                                                            }

                                                            Gmsh.Model.Occ.Synchronize();
                                                            embSurfaceTags = (outCuttedSurfaceTags ?? new (int, int)[0]).Where(dimTag => dimTag.Item1 == 2).Select(dimTag => dimTag.Item2).ToArray();
                                                        }

                                                        surfTagBuffer.AddRange(embSurfaceTags);

                                                        generateMeshStatus.EmbeddedShapes += 1;
                                                        listEmbShape.Add(embS);

                                                        int phgTag = Gmsh.Model.AddPhysicalGroup(2, embSurfaceTags);

                                                        // aggiungo il tag ai relativi dizionari
                                                        shapeSurfaceTagAssociation.Add(embS, embSurfaceTags);
                                                        foreach (int embSurfaceTag in embSurfaceTags)
                                                            physicalGroupMeshTagAssociation.Add((physicalTag, phgTag, embSurfaceTag));
                                                    }
                                                    catch (Exception e)
                                                    {
                                                        generateMeshStatus.AddException(e, $"{GMeshGenerateMeshStatus.FailedToEmbedTheShape} {geometry}");
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
                        // the boundaries of the surfaces, read from Gmsh once (before, for every part of every embedded line)
                        var boundaries = new SurfaceBoundaries();

                        for (int t = 0; t < shapes.Count(); t++)
                        {
                            // Creo una lista splitLine di tutte le linee (linee2d/3d polygon2d/3d) che andrò ad embeddare
                            // ne cerco le intersezioni e le inserisco nella lista splitPoints

                            // lista di tutte le linee emb
                            HashSet<Line3d> splitLine = new HashSet<Line3d>();
                            // associo le linee spezzate alla geometria originale
                            Dictionary<Line3d, Line3d[]> splitLines = new Dictionary<Line3d, Line3d[]>(GeometryKeyComparer.Instance);
                            // lista dei punti di intersezione
                            HashSet<Point3d> splitPoints = new HashSet<Point3d>();
                            // hashset per controllare di non controllare la stessa geometria 2 volte
                            HashSet<GeometryBase> hashSet = new HashSet<GeometryBase>(GeometryKeyComparer.Instance);

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
                                        Line3d[] lineSplit = splitLineArray[i].Split(splitPointsArray, toleranceIntersection);        // splitto le linee in base ai punti di intersezione trovati (before, ToArray for every line)
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

                                int physicalTag = physicalGroupTagSurfacesAssociation[t].physicalGroupTag;

                                // the surfaces of the shape and of the shapes embedded in it (with their current tags: a fragment done for a
                                // previous shape can have changed them)
                                List<int> surfaceTagArray = new List<int>(boundaries.Current(physicalGroupTagSurfacesAssociation[t].surfacesTag));
                                foreach (var embeddedShapeSurface in physicalGroupMeshTagAssociation)
                                {
                                    if (embeddedShapeSurface.physicalGroupTag != physicalTag)
                                        continue;
                                    foreach (int surface in boundaries.Current(new[] { embeddedShapeSurface.surfaceTag }))
                                    {
                                        if (!surfaceTagArray.Contains(surface))
                                            surfaceTagArray.Add(surface);
                                    }
                                }

                                for (int s = 0; s < embeddedGeometries[shapes[t]].Count(); s++)
                                {
                                    if (!hashSet.Contains(embeddedGeometries[shapes[t]][s]))
                                    {
                                        // once (before, repeated for every surface of the host shape: the same work, and a polygon was embedded
                                        // again and its second mapping threw an exception)
                                        do
                                        {

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

                                                        // a point on a side becomes a vertex of the side, the other ones are embedded in the surface
                                                        // (before, a point on a side was embedded in one of the surfaces of the side)
                                                        p1 = MakeVertexOnSides(point, p1, surfaceTagArray, boundaries, occw, options, toleranceIntersection, out bool onSide);
                                                        if (!onSide)
                                                            boundaries.Embed(0, p1, surfaceWhereEmbedPoint);
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

                                                    if (splitLines.ContainsKey(l) && !embeddedGeometriesTagAssociation[physicalTag].ContainsKey(l))
                                                    {
                                                        // every part of the line split by the other embedded geometries (see EmbedSegment)
                                                        List<int> lineTagToEmbed = new List<int>();
                                                        foreach (Line3d line in splitLines[l])
                                                            lineTagToEmbed.AddRange(EmbedSegment(line, occw, surfaceTagArray, boundaries, options, toleranceMatch, toleranceIntersection, hashSet));

                                                        // the line is mapped if a part of it is in the surfaces (before, not mapped if a part was out of them)
                                                        if (lineTagToEmbed.Count > 0)
                                                        {
                                                            embeddedGeometriesTagAssociation[physicalTag].Add(embeddedGeometries[shapes[t]][s], lineTagToEmbed.ToArray());
                                                            generateMeshStatus.EmbeddedLines += 1;
                                                        }
                                                    }
                                                }
                                                catch (Exception e)
                                                {
                                                    generateMeshStatus.AddException(e, $"{GMeshGenerateMeshStatus.FailedToEmbedTheLine} {embeddedGeometries[shapes[t]][s]}");
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

                                                    // the sides are embedded as the lines (before, the same code was duplicated with a different treatment of
                                                    // the parts out of the surfaces)
                                                    List<int> embeddedPolygonTagAssociation = new List<int>();
                                                    foreach (Line3d side in polygon.Explode())
                                                    {
                                                        if (!splitLines.TryGetValue(side, out Line3d[] parts))
                                                            continue;
                                                        foreach (Line3d line in parts)
                                                            embeddedPolygonTagAssociation.AddRange(EmbedSegment(line, occw, surfaceTagArray, boundaries, options, toleranceMatch, toleranceIntersection, hashSet));
                                                    }

                                                    // mapped with the input geometry (before, with the Polygon3d made from a Polygon2d)
                                                    if (embeddedPolygonTagAssociation.Count > 0)
                                                    {
                                                        embeddedGeometriesTagAssociation[physicalTag].Add(embeddedGeometries[shapes[t]][s], embeddedPolygonTagAssociation.ToArray());
                                                        generateMeshStatus.EmbeddedPolygons += 1;
                                                    }
                                                    hashSet.Add(polygon);
                                                }
                                                catch (Exception e)
                                                {
                                                    generateMeshStatus.AddException(e, $"{GMeshGenerateMeshStatus.FailedToEmbedThePolygon} {embeddedGeometries[shapes[t]][s]}");
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
                                        while (false);
                                    }
                                }
                            }
                        }

                        if (boundaries.Fragmented)
                        {
                            // The surfaces fragmented to put the embedded points on their sides lost their physical groups: the groups are
                            // defined again, with the new tags of the surfaces (usually the same)
                            int[] NewTags(IEnumerable<int> tags) => boundaries.Current(tags);

                            Gmsh.Model.RemovePhysicalGroups(new (int, int)[0]); // all the groups
                            for (int i = 0; i < physicalGroupTagSurfacesAssociation.Count; i++)
                            {
                                var association = physicalGroupTagSurfacesAssociation[i];
                                int[] surfaces = NewTags(association.surfacesTag);
                                Gmsh.Model.AddPhysicalGroup(association.dim, surfaces, association.physicalGroupTag);
                                physicalGroupTagSurfacesAssociation[i] = (association.physicalGroupTag, association.dim, surfaces, association.shape);
                            }

                            var embeddedShapeSurfaces = new List<(int physicalGroupTag, int embPhysicalGroupTag, int surfaceTag)>();
                            foreach (var embeddedShape in physicalGroupMeshTagAssociation)
                                foreach (int surface in NewTags(new[] { embeddedShape.surfaceTag }))
                                    embeddedShapeSurfaces.Add((embeddedShape.physicalGroupTag, embeddedShape.embPhysicalGroupTag, surface));
                            physicalGroupMeshTagAssociation.Clear();
                            physicalGroupMeshTagAssociation.AddRange(embeddedShapeSurfaces);
                            foreach (var group in embeddedShapeSurfaces.GroupBy(i => i.embPhysicalGroupTag))
                                Gmsh.Model.AddPhysicalGroup(2, group.Select(i => i.surfaceTag).ToArray(), group.Key);

                            // the surfaces of the embedded shapes, used to map their faces
                            foreach (Dictionary<GeometryBase, int[]> tagsOfGeometries in embeddedGeometriesTagAssociation.Values)
                            {
                                foreach (GeometryBase geometry in tagsOfGeometries.Keys.ToArray())
                                {
                                    if (geometry is Shape)
                                        tagsOfGeometries[geometry] = NewTags(tagsOfGeometries[geometry]);
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    generateMeshStatus.AddException(e, $"Failed to embed the geometries");
                    return false;
                }

                #endregion

                #endregion 

                TimeSpan dt6 = clock.Elapsed;

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
                    return false;
                }

                // TRANFINITE LINE / POLYGON / POINT MESH REFINEMENT SIZE 
                try
                {
                    if (options.Transfinite && embeddedGeomMeshSize != null)
                    {
                        for (int s = 0; s < shapes.Length; s++)
                        {
                            int physicalTag = physicalGroupTagSurfacesAssociation[s].physicalGroupTag;
                            if (!embGeomAssociation.TryGetValue(shapes[s], out Dictionary<GeometryBase, double> sizes) ||
                                !embeddedGeometriesTagAssociation.TryGetValue(physicalTag, out Dictionary<GeometryBase, int[]> tagsOfGeometries))
                                continue;

                            foreach (KeyValuePair<GeometryBase, double> size in sizes)
                            {
                                GeometryBase geometry = size.Key;
                                bool isPoint = geometry is Point3d || geometry is Point2d;
                                bool isLine = geometry is Line3d || geometry is Line2d;
                                bool isPolygon = geometry is Polygon3d || geometry is Polygon2d;

                                if (!isPoint && !isLine && !isPolygon)
                                {
                                    if (geometry is Shape)
                                        continue;
                                    throw new ArgumentException($"Geom {geometry.GetType()} not supported");
                                }

                                // a geometry that has not been embedded (e.g. out of the shape) is skipped (before, KeyNotFoundException:
                                // the sizes of all the following geometries were not set)
                                if (!tagsOfGeometries.TryGetValue(geometry, out int[] tags))
                                    continue;

                                try
                                {
                                    for (int j = 0; j < tags.Length; j++)
                                    {
                                        if (isPoint)
                                            Gmsh.Model.Mesh.SetSize(new (int, int)[1] { (0, tags[j]) }, size.Value);
                                        else
                                            TransfiniteLine(tags[j], size.Value, options.TransfiniteLineType.ToString(), options.TransfiniteFactor);
                                    }
                                }
                                catch (GmshException)
                                {
                                    string warning = isPoint ? "Warning: fail to set the size in the embed point" :
                                        isLine ? "Warning: fail to set the size in the embed line" : "Warning: fail to set the size in the embed polygon";
                                    generateMeshStatus.AddWarning(warning);
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
                                int[] surfacesTag = physicalGroupTagSurfacesAssociation[s].surfacesTag;
                                for (int j = 0; j < surfacesTag.Length; j++)
                                {
                                    int surfaceTag = surfacesTag[j];
                                    // the curves of the boundary, in order (before, recursive = true: the tags of the points were used as tags of curves)
                                    var boundaryCurveTag = Gmsh.Model.GetBoundary(new (int, int)[1] { (2, surfaceTag) }, true, false, false).Select(i => Math.Abs(i.Item2)).ToList();

                                    if (boundaryCurveTag.Count == 4)
                                    {
                                        try
                                        {
                                            // the opposite curves (0-2 and 1-3) get the same number of nodes
                                            for (int i = 0; i < 2; i++)
                                            {
                                                int node = Math.Min(CurveNodes(boundaryCurveTag[i], options.MeshSize), CurveNodes(boundaryCurveTag[i + 2], options.MeshSize));

                                                Gmsh.Model.Mesh.SetTransfiniteCurve(boundaryCurveTag[i], node, options.TransfiniteLineType.ToString(), options.TransfiniteFactor);
                                                Gmsh.Model.Mesh.SetTransfiniteCurve(boundaryCurveTag[i + 2], node, options.TransfiniteLineType.ToString(), options.TransfiniteFactor);
                                            }
                                        }
                                        catch (GmshException ge)
                                        {
                                            generateMeshStatus.AddException(ge, "Boundary transfinite failed");
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

                    return false;
                }
                catch (Exception ex)
                {
                    // before, the error was recorded but the generation went on without a mesh
                    generateMeshStatus.AddException(ex, "Failed to generate the mesh");
                    return false;
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
                catch (GmshException e) when (RetryRecombineWithSimple(options, e, generateMeshStatus))
                {
                    // the recombination has been done with the simple algorithm (warning in the status)
                }
                catch (GmshException e)
                {
                    generateMeshStatus.AddException(e, "Failed to recombine the mesh, try to change the recombine algorithm");
                    return false;
                }
                catch (Exception e)
                {
                    generateMeshStatus.AddException(e, "Failed to recombine the mesh");
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

                TimeSpan dt7 = clock.Elapsed;

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
                // Gmsh.Net returns null for an empty result
                elementTypes = elementTypes ?? new int[0];

                int progressVertexId = 0;
                int progressPlateId = 0;
                int progressEdgeId = 0;

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

                    TimeSpan dtV1 = clock.Elapsed;

                    // tag del gruppo fisico
                    (int physicalGroupTag, int dim, int[] surfacesTag, Shape shape)[] pairs = physicalGroupTagSurfacesAssociation.Where(i => i.physicalGroupTag == phGTags[p]).ToArray();
                    for (int m = 0; m < pairs.Length; m++)
                    {
                        Gmsh.Model.Mesh.GetNodesForPhysicalGroup(pairs[m].dim, phGTags[p], out long[] nodeTags, out double[] nodeTagsCoord);
                        nodeTags = nodeTags ?? new long[0]; // null for an empty group

                        // One vertex for every node of Gmsh in the mesh. Before, a node shared by the shape and an embedded shape (their corners)
                        // got two vertices, merged later by Clean; the search by MeshVertex (whose equality includes the Id) never found a vertex,
                        // and the vertices "common to other meshes" were never used (pointIdAssociation.Concat discarded the result, and there was
                        // a break instead of a continue): the vertices are not shared between the meshes, as before
                        int VertexId(long nodeTag, double[] coordinates, int j)
                        {
                            if (vertexIdAssociation.TryGetValue((int)nodeTag, out int id))
                                return id;

                            Point3d point = new Point3d(coordinates[j * 3], coordinates[j * 3 + 1], coordinates[j * 3 + 2]).Scale(options.MeshScalingFactor);
                            MeshVertex mv = new MeshVertex(point);
                            id = options.UseGlobalProgressID ? meshBuffer._vertices.Add(mv, ++progressVertexId) : meshBuffer._vertices.Add(mv);

                            vertexIdAssociation[(int)nodeTag] = id;
                            nodeTagHashSet.Add((int)nodeTag);
                            return id;
                        }

                        try
                        {
                            #region VERTICI

                            for (int j = 0; j < nodeTags.Length; j++)
                                VertexId(nodeTags[j], nodeTagsCoord, j);

                            #endregion
                        }
                        catch (Exception e)
                        {
                            generateMeshStatus.AddException(e, "Failed to postprocess the mesh vertices");
                            return false;
                        }

                        // se ho una shape embeddata (one physical group can have more surfaces: read once)
                        foreach (int embPhysicalGroupTag in physicalGroupMeshTagAssociation.Where(i => i.physicalGroupTag == phGTags[p]).Select(i => i.embPhysicalGroupTag).Distinct())
                        {
                            Gmsh.Model.Mesh.GetNodesForPhysicalGroup(2, embPhysicalGroupTag, out nodeTags, out nodeTagsCoord);
                            nodeTags = nodeTags ?? new long[0];

                            try
                            {
                                #region VERTICI EVENTUALI SHAPES EMBEDDED

                                for (int j = 0; j < nodeTags.Length; j++)
                                    VertexId(nodeTags[j], nodeTagsCoord, j);

                                #endregion
                            }
                            catch (Exception e)
                            {
                                generateMeshStatus.AddException(e, "Failed to postprocess the mesh vertices of embedded shape");
                                return false;
                            }
                        }

                        TimeSpan dtV2 = clock.Elapsed;
                        nodeTime += (dtV2 - dtV1).TotalSeconds;

                        try
                        {
                            #region ELEMENTI - PUNTI, LINEE, PLATE

                            // an edge shared by two faces is added once (before, every face added all its edges)
                            var edgeKeys = new HashSet<long>();
                            void AddEdge(int a, int b)
                            {
                                long key = a < b ? ((long)a << 32) | (uint)b : ((long)b << 32) | (uint)a;
                                if (edgeKeys.Add(key))
                                    meshBuffer._edges.Add(new MeshEdge(a, b), ++progressEdgeId);
                            }

                            for (int count = 0; count < meshElementParameters.Count; count++)
                            {
                                GMeshElementParameters parameters = meshElementParameters[count];
                                if (parameters.ElementType != 2 && parameters.ElementType != 3)
                                    continue; // TRI 3 o QUAD4: points and lines are not faces

                                int elementTypeIndex = Array.IndexOf(elementTypes, parameters.ElementType);
                                if (elementTypeIndex == -1)
                                    continue;

                                int nodesNumber = parameters.PrimaryNodesNumber;
                                long[] typeNodeTags = elementNodeTags[elementTypeIndex];

                                for (int i = 0; i < elementTags[elementTypeIndex].Length; i++)
                                {
                                    // tag singolo elemento in gmsh: è del gruppo fisico in analisi se lo sono tutti i suoi nodi
                                    long elTag = elementTags[elementTypeIndex][i];

                                    bool isInGroup = true;
                                    for (int k = 0; k < nodesNumber; k++)
                                    {
                                        if (!nodeTagHashSet.Contains((int)typeNodeTags[nodesNumber * i + k]))
                                        {
                                            isInGroup = false;
                                            break;
                                        }
                                    }

                                    // not of this group, or already in another mesh (before, found by the exception of Dictionary.Add)
                                    if (!isInGroup || faceIdAssociation.ContainsKey((int)elTag))
                                        continue;

                                    int[] nodes = new int[nodesNumber];
                                    for (int k = 0; k < nodesNumber; k++)
                                        nodes[k] = vertexIdAssociation[(int)typeNodeTags[nodesNumber * i + k]];

                                    // the id of this face (before, the id of the previous face: the faces of the embedded shapes were shifted by one)
                                    int faceId = ++progressPlateId;
                                    faceIdAssociation.Add((int)elTag, faceId);

                                    // creo una faccia e i rispettivi bordi
                                    meshBuffer._faces.Add(nodesNumber == 3 ? new MeshFace(nodes[0], nodes[1], nodes[2]) : new MeshFace(nodes[0], nodes[1], nodes[2], nodes[3]), faceId);

                                    for (int k = 0; k < nodesNumber; k++)
                                        AddEdge(nodes[k], nodes[(k + 1) % nodesNumber]);
                                }
                            }

                            meshBuffer.Clean(tolerance, tolerance);

                            #endregion
                        }
                        catch (Exception e)
                        {
                            generateMeshStatus.AddException(e, "Failed to postprocess the mesh elements");
                            return false;
                        }

                        TimeSpan dtE2 = clock.Elapsed;
                        elementTime += (dtE2 - dtV2).TotalSeconds;

                        try
                        {
                            #region PER OGNI GEOMETRIA EMBEDD mappo i nodi o le facce

                            TimeSpan dtEmb1 = clock.Elapsed;

                            if (embeddedGeometriesTagAssociation.Count > 0 && embeddedGeometriesTagAssociation.ContainsKey(phGTags[p]))
                            {
                                try
                                {
                                    Dictionary<GeometryBase, int[]> embeddedGeometriesSingleMesh = new Dictionary<GeometryBase, int[]>(GeometryKeyComparer.Instance);

                                    // before, ElementAt(k) on the dictionary: O(n^2)
                                    foreach (KeyValuePair<GeometryBase, int[]> embedded in embeddedGeometriesTagAssociation[phGTags[p]])
                                    {
                                        GeometryBase geometry = embedded.Key;
                                        int[] geometryTag = embedded.Value;

                                        List<int> nodesBuffer = new List<int>();            // lista temporanea
                                        List<int> nodesBufferOut = new List<int>();         // lista che va in output con i vertici corretti nel giusto ordine
                                        HashSet<int> nodesInOutput = new HashSet<int>();    // before, nodesBufferOut.Contains: O(n^2)

                                        // the first point of a line or of a polygon, to know the direction of the first curve
                                        // (before, a Polygon2d was cast to Polygon3d: InvalidCastException)
                                        Point3d firstPoint = null;
                                        if (geometry is Line3d l3d)
                                            firstPoint = l3d.Start;
                                        else if (geometry is Line2d l2d)
                                            firstPoint = new Line3d(l2d).Start;
                                        else if (geometry is Polygon3d p3d)
                                            firstPoint = p3d[0];
                                        else if (geometry is Polygon2d p2d)
                                            firstPoint = new Point3d(p2d[0]);

                                        for (int j = 0; j < geometryTag.Length; j++)
                                        {
                                            if (firstPoint != null)
                                            {
                                                // Gabriele: messo parametro includeBoundary a true. restituisce i tag degli estremi però li mette in fondo all'array.
                                                // l'array è ordinato così: tag 1 - 2 - 3 - .... - start - end. lo riscrivo ordinato prima di aggiungerlo alla mappa dei nodi
                                                // problema: lo restituisce secondo l'ordine interno di gmsh e non quello mio di inserimento. devo flipparlo se nel verso opposto.
                                                // faccio il controllo prima sullo start della linea e poi sull'ultimo tag della lista
                                                // (the same code was duplicated for lines and polygons)
                                                Gmsh.Model.Mesh.GetNodes(out long[] nTags, out double[] coord, out double[] parametricCoord, 1, geometryTag[j], true, false);

                                                nodesBuffer.Add(vertexIdAssociation[(int)nTags[nTags.Length - 2]]);

                                                for (int i = 0; i < nTags.Length; i++)
                                                {
                                                    if (i != nTags.Length - 2 && vertexIdAssociation.TryGetValue((int)nTags[i], out int vertexId))
                                                        nodesBuffer.Add(vertexId);
                                                }

                                                if (nodesBufferOut.Count == 0)
                                                {
                                                    double[] startCoord = Gmsh.Model.GetValue(1, geometryTag[j], new double[] { 0 });

                                                    if (!(Math.Abs(firstPoint.X - startCoord[0]) < tolerance && Math.Abs(firstPoint.Y - startCoord[1]) < tolerance && Math.Abs(firstPoint.Z - startCoord[2]) < tolerance))
                                                    {
                                                        // significa che lo start della mia linea NON coincide con lo start della linea di gmsh => è al contrario. la rigiro
                                                        nodesBuffer.Reverse();
                                                    }
                                                }
                                                else if (nodesBufferOut[nodesBufferOut.Count - 1] != nodesBuffer[0])
                                                {
                                                    // significa che la lista che ha tirato fuori gmsh è in ordine opposto a quella che vorrei => è al contrario. la rigiro
                                                    nodesBuffer.Reverse();
                                                }

                                                for (int i = 0; i < nodesBuffer.Count; i++)
                                                {
                                                    if (nodesInOutput.Add(nodesBuffer[i]))
                                                        nodesBufferOut.Add(nodesBuffer[i]);
                                                }
                                                nodesBuffer.Clear();            // lo pulisco almeno evito calcoli inutili
                                            }

                                            else if (geometry is Point3d || geometry is Point2d)
                                            {
                                                // Giorgio: messo parametro includeBoundary a false altrimenti restituiva tag inesistenti
                                                Gmsh.Model.Mesh.GetNodes(out long[] nTags, out double[] coord, out double[] parametricCoord, 0, geometryTag[j], false, false);

                                                for (int i = 0; i < (nTags?.Length ?? 0); i++)
                                                {
                                                    nodesBufferOut.Add(vertexIdAssociation[(int)nTags[i]]);
                                                }
                                            }

                                            else if (geometry is Shape)
                                            {
                                                // the faces of the embedded shape (elementTag[0] => facce triangolari, elementTag[1] => facce quadrangolari)
                                                Gmsh.Model.Mesh.GetElements(out int[] elementType, out long[][] elementTag, out long[][] nTags, 2, geometryTag[j]);
                                                for (int i = 0; i < (elementTag?.Length ?? 0); i++)
                                                {
                                                    for (int h = 0; h < (elementTag[i]?.Length ?? 0); h++)
                                                    {
                                                        int faceId = faceIdAssociation[Convert.ToInt32(elementTag[i][h])];
                                                        if (nodesInOutput.Add(faceId))
                                                            nodesBufferOut.Add(faceId);
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
                            return false;
                        }

                        meshes.Add(meshBuffer);

                        TimeSpan dtEmb2 = clock.Elapsed;
                        embTime += (dtEmb2 - dtE2).TotalSeconds;
                    }
                }

                #endregion

                TimeSpan dt8 = clock.Elapsed;

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
                return false;
            }

            //Gmsh.Logger.Stop();

            generateMeshStatus.AddExecutionTimeMessage(GMeshGenerateMeshStatus.TotalTime, (clock.Elapsed - dt0).TotalSeconds);


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
        /// Embeds a segment of an embedded line or polygon in the surfaces <paramref name="surfaceTags"/>: the segment is split where it crosses the
        /// boundaries of the surfaces and every part is embedded in the surface that contains its midpoint (the parts out of the surfaces are skipped).
        /// A segment that is a side of a surface is not added again: the curve of the side is returned
        /// </summary>
        /// <returns>The tags of the curves of the segment (empty if the segment is out of the surfaces)</returns>
        /// <remarks>Before (the code was duplicated for lines and polygons):
        /// <list type="bullet">
        /// <item>only the first crossing was considered: a segment through three surfaces, or going out and back in the same surface, was embedded
        /// across a boundary ("Unable to recover the edge") or not embedded at all;</item>
        /// <item>a segment with an end not recognized inside a surface was embedded in the surface -1 (exception);</item>
        /// <item>a line with the midpoint out of the surfaces (e.g. across a hole) was not embedded, a polygon side with an end out of them neither;</item>
        /// <item>the crossings near the ends were ignored with the tolerance ProductTolerance(a^2, Math.Max(b, 2), ...): Math.Max instead of Math.Pow
        /// and, anyway, a quantity proportional to the fourth power of the lengths (about 70 for segments of 500)</item>
        /// </list></remarks>
        private static List<int> EmbedSegment(Line3d line, OpenCascadeWrapper occw, List<int> surfaceTags, SurfaceBoundaries boundaries,
            GMeshGenerateOptions options, double toleranceMatch, double toleranceIntersection, HashSet<GeometryBase> processed)
        {
            var curves = new List<int>();

            int startPointTag = occw.AddPointAndSync(line.Start, toleranceMatch);
            int endPointTag = occw.AddPointAndSync(line.End, toleranceMatch);

            // the segment is a side of a surface: its curve is used for the mapping of the nodes
            var boundaryCurves = new List<int>();
            foreach (int surfaceTag in surfaceTags)
            {
                foreach (int curve in boundaries.Curves(surfaceTag))
                {
                    (int a, int b) = boundaries.CurveEnds(curve);
                    if ((a == startPointTag && b == endPointTag) || (a == endPointTag && b == startPointTag))
                    {
                        processed.Add(line);
                        curves.Add(curve);
                        return curves;
                    }
                    if (!boundaryCurves.Contains(curve))
                        boundaryCurves.Add(curve);
                }
            }

            // the crossings with the boundaries, not at the ends of the segment, sorted from the start; tag = the vertex of the boundary
            // at the crossing, -1 if the crossing is inside a side
            var crossings = new List<(double distance, Point3d point, int tag)>();
            void AddCrossing(Point3d point, int tag)
            {
                double toEnd = point.DistanceTo(line.End);
                double toStart = point.DistanceTo(line.Start);
                double tolerance = ErrorPropagation.ProductTolerance(toEnd, toStart, toleranceIntersection, toleranceIntersection);
                if (toEnd <= tolerance || toStart <= tolerance)
                    return;

                if (!crossings.Any(c => c.point.DistanceTo(point) <= toleranceIntersection))
                    crossings.Add((toStart, point, tag));
            }

            // first the vertices of the boundaries on the segment: the segment passes through a vertex. Before, only the intersections
            // with the sides were considered, and GetIntersection does not find an intersection less than the tolerance from an end of
            // the side but not on it (a vertex moved by a previous fragment): the segment was not split there and was embedded across two
            // surfaces (degenerate faces)
            foreach (int curve in boundaryCurves)
            {
                Line3d side = boundaries.CurveLine(curve);
                if (side is null)
                    continue;

                (int a, int b) = boundaries.CurveEnds(curve);
                if (line.IsPointOnLine(side.Start, toleranceIntersection))
                    AddCrossing(side.Start, a);
                if (line.IsPointOnLine(side.End, toleranceIntersection))
                    AddCrossing(side.End, b);
            }

            foreach (int curve in boundaryCurves)
            {
                Line3d side = boundaries.CurveLine(curve);
                if (side is null || !side.GetIntersection(line, out Point3d point, toleranceIntersection) || point is null)
                    continue;

                AddCrossing(point, -1);
            }
            crossings.Sort((x, y) => x.distance.CompareTo(y.distance));

            var points = new List<Point3d> { line.Start };
            points.AddRange(crossings.Select(c => c.point));
            points.Add(line.End);

            var pointTags = new List<int> { startPointTag };
            foreach ((double _, Point3d point, int tag) in crossings)
                pointTags.Add(tag != -1 ? tag : occw.AddPointAndSync(point, toleranceMatch));
            pointTags.Add(endPointTag);

            // a point on a side of a surface (a crossing, or an end on the side) must be a vertex of the side (before, the part ended on the side
            // without a vertex there: "Unable to recover the edge", or Gmsh did not end)
            int fragments = boundaries.Fragments;
            for (int i = 0; i < points.Count; i++)
                pointTags[i] = MakeVertexOnSides(points[i], pointTags[i], surfaceTags, boundaries, occw, options, toleranceIntersection);

            // a fragment can have renumbered the points of the segment made vertices before: they are found again among the OCC points
            if (boundaries.Fragments != fragments)
            {
                int[] tags = occw.FindOccPoints(points, toleranceIntersection);
                for (int i = 0; i < points.Count; i++)
                {
                    if (tags[i] != -1)
                        pointTags[i] = tags[i];
                }
            }

            for (int i = 0; i < points.Count - 1; i++)
            {
                int surface = FindSurface(new Line3d(points[i], points[i + 1]).GetMidPoint(), surfaceTags);
                if (surface == -1)
                    continue; // part out of the surfaces

                int curve = occw.AddLineAndSync(pointTags[i], pointTags[i + 1]);
                boundaries.Embed(1, curve, surface);

                if (options.Transfinite)
                    TransfiniteLine(curve, options.MeshSize, options.TransfiniteLineType.ToString(), options.TransfiniteFactor);

                curves.Add(curve);
            }

            if (curves.Count > 0)
                processed.Add(line);

            return curves;
        }

        /// <summary>
        /// Makes <paramref name="point"/> a vertex of the sides of <paramref name="surfaceTags"/> where it is (not at their ends): the surfaces of the side
        /// are fragmented with the point (Mesh.Embed does not accept points in curves). The fragmented surfaces get new sides (the transfinite
        /// is set again) and lose their embedded entities (they are embedded again)
        /// </summary>
        /// <param name="point">The point</param>
        /// <param name="pointTag">The tag of the point</param>
        /// <param name="surfaceTags">The surfaces</param>
        /// <param name="boundaries">The boundaries of the surfaces (updated after the fragment)</param>
        /// <param name="occw">The OpenCASCADE entities</param>
        /// <param name="options">The options (transfinite)</param>
        /// <param name="toleranceIntersection">The tolerance on the distance of the point from the sides</param>
        /// <returns>The tag of the point after the fragment</returns>
        private static int MakeVertexOnSides(Point3d point, int pointTag, List<int> surfaceTags, SurfaceBoundaries boundaries, OpenCascadeWrapper occw, GMeshGenerateOptions options,
            double toleranceIntersection)
        {
            return MakeVertexOnSides(point, pointTag, surfaceTags, boundaries, occw, options, toleranceIntersection, out _);
        }

        /// <summary>
        /// Makes <paramref name="point"/> a vertex of the sides of <paramref name="surfaceTags"/> where it is (see the overload without <paramref name="onSide"/>)
        /// </summary>
        /// <param name="point">The point</param>
        /// <param name="pointTag">The tag of the point</param>
        /// <param name="surfaceTags">The surfaces</param>
        /// <param name="boundaries">The boundaries of the surfaces (updated after the fragment)</param>
        /// <param name="occw">The OpenCASCADE entities</param>
        /// <param name="options">The options (transfinite)</param>
        /// <param name="toleranceIntersection">The tolerance on the distance of the point from the sides</param>
        /// <param name="onSide">True if the point was on a side (it is now a vertex of the surfaces: it must not be embedded)</param>
        /// <returns>The tag of the point after the fragment</returns>
        private static int MakeVertexOnSides(Point3d point, int pointTag, List<int> surfaceTags, SurfaceBoundaries boundaries, OpenCascadeWrapper occw, GMeshGenerateOptions options,
            double toleranceIntersection, out bool onSide)
        {
            onSide = false;
            bool fragmented;
            int fragments = 0;
            do
            {
                // a point is on a few sides: the limit only protects from a loop without end if Gmsh does not make it a vertex
                if (++fragments > 64)
                    throw new InvalidOperationException($"The point {point} cannot be made a vertex of the sides of the surfaces");

                fragmented = false;
                foreach (int surfaceTag in surfaceTags.ToArray())
                {
                    foreach (int curve in boundaries.Curves(surfaceTag))
                    {
                        (int a, int b) = boundaries.CurveEnds(curve);
                        if (pointTag == a || pointTag == b)
                            continue;

                        Line3d side = boundaries.CurveLine(curve);
                        if (side is null || !side.IsPointOnLine(point, toleranceIntersection))
                            continue;

                        // the point is an end of the side, with another tag (a point added at the place of a vertex): the vertex is used
                        // (before, the side was fragmented with the point: a side of length ~0 when the vertex had been moved by a fragment)
                        if (side.Start.DistanceTo(point) <= toleranceIntersection || side.End.DistanceTo(point) <= toleranceIntersection)
                        {
                            pointTag = side.Start.DistanceTo(point) <= side.End.DistanceTo(point) ? a : b;
                            onSide = true;
                            continue;
                        }

                        // the point exactly on the side, projected on the curve of Gmsh (a crossing computed with a tolerance can be 1e-6 from
                        // it, more than the tolerance of OpenCascade: the fragment would not divide the side; the ends of the side can be moved
                        // by the previous fragments, so the segment between them is not the curve)
                        Gmsh.Model.GetClosestPoint(1, curve, new double[] { point.X, point.Y, point.Z }, out double[] closest, out _);
                        Point3d onTheSide = closest != null && closest.Length == 3 ? new Point3d(closest[0], closest[1], closest[2]) : ClosestPoint(side, point);
                        if (onTheSide.DistanceTo(point) > 0)
                        {
                            Gmsh.Model.Occ.Translate(new (int, int)[] { (0, pointTag) }, onTheSide.X - point.X, onTheSide.Y - point.Y, onTheSide.Z - point.Z);
                            Gmsh.Model.Occ.Synchronize();
                            point = onTheSide;
                        }

                        // all the surfaces of the side, also of the other shapes: a side shared by the surfaces of two shapes must be divided
                        // for all of them (otherwise the meshes of the two shapes are not conforming)
                        (int, int)[] adjacent = (Gmsh.Model.GetEntities(2) ?? new (int, int)[0]).Select(dimTag => dimTag.Item2)
                            .Where(s => boundaries.Curves(s).Contains(curve)).Select(s => (2, s)).ToArray();
                        Gmsh.Model.Occ.Fragment(adjacent, new (int, int)[] { (0, pointTag) }, out _, out (int, int)[][] resultMap, -1, true, true);
                        Gmsh.Model.Occ.Synchronize();

                        // the new tags of the surfaces (usually the same) and of the point; Gmsh.Net returns null for an empty map
                        (int, int)[] Map(int k) => resultMap != null && k < resultMap.Length && resultMap[k] != null ? resultMap[k] : new (int, int)[0];
                        for (int k = 0; k < adjacent.Length; k++)
                        {
                            int[] newTags = Map(k).Where(dimTag => dimTag.Item1 == 2).Select(dimTag => dimTag.Item2).ToArray();
                            boundaries.Replace(adjacent[k].Item2, newTags, surfaceTags);

                            if (options.Transfinite && !options.TransfiniteSurface)
                            {
                                foreach (int newSurface in newTags)
                                    foreach (int newSide in boundaries.Curves(newSurface))
                                        TransfiniteLine(newSide, options.MeshSize, options.TransfiniteLineType.ToString(), options.TransfiniteFactor);
                            }
                        }

                        int[] newPoint = Map(adjacent.Length).Where(dimTag => dimTag.Item1 == 0).Select(dimTag => dimTag.Item2).ToArray();
                        if (newPoint.Length > 0)
                            pointTag = newPoint[0];

                        // the vertices of the fragmented surfaces can have been renumbered: the map of the points is rebuilt (before, the
                        // following fragments used the old tags: "Unknown OpenCASCADE entity of dimension 0")
                        occw.RebuildObjectTags();

                        fragmented = true;
                        onSide = true;
                        break;
                    }

                    if (fragmented)
                        break;
                }
            }
            while (fragmented);

            return pointTag;
        }

        /// <returns>The point of the segment <paramref name="line"/> closest to <paramref name="point"/></returns>
        private static Point3d ClosestPoint(Line3d line, Point3d point)
        {
            double dx = line.End.X - line.Start.X, dy = line.End.Y - line.Start.Y, dz = line.End.Z - line.Start.Z;
            double length2 = dx * dx + dy * dy + dz * dz;
            if (length2 == 0)
                return new Point3d(line.Start.X, line.Start.Y, line.Start.Z);

            double t = ((point.X - line.Start.X) * dx + (point.Y - line.Start.Y) * dy + (point.Z - line.Start.Z) * dz) / length2;
            t = Math.Max(0, Math.Min(1, t));
            return new Point3d(line.Start.X + t * dx, line.Start.Y + t * dy, line.Start.Z + t * dz);
        }

        /// <returns>The first surface of <paramref name="surfaceTags"/> that contains <paramref name="point"/>, -1 if there is none</returns>
        private static int FindSurface(Point3d point, List<int> surfaceTags)
        {
            foreach (int surfaceTag in surfaceTags)
            {
                if (IsInside(point, surfaceTag) != -1)
                    return surfaceTag;
            }
            return -1;
        }

        /// <summary>
        /// The boundaries of the surfaces, read from Gmsh once, and the entities embedded in the surfaces. When a surface is fragmented to add a
        /// vertex on a side (see <see cref="MakeVertexOnSides(Point3d, int, List{int}, SurfaceBoundaries, OpenCascadeWrapper, GMeshGenerateOptions, double, out bool)"/>), its sides are read again and its embedded entities are embedded again
        /// </summary>
        private sealed class SurfaceBoundaries
        {
            /// <summary>
            /// The curves of the boundary of each surface
            /// </summary>
            private readonly Dictionary<int, int[]> _curves = new Dictionary<int, int[]>();
            /// <summary>
            /// The end points of each curve
            /// </summary>
            private readonly Dictionary<int, (int, int)> _curveEnds = new Dictionary<int, (int, int)>();
            /// <summary>
            /// The segment between the ends of each curve
            /// </summary>
            private readonly Dictionary<int, Line3d> _curveLines = new Dictionary<int, Line3d>();
            /// <summary>
            /// The entities embedded in each surface
            /// </summary>
            private readonly Dictionary<int, List<(int dim, int tag)>> _embedded = new Dictionary<int, List<(int dim, int tag)>>();

            /// <summary>
            /// True if some surfaces have been fragmented: their physical groups must be defined again
            /// </summary>
            public bool Fragmented => Fragments > 0;

            /// <summary>
            /// The number of fragments done
            /// </summary>
            public int Fragments { get; private set; }

            /// <summary>
            /// The surfaces whose tag has been changed by a fragment: old tag -> new tags
            /// </summary>
            public Dictionary<int, int[]> Renamed { get; } = new Dictionary<int, int[]>();

            /// <returns>The current tags of the surfaces <paramref name="tags"/> (they can have been changed by a fragment)</returns>
            public int[] Current(IEnumerable<int> tags)
            {
                var result = new List<int>();
                foreach (int tag in tags)
                {
                    if (Renamed.TryGetValue(tag, out int[] renamed))
                        result.AddRange(Current(renamed));
                    else
                        result.Add(tag);
                }
                return result.ToArray();
            }

            /// <summary>
            /// Embeds the entity in the surface and records it
            /// </summary>
            public void Embed(int dim, int tag, int surfaceTag)
            {
                Gmsh.Model.Mesh.Embed(dim, new int[1] { tag }, 2, surfaceTag);
                if (!_embedded.TryGetValue(surfaceTag, out List<(int dim, int tag)> entities))
                    _embedded[surfaceTag] = entities = new List<(int dim, int tag)>();
                entities.Add((dim, tag));
            }

            /// <summary>
            /// The surface <paramref name="oldTag"/> has been fragmented into <paramref name="newTags"/>: its sides are read again, its embedded
            /// entities are embedded again in the first new surface (a surface fragmented with a point on a side is not divided), the list
            /// <paramref name="surfaceTags"/> is updated
            /// </summary>
            public void Replace(int oldTag, int[] newTags, List<int> surfaceTags)
            {
                Fragments++;

                // the fragment renumbers the sides and reuses the tags of the removed ones: all the sides are read again
                _curves.Clear();
                _curveEnds.Clear();
                _curveLines.Clear();

                if (newTags.Length != 1 || newTags[0] != oldTag)
                {
                    Renamed[oldTag] = newTags;
                    int index = surfaceTags.IndexOf(oldTag);
                    if (index >= 0)
                    {
                        surfaceTags.RemoveAt(index);
                        surfaceTags.InsertRange(index, newTags);
                    }
                }

                if (newTags.Length > 0 && _embedded.TryGetValue(oldTag, out List<(int dim, int tag)> entities))
                {
                    _embedded.Remove(oldTag);
                    foreach ((int dim, int tag) in entities)
                        Embed(dim, tag, newTags[0]);
                }
            }

            /// <summary>
            /// The curves of the boundary of a surface (read from Gmsh the first time)
            /// </summary>
            /// <param name="surfaceTag">The tag of the surface</param>
            /// <returns>The tags of the curves (positive)</returns>
            public int[] Curves(int surfaceTag)
            {
                if (!_curves.TryGetValue(surfaceTag, out int[] curves))
                {
                    // Gmsh.Net returns null for an empty result
                    curves = (Gmsh.Model.GetBoundary(new (int, int)[1] { (2, surfaceTag) }, false, false, false) ?? new (int, int)[0])
                        .Select(i => Math.Abs(i.Item2)).ToArray();
                    _curves[surfaceTag] = curves;
                }
                return curves;
            }

            /// <summary>
            /// The end points of a curve (read from Gmsh the first time)
            /// </summary>
            /// <param name="curveTag">The tag of the curve</param>
            /// <returns>The tags of the two end points; (-1, -1) if the curve has not two ends</returns>
            public (int, int) CurveEnds(int curveTag)
            {
                if (!_curveEnds.TryGetValue(curveTag, out (int, int) ends))
                {
                    int[] points = (Gmsh.Model.GetBoundary(new (int, int)[1] { (1, curveTag) }, false, false, true) ?? new (int, int)[0]).Select(i => i.Item2).ToArray();
                    ends = points.Length >= 2 ? (points[0], points[1]) : (-1, -1);
                    _curveEnds[curveTag] = ends;
                }
                return ends;
            }

            /// <returns>The segment between the ends of the curve, null if the curve is closed</returns>
            public Line3d CurveLine(int curveTag)
            {
                if (!_curveLines.TryGetValue(curveTag, out Line3d curveLine))
                {
                    (int a, int b) = CurveEnds(curveTag);
                    if (a != -1 && b != -1 && a != b)
                    {
                        Gmsh.Model.Occ.GetBoundingBox(0, a, out double x1, out double y1, out double z1, out _, out _, out _);
                        Gmsh.Model.Occ.GetBoundingBox(0, b, out double x2, out double y2, out double z2, out _, out _, out _);
                        curveLine = new Line3d(new Point3d(x1, y1, z1), new Point3d(x2, y2, z2));
                    }
                    _curveLines[curveTag] = curveLine;
                }
                return curveLine;
            }
        }

        /// <summary>
        /// Check if the input point is inside the input surface
        /// </summary>
        /// <param name="point">Point to test</param>
        /// <param name="surfaceTag">The tag of the surface</param>
        /// <returns>The surface tag if the point is inside, -1 if it's outside</returns>
        /// <remarks>The point is tested with its coordinates: for a plane face Gmsh computes the winding number on the edges of the face, so the
        /// face cut by Fragment or by holes is considered. Before, the parametric coordinates were tested against the parametric bounds of the
        /// surface: after Fragment every part of a shape has the surface of the whole shape, so a point was inside all the parts and the embedded
        /// geometries were put in the wrong part (or not embedded)</remarks>
        private static int IsInside(Point3d point, int surfaceTag)
        {
            try
            {
                if (Gmsh.Model.IsInside(2, surfaceTag, new double[3] { point.X, point.Y, point.Z }, false) != 0) // significa che abbiamo beccato la superficie dove sta il punto
                    return surfaceTag;
            }
            catch (GmshException)
            {
                // entity where the test is not available: the point is considered outside
            }
            return -1;
        }

        /// <summary>
        /// The Blossom recombinations of Gmsh can fail ("Perfect Match failed in quadrangulation, try something else") where the simple ones work:
        /// the mesh is recombined with the simple algorithm of the same kind (Blossom -> Simple, Blossom Full-Quad -> Simple Full-Quad).
        /// After the failure Gmsh does not recombine the same mesh again (the call does nothing): the mesh is generated again with the same steps
        /// (the generation is deterministic)
        /// </summary>
        /// <returns>True if the recombination with the simple algorithm has been done (a warning is added to <paramref name="status"/>),
        /// false if the algorithm was already simple or the second recombination failed too</returns>
        private static bool RetryRecombineWithSimple(GMeshGenerateOptions options, GmshException exception, GMeshGenerateMeshStatus status)
        {
            GMeshGenerateOptions.RecombinationMeshAlgorithm simple;
            if (options.RecombinationAlgorithm == GMeshGenerateOptions.RecombinationMeshAlgorithm.Blossom)
                simple = GMeshGenerateOptions.RecombinationMeshAlgorithm.Simple;
            else if (options.RecombinationAlgorithm == GMeshGenerateOptions.RecombinationMeshAlgorithm.BlossomFullQuad)
                simple = GMeshGenerateOptions.RecombinationMeshAlgorithm.SimpleFullQuad;
            else
                return false;

            try
            {
                Gmsh.Option.SetNumber("Mesh.RecombinationAlgorithm", (int)simple);

                Gmsh.Model.Mesh.Clear(new (int, int)[0]);
                Gmsh.Model.Mesh.Generate(1);
                Gmsh.Model.Mesh.Generate(2);
                try
                {
                    if (options.Refine)
                        Gmsh.Model.Mesh.Refine();
                }
                catch (GmshException)
                {
                    // as in the first generation: the mesh is not refined (the warning is already in the status)
                }
                if (options.Renumber)
                {
                    Gmsh.Model.Mesh.RenumberNodes();
                    Gmsh.Model.Mesh.RenumberElements();
                }

                Gmsh.Model.Mesh.SplitQuadrangles(options.MinQuality, -1);
                Gmsh.Model.Mesh.Recombine();
            }
            catch (GmshException)
            {
                return false;
            }
            finally
            {
                Gmsh.Option.SetNumber("Mesh.RecombinationAlgorithm", (int)options.RecombinationAlgorithm);
            }

            status.AddWarning($"The recombination {options.RecombinationAlgorithm} failed ({exception.Message.Trim()}): the mesh has been recombined with {simple}");
            return true;
        }

        /// <returns>The number of nodes of a straight curve divided in parts not longer than <paramref name="meshSize"/> (at least the two ends)</returns>
        private static int CurveNodes(int curveTag, double meshSize)
        {
            Gmsh.Model.Occ.GetBoundingBox(1, curveTag, out double x1, out double y1, out double z1, out double x2, out double y2, out double z2);
            double length = new Point3d(x1, y1, z1).DistanceTo(new Point3d(x2, y2, z2));
            return Math.Max(2, (int)Math.Round(length / meshSize, 0, MidpointRounding.AwayFromZero) + 1);
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
            // at least the two ends (before, a curve shorter than half the mesh size had 1 node)
            int geometryNumNodel1 = Math.Max(2, (int)Math.Round((l1lenght / meshSize), 0, MidpointRounding.AwayFromZero) + 1);

            Gmsh.Model.Mesh.SetTransfiniteCurve(lineTag, geometryNumNodel1, transfiniteLineType, transfiniteFactor);
        }

        #endregion

        #endregion

        #region Nested classes

        /// <summary>
        /// Equality by reference, for dictionaries whose keys (shapes) are modified after they have been added
        /// </summary>
        private sealed class GeometryKeyComparer : IEqualityComparer<GeometryBase>, IEqualityComparer<Line3d>
        {
            public static readonly GeometryKeyComparer Instance = new GeometryKeyComparer();

            public bool Equals(GeometryBase x, GeometryBase y)
            {
                if (ReferenceEquals(x, y)) return true;
                if (x is null || y is null) return false;
                if (x is Point3d p && y is Point3d q) return Point3d.ExactComparer.Equals(p, q);
                if (x is Point2d p2 && y is Point2d q2) return Point2d.ExactComparer.Equals(p2, q2);
                if (x is Line3d a && y is Line3d b)
                    return (Equals(a.Start, b.Start) && Equals(a.End, b.End)) || (Equals(a.Start, b.End) && Equals(a.End, b.Start));
                if (x is Line2d a2 && y is Line2d b2)
                    return (Equals(a2.Start, b2.Start) && Equals(a2.End, b2.End)) || (Equals(a2.Start, b2.End) && Equals(a2.End, b2.Start));
                if (x is Polygon3d poly && y is Polygon3d other) return poly.SequenceEqual(other, Point3d.ExactComparer);
                if (x is Polygon2d poly2 && y is Polygon2d other2) return poly2.SequenceEqual(other2, Point2d.ExactComparer);
                if (x is Shape shape && y is Shape otherShape)
                    return Equals(shape.Fill, otherShape.Fill) && SameItems(shape.Holes, otherShape.Holes) && SameItems(shape.Childs, otherShape.Childs);
                return false;
            }

            private bool SameItems(GeometryBase[] x, GeometryBase[] y)
            {
                if (ReferenceEquals(x, y)) return true;
                if (x == null || y == null || x.Length != y.Length) return false;
                var matched = new bool[y.Length];
                foreach (var item in x)
                {
                    int i = 0;
                    while (i < y.Length && (matched[i] || !Equals(item, y[i]))) i++;
                    if (i == y.Length) return false;
                    matched[i] = true;
                }
                return true;
            }

            public int GetHashCode(GeometryBase geometry)
            {
                if (geometry is null) return 0;
                if (geometry is Point3d p) return Point3d.ExactComparer.GetHashCode(p);
                if (geometry is Point2d p2) return Point2d.ExactComparer.GetHashCode(p2);
                if (geometry is Line3d line) return GetHashCode(line.Start) ^ GetHashCode(line.End);
                if (geometry is Line2d line2) return GetHashCode(line2.Start) ^ GetHashCode(line2.End);
                unchecked
                {
                    int hash = 17;
                    if (geometry is Polygon3d poly) foreach (var point in poly) hash = hash * 31 + GetHashCode(point);
                    else if (geometry is Polygon2d poly2) foreach (var point in poly2) hash = hash * 31 + GetHashCode(point);
                    else if (geometry is Shape shape)
                    {
                        hash = GetHashCode(shape.Fill);
                        if (shape.Holes != null) foreach (var hole in shape.Holes) hash += 31 * GetHashCode(hole);
                        if (shape.Childs != null) foreach (var child in shape.Childs) hash += 37 * GetHashCode(child);
                    }
                    return hash;
                }
            }

            bool IEqualityComparer<Line3d>.Equals(Line3d x, Line3d y) => Equals((GeometryBase)x, y);
            int IEqualityComparer<Line3d>.GetHashCode(Line3d line) => GetHashCode((GeometryBase)line);
        }

        private sealed class ReferenceComparer<T> : IEqualityComparer<T> where T : class
        {
            /// <summary>
            /// The shared instance (the comparer has no state)
            /// </summary>
            public static readonly ReferenceComparer<T> Instance = new ReferenceComparer<T>();

            /// <summary>
            /// Equality by reference
            /// </summary>
            /// <param name="x">The first object</param>
            /// <param name="y">The second object</param>
            /// <returns>True if they are the same instance</returns>
            public bool Equals(T x, T y) => ReferenceEquals(x, y);

            /// <summary>
            /// The hash code of the instance (not of its content)
            /// </summary>
            /// <param name="obj">The object</param>
            /// <returns>The hash code</returns>
            public int GetHashCode(T obj) => RuntimeHelpers.GetHashCode(obj);
        }

        /// <summary>
        /// Struct to collect the Gmesh element parameters
        /// </summary>
        private struct GMeshElementParameters
        {
            /// <summary>
            /// The name of the element type
            /// </summary>
            public string ElementName;
            /// <summary>
            /// The Gmsh element type
            /// </summary>
            public int ElementType;
            /// <summary>
            /// The number of nodes
            /// </summary>
            public int NodesNumber;
            /// <summary>
            /// The number of primary (corner) nodes
            /// </summary>
            public int PrimaryNodesNumber;
            /// <summary>
            /// The dimension of the element
            /// </summary>
            public int Dimension;
            /// <summary>
            /// The order of the element
            /// </summary>
            public int Order;
        }

        /// <summary>
        /// The options of the generation with Gmsh (most of them are Gmsh options, see the Gmsh reference manual)
        /// </summary>
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

            /// <summary>Maximum chord deviation when discretizing embedded Curve3d objects, in input model units.</summary>
            public double CurveChordTolerance = GeometryBase.Tolerance;

            /// <summary>Optional maximum chord length of embedded curves, in input model units.</summary>
            public double CurveMaxSegmentLength = double.PositiveInfinity;

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
            /// <summary>
            /// The 2D mesh algorithms of Gmsh (option Mesh.Algorithm)
            /// </summary>
            public enum MeshAlgorithm
            {
                /// <summary>MeshAdapt (1)</summary>
                MeshAdapt = 1,
                /// <summary>Automatic (2)</summary>
                Automatic = 2,
                /// <summary>Initial mesh only (3): only the points of the boundary</summary>
                InitialMeshOnly = 3,
                /// <summary>Delaunay (5)</summary>
                Delaunay = 5,
                /// <summary>Frontal-Delaunay (6)</summary>
                [Description("Frontal-Delaunay")]
                FrontalDelaunay = 6,
                /// <summary>BAMG (7)</summary>
                BAMG = 7,
                /// <summary>Frontal-Delaunay for quads (8)</summary>
                [Description("Frontal-Delaunay for Quads")]
                FrontalDelaunayForQuads = 8,
                /// <summary>Packing of parallelograms (9); with Gmsh 4.15.2 Frontal-Delaunay for quads is used instead (native crash with embedded surfaces)</summary>
                [Description("Packing of Parallelograms")]
                PackingOfParallelograms = 9,
                /// <summary>Quasi-structured quad (11)</summary>
                [Description("Quasi-structured Quad")]
                QuasiStructuredQuad = 11,
            }

            /// <summary>
            /// The recombination algorithms of Gmsh (option Mesh.RecombinationAlgorithm)
            /// </summary>
            public enum RecombinationMeshAlgorithm
            {
                /// <summary>Simple (0)</summary>
                Simple = 0,
                /// <summary>Blossom (1)</summary>
                Blossom = 1,
                /// <summary>Simple full-quad (2)</summary>
                [Description("Simple Full-Quad")]
                SimpleFullQuad = 2,
                /// <summary>Blossom full-quad (3)</summary>
                [Description("Blossom Full-Quad")]
                BlossomFullQuad = 3,
            }

            /// <summary>
            /// The distributions of the nodes of the transfinite curves
            /// </summary>
            public enum TransfiniteType
            {
                /// <summary>Geometrical progression: every segment is the previous one multiplied by the factor</summary>
                [Description("Progression")]
                Progression = 0,
                /// <summary>Refinement toward both extremities of the curve</summary>
                [Description("Bump")]
                Bump = 1,
            }

            /// <summary>
            /// The optimization methods of Gmsh (Gmsh.Model.Mesh.Optimize)
            /// </summary>
            public enum MeshOptimize
            {
                /// <summary>The default optimizer of the tetrahedra</summary>
                [Description("")]
                Tetrahedral = 0,
                /// <summary>Netgen optimizer</summary>
                [Description("Netgen")]
                Netgen = 1,
                /// <summary>Optimization of the high order elements</summary>
                [Description("HighOrder")]
                HighOrder = 2,
                /// <summary>Elastic optimization of the high order elements</summary>
                [Description("HighOrderElastic")]
                HighOrderElastic = 3,
                /// <summary>Fast curving of the high order elements</summary>
                [Description("HighOrderFastCurving")]
                HighOrderFastCurving = 4,
                /// <summary>Relocation of the 2D nodes</summary>
                [Description("Relocate2D")]
                Relocate2D = 5,
                /// <summary>Relocation of the 3D nodes</summary>
                [Description("Relocate3D")]
                Relocate3D = 6,
            }

            #endregion

            /// <summary>
            /// The default options: Frontal-Delaunay for quads with Simple Full-Quad recombination, no size limit, transfinite curves, healing of the shapes
            /// </summary>
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

            /// <summary>
            /// Creates a copy of the options
            /// </summary>
            /// <returns>The copy</returns>
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
                    TransfiniteSurface = TransfiniteSurface, // it was not copied

                    HealShapes = HealShapes,
                    MatchGeomAndMesh = MatchGeomAndMesh,
                    SewFaces = SewFaces,
                    OCCUnionUnify = OCCUnionUnify,

                    Tolerance = Tolerance,
                    MatchMeshTolerance = MatchMeshTolerance,
                    ToleranceInitialDelaunay = ToleranceInitialDelaunay,
                    ToleranceEdgeLength = ToleranceEdgeLength,
                    AngleToleranceFacetOverlap = AngleToleranceFacetOverlap,
                    CurveChordTolerance = CurveChordTolerance,
                    CurveMaxSegmentLength = CurveMaxSegmentLength,

                    GeometryBaseScaleFactor = GeometryBaseScaleFactor,
                    MeshScalingFactor = MeshScalingFactor
                };

                return clone;
            }
        }

        /// <summary>
        /// The result of the generation with Gmsh: errors, warnings, times and the vertices of the embedded geometries
        /// </summary>
        [Serializable]
        public sealed class GMeshGenerateMeshStatus : GenerateMeshStatus
        {
            /// <summary>
            /// The description of the total time of the generation
            /// </summary>
            public const string TotalTime = "Total time";
            /// <summary>
            /// Error message: the transfinite constraint failed
            /// </summary>
            public const string FailedToTransfinite = "GmshException: Failed to set transfinite";
            /// <summary>
            /// Error message: the shape can not be created
            /// </summary>
            public const string FailedToCreateTheShape = "Failed to create the shape";
            /// <summary>
            /// Error message: a line can not be embedded
            /// </summary>
            public const string FailedToEmbedTheLine = "Failed to embed the line";
            /// <summary>
            /// Error message: a point can not be embedded
            /// </summary>
            public const string FailedToEmbedThePoint = "Failed to embed the point";
            /// <summary>
            /// Error message: a polygon can not be embedded
            /// </summary>
            public const string FailedToEmbedThePolygon = "Failed to embed the polygon";
            /// <summary>
            /// Error message: a shape can not be embedded
            /// </summary>
            public const string FailedToEmbedTheShape = "Failed to embed the shape";
            /// <summary>
            /// Gmsh error: an edge of a curve can not be recovered
            /// </summary>
            public const string UnableToRecover = "GmshException: Mesh.Generate Unable to recover the edge on curve";
            /// <summary>
            /// Gmsh error: identical points in the triangulation
            /// </summary>
            public const string IdenticalPoints = "GmshException: Identical points in triangulation";
            /// <summary>
            /// Gmsh error: singular 3x3 matrix
            /// </summary>
            public const string SingularMatrix = "GmshException: Singular matrix 3x3";

            /// <summary>
            /// For each mesh, the ids of its vertices on each embedded geometry
            /// </summary>
            private readonly Dictionary<Mesh, Dictionary<GeometryBase, int[]>> _embeddedGeometriesVertexMap;

            /// <summary>
            /// The number of embedded points
            /// </summary>
            private int _embeddedPoints;
            /// <summary>
            /// The number of embedded lines
            /// </summary>
            private int _embeddedLines;
            /// <summary>
            /// The number of embedded polygons
            /// </summary>
            private int _embeddedPolygons;
            /// <summary>
            /// The number of embedded shapes
            /// </summary>
            private int _embeddedShapes;

            /// <summary>
            /// For each mesh, the ids of its vertices on each embedded geometry
            /// </summary>
            public Dictionary<Mesh, Dictionary<GeometryBase, int[]>> EmbeddedGeometriesVertexMap => _embeddedGeometriesVertexMap;

            /// <summary>
            /// The number of embedded points
            /// </summary>
            public int EmbeddedPoints { get => _embeddedPoints; set => _embeddedPoints = value; }

            /// <summary>
            /// The number of embedded lines
            /// </summary>
            public int EmbeddedLines { get => _embeddedLines; set => _embeddedLines = value; }

            /// <summary>
            /// The number of embedded polygons
            /// </summary>
            public int EmbeddedPolygons { get => _embeddedPolygons; set => _embeddedPolygons = value; }

            /// <summary>
            /// The number of embedded shapes
            /// </summary>
            public int EmbeddedShapes { get => _embeddedShapes; set => _embeddedShapes = value; }

            /// <summary>
            /// Creates an empty status
            /// </summary>
            public GMeshGenerateMeshStatus()
                : base()
            {
                _embeddedGeometriesVertexMap = new Dictionary<Mesh, Dictionary<GeometryBase, int[]>>(ReferenceComparer<Mesh>.Instance);
            }

            /// <summary>
            /// Sets the vertices of the embedded geometries of a mesh
            /// </summary>
            /// <param name="mesh">The mesh</param>
            /// <param name="embeddedGeometries">The ids of the vertices of the mesh on each embedded geometry</param>
            public void AddEmbeddedGeometries(Mesh mesh, Dictionary<GeometryBase, int[]> embeddedGeometries)
            {
                _embeddedGeometriesVertexMap[mesh] = embeddedGeometries;
            }
        }

        /// <summary>
        /// The OpenCASCADE entities created through Gmsh: the points are created once and reused (map point -> tag)
        /// </summary>
        private sealed class OpenCascadeWrapper
        {
            /// <summary>
            /// The tags of the points already created
            /// </summary>
            private Dictionary<Point3d, int> _pointTag;

            // (the map line -> tag was only written, and AddLineAndSync(int, int) read the bounding box of every line to build its key: removed)

            /// <summary>
            /// This Class use <see cref="Gmsh"/> then <c>Gmsh.Initialize</c> needs to be called before calling any of its methods
            /// </summary>
            public OpenCascadeWrapper()
            {
                _pointTag = new Dictionary<Point3d, int>(Point3d.ExactComparer);
            }

            /// <summary>
            /// Add a point to OCC and return the tag. If point already exist in OCC return the tag of the point already present
            /// </summary>
            /// <param name="point">Point to Add</param>
            /// <returns>Point tag inside OCC</returns>
            /// <remarks>This method use <see cref="Gmsh"/> then <c>Gmsh.Initialize</c> needs to be called before calling this method </remarks>
            public int AddPoint(Point3d point)
            {
                if (_pointTag.TryGetValue(point, out int tag))
                    return tag;

                int t = Gmsh.Model.Occ.AddPoint(point.X, point.Y, point.Z);
                return _pointTag[point] = t;
            }

            /// <summary>
            /// Add a point to OCC and return the tag. If point already exist in OCC return the tag of the point already present
            /// </summary>
            /// <param name="point">Point to Add</param>
            /// <returns>Point tag inside OCC</returns>
            /// <remarks>This method use <see cref="Gmsh"/> then <c>Gmsh.Initialize</c> needs to be called before calling this method </remarks>
            public int AddPointAndSync(Point3d point)
            {
                if (_pointTag.TryGetValue(point, out int tag))
                    return tag;

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
            /// <remarks>This method use <see cref="Gmsh"/> then <c>Gmsh.Initialize</c> needs to be called before calling this method </remarks>
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
            /// <remarks>This method use <see cref="Gmsh"/> then <c>Gmsh.Initialize</c> needs to be called before calling this method </remarks>
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
                int startTag = AddPoint(line.Start, tolerance);
                int endTag = AddPoint(line.End, tolerance);

                int t = Gmsh.Model.Occ.AddLine(startTag, endTag);
                Gmsh.Model.Occ.Synchronize();
                return t;
            }

            /// <summary>
            /// Add a line to OCC and return the tag
            /// </summary>
            /// <param name="startTag">Start line gmsh tag</param>
            /// <param name="endTag">End line gmsh tag</param>
            /// <returns>The tag of the line</returns>
            public int AddLineAndSync(int startTag, int endTag)
            {
                int t = Gmsh.Model.Occ.AddLine(startTag, endTag);
                Gmsh.Model.Occ.Synchronize();
                return t;
            }

            /// <summary>
            /// Get the point tag
            /// </summary>
            /// <param name="point">Point to Add</param>
            /// <returns>Point tag inside OCC if already added. If the point does not exist return -1</returns>
            public int GetPointTag(Point3d point)
            {
                return _pointTag.TryGetValue(point, out int tag) ? tag : -1;
            }

            /// <summary>
            /// Get the point tag
            /// </summary>
            /// <param name="point">Point to Add</param>
            /// <param name="tolerance">The tolerance of matching</param>
            /// <returns>Point tag inside OCC if already added. If the point does not exist return -1</returns>
            /// <remarks>The first point within the tolerance, in the order of insertion. Before, a Parallel.For with ElementAt on the dictionary
            /// (O(n^2) for every call) where every thread wrote the same variable: the tag returned was not deterministic</remarks>
            public int GetPointTag(Point3d point, double tolerance)
            {
                if (_pointTag.TryGetValue(point, out int tag))
                    return tag;

                foreach (KeyValuePair<Point3d, int> pointTag in _pointTag)
                {
                    if (IsSamePoint(point.DistanceTo(pointTag.Key), tolerance))
                        return pointTag.Value;
                }

                return -1;
            }

            /// <summary>
            /// The criterion of <see cref="GetPointTag(Point3d, double)"/>: the square root of the distance lower than the square root of the propagated
            /// tolerance of the product distance * distance (the distance lower than <paramref name="tolerance"/> for the small distances)
            /// </summary>
            private static bool IsSamePoint(double distance, double tolerance)
            {
                double dist = Math.Sqrt(distance);
                return dist < Math.Sqrt(ErrorPropagation.ProductTolerance(dist, dist, tolerance, tolerance));
            }

            /// <returns>The coordinates of the OCC points <paramref name="tags"/>: one call to Gmsh for every point</returns>
            private static Point3d[] OccPoints((int, int)[] tags)
            {
                var points = new Point3d[tags.Length];
                for (int j = 0; j < tags.Length; j++)
                {
                    Gmsh.Model.Occ.GetBoundingBox(0, tags[j].Item2, out double x, out double y, out double z, out _, out _, out _);
                    points[j] = new Point3d(x, y, z);
                }
                return points;
            }

            /// <returns>For every point of <paramref name="points"/>, the tag of the closest OCC point if it is not farther than <paramref name="tolerance"/>,
            /// -1 otherwise (the coordinates of the OCC points are read once)</returns>
            public int[] FindOccPoints(IList<Point3d> points, double tolerance)
            {
                (int, int)[] tags = Gmsh.Model.Occ.GetEntities(0) ?? new (int, int)[0];
                Point3d[] occPoints = OccPoints(tags);

                var result = new int[points.Count];
                for (int i = 0; i < points.Count; i++)
                {
                    int j = Closest(occPoints, points[i], out double squareDistance);
                    result[i] = j != -1 && Math.Sqrt(squareDistance) <= tolerance ? tags[j].Item2 : -1;
                }
                return result;
            }

            /// <returns>The index of the point of <paramref name="points"/> closest to <paramref name="point"/> (the first one if more are at the same distance),
            /// -1 if <paramref name="points"/> is empty</returns>
            private static int Closest(Point3d[] points, Point3d point, out double squareDistanceMin)
            {
                int indexMin = -1;
                squareDistanceMin = double.MaxValue;
                for (int j = 0; j < points.Length; j++)
                {
                    double squareDistance = points[j].SquareDistanceTo(point);
                    if (squareDistance < squareDistanceMin)
                    {
                        indexMin = j;
                        squareDistanceMin = squareDistance;
                    }
                }
                return indexMin;
            }

            /// <summary>
            /// Rebuild the objects tag associations inside OpenCascadeWrapper
            /// </summary>
            /// <param name="surfaceTag">If is set, rebuilt only the points of the selected surface (the input is the surfaceTag). Otherwise rebuilt all the points of the model</param>
            /// <remarks>This method use <see cref="Gmsh"/> then <c>Gmsh.Initialize</c> needs to be called before calling this method.
            /// The coordinates of the OCC points are read once (before, once for every point of the map: n * m calls to Gmsh, and ElementAt on the dictionary)</remarks>
            public void RebuildObjectTags(int surfaceTag = -1)
            {
                Dictionary<Point3d, int> pointsTagCopy = _pointTag;
                _pointTag = new Dictionary<Point3d, int>(Point3d.ExactComparer); // Reset della mappa

                if (surfaceTag == -1)
                {
                    (int, int)[] tags = Gmsh.Model.Occ.GetEntities(0); // Recupero tutte le entità
                    Point3d[] occPoints = OccPoints(tags);

                    foreach (Point3d point in pointsTagCopy.Keys)
                    {
                        int j = Closest(occPoints, point, out _);
                        if (!_pointTag.ContainsKey(point))
                            _pointTag.Add(point, j == -1 ? -1 : tags[j].Item2); // Ricostruisco mappa
                    }
                }
                else
                {
                    (int, int)[] tags = Gmsh.Model.GetBoundary(new (int, int)[] { (2, surfaceTag) }, false, false, true);
                    Point3d[] occPoints = OccPoints(tags);
                    Point3d[] points = pointsTagCopy.Keys.ToArray();

                    for (int i = 0; i < tags.Length; i++)
                    {
                        int j = Closest(points, occPoints[i], out _);
                        if (j != -1 && !_pointTag.ContainsKey(points[j]))
                            _pointTag.Add(points[j], tags[i].Item2); // Ricostruisco mappa
                    }

                    foreach (KeyValuePair<Point3d, int> pointTag in pointsTagCopy)
                    {
                        if (!_pointTag.ContainsKey(pointTag.Key))
                            _pointTag.Add(pointTag.Key, pointTag.Value);
                    }
                }
            }

            /// <summary>
            /// Rebuild the objects tag associations inside OpenCascadeWrapper and check if each point exist
            /// </summary>
            /// <remarks>This method use <see cref="Gmsh"/> then <c>Gmsh.Initialize</c> needs to be called before calling this method.
            /// A point exists if the closest OCC point is within the tolerance of <see cref="GetPointTag(Point3d, double)"/>, otherwise it is added again.
            /// Before, the tolerance was computed from the distance of the last OCC point of the list (not the closest one), with the square root of a
            /// distance used as distance</remarks>
            public void RebuildObjectTagsAndCheck(double tolerance)
            {
                Dictionary<Point3d, int> pointsTagCopy = _pointTag;
                _pointTag = new Dictionary<Point3d, int>(Point3d.ExactComparer); // Reset della mappa

                (int, int)[] tags = Gmsh.Model.Occ.GetEntities(0); // Recupero tutte le entità
                Point3d[] occPoints = OccPoints(tags);

                foreach (Point3d point in pointsTagCopy.Keys)
                {
                    int j = Closest(occPoints, point, out double squareDistance);

                    if (j != -1 && IsSamePoint(Math.Sqrt(squareDistance), tolerance))
                    {
                        if (!_pointTag.ContainsKey(point))
                            _pointTag.Add(point, tags[j].Item2); // Ricostruisco mappa
                    }
                    else
                    {
                        _pointTag[point] = Gmsh.Model.Occ.AddPoint(point.X, point.Y, point.Z);
                    }
                }
            }
        }

        #endregion
    }
}
