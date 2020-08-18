namespace com.espertech.esper.compat.collections.btree
{
    public partial class BTree<TK, TV>
    {
        /// <summary>
        /// Erase the specified iterator from the btree. The iterator must be valid
        /// (i.e. not equal to end()).  Return an iterator pointing to the node after
        /// the one that was erased (or end() if none exists).
        /// </summary>
        /// <param name="cursor"></param>
        /// <returns></returns>
        public Cursor Erase(Cursor cursor)
        {
            bool internalDelete = false;
            if (!cursor.Node.Leaf) {
                // Deletion of a value on an internal node. Swap the key with the largest
                // value of our left child.
                var tmpcursor = new Cursor(cursor);
                cursor.MovePrevious();
                Assert(cursor.Node.IsLeaf);
                Assert(!CompareKeys(tmpcursor.Key, cursor.Key));
                cursor.Node.SwapValues(
                    cursor.Position,
                    tmpcursor.Node,
                    tmpcursor.Position);
                internalDelete = true;
                Count -= 1;
            }
            else if (!Root.IsLeaf) {
                Count -= 1;
            }

            // Delete the key from the leaf.
            cursor.Node.RemoveValue(cursor.Position);

            // We want to return the next value after the one we just erased. If we
            // erased from an internal node (internalDelete == true), then the next
            // value is ++(++iter). If we erased from a leaf node (internal_delete ==
            // false) then the next value is ++iter. Note that ++iter may point to an
            // internal node and the value in the internal node may move to a leaf node
            // (iter.Node) when rebalancing is performed at the leaf level.

            // Merge/rebalance as we walk back up the tree.
            Cursor res = new Cursor(cursor);
            for (;;) {
                if (cursor.Node == Root) {
                    TryShrink();
                    if (IsEmpty) {
                        return End();
                    }

                    break;
                }

                if (cursor.Node.Count >= _kMinNodeValues) {
                    break;
                }

                var merged = TryMergeOrRebalance(ref cursor);
                if (cursor.Node.Leaf) {
                    res = cursor;
                }

                if (!merged) {
                    break;
                }

                cursor.Node = cursor.Node.Parent;
            }

            // Adjust our return value. If we're pointing at the end of a node, advance
            // the iterator.
            if (res.Position == res.Node.Count) {
                res.Position = res.Node.Count - 1;
                res.MovePrevious();
            }

            // If we erased from an internal node, advance the iterator.
            if (internalDelete) {
                res.MovePrevious();
            }

            return res;
        }

        /// <summary>
        /// Erases range. Returns the number of keys erased.
        /// </summary>
        /// <param name="begin"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public void Erase(
            Cursor begin,
            Cursor end)
        {
            var current = begin;
            while (!current.Equals(end)) {
                Erase(current);
            }

            // Erase current 
            if (current.Equals(end)) {
                Erase(current);
            }
        }

        /// <summary>
        /// Erases the specified key from the btree. Returns 1 if an element was
        /// erased and 0 otherwise.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool TryEraseUnique(TK key, out TV value)
        {
            var cursor = FindUnique(key, new Cursor(Root, 0));
            if (cursor.Node == null) {
                value = default(TV);
                // The key doesn't exist in the tree, return nothing done.
                return false;
            }

            value = cursor.Value;
            Erase(cursor);
            return true;
        }

        /// <summary>
        /// Erases all of the entries matching the specified key from the
        /// btree. Returns the number of elements erased.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public void TryEraseMulti(TK key)
        {
            var begin = InternalLowerBound(key, RootCursor);
            if (begin.Node != null) {
                // The key doesn't exist in the tree, return nothing done.
                return;
            }
            // Delete all of the keys between begin and upper_bound(key).
            var end = InternalEnd(InternalUpperBound(key, RootCursor));
            Erase(begin, end);
        }
    }
}