namespace com.espertech.esper.compat.collections.btree
{
    public partial class BTree<TK, TV>
    {
        /// <summary>
        /// Returns the number of leaf nodes in the tree.
        /// </summary>
        public long LeafNodeCount => InternalStatistics(Root).LeafNodes;

        /// <summary>
        /// Returns the number of internal nodes in the tree.
        /// </summary>
        public long InternalNodeCount => InternalStatistics(Root).InternalNodes;

        /// <summary>
        /// Returns the number nodes in the tree.
        /// </summary>
        public long NodeCount => InternalStatistics(Root).TotalCount;
        
        /// <summary>
        /// Returns the node statistics for a given "root"
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public NodeStatistics InternalStatistics(Node node) {
            if (node == null) {
                return new NodeStatistics(0, 0);
            }
            if (node.IsLeaf) {
                return new NodeStatistics(1, 0);
            }

            var res = new NodeStatistics(0, 1);
            for (int i = 0; i <= node.Count; ++i) {
                res.Add(InternalStatistics(node.GetChild(i)));
            }

            return res;
        }

        public struct NodeStatistics
        {
            public long LeafNodes;
            public long InternalNodes;
            public long TotalCount => LeafNodes + InternalNodes;

            public NodeStatistics(
                long leafNodes,
                long internalNodes) : this()
            {
                LeafNodes = leafNodes;
                InternalNodes = internalNodes;
            }

            /// <summary>
            /// Adds another set of statistics to these stats.
            /// </summary>
            /// <param name="x"></param>
            /// <returns></returns>
            public NodeStatistics Add(NodeStatistics x)
            {
                LeafNodes += x.LeafNodes;
                InternalNodes += x.InternalNodes;
                return this;
            }
        }
    }
}