using System;
using System.Collections.Generic;

namespace com.espertech.esper.compat.collections.btree
{
    public partial class BTree<TK, TV>
    {
        private IComparer<TK> _comparer;
        private Node _root;
        private Mutable<Node> _mutableRoot;
        private Mutable<int> _mutableSize;
        private Func<TV,TK> _keyAccessor;

        // Compute how many values we can fit onto a leaf node.
        private int kNodeTargetValues;
        // We need a minimum of 3 values per internal node in order to perform
        // splitting (1 value for the two nodes involved in the split and 1 value
        // propagated to the parent as the delimiter for the split).
        private int kNodeValues;
        private int kMinNodeValues;

        /// <summary>
        /// Returns the root node.
        /// </summary>
        public Node Root {
            get => _root;
            internal set => _root = value;
        }

        /// <summary>
        /// Returns true if the tree is empty.
        /// </summary>
        public bool IsEmpty => _root == null;

        /// <summary>
        /// Returns the number of items in the tree.
        /// </summary>
        public int Count {
            get {
                if (IsEmpty) return 0;
                if (Root.IsLeaf) return Root.Count;
                return Root.Size;
            }
            internal set {
                Root.Size = value;
            }
        }

        /// <summary>
        /// Returns the key comparer.
        /// </summary>
        public IComparer<TK> KeyComparer => _comparer;

        /// <summary>
        /// Returns the key accessor.
        /// </summary>
        public Func<TV,TK> KeyAccessor => _keyAccessor;

        /// <summary>
        /// Returns the left-most node.
        /// </summary>
        public Node LeftMost => _root?.Parent;

        /// <summary>
        /// Returns the right-most node.
        /// </summary>
        public Node RightMost {
            get => ((_root == null) || (_root.IsLeaf))
                ? _root
                : _root.RightMost;
            internal set => _root.RightMost = value;
        }

        /// <summary>
        /// Returns the height of the btree.  An empty tree will have a height of zero.
        /// </summary>
        public int Height {
            get {
                int h = 0;
                if (Root != null) {
                    // Count the length of the chain from the leftmost node up to the
                    // root. We actually count from the root back around to the level below
                    // the root, but the calculation is the same because of the circularity
                    // of that traversal.
                    var n = Root;
                    do {
                        ++h;
                        n = n.Parent;
                    } while (n != Root);
                }

                return h;
            }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="comparer"></param>
        /// <param name="targetNodeCount"></param>
        public BTree(
            Func<TV,TK> keyAccessor,
            IComparer<TK> comparer,
            int targetNodeCount = 3)
        {
            _keyAccessor = keyAccessor ?? throw new ArgumentNullException(nameof(keyAccessor));
            _comparer = comparer ?? throw new ArgumentNullException(nameof(comparer));
            _root = null;

            kNodeTargetValues = targetNodeCount;
            kNodeValues = kNodeTargetValues >= 3 ? kNodeTargetValues : 3;
            kMinNodeValues = kNodeValues / 2;
        }
    }
}