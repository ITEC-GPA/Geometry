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
            /// <summary>
            /// The minimum corner
            /// </summary>
            public Point3d Min;
            /// <summary>
            /// The maximum corner
            /// </summary>
            public Point3d Max;

            /// <summary>
            /// Creates an empty box (minimum at +MaxValue, maximum at -MaxValue)
            /// </summary>
            public AABB()
            {
                Min = new Point3d(double.MaxValue, double.MaxValue, double.MaxValue);
                Max = new Point3d(-double.MaxValue, -double.MaxValue, -double.MaxValue);
            }

            /// <summary>
            /// Creates a box from its corners (the instances are kept)
            /// </summary>
            /// <param name="min">The minimum corner</param>
            /// <param name="max">The maximum corner</param>
            public AABB(Point3d min, Point3d max)
            {
                Min = min; Max = max;
            }

            /// <summary>
            /// The center of the box
            /// </summary>
            /// <returns>The middle point of the corners</returns>
            public Point3d GetCenter()
            {
                return new Point3d(
                    (Min.X + Max.X) * 0.5,
                    (Min.Y + Max.Y) * 0.5,
                    (Min.Z + Max.Z) * 0.5
                    );
            }

            /// <summary>
            /// Tell if a point is inside the box (border included)
            /// </summary>
            /// <param name="point">The point</param>
            /// <returns>True if the point is inside</returns>
            public bool Contains(Point3d point)
            {
                return point.X >= Min.X && point.Y >= Min.Y && point.Z >= Min.Z && point.X <= Max.X && point.Y <= Max.Y && point.Z <= Max.Z;
            }

            /// <summary>
            /// Tell if two boxes overlap (touching boxes included)
            /// </summary>
            /// <param name="box">The other box</param>
            /// <returns>True if the boxes overlap</returns>
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

            /// <summary>
            /// Enlarges the box to contain another box (the coordinates of the corners are changed in place)
            /// </summary>
            /// <param name="box">The box to contain</param>
            public void Union(AABB box)
            {
                this.Min.X = Math.Min(this.Min.X, box.Min.X);
                this.Min.Y = Math.Min(this.Min.Y, box.Min.Y);
                this.Min.Z = Math.Min(this.Min.Z, box.Min.Z);
                this.Max.X = Math.Max(this.Max.X, box.Max.X);
                this.Max.Y = Math.Max(this.Max.Y, box.Max.Y);
                this.Max.Z = Math.Max(this.Max.Z, box.Max.Z);
            }

            /// <summary>
            /// Enlarges the box to contain other boxes
            /// </summary>
            /// <param name="boxes">The boxes to contain</param>
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
            /// <summary>
            /// The id of the point (leaf nodes); -1 for the internal nodes
            /// </summary>
            public int Id;
            /// <summary>
            /// The box of the node
            /// </summary>
            public AABB Box;
            /// <summary>
            /// The children of the node (empty for the leaves)
            /// </summary>
            public List<AABBNode> Children;

            /// <summary>
            /// Creates an internal node with an empty box
            /// </summary>
            public AABBNode()
            {
                Id = -1;
                Box = new AABB();
                Children = new List<AABBNode>();
            }

            /// <summary>
            /// Creates a node with a cube around a point. Not used: <c>Box</c> is null, so it throws <see cref="NullReferenceException"/>
            /// </summary>
            /// <param name="coord">The center of the cube</param>
            /// <param name="size">Half the side of the cube</param>
            public AABBNode(Point3d coord, double size)
            {
                Box.Min = new Point3d(coord.X - size, coord.Y - size, coord.Z - size);
                Box.Max = new Point3d(coord.X + size, coord.Y + size, coord.Z + size);
            }

            /// <summary>
            /// Splits the box in two halves across its longest side
            /// </summary>
            /// <returns>The two halves; the box itself if it has zero size</returns>
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

            /// <summary>
            /// Enlarges the box of the node to contain the boxes of the children
            /// </summary>
            /// <param name="children">The children</param>
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
        /// <param name="points">The points used as nodes in the hierarchy (the instances are kept)</param>
        /// <param name="ids">The ids of the points; if the counts are different, the extra points or ids are ignored</param>
        public AABBBVH(IEnumerable<Point3d> points, IEnumerable<int> ids)
        {
            _root = new AABBNode();

            var pointList = points as IList<Point3d> ?? points.ToList();
            var idList = ids as IList<int> ?? ids.ToList();
            var count = Math.Min(pointList.Count, idList.Count);

            AABBNode[] nodes = new AABBNode[count];
            for (int i = 0; i < count; i++)
            {
                nodes[i] = new AABBNode();
                nodes[i].Id = idList[i];
                nodes[i].Box.Min = pointList[i];
                nodes[i].Box.Max = pointList[i];
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
        /// Search the points closer than a radius to a given point
        /// </summary>
        /// <param name="point">Origin point where to search</param>
        /// <param name="radius">Radius of the search</param>
        /// <returns>The ids of the points not farther than <paramref name="radius"/> from <paramref name="point"/></returns>
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


    /// <summary>
    /// Bounding Volume Hierarchy of spheres, to search points or elements (groups of points) near a point or crossed by a line
    /// </summary>
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
            /// <summary>
            /// The center
            /// </summary>
            public Point3d Coord;
            /// <summary>
            /// The radius
            /// </summary>
            public double Radius;

            /// <summary>
            /// Creates a sphere with center at the origin and zero radius
            /// </summary>
            public Sphere()
            {
                Coord = new Point3d(0, 0, 0);
                Radius = 0;
            }

            /// <summary>
            /// Creates a sphere (the instance of the center is kept)
            /// </summary>
            /// <param name="coord">The center</param>
            /// <param name="radius">The radius</param>
            public Sphere(Point3d coord, double radius)
            {
                Coord = coord;
                Radius = radius;
            }

            /// <summary>
            /// Tell if a point is inside the sphere (surface included)
            /// </summary>
            /// <param name="point">The point</param>
            /// <returns>True if the point is inside</returns>
            public bool Contains(Point3d point)
            {
                return Coord.DistanceTo(point) <= Radius;
            }

            /// <summary>
            /// Tell if two spheres overlap (touching spheres included)
            /// </summary>
            /// <param name="sphere">The other sphere</param>
            /// <returns>True if the spheres overlap</returns>
            public bool Intersects(Sphere sphere)
            {
                return Coord.DistanceTo(sphere.Coord) <= Radius + sphere.Radius;
            }

            /// <summary>
            /// Tell if the infinite line of a ray crosses or touches the sphere (both the directions are considered)
            /// </summary>
            /// <param name="ray">The ray</param>
            /// <returns>True if the line crosses the sphere</returns>
            public bool RayIntersects(Ray3d ray)
            {
                var oc = new Vector3d(ray.Point - Coord);
                var a = ray.Direction.DotProduct(ray.Direction);
                var b = 2.0 * oc.DotProduct(ray.Direction);
                var c = oc.DotProduct(oc) - Radius * Radius;
                var discriminant = b * b - 4 * a * c;
                return discriminant >= 0;
            }

            /// <summary>
            /// Changes the sphere into the smallest sphere that contains this sphere and another one
            /// </summary>
            /// <param name="sphere">The other sphere</param>
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
            /// <summary>
            /// Sets the sphere around other spheres: the center is the mean of the centers (added to the current one: the sphere must be new),
            /// the radius the largest distance of their surfaces
            /// </summary>
            /// <param name="spheres">The spheres to contain (not empty)</param>
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
            /// <summary>
            /// The id of the point or element (leaf nodes); -1 for the internal nodes
            /// </summary>
            public int Id;
            /// <summary>
            /// The sphere of the node
            /// </summary>
            public Sphere Sphere;
            /// <summary>
            /// The children of the node (empty for the leaves)
            /// </summary>
            public List<SphereNode> Children;

            /// <summary>
            /// Creates an internal node with a sphere at the origin and zero radius
            /// </summary>
            public SphereNode()
            {
                Id = -1;
                Sphere = new Sphere();
                Children = new List<SphereNode>();
            }

            /// <summary>
            /// Creates a node with a sphere. Not used: <c>Sphere</c> is null, so it throws <see cref="NullReferenceException"/>
            /// </summary>
            /// <param name="coord">The center</param>
            /// <param name="size">The radius</param>
            public SphereNode(Point3d coord, double size)
            {
                Sphere.Coord = coord;
                Sphere.Radius = size;
            }

            /// <summary>
            /// Sets the sphere of a new node around the spheres of the children: the center is the mean of their centers, the radius the largest
            /// distance of their surfaces
            /// </summary>
            /// <param name="children">The children (not empty)</param>
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
        /// <param name="points">The points used as nodes in the hierarchy (the instances are kept)</param>
        /// <param name="ids">The ids of the points; if the counts are different, the extra points or ids are ignored</param>
        public SphereBVH(IEnumerable<Point3d> points, IEnumerable<int> ids)
        {
            _root = new SphereNode();

            var pointList = points as IList<Point3d> ?? points.ToList();
            var idList = ids as IList<int> ?? ids.ToList();
            var count = Math.Min(pointList.Count, idList.Count);

            SphereNode[] nodes = new SphereNode[count];
            for (int i = 0; i < count; i++)
            {
                nodes[i] = new SphereNode();
                nodes[i].Id = idList[i];
                nodes[i].Sphere.Coord = pointList[i];
                nodes[i].Sphere.Radius = 0;
            }

            BuildRecursion(ref _root, nodes.ToList(), 0);
        }

        /// <summary>
        /// Constructor of a Sphere Bounding Volulme Hierarchy used to search elements
        /// </summary>
        /// <param name="elements">The elements as list of points defining a volume (each element must have at least one point)</param>
        /// <param name="ids">The ids of the elements; if the counts are different, the extra elements or ids are ignored</param>
        public SphereBVH(IEnumerable<IEnumerable<Point3d>> elements, IEnumerable<int> ids)
        {
            _root = new SphereNode();

            var elementList = elements as IList<IEnumerable<Point3d>> ?? elements.ToList();
            var idList = ids as IList<int> ?? ids.ToList();
            var count = Math.Min(elementList.Count, idList.Count);

            SphereNode[] nodes = new SphereNode[count];
            for (int i = 0; i < count; i++)
            {
                nodes[i] = new SphereNode();
                nodes[i].Id = idList[i];
                var element = elementList[i] as IList<Point3d> ?? elementList[i].ToList();
                var pointSpheres = new Sphere[element.Count];
                for (int j = 0; j < pointSpheres.Length; j++)
                {
                    pointSpheres[j] = new Sphere(element[j], 0);
                }
                nodes[i].Sphere.SetVolume(pointSpheres);
            }

            BuildRecursion(ref _root, nodes.ToList(), 0, 8);
        }


        /// <summary>
        /// Recursion method to build the BVH: the nodes are sorted along an axis and split in <paramref name="divisions"/> groups (plus one for the remainder)
        /// </summary>
        /// <param name="node">Current recursion node</param>
        /// <param name="toAdd">Nodes to add to current node (the list is sorted in place)</param>
        /// <param name="axis">The axis of the sorting: 0 X, 1 Y, 2 Z (the next level uses the next axis)</param>
        /// <param name="divisions">The maximum number of children of a node</param>
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
        /// Search the points or the elements near a point
        /// </summary>
        /// <param name="point">Origin point where to search</param>
        /// <param name="radius">Radius of the search</param>
        /// <returns>The ids of the leaves whose sphere overlaps the sphere of the search: the points within the radius, the elements that can be
        /// within the radius (their bounding sphere is)</returns>
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
        /// Search the points or the elements crossed by a line
        /// </summary>
        /// <param name="ray">The ray that intesects the spheres (its infinite line)</param>
        /// <returns>The ids of the leaves whose sphere is crossed by the line</returns>
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
