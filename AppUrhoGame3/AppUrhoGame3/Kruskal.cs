﻿using Maze;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kruskal
{
    /// <summary>
    /// A UnionFindNode represents a set of nodes that it is a member of.
    /// 
    /// You can get the unique representative node of the set a given node is in by using the Find method.
    /// Two nodes are in the same set when their Find methods return the same representative.
    /// The IsUnionedWith method will check if two nodes' sets are the same (i.e. the nodes have the same representative).
    ///
    /// You can merge the sets two nodes are in by using the Union operation.
    /// There is no way to split sets after they have been merged.
    /// </summary>
    public class UnionFindNode
    {
        private UnionFindNode _parent;
        private uint _rank;

        /// <summary>
        /// Creates a new disjoint node, representative of a set containing only the new node.
        /// </summary>
        public UnionFindNode()
        {
            _parent = this;
        }

        /// <summary>
        /// Returns the current representative of the set this node is in.
        /// Note that the representative is only accurate untl the next Union operation.
        /// </summary>
        public UnionFindNode Find()
        {
            if (!ReferenceEquals(_parent, this)) _parent = _parent.Find();
            return _parent;
        }

        /// <summary>
        /// Determines whether or not this node and the other node are in the same set.
        /// </summary>
        public bool IsUnionedWith(UnionFindNode other)
        {
            if (other == null) throw new ArgumentNullException("other");
            return ReferenceEquals(Find(), other.Find());
        }

        /// <summary>
        /// Merges the sets represented by this node and the other node into a single set.
        /// Returns whether or not the nodes were disjoint before the union operation (i.e. if the operation had an effect).
        /// </summary>
        /// <returns>True when the union had an effect, false when the nodes were already in the same set.</returns>
        public bool Union(UnionFindNode other)
        {
            if (other == null) throw new ArgumentNullException("other");
            var root1 = this.Find();
            var root2 = other.Find();
            if (ReferenceEquals(root1, root2)) return false;

            if (root1._rank < root2._rank)
            {
                root1._parent = root2;
            }
            else if (root1._rank > root2._rank)
            {
                root2._parent = root1;
            }
            else
            {
                root2._parent = root1;
                root1._rank++;
            }
            return true;
        }
    }

    interface IDataWall
    {
        bool Visited { get; set; }
    }

    interface IDataCell
    {
        UnionFindNode Node { get; set; }
    }

    class Kruskal<DataCell, DataWall> where DataCell : IDataCell where DataWall : IDataWall
    {
        static public void generate(List<Cell<DataCell, DataWall>> cells)
        {
            var walls = new List<Wall<DataCell, DataWall>>();

            foreach (var cell in cells)
            {
                cell.Data.Node = new UnionFindNode();
                foreach (var wall in cell.Walls)
                {
                    wall.Data.Visited = false;
                }
            }

            foreach (var cell in cells)
            {
                foreach (var wall in cell.Walls)
                {
                    if (wall.Data.Visited == false)
                    {
                        wall.Data.Visited = true;
                        walls.Add(wall);
                    }
                }
            }

            Random rng = new Random();
            int n = walls.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                var value = walls[k];
                walls[k] = walls[n];
                walls[n] = value;
            }

            foreach (var wall in walls)
            {
                if (wall.First.Data.Node.IsUnionedWith(wall.Second.Data.Node) == false)
                {
                    wall.Open = true;
                    wall.First.Data.Node.Union(wall.Second.Data.Node);
                }
            }
        }
    }

}
