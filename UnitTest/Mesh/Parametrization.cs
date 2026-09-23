using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GPC.Geometry;
using GPC.Geometry.Meshes;

namespace Meshes
{/*
    [TestClass]
    public class Parametrization : GMeshTest
    {
        private void CommonParametrizationAssert(List<Mesh> meshes, List<Shape> shapes)
        {
            Assert.IsTrue(meshes.First().Faces.Count() == (shapes.First().Fill.Explode().Count() - 2),
                $"Faces count: {meshes.First().Faces.Count()}, faces expected: {(shapes.First().Fill.Explode().Count() - 2)}");
        }

        [TestMethod]
        public void TriangleEquilateral1()
        {
            Polygon3d p1 = new Polygon3d()
            {
                new Point3d(0, 0, 0),
                new Point3d(0, 100, 0),
                new Point3d(86.60, 50, 0),
            };
            Shape s1 = new Shape(p1);

            var shapes = new List<Shape>();
            shapes.Add(s1);

            Mesh.GenerateOptions options = new Mesh.GenerateOptions();

            options.Algorithm = Mesh.GenerateOptions.MeshAlgorithm.MeshAdapt;
            options.Recombine = true;
            options.RecombinationAlgorithm = Mesh.GenerateOptions.RecombinationMeshAlgorithm.Simple;
            options.MeshSize = 200;
            options.UseGlobalProgressID = true;
            options.RecombineOptimizeTopology = 5;
            options.HealShapes = false;
            options.Optimize = false;
            options.OptimizeIteration = 1;
            options.OptimizeAlgorithm = Mesh.GenerateOptions.MeshOptimize.Netgen;
            options.OptimizeNetgen = 1;
            options.Transfinite = true;
            options.MinQuality = 0.90;
            options.Refine = false;
            options.Smoothing = 10;

            Mesh.Generate(shapes, null, null, options, out List <Mesh> meshes, out Mesh.GenerateMeshStatus generateMeshStatus);
            MeshExport.ExportToMshFormatv2(Path.ChangeExtension(Path.Combine(_outputFolder, TestContext.TestName), ".msh"), meshes);
            CommonParametrizationAssert(meshes, shapes);
        }

        [TestMethod]
        public void TriangleIsoscele1()
        {
            Polygon3d p1 = new Polygon3d()
            {
                new Point3d(0, 0, 0),
                new Point3d(0, 50, 0),
                new Point3d(50, 50, 0),
            };
            Shape s1 = new Shape(p1);

            var shapes = new List<Shape>();
            shapes.Add(s1);

            Mesh.GenerateOptions options = new Mesh.GenerateOptions();

            options.Algorithm = Mesh.GenerateOptions.MeshAlgorithm.MeshAdapt;
            options.Recombine = true;
            options.RecombinationAlgorithm = Mesh.GenerateOptions.RecombinationMeshAlgorithm.Simple;
            options.MeshSize = 200;
            options.UseGlobalProgressID = true;
            options.RecombineOptimizeTopology = 5;
            options.HealShapes = false;
            options.Optimize = false;
            options.OptimizeIteration = 1;
            options.OptimizeAlgorithm = Mesh.GenerateOptions.MeshOptimize.Netgen;
            options.OptimizeNetgen = 1;
            options.Transfinite = true;
            options.MinQuality = 0.90;
            options.Refine = false;
            options.Smoothing = 10;

            Mesh.Generate(shapes, null, null, options, out List<Mesh> meshes, out Mesh.GenerateMeshStatus generateMeshStatus);
            MeshExport.ExportToMshFormatv2(Path.ChangeExtension(Path.Combine(_outputFolder, TestContext.TestName), ".msh"), meshes);
            CommonParametrizationAssert(meshes, shapes);
        }

        [TestMethod]
        public void TriangleIsoscele2()
        {
            Polygon3d p1 = new Polygon3d()
            {
                new Point3d(0, 0, 0),
                new Point3d(0, 50, 0),
                new Point3d(100, 50, 0),
            };
            Shape s1 = new Shape(p1);

            var shapes = new List<Shape>();
            shapes.Add(s1);

            Mesh.GenerateOptions options = new Mesh.GenerateOptions();

            options.Algorithm = Mesh.GenerateOptions.MeshAlgorithm.MeshAdapt;
            options.Recombine = true;
            options.RecombinationAlgorithm = Mesh.GenerateOptions.RecombinationMeshAlgorithm.Simple;
            options.MeshSize = 200;
            options.UseGlobalProgressID = true;
            options.RecombineOptimizeTopology = 5;
            options.HealShapes = false;
            options.Optimize = false;
            options.OptimizeIteration = 1;
            options.OptimizeAlgorithm = Mesh.GenerateOptions.MeshOptimize.Netgen;
            options.OptimizeNetgen = 1;
            options.Transfinite = true;
            options.MinQuality = 0.90;
            options.Refine = false;
            options.Smoothing = 10;

            Mesh.Generate(shapes, null, null, options, out List<Mesh> meshes, out Mesh.GenerateMeshStatus generateMeshStatus);
            MeshExport.ExportToMshFormatv2(Path.ChangeExtension(Path.Combine(_outputFolder, TestContext.TestName), ".msh"), meshes);
            CommonParametrizationAssert(meshes, shapes);
        }

        [TestMethod]
        public void TriangleIsoscele3()
        {
            Polygon3d p1 = new Polygon3d()
            {
                new Point3d(0, 0, 0),
                new Point3d(0, 50, 0),
                new Point3d(200, 50, 0),
            };
            Shape s1 = new Shape(p1);

            var shapes = new List<Shape>();
            shapes.Add(s1);

            Mesh.GenerateOptions options = new Mesh.GenerateOptions();

            options.Algorithm = Mesh.GenerateOptions.MeshAlgorithm.MeshAdapt;
            options.Recombine = true;
            options.RecombinationAlgorithm = Mesh.GenerateOptions.RecombinationMeshAlgorithm.Simple;
            options.MeshSize = 200;
            options.UseGlobalProgressID = true;
            options.RecombineOptimizeTopology = 5;
            options.HealShapes = false;
            options.Optimize = false;
            options.OptimizeIteration = 1;
            options.OptimizeAlgorithm = Mesh.GenerateOptions.MeshOptimize.Netgen;
            options.OptimizeNetgen = 1;
            options.Transfinite = true;
            options.MinQuality = 0.90;
            options.Refine = false;
            options.Smoothing = 10;

            Mesh.Generate(shapes, null, null, options, out List<Mesh> meshes, out Mesh.GenerateMeshStatus generateMeshStatus);
            MeshExport.ExportToMshFormatv2(Path.ChangeExtension(Path.Combine(_outputFolder, TestContext.TestName), ".msh"), meshes);
            CommonParametrizationAssert(meshes, shapes);
        }

        [TestMethod]
        public void Quad1()
        {
            Polygon3d p1 = new Polygon3d()
            {
                new Point3d(0, 0, 0),
                new Point3d(0, 100, 0),
                new Point3d(100, 100, 0),
                new Point3d(100, 0, 0)
            };
            Shape s1 = new Shape(p1);

            var shapes = new List<Shape>();
            shapes.Add(s1);

            Mesh.GenerateOptions options = new Mesh.GenerateOptions();

            options.Algorithm = Mesh.GenerateOptions.MeshAlgorithm.MeshAdapt;
            options.Recombine = false;
            options.RecombinationAlgorithm = Mesh.GenerateOptions.RecombinationMeshAlgorithm.Simple;
            options.MeshSize = 1000;
            options.MeshSizeMin = 1000;
            options.UseGlobalProgressID = true;
            options.HealShapes = false;
            options.Optimize = false;
            options.Transfinite = false;
            options.Refine = false;

            Mesh.Generate(shapes, null, null, options, out List<Mesh> meshes, out Mesh.GenerateMeshStatus generateMeshStatus);
            MeshExport.ExportToMshFormatv2(Path.ChangeExtension(Path.Combine(_outputFolder, TestContext.TestName), ".msh"), meshes);
            CommonParametrizationAssert(meshes, shapes);
        }

        [TestMethod]
        public void Rectangle0()
        {
            Polygon3d p1 = new Polygon3d()
            {
                new Point3d(0, 0, 0),
                new Point3d(0, 120, 0),
                new Point3d(100, 120, 0),
                new Point3d(100, 0, 0)
            };
            Shape s1 = new Shape(p1);

            var shapes = new List<Shape>();
            shapes.Add(s1);

            Mesh.GenerateOptions options = new Mesh.GenerateOptions();

            options.Algorithm = Mesh.GenerateOptions.MeshAlgorithm.MeshAdapt;
            options.Recombine = false;
            options.RecombinationAlgorithm = Mesh.GenerateOptions.RecombinationMeshAlgorithm.Simple;
            options.MeshSize = 200;
            options.UseGlobalProgressID = true;
            options.RecombineOptimizeTopology = 5;
            options.HealShapes = false;
            options.Optimize = false;
            options.OptimizeIteration = 1;
            options.OptimizeAlgorithm = Mesh.GenerateOptions.MeshOptimize.Netgen;
            options.OptimizeNetgen = 1;
            options.Transfinite = false;
            options.MinQuality = 0.90;
            options.Refine = false;
            options.Smoothing = 10;

            Mesh.Generate(shapes, null, null, options, out List<Mesh> meshes, out Mesh.GenerateMeshStatus generateMeshStatus);
            MeshExport.ExportToMshFormatv2(Path.ChangeExtension(Path.Combine(_outputFolder, TestContext.TestName), ".msh"), meshes);
            CommonParametrizationAssert(meshes, shapes);
        }

        [TestMethod]
        public void Rectangle1()
        {
            Polygon3d p1 = new Polygon3d()
            {
                new Point3d(0, 0, 0),
                new Point3d(0, 200, 0),
                new Point3d(100, 200, 0),
                new Point3d(100, 0, 0)
            };
            Shape s1 = new Shape(p1);

            var shapes = new List<Shape>();
            shapes.Add(s1);

            Mesh.GenerateOptions options = new Mesh.GenerateOptions();

            options.Algorithm = Mesh.GenerateOptions.MeshAlgorithm.MeshAdapt;
            options.Recombine = false;
            options.RecombinationAlgorithm = Mesh.GenerateOptions.RecombinationMeshAlgorithm.Simple;
            options.MeshSize = 200;
            options.UseGlobalProgressID = true;
            options.RecombineOptimizeTopology = 5;
            options.HealShapes = false;
            options.Optimize = false;
            options.OptimizeIteration = 1;
            options.OptimizeAlgorithm = Mesh.GenerateOptions.MeshOptimize.Netgen;
            options.OptimizeNetgen = 1;
            options.Transfinite = false;
            options.MinQuality = 0.90;
            options.Refine = false;
            options.Smoothing = 10;

            Mesh.Generate(shapes, null, null, options, out List<Mesh> meshes, out Mesh.GenerateMeshStatus generateMeshStatus);
            MeshExport.ExportToMshFormatv2(Path.ChangeExtension(Path.Combine(_outputFolder, TestContext.TestName), ".msh"), meshes);
            CommonParametrizationAssert(meshes, shapes);
        }

        [TestMethod]
        public void Rectangle2()
        {
            Polygon3d p1 = new Polygon3d()
            {
                new Point3d(0, 0, 0),
                new Point3d(0, 400, 0),
                new Point3d(100, 400, 0),
                new Point3d(100, 0, 0)
            };
            Shape s1 = new Shape(p1);

            var shapes = new List<Shape>();
            shapes.Add(s1);

            Mesh.GenerateOptions options = new Mesh.GenerateOptions();

            options.Algorithm = Mesh.GenerateOptions.MeshAlgorithm.MeshAdapt;
            options.Recombine = false;
            options.RecombinationAlgorithm = Mesh.GenerateOptions.RecombinationMeshAlgorithm.Simple;
            options.MeshSize = 400;
            options.UseGlobalProgressID = true;
            options.RecombineOptimizeTopology = 5;
            options.HealShapes = false;
            options.Optimize = false;
            options.OptimizeIteration = 1;
            options.OptimizeAlgorithm = Mesh.GenerateOptions.MeshOptimize.Netgen;
            options.OptimizeNetgen = 1;
            options.Transfinite = false;
            options.MinQuality = 0.90;
            options.Refine = false;
            options.Smoothing = 10;

            Mesh.Generate(shapes, null, null, options, out List<Mesh> meshes, out Mesh.GenerateMeshStatus generateMeshStatus);
            MeshExport.ExportToMshFormatv2(Path.ChangeExtension(Path.Combine(_outputFolder, TestContext.TestName), ".msh"), meshes);
            CommonParametrizationAssert(meshes, shapes);
        }

        [TestMethod]
        public void Rhombus1()
        {
            Polygon3d p1 = new Polygon3d()
            {
                new Point3d(50, 0, 0),
                new Point3d(100, 200, 0),
                new Point3d(50, 400, 0),
                new Point3d(0, 200, 0)
            };
            Shape s1 = new Shape(p1);

            var shapes = new List<Shape>();
            shapes.Add(s1);

            Mesh.GenerateOptions options = new Mesh.GenerateOptions();

            options.Algorithm = Mesh.GenerateOptions.MeshAlgorithm.MeshAdapt;
            options.Recombine = false;
            options.RecombinationAlgorithm = Mesh.GenerateOptions.RecombinationMeshAlgorithm.Simple;
            options.MeshSize = 400;
            options.UseGlobalProgressID = true;
            options.RecombineOptimizeTopology = 5;
            options.HealShapes = false;
            options.Optimize = false;
            options.OptimizeIteration = 1;
            options.OptimizeAlgorithm = Mesh.GenerateOptions.MeshOptimize.Netgen;
            options.OptimizeNetgen = 1;
            options.Transfinite = false;
            options.MinQuality = 0.90;
            options.Refine = false;
            options.Smoothing = 10;

            Mesh.Generate(shapes, null, null, options, out List<Mesh> meshes, out Mesh.GenerateMeshStatus generateMeshStatus);
            MeshExport.ExportToMshFormatv2(Path.ChangeExtension(Path.Combine(_outputFolder, TestContext.TestName), ".msh"), meshes);
            CommonParametrizationAssert(meshes, shapes);
        }

        [TestMethod]
        public void Rhombus2()
        {
            Polygon3d p1 = new Polygon3d()
            {
                new Point3d(50, 180, 0),
                new Point3d(100, 200, 0),
                new Point3d(50, 220, 0),
                new Point3d(0, 200, 0)
            };
            Shape s1 = new Shape(p1);

            var shapes = new List<Shape>();
            shapes.Add(s1);

            Mesh.GenerateOptions options = new Mesh.GenerateOptions();

            options.Algorithm = Mesh.GenerateOptions.MeshAlgorithm.MeshAdapt;
            options.Recombine = false;
            options.RecombinationAlgorithm = Mesh.GenerateOptions.RecombinationMeshAlgorithm.Simple;
            options.MeshSize = 400;
            options.UseGlobalProgressID = true;
            options.RecombineOptimizeTopology = 5;
            options.HealShapes = false;
            options.Optimize = false;
            options.OptimizeIteration = 1;
            options.OptimizeAlgorithm = Mesh.GenerateOptions.MeshOptimize.Netgen;
            options.OptimizeNetgen = 1;
            options.Transfinite = false;
            options.MinQuality = 0.90;
            options.Refine = false;
            options.Smoothing = 10;

            Mesh.Generate(shapes, null, null, options, out List<Mesh> meshes, out Mesh.GenerateMeshStatus generateMeshStatus);
            MeshExport.ExportToMshFormatv2(Path.ChangeExtension(Path.Combine(_outputFolder, TestContext.TestName), ".msh"), meshes);
            CommonParametrizationAssert(meshes, shapes);
        }

        [TestMethod]
        public void Pentagon1()
        {
            Polygon3d p1 = new Polygon3d()
            {
                new Point3d(0, 0, 0),
                new Point3d(100, 0, 0),
                new Point3d(100, 100, 0),
                new Point3d(50, 150, 0),
                new Point3d(0, 100, 0)
            };
            Shape s1 = new Shape(p1);

            var shapes = new List<Shape>();
            shapes.Add(s1);

            Mesh.GenerateOptions options = new Mesh.GenerateOptions();

            options.Algorithm = Mesh.GenerateOptions.MeshAlgorithm.MeshAdapt;
            options.Recombine = false;
            options.RecombinationAlgorithm = Mesh.GenerateOptions.RecombinationMeshAlgorithm.Simple;
            options.MeshSize = 1000;
            options.UseGlobalProgressID = true;
            options.RecombineOptimizeTopology = 5;
            options.HealShapes = false;
            options.Optimize = false;
            options.OptimizeIteration = 1;
            options.OptimizeAlgorithm = Mesh.GenerateOptions.MeshOptimize.Netgen;
            options.OptimizeNetgen = 1;
            options.Transfinite = false;
            options.MinQuality = 0.90;
            options.Refine = false;
            options.Smoothing = 10;

            Mesh.Generate(shapes, null, null, options, out List<Mesh> meshes, out Mesh.GenerateMeshStatus generateMeshStatus);
            MeshExport.ExportToMshFormatv2(Path.ChangeExtension(Path.Combine(_outputFolder, TestContext.TestName), ".msh"), meshes);
            CommonParametrizationAssert(meshes, shapes);
        }

        [TestMethod]
        public void Pentagon2()
        {
            Polygon3d p1 = new Polygon3d()
            {
                new Point3d(0, 0, 0),
                new Point3d(100, 20, 0),
                new Point3d(100, 100, 0),
                new Point3d(50, 150, 0),
                new Point3d(20, 100, 0)
            };
            Shape s1 = new Shape(p1);

            var shapes = new List<Shape>();
            shapes.Add(s1);

            Mesh.GenerateOptions options = new Mesh.GenerateOptions();

            options.Algorithm = Mesh.GenerateOptions.MeshAlgorithm.MeshAdapt;
            options.Recombine = false;
            options.RecombinationAlgorithm = Mesh.GenerateOptions.RecombinationMeshAlgorithm.Simple;
            options.MeshSize = 1000;
            options.UseGlobalProgressID = true;
            options.RecombineOptimizeTopology = 5;
            options.HealShapes = false;
            options.Optimize = false;
            options.Transfinite = false;
            options.Refine = false;

            Mesh.Generate(shapes, null, null, options, out List<Mesh> meshes, out Mesh.GenerateMeshStatus generateMeshStatus);
            MeshExport.ExportToMshFormatv2(Path.ChangeExtension(Path.Combine(_outputFolder, TestContext.TestName), ".msh"), meshes);
            CommonParametrizationAssert(meshes, shapes);
        }

        [TestMethod]
        public void Pentagon3()
        {
            Polygon3d p1 = new Polygon3d()
            {
                new Point3d(0, 0, 0),
                new Point3d(120, 20, 0),
                new Point3d(160, 100, 0),
                new Point3d(50, 150, 0),
                new Point3d(-20, 100, 0)
            };
            Shape s1 = new Shape(p1);

            var shapes = new List<Shape>();
            shapes.Add(s1);

            Mesh.GenerateOptions options = new Mesh.GenerateOptions();

            options.Algorithm = Mesh.GenerateOptions.MeshAlgorithm.MeshAdapt;
            options.Recombine = false;
            options.RecombinationAlgorithm = Mesh.GenerateOptions.RecombinationMeshAlgorithm.Simple;
            options.MeshSize = 1000;
            options.UseGlobalProgressID = true;
            options.RecombineOptimizeTopology = 5;
            options.HealShapes = false;
            options.Optimize = false;
            options.Transfinite = false;
            options.Refine = false;

            Mesh.Generate(shapes, null, null, options, out List<Mesh> meshes, out Mesh.GenerateMeshStatus generateMeshStatus);
            MeshExport.ExportToMshFormatv2(Path.ChangeExtension(Path.Combine(_outputFolder, TestContext.TestName), ".msh"), meshes);
            CommonParametrizationAssert(meshes, shapes);
        }

        [TestMethod]
        public void Pentagon4()
        {
            Polygon3d p1 = new Polygon3d()
            {
                new Point3d(0, 0, 0),
                new Point3d(120, 20, 0),
                new Point3d(160, 100, 0),
                new Point3d(50, 150, 0),
                new Point3d(-20, 100, 0)
            };
            Shape s1 = new Shape(p1);

            var shapes = new List<Shape>();
            shapes.Add(s1);

            Mesh.GenerateOptions options = new Mesh.GenerateOptions();

            options.Algorithm = Mesh.GenerateOptions.MeshAlgorithm.MeshAdapt;
            options.Recombine = false;
            options.RecombinationAlgorithm = Mesh.GenerateOptions.RecombinationMeshAlgorithm.Simple;
            options.MeshSize = 1000;
            options.UseGlobalProgressID = true;
            options.RecombineOptimizeTopology = 5;
            options.HealShapes = false;
            options.Optimize = false;
            options.Transfinite = false;
            options.Refine = false;

            Mesh.Generate(shapes, null, null, options, out List<Mesh> meshes, out Mesh.GenerateMeshStatus generateMeshStatus);
            MeshExport.ExportToMshFormatv2(Path.ChangeExtension(Path.Combine(_outputFolder, TestContext.TestName), ".msh"), meshes);
            CommonParametrizationAssert(meshes, shapes);
        }

        [TestMethod]
        public void Exagon1()
        {
            Polygon3d p1 = new Polygon3d()
            {
                new Point3d(0, 0, 0),
                new Point3d(120, 20, 0),
                new Point3d(160, 100, 0),
                new Point3d(50, 150, 0),
                new Point3d(-20, 100, 0),
                new Point3d(-20, 20, 0)
            };
            Shape s1 = new Shape(p1);

            var shapes = new List<Shape>();
            shapes.Add(s1);

            Mesh.GenerateOptions options = new Mesh.GenerateOptions();

            options.Algorithm = Mesh.GenerateOptions.MeshAlgorithm.MeshAdapt;
            options.Recombine = false;
            options.RecombinationAlgorithm = Mesh.GenerateOptions.RecombinationMeshAlgorithm.Simple;
            options.MeshSize = 1000;
            options.MeshSizeMin = 1000;
            options.UseGlobalProgressID = true;
            options.RecombineOptimizeTopology = 5;
            options.HealShapes = false;
            options.Optimize = false;
            options.Transfinite = false;
            options.Refine = false;

            Mesh.Generate(shapes, null, null, options, out List<Mesh> meshes, out Mesh.GenerateMeshStatus generateMeshStatus);
            MeshExport.ExportToMshFormatv2(Path.ChangeExtension(Path.Combine(_outputFolder, TestContext.TestName), ".msh"), meshes);
            CommonParametrizationAssert(meshes, shapes);
        }

        [TestMethod]
        public void Exagon2()
        {
            Polygon3d p1 = new Polygon3d()
            {
                new Point3d(0, 0, 0),
                new Point3d(50, 0, 0),
                new Point3d(50, 200, 0),
                new Point3d(100, 200, 0),
                new Point3d(50, 300, 0),
                new Point3d(0, 200, 0)
            };
            Shape s1 = new Shape(p1);

            var shapes = new List<Shape>();
            shapes.Add(s1);

            Mesh.GenerateOptions options = new Mesh.GenerateOptions();

            options.Algorithm = Mesh.GenerateOptions.MeshAlgorithm.MeshAdapt;
            options.Recombine = false;
            options.RecombinationAlgorithm = Mesh.GenerateOptions.RecombinationMeshAlgorithm.Simple;
            options.MeshSize = 1000;
            options.UseGlobalProgressID = true;
            options.RecombineOptimizeTopology = 5;
            options.HealShapes = false;
            options.Optimize = false;
            options.Transfinite = false;
            options.Refine = false;

            Mesh.Generate(shapes, null, null, options, out List<Mesh> meshes, out Mesh.GenerateMeshStatus generateMeshStatus);
            MeshExport.ExportToMshFormatv2(Path.ChangeExtension(Path.Combine(_outputFolder, TestContext.TestName), ".msh"), meshes);
            CommonParametrizationAssert(meshes, shapes);
        }

        [TestMethod]
        public void Exagon3()
        {
            Polygon3d p1 = new Polygon3d()
            {
                new Point3d(0, 50, 0),
                new Point3d(50, 0, 0),
                new Point3d(50, 200, 0),
                new Point3d(80, 200, 0),
                new Point3d(50, 220, 0),
                new Point3d(20, 200, 0)
            };
            Shape s1 = new Shape(p1);

            var shapes = new List<Shape>();
            shapes.Add(s1);

            Mesh.GenerateOptions options = new Mesh.GenerateOptions();

            options.Algorithm = Mesh.GenerateOptions.MeshAlgorithm.MeshAdapt;
            options.Recombine = false;
            options.RecombinationAlgorithm = Mesh.GenerateOptions.RecombinationMeshAlgorithm.Simple;
            options.MeshSize = 1000;
            options.UseGlobalProgressID = true;
            options.RecombineOptimizeTopology = 5;
            options.HealShapes = false;
            options.Optimize = false;
            options.Transfinite = false;
            options.Refine = false;

            Mesh.Generate(shapes, null, null, options, out List<Mesh> meshes, out Mesh.GenerateMeshStatus generateMeshStatus);
            MeshExport.ExportToMshFormatv2(Path.ChangeExtension(Path.Combine(_outputFolder, TestContext.TestName), ".msh"), meshes);
            CommonParametrizationAssert(meshes, shapes);
        }

        [TestMethod]
        public void Exagon4()
        {
            Polygon3d p1 = new Polygon3d()
            {
                new Point3d(0, 50, 0),
                new Point3d(0, 100, 0),
                new Point3d(50, 150, 0),
                new Point3d(100, 100, 0),
                new Point3d(100, 50, 0),
                new Point3d(50, 0, 0)
            };
            Shape s1 = new Shape(p1);

            var shapes = new List<Shape>();
            shapes.Add(s1);

            Mesh.GenerateOptions options = new Mesh.GenerateOptions();

            options.Algorithm = Mesh.GenerateOptions.MeshAlgorithm.MeshAdapt;
            options.Recombine = false;
            options.RecombinationAlgorithm = Mesh.GenerateOptions.RecombinationMeshAlgorithm.Simple;
            options.MeshSize = 1000;
            options.MeshSizeMin = 1000;
            options.UseGlobalProgressID = true;
            options.RecombineOptimizeTopology = 5;
            options.HealShapes = false;
            options.Optimize = false;
            options.Transfinite = false;
            options.Refine = false;

            Mesh.Generate(shapes, null, null, options, out List<Mesh> meshes, out Mesh.GenerateMeshStatus generateMeshStatus);
            MeshExport.ExportToMshFormatv2(Path.ChangeExtension(Path.Combine(_outputFolder, TestContext.TestName), ".msh"), meshes);
            CommonParametrizationAssert(meshes, shapes);
        }

        [TestMethod]
        public void Eptagon1()
        {
            Polygon3d p1 = new Polygon3d()
            {
                new Point3d(0, 50, 0),
                new Point3d(0, 100, 0),
                new Point3d(50, 150, 0),
                new Point3d(100, 100, 0),
                new Point3d(100, 50, 0),
                new Point3d(50, 0, 0),
                new Point3d(0, 0, 0)
            };
            Shape s1 = new Shape(p1);

            var shapes = new List<Shape>();
            shapes.Add(s1);

            Mesh.GenerateOptions options = new Mesh.GenerateOptions();

            options.Algorithm = Mesh.GenerateOptions.MeshAlgorithm.MeshAdapt;
            options.Recombine = false;
            options.RecombinationAlgorithm = Mesh.GenerateOptions.RecombinationMeshAlgorithm.Simple;
            options.MeshSize = 1000;
            options.MeshSizeMin = 1000;
            options.UseGlobalProgressID = true;
            options.RecombineOptimizeTopology = 5;
            options.HealShapes = false;
            options.Optimize = false;
            options.Transfinite = false;
            options.Refine = false;

            Mesh.Generate(shapes, null, null, options, out List<Mesh> meshes, out Mesh.GenerateMeshStatus generateMeshStatus);
            MeshExport.ExportToMshFormatv2(Path.ChangeExtension(Path.Combine(_outputFolder, TestContext.TestName), ".msh"), meshes);
            CommonParametrizationAssert(meshes, shapes);
        }

        [TestMethod]
        public void Ottagono1()
        {
            Polygon3d p1 = new Polygon3d()
            {
                new Point3d(100, 0, 0),
                new Point3d(70, 70, 0),
                new Point3d(0, 100, 0),
                new Point3d(-70, 70, 0),
                new Point3d(-100, 0, 0),
                new Point3d(-70, -70, 0),
                new Point3d(0, -100, 0),
                new Point3d(70, -70, 0)
            };
            Shape s1 = new Shape(p1);

            var shapes = new List<Shape>();
            shapes.Add(s1);

            Mesh.GenerateOptions options = new Mesh.GenerateOptions();

            options.Algorithm = Mesh.GenerateOptions.MeshAlgorithm.MeshAdapt;
            options.Recombine = false;
            options.RecombinationAlgorithm = Mesh.GenerateOptions.RecombinationMeshAlgorithm.Simple;
            options.MeshSize = 1000;
            options.MeshSizeMin = 1000;
            options.UseGlobalProgressID = true;
            options.RecombineOptimizeTopology = 5;
            options.HealShapes = false;
            options.Optimize = false;
            options.Transfinite = false;
            options.Refine = false;

            Mesh.Generate(shapes, null, null, options, out List<Mesh> meshes, out Mesh.GenerateMeshStatus generateMeshStatus);
            MeshExport.ExportToMshFormatv2(Path.ChangeExtension(Path.Combine(_outputFolder, TestContext.TestName), ".msh"), meshes);
            CommonParametrizationAssert(meshes, shapes);
        }
    }*/
}
