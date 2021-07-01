using System;

namespace com.espertech.esper.compat.collections.btree
{
    public partial class BTree<TK, TV>
    {
        internal void RebalanceOrSplit(ref Cursor cursor)
        {
            Assert(cursor.Node.Count == cursor.Node.MaxCount);

            // First try to make room on the node by rebalancing.
            var parent = cursor.Node.Parent;
            if (cursor.Node != Root) {
                if (cursor.Node.Position > 0) {
                    // Try rebalancing with our left sibling.
                    var left = parent.GetChild(cursor.Node.Position - 1);
                    if (left.Count < left.MaxCount) {
                        // We bias rebalancing based on the position being inserted. If we're
                        // inserting at the end of the right node then we bias rebalancing to
                        // fill up the left node.
                        var offset = cursor.Position < left.MaxCount ? 1 : 0;
                        var toMove = (left.MaxCount - left.Count) / (1 + offset);
                        toMove = Math.Max(1, toMove);

                        if (((cursor.Position - toMove) >= 0) ||
                            ((left.Count + toMove) < left.MaxCount)) {

                            left.RebalanceRTL(cursor.Node, toMove);

                            Assert(cursor.Node.MaxCount - cursor.Node.Count == toMove);
                            cursor.Position = cursor.Position - toMove;
                            if (cursor.Position < 0) {
                                cursor.Position = cursor.Position + left.Count + 1;
                                cursor.Node = left;
                            }

                            Assert(cursor.Node.Count < cursor.Node.MaxCount);
                            return;
                        }
                    }
                }

                if (cursor.Node.Position < parent.Count) {
                    // Try rebalancing with our right sibling.
                    var right = parent.GetChild(cursor.Node.Position + 1);
                    if (right.Count < right.MaxCount) {
                        // We bias rebalancing based on the position being inserted. If we're
                        // inserting at the beginning of the left node then we bias rebalancing
                        // to fill up the right node.
                        var offset = (cursor.Position > 0) ? 1 : 0;
                        var to_move = (right.MaxCount - right.Count) / (1 + offset);
                        to_move = Math.Max(1, to_move);

                        if ((cursor.Position <= (cursor.Node.Count - to_move)) ||
                            ((right.Count + to_move) < right.MaxCount)) {
                            cursor.Node.RebalanceLTR(right, to_move);

                            if (cursor.Position > cursor.Node.Count) {
                                cursor.Position = cursor.Position - cursor.Node.Count - 1;
                                cursor.Node = right;
                            }

                            Assert(cursor.Node.Count < cursor.Node.MaxCount);
                            return;
                        }
                    }
                }

                // Rebalancing failed, make sure there is room on the parent node for a new
                // value.
                if (parent.Count == parent.MaxCount) {
                    var parentCursor = new Cursor(cursor.Node.Parent, cursor.Node.Position);
                    RebalanceOrSplit(ref parentCursor);
                }
            }
            else {
                // Rebalancing not possible because this is the root node.
                if (Root.IsLeaf) {
                    // The root node is currently a leaf node: create a new root node and set
                    // the current root node as the child of the new root.
                    parent = NewInternalRootNode();
                    parent.SetChild(0, Root);
                    Root = parent;
                    Assert(RightMost == parent.GetChild(0));
                }
                else {
                    // The root node is an internal node. We do not want to create a new root
                    // node because the root node is special and holds the size of the tree
                    // and a pointer to the rightmost node. So we create a new internal node
                    // and move all of the items on the current root into the new node.
                    parent = NewInternalNode(parent);
                    parent.SetChild(0, parent);
                    parent.Swap(Root);
                    cursor.Node = parent;
                }
            }

            // Split the node.
            Node splitNode;
            if (cursor.Node.IsLeaf) {
                splitNode = NewLeafNode(parent);
                cursor.Node.Split(splitNode, cursor.Position);
                if (RightMost == cursor.Node) {
                    RightMost = splitNode;
                }
            }
            else {
                splitNode = NewInternalNode(parent);
                cursor.Node.Split(splitNode, cursor.Position);
            }

            if (cursor.Position > cursor.Node.Count) {
                cursor.Position = cursor.Position - cursor.Node.Count - 1;
                cursor.Node = splitNode;
            }
        }

        public bool DebugFlag = false;
        
        internal void MergeNodes(
            Node left,
            Node right)
        {
            left.Merge(right);
            if (right.IsLeaf && (RightMost == right)) {
                RightMost = left;
            }

            right.DestroyRecursive();
        }

        internal bool TryMergeOrRebalance(ref Cursor cursor)
        {
            var parent = cursor.Node.Parent;
            if (cursor.Node.Position > 0) {
                // Try merging with our left sibling.
                var left = parent.GetChild(cursor.Node.Position - 1);
                if ((1 + left.Count + cursor.Node.Count) <= left.MaxCount) {
                    cursor.Position += 1 + left.Count;
                    MergeNodes(left, cursor.Node);
                    cursor.Node = left;
                    return true;
                }
            }

            if (cursor.Node.Position < parent.Count) {
                // Try merging with our right sibling.
                var right = parent.GetChild(cursor.Node.Position + 1);
                if ((1 + cursor.Node.Count + right.Count) <= right.MaxCount) {
                    MergeNodes(cursor.Node, right);
                    return true;
                }

                // Try rebalancing with our right sibling. We don't perform rebalancing if
                // we deleted the first element from iter.node and the node is not
                // empty. This is a small optimization for the common pattern of deleting
                // from the front of the tree.
                if ((right.Count > _kMinNodeValues) &&
                    ((cursor.Node.Count == 0) ||
                     (cursor.Position > 0))) {
                    var toMove = (right.Count - cursor.Node.Count) / 2;
                    toMove = Math.Min(toMove, right.Count - 1);
                    cursor.Node.RebalanceRTL(right, toMove);
                    return false;
                }
            }

            if (cursor.Node.Position > 0) {
                // Try rebalancing with our left sibling. We don't perform rebalancing if
                // we deleted the last element from iter.node and the node is not
                // empty. This is a small optimization for the common pattern of deleting
                // from the back of the tree.
                var left = parent.GetChild(cursor.Node.Position - 1);
                if ((left.Count > _kMinNodeValues) &&
                    ((cursor.Node.Count == 0) ||
                     (cursor.Position < cursor.Node.Count))) {
                    var toMove = (left.Count - cursor.Node.Count) / 2;
                    toMove = Math.Min(toMove, left.Count - 1);
                    left.RebalanceLTR(cursor.Node, toMove);
                    cursor.Position += toMove;
                    return false;
                }
            }

            return false;
        }
    }
}