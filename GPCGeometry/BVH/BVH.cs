using System;
using System.Collections.Generic;
using System.Linq;

namespace GPC.Geometry.BVH
{
    /// <summary>
    /// Axis Aligned Bounding Box implementation of Bounding Volume Hierarchy
    /// </summary>
    public class AABBBVH
    {
        /// <summary>
        /// Root node of the hierarchy
        /// </summary>
        private AABBNode _root;

        /// <summary>
        /// Axis Aligned Bounding Box Class
        /// </summary>
        private class AABB
        {
            public Point3d Min;
            public Point3d Max;

            public AABB()
            {
                Min = new Point3d(double.MaxValue, double.MaxValue, double.MaxValue);
                Max = new Point3d(-double.MaxValue, -double.MaxValue, -double.MaxValue);
            }

            public AABB(Point3d min, Point3d max)
            {
                Min = min; Max = max;
            }

            public Point3d GetCenter()
            {
                return new Point3d(
                    (Min.X + Max.X) * 0.5,
                    (Min.Y + Max.Y) * 0.5,
                    (Min.Z + Max.Z) * 0.5
                    );
            }

            public bool Contains(Point3d point)
            {
                return point.X >= Min.X && point.Y >= Min.Y && point.Z >= Min.Z && point.X <= Max.X && point.Y <= Max.Y && point.Z <= Max.Z;
            }

            public bool Intersects(AABB box)
            {
                if (Min.X > box.Max.X || Min.Y > box.Max.Y || Min.Z > box.Max.Z)
                {
                    return false;
                }
                if (box.Min.X > Max.X || box.Min.Y > Max.Y || box.Min.Z > Max.Z)
                {
                    return false;
                }
                return true;
            }

            public void Union(AABB box)
            {
                this.Min.X = Math.Min(this.Min.X, box.Min.X);
                this.Min.Y = Math.Min(this.Min.Y, box.Min.Y);
                this.Min.Z = Math.Min(this.Min.Z, box.Min.Z);
                this.Max.X = Math.Max(this.Max.X, box.Max.X);
                this.Max.Y = Math.Max(this.Max.Y, box.Max.Y);
                this.Max.Z = Math.Max(this.Max.Z, box.Max.Z);
            }

            public void SetVolume(IEnumerable<AABB> boxes)
            {
                for (int i = 0; i < boxes.Count(); ++i)
                {
                    Union(boxes.ElementAt(i));
                }
            }
        }

        /// <summary>
        /// Axis Aligned Bounding Box Node
        /// </summary>
        private class AABBNode
        {
            public int Id;
            public AABB Box;
            public List<AABBNode> Children;

            public AABBNode()
            {
                Id = -1;
                Box = new AABB();
                Children = new List<AABBNode>();
            }

            public AABBNode(Point3d coord, double size)
            {
                Box.Min = new Point3d(coord.X - size, coord.Y - size, coord.Z - size);
                Box.Max = new Point3d(coord.X + size, coord.Y + size, coord.Z + size);
            }

            public AABB[] HalveVolume()
            {
                var sizes = new double[3] {
                    Box.Max.X - Box.Min.X,
                    Box.Max.Y - Box.Min.Y,
                    Box.Max.Z - Box.Min.Z
                };

                if (sizes[0] > sizes[1] && sizes[0] > sizes[2] && sizes[0] > 0)
                {
                    double half = (Box.Min.X + Box.Max.X) * 0.5;
                    return (new AABB[2] {
                        new AABB(Box.Min, new Point3d(half, Box.Max.Y, Box.Max.Z)),
                        new AABB(new Point3d(half, Box.Min.Y, Box.Min.Z), Box.Max)
                    });
                }
                else if (sizes[1] > sizes[2] && sizes[1] > 0)
                {
                    double half = (Box.Min.Y + Box.Max.Y) * 0.5;
                    return (new AABB[2] {
                        new AABB(Box.Min, new Point3d(Box.Max.X, half, Box.Max.Z)),
                        new AABB(new Point3d(Box.Min.X, half, Box.Min.Z), Box.Max)
                    });
                }
                else if (sizes[2] > 0)
                {
                    double half = (Box.Min.Z + Box.Max.Z) * 0.5;
                    return (new AABB[2] {
                        new AABB(Box.Min, new Point3d(Box.Max.X, Box.Max.Y, half)),
                        new AABB(new Point3d(Box.Min.X, Box.Min.Y, half), Box.Max)
                    });
                }

                return new AABB[1] { Box };
            }

            public void SetVolume(IEnumerable<AABBNode> children)
            {
                for (int i = 0; i < children.Count(); i++)
                {
                    Box.Union(children.ElementAt(i).Box);
                }
            }

        }

        /// <summary>
        /// Constructor of an Axis Aligned Bounding Box Bounding Volulme Hierarchy used to search points
        /// </summary>
        /// <param name="points"> The points used as nodes in the hierarchy</param>
        /// <param name="ids"> The ids of the points</param>
        public AABBBVH(IEnumerable<Point3d> points, IEnumerable<int> ids)
        {
            _root = new AABBNode();

            var count = Math.Min(points.Count(), ids.Count());

            AABBNode[] nodes = new AABBNode[count];
            for (int i = 0; i < count; i++)
            {
                nodes[i] = new AABBNode();
                nodes[i].Id = ids.ElementAt(i);
                nodes[i].Box.Min = points.ElementAt(i);
                nodes[i].Box.Max = points.ElementAt(i);
            }

            BuildRecursion(ref _root, nodes);
        }

        /// <summary>
        /// Recursion method to build the BVH
        /// </summary>
        /// <param name="node"> Current recursion node</param>
        /// <param name="toAdd"> Nodes to add to current node</param>
        /// <returns>true if it is a leaf node, false otherwise</returns>
        private static bool BuildRecursion(ref AABBNode node, IEnumerable<AABBNode> toAdd)
        {
            node.SetVolume(toAdd);

            if (toAdd.Count() < 3)
            {
                node.Children.AddRange(toAdd);
                return true;
            }

            var halves = node.HalveVolume();

            if (halves.Count() == 1)
            {
                node.Children.AddRange(toAdd);
                return true;
            }

            var left = new List<AABBNode>();
            var right = new List<AABBNode>();

            for (int i = 0; i < toAdd.Count(); i++)
            {
                var element = toAdd.ElementAt(i);
                var center = element.Box.GetCenter();
                if (halves[0].Contains(center))
                {
                    left.Add(element);
                }
                else
                {
                    right.Add(element);
                }
            }

            var leftNode = new AABBNode();
            var rightNode = new AABBNode();

            BuildRecursion(ref leftNode, left);
            BuildRecursion(ref rightNode, right);

            node.Children.AddRange(new AABBNode[2] {
                leftNode,
                rightNode
            });

            return false;
        }

        /// <summary>
        /// Search intersections with a point in a given radius
        /// </summary>
        /// <param name="point"> Origin point where to search </param>
        /// <param name="radius"> Radius of the search </param>
        /// <returns>Only leaf nodes ids within the radius</returns>
        public List<int> GetIntersections(Point3d point, double radius)
        {
            var fullRadius = new Point3d(1, 1, 1) * (radius * Math.Sqrt(2));
            var box = new AABB(point - fullRadius, point + fullRadius);

            var intersectingNodes = new List<AABBNode>();
            GetIntersectionsRecursion(box, _root, ref intersectingNodes);

            var ids = new List<int>();
            for (int i = 0; i < intersectingNodes.Count; ++i)
            {
                var node = intersectingNodes[i];
                if (node.Children.Count == 0 && node.Box.GetCenter().DistanceTo(point) <= radius)
                {
                    ids.Add(node.Id);
                }
            }

            return ids;
        }

        /// <summary>
        /// Search intersections recursion
        /// </summary>
        /// <param name="box"> The box used to search </param>
        /// <param name="currentNode"> Current recursion node </param>
        /// <param name="nodes"> List of the resulting nodes. Maximum depth nodes are returned for each intersection.</param>
        /// <returns>true if there is an intersection, false otherwise</returns>
        private bool GetIntersectionsRecursion(AABB box, AABBNode currentNode, ref List<AABBNode> nodes)
        {
            if (!currentNode.Box.Intersects(box))
            {
                return false;
            }

            bool betterBounds = false;
            for (int i = 0; i < currentNode.Children.Count; i++)
            {
                betterBounds = GetIntersectionsRecursion(box, currentNode.Children[i], ref nodes) || betterBounds;
            }

            if (!betterBounds)
            {
                nodes.Add(currentNode);
            }

            return true;
        }

    }


    public class SphereBVH
    {
        /// <summary>
        /// Root node of the hierarchy
        /// </summary>
        private SphereNode _root;

        /// <summary>
        /// A Sphere
        /// </summary>
        private class Sphere
        {
            public Point3d Coord;
            public double Radius;

            public Sphere()
            {
                Coord = new Point3d(0, 0, 0);
                Radius = 0;
            }

            public Sphere(Point3d coord, double radius)
            {
                Coord = coord;
                Radius = radius;
            }

            public bool Contains(Point3d point)
            {
                return Coord.DistanceTo(point) <= Radius;
            }

            public bool Intersects(Sphere sphere)
            {
                return Coord.DistanceTo(sphere.Coord) <= Radius + sphere.Radius;
            }

            public bool RayIntersects(Ray3d ray)
            {
                var oc = new Vector3d(ray.Point - Coord);
                var a = ray.Direction.DotProduct(ray.Direction);
                var b = 2.0 * oc.DotProduct(ray.Direction);
                var c = oc.DotProduct(oc) - Radius * Radius;
                var discriminant = b * b - 4 * a * c;
                return discriminant >= 0;
            }

            public void Union(Sphere sphere)
            {
                var dist = Coord.DistanceTo(sphere.Coord);
                if (Radius >= dist + sphere.Radius)
                {
                    return; // The other sphere is contained
                }
                else if (sphere.Radius >= dist + Radius)
                {
                    Coord = sphere.Coord;
                    Radius = sphere.Radius;
                    return; // The other sphere contains this one
                }
                else
                {
                    var vec = new Vector3d(sphere.Coord - Coord);
                    vec.Unitize();
                    vec = vec * (sphere.Radius + dist - Radius) * 0.5;
                    Coord = Coord + vec;
                    Radius = (Radius + dist + sphere.Radius) * 0.5;
                }
            }
            public void SetVolume(IEnumerable<Sphere> spheres)
            {
                for (int i = 0; i < spheres.Count(); ++i)
                {
                    Coord += spheres.ElementAt(i).Coord;
                }

                Coord = Coord / spheres.Count();

                for (int i = 0; i < spheres.Count(); ++i)
                {
                    var sphere = spheres.ElementAt(i);
                    var r = Coord.DistanceTo(sphere.Coord) + sphere.Radius;
                    if (r > Radius)
                    {
                        Radius = r;
                    }
                }

            }
        }

        /// <summary>
        /// A Sphere Node
        /// </summary>
        private class SphereNode
        {
            public int Id;
            public Sphere Sphere;
            public List<SphereNode> Children;

            public SphereNode()
            {
                Id = -1;
                Sphere = new Sphere();
                Children = new List<SphereNode>();
            }

            public SphereNode(Point3d coord, double size)
            {
                Sphere.Coord = coord;
                Sphere.Radius = size;
            }

            public void SetVolume(IEnumerable<SphereNode> children)
            {
                for (int i = 0; i < children.Count(); ++i)
                {
                    Sphere.Coord = Sphere.Coord + children.ElementAt(i).Sphere.Coord;
                }
                Sphere.Coord = Sphere.Coord / children.Count();

                for (int i = 0; i < children.Count(); ++i)
                {
                    var sphere = children.ElementAt(i).Sphere;
                    var r = Sphere.Coord.DistanceTo(sphere.Coord) + sphere.Radius;
                    if (r > Sphere.Radius)
                    {
                        Sphere.Radius = r;
                    }
                }
            }
        }

        /// <summary>
        /// Constructor of a Sphere Bounding Volulme Hierarchy used to search points
        /// </summary>
        /// <param name="points"> The points used as nodes in the hierarchy</param>
        /// <param name="ids"> The ids of the points</param>
        public SphereBVH(IEnumerable<Point3d> points, IEnumerable<int> ids)
        {
            _root = new SphereNode();

            var count = Math.Min(points.Count(), ids.Count());

            SphereNode[] nodes = new SphereNode[count];
            for (int i = 0; i < count; i++)
            {
                nodes[i] = new SphereNode();
                nodes[i].Id = ids.ElementAt(i);
                nodes[i].Sphere.Coord = points.ElementAt(i);
                nodes[i].Sphere.Radius = 0;
            }

            BuildRecursion(ref _root, nodes.ToList(), 0);
        }

        /// <summary>
        /// Constructor of a Sphere Bounding Volulme Hierarchy used to search elements
        /// </summary>
        /// <param name="elements"> The elements as list of points defining a volume </param>
        /// <param name="ids"> The ids of the elements </param>
        public SphereBVH(IEnumerable<IEnumerable<Point3d>> elements, IEnumerable<int> ids)
        {
            _root = new SphereNode();

            var count = Math.Min(elements.Count(), ids.Count());

            SphereNode[] nodes = new SphereNode[count];
            for (int i = 0; i < count; i++)
            {
                nodes[i] = new SphereNode();
                nodes[i].Id = ids.ElementAt(i);
                var element = elements.ElementAt(i);
                var pointSpheres = new Sphere[element.Count()];
                for (int j = 0; j < pointSpheres.Length; j++)
                {
                    pointSpheres[j] = new Sphere(element.ElementAt(j), 0);
                }
                nodes[i].Sphere.SetVolume(pointSpheres);
            }

            BuildRecursion(ref _root, nodes.ToList(), 0, 8);
        }


        /// <summary>
        /// Recursion method to build the BVH
        /// </summary>
        /// <param name="node"> Current recursion node</param>
        /// <param name="toAdd"> Nodes to add to current node</param>
        /// <param name="divisions"> number of splits of the nodes to add</param>
        /// <returns>true if it is a leaf node, false otherwise</returns>
        private static bool BuildRecursion(ref SphereNode node, List<SphereNode> toAdd, int axis, int divisions = 8)
        {
            node.SetVolume(toAdd);

            if (toAdd.Count() <= divisions)
            {
                node.Children.AddRange(toAdd);
                return true;
            }

            var nodeCount = toAdd.Count() / divisions;
            var remain = toAdd.Count() % divisions;

            var splitCount = remain == 0 ? divisions : divisions + 1;

            switch (axis)
            {
                case 0:
                    toAdd.Sort((x, y) => { return x.Sphere.Coord.X.CompareTo(y.Sphere.Coord.X); });
                    axis++;
                    break;
                case 1:
                    toAdd.Sort((x, y) => { return x.Sphere.Coord.Y.CompareTo(y.Sphere.Coord.Y); });
                    axis++;
                    break;
                case 2:
                    toAdd.Sort((x, y) => { return x.Sphere.Coord.Z.CompareTo(y.Sphere.Coord.Z); });
                    axis = 0;
                    break;
                default:
                    break;
            }

            for (int i = 0; i < splitCount; ++i)
            {
                var sbset = new List<SphereNode>();
                var count = i == divisions ? remain : nodeCount;

                for (int j = 0; j < count; ++j)
                {
                    sbset.Add(toAdd.ElementAt(i * nodeCount + j));
                }

                var newNode = new SphereNode();
                BuildRecursion(ref newNode, sbset, axis, divisions);
                node.Children.Add(newNode);
            }

            return false;
        }

        /// <summary>
        /// Search intersections with a point in a given radius
        /// </summary>
        /// <param name="point"> Origin point where to search </param>
        /// <param name="radius"> Radius of the search </param>
        /// <returns>Only leaf nodes ids within the radius</returns>
        public List<int> GetIntersections(Point3d point, double radius)
        {
            var sphere = new Sphere(point, radius);

            var intersectingNodes = new List<SphereNode>();
            GetIntersectionsRecursion(sphere, _root, ref intersectingNodes);

            var ids = new List<int>();
            for (int i = 0; i < intersectingNodes.Count; ++i)
            {
                var node = intersectingNodes[i];
                if (node.Children.Count == 0)
                {
                    ids.Add(node.Id);
                }
            }

            return ids;
        }

        /// <summary>
        /// Search intersections recursion
        /// </summary>
        /// <param name="sphere"> The sphere used to search </param>
        /// <param name="currentNode"> Current recursion node </param>
        /// <param name="nodes"> List of the resulting nodes. Maximum depth nodes are returned for each intersection.</param>
        /// <returns>true if there is an intersection, false otherwise</returns>
        private bool GetIntersectionsRecursion(Sphere sphere, SphereNode currentNode, ref List<SphereNode> nodes)
        {
            if (!currentNode.Sphere.Intersects(sphere))
            {
                return false;
            }

            bool betterBounds = false;
            for (int i = 0; i < currentNode.Children.Count; i++)
            {
                betterBounds = GetIntersectionsRecursion(sphere, currentNode.Children[i], ref nodes) || betterBounds;
            }

            if (!betterBounds)
            {
                nodes.Add(currentNode);
            }

            return true;
        }

        /// <summary>
        /// Search intersections with a ray
        /// </summary>
        /// <param name="ray"> The ray that intesects the spheres </param>
        /// <returns>Only leaf nodes ids within the radius</returns>
        public List<int> GetRayIntersections(Ray3d ray)
        {
            var intersectingNodes = new List<SphereNode>();
            GetRayIntersectionsRecursion(ray, _root, ref intersectingNodes);

            var ids = new List<int>();
            for (int i = 0; i < intersectingNodes.Count; ++i)
            {
                var node = intersectingNodes[i];
                if (node.Children.Count == 0)
                {
                    ids.Add(node.Id);
                }
            }

            return ids;
        }

        /// <summary>
        /// Search intersections with ray recursion
        /// </summary>
        /// <param name="ray"> The ray that intesects the spheres </param>
        /// <param name="currentNode"> Current recursion node </param>
        /// <param name="nodes"> List of the resulting nodes. Maximum depth nodes are returned for each intersection.</param>
        /// <returns>true if there is an intersection, false otherwise</returns>
        private bool GetRayIntersectionsRecursion(Ray3d ray, SphereNode currentNode, ref List<SphereNode> nodes)
        {
            if (!currentNode.Sphere.RayIntersects(ray))
            {
                return false;
            }

            bool betterBounds = false;
            for (int i = 0; i < currentNode.Children.Count; i++)
            {
                betterBounds = GetRayIntersectionsRecursion(ray, currentNode.Children[i], ref nodes) || betterBounds;
            }

            if (!betterBounds)
            {
                nodes.Add(currentNode);
            }

            return true;
        }
    }
}
