// --------------------------------------------------------------------------------
// Copyright 2013 Google Inc. All Rights Reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// --------------------------------------------------------------------------------

namespace com.espertech.esper.compat.collections.btree
{
    /// <summary>
    /// A btree implementation of the STL set and map interfaces. A btree is both
    /// smaller and faster than STL set/map. The red-black tree implementation of
    /// STL set/map has an overhead of 3 pointers (left, right and parent) plus the
    /// node color information for each stored value. So a set&lt;int32&gt; consumes 20
    /// bytes for each value stored. This btree implementation stores multiple
    /// values on fixed size nodes (usually 256 bytes) and doesn't store child
    /// pointers for leaf nodes. The result is that a btree_set&lt;int32&gt; may use much
    /// less memory per stored value. For the random insertion benchmark in
    /// btree_test.cc, a btree_set&lt;int32&gt; with node-size of 256 uses 4.9 bytes per
    /// stored value.
    ///
    /// The packing of multiple values on to each node of a btree has another effect
    /// besides better space utilization: better cache locality due to fewer cache
    /// lines being accessed. Better cache locality translates into faster
    /// operations.
    /// </summary>
    /// <typeparam name="TK"></typeparam>
    /// <typeparam name="TV"></typeparam>
    public partial class BTree<TK, TV>
    {
        private bool CompareKeys(
            TK lKey,
            TK rKey)
        {
            return _comparer.Compare(lKey, rKey) == 0;
        }

        public void Clear()
        {
            Root?.DestroyRecursive();
            Root = null;
        }

        internal void Swap(BTree<TK, TV> that)
        {
            RefSwap(ref _comparer, ref that._comparer);
            RefSwap(ref _root, ref that._root);
        }

        internal void Verify()
        {
            if (Root != null) {
                Assert(Count == InternalVerify(Root, default(TK), default(TK)));
                //Assert(LeftMost == (++const_iterator(Root, -1)).node);
                //Assert(RightMost == (--const_iterator(Root, Root.Count)).node);
                Assert(LeftMost.IsLeaf);
                Assert(RightMost.IsLeaf);
            }
            else {
                Assert(Count == 0);
                Assert(LeftMost == null);
                Assert(RightMost == null);
            }
        }

        internal void TryShrink()
        {
            if (Root.Count > 0) {
                return;
            }

            // Deleted the last item on the root node, shrink the height of the tree.
            if (Root.IsLeaf) {
                Assert(Count == 0);
                Root.DestroyRecursive();
                Root = null;
            }
            else {
                var child = Root.GetChild(0);
                if (child.IsLeaf) {
                    // The child is a leaf node so simply make it the root node in the tree.
                    child.MakeRoot();
                    Root.DestroyRecursive();
                    Root = child;
                }
                else {
                    // The child is an internal node. We want to keep the existing root node
                    // so we move all of the values from the child node into the existing
                    // (empty) root node.
                    child.Swap(Root);
                    child.DestroyRecursive();
                }
            }
        }

        // --------------------------------------------------------------------------------
        // Node creation/deletion routines.
        // --------------------------------------------------------------------------------

        private Node NewInternalNode(Node parent)
        {
            // internal_fields *p = reinterpret_cast<internal_fields*>(
            //   mutable_internal_allocator()->allocate(sizeof(internal_fields)));
            // return node_type::init_internal(p, parent);
            return new Node(_keyAccessor, false, _kNodeValues, parent);
        }

        private Node NewInternalRootNode()
        {
            // root_fields *p = reinterpret_cast<root_fields*>(
            //   mutable_internal_allocator()->allocate(sizeof(root_fields)));
            // return node_type::init_root(p, root()->parent());

            var parent = _root.Parent;
            return new Node(
                _keyAccessor,
                false,
                _kNodeValues,
                parent,
                parent,
                parent.Count);
        }

        private Node NewLeafNode(Node parent)
        {
            // leaf_fields *p = reinterpret_cast<leaf_fields*>(
            //   mutable_internal_allocator()->allocate(sizeof(leaf_fields)));
            // return node_type::init_leaf(p, parent, kNodeValues);
            return new Node(_keyAccessor, true, _kNodeValues, parent);
        }

        private Node NewLeafRootNode(int maxCount)
        {
            //leaf_fields *p = reinterpret_cast<leaf_fields*>(
            //  mutable_internal_allocator()->allocate(
            //    sizeof(base_fields) + max_count * sizeof(value_type)));
            // return node_type::init_leaf(p, reinterpret_cast<node_type*>(p), max_count);
            return new Node(_keyAccessor, true, maxCount, null);
        }
    }
}