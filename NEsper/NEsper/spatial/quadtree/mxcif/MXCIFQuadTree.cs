///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.spatial.quadtree.mxcif
{
    /// <summary>
    /// <para>
    /// Quad tree.
    /// </para>
    /// <para>
    /// Nodes can either be leaf nodes or branch nodes. Both leaf nodes and branch nodes have data. "Data" is modelled as a generic type.
    /// </para>
    /// <para>
    /// Branch nodes have 4 regions or child nodes that subdivide the parent region in NW/NE/SW/SE.
    /// </para>
    /// <para>
    /// The tree is polymorphic: leaf nodes can become branches and branches can become leafs.
    /// </para>
    /// <para>
    /// Manipulation and querying of the quad tree is done through tool classes.
    /// As the tree can be polymorphic users should not hold on to the root node as it could change.
    /// </para>
    /// </summary>
    public class MXCIFQuadTree<TL>
    {
        private readonly int _leafCapacity;
        private readonly int _maxTreeHeight;
        private MXCIFQuadTreeNode<TL> _root;

        internal MXCIFQuadTree(int leafCapacity, int maxTreeHeight, MXCIFQuadTreeNode<TL> root)
        {
            _leafCapacity = leafCapacity;
            _maxTreeHeight = maxTreeHeight;
            _root = root;
        }

        public int LeafCapacity => _leafCapacity;

        public int MaxTreeHeight => _maxTreeHeight;

        public MXCIFQuadTreeNode<TL> Root
        {
            get => _root;
            set => _root = value;
        }
    
        public void Clear()
        {
            _root = new MXCIFQuadTreeNodeLeaf<TL>(_root.Bb, _root.Level, default(TL), 0);
        }
    }
} // end of namespace
