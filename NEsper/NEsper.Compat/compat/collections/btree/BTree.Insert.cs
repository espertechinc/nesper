using System;

namespace com.espertech.esper.compat.collections.btree
{
    public partial class BTree<TK, TV>
    {
        /// <summary>
        /// Inserts an item into the tree.  If the item is not unique this method will ???
        /// </summary>
        /// <param name="cursor"></param>
        /// <param name="v"></param>
        /// <returns></returns>

        private Cursor InternalInsert(
            Cursor cursor,
            TV v)
        {
            if (!cursor.Node.IsLeaf) {
                // We can't insert on an internal node. Instead, we'll insert after the
                // previous value which is guaranteed to be on a leaf node.
                cursor.MovePrevious();
                cursor.MoveNext();
            }

            if (cursor.Node.Count == cursor.Node.MaxCount) {
                // Make room in the leaf for the new item.
                if (cursor.Node.MaxCount < kNodeValues) {
                    // Insertion into the root where the root is smaller that the full node
                    // size. Simply grow the size of the root node.
                    Assert(cursor.Node == Root);
                    cursor.Node = NewLeafRootNode(Math.Min(kNodeValues, 2 * cursor.Node.MaxCount));
                    cursor.Node.Swap(Root);
                    Root.DestroyValues();
                    Root = cursor.Node;
                }
                else {
                    RebalanceOrSplit(ref cursor);
                    // Create a mutable increment
                    Count += 1;
                }
            }
            else if (!Root.IsLeaf) {
                Count += 1;
            }

            cursor.Node.InsertValue(cursor.Position, v);
            return cursor;
        }

        /// <summary>
        /// Insert 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public InsertResult InsertUnique(TV value) {
            if (IsEmpty) {
                Root = NewLeafRootNode(1);
            }

            var key = _keyAccessor(value);
            var locate = Locate(key, new Cursor(Root, 0));
            var locateCursor = locate.Cursor;
            if (locate.IsExactMatch) {
                // The key already exists in the tree, do nothing.
                return new InsertResult(InternalLast(locateCursor), false);
            } else {
                var last = InternalLast(locateCursor);
                if ((last.Node != null) && CompareKeys(key, last.Key)) {
                    // The key already exists in the tree, do nothing.
                    return new InsertResult(last, false);
                }
            }

            return new InsertResult(InternalInsert(locateCursor, value), true);
        }

        public struct InsertResult
        {
            public Cursor Cursor;
            public bool Succeeded;

            public InsertResult(
                Cursor cursor,
                bool succeeded)
            {
                Cursor = cursor;
                Succeeded = succeeded;
            }
        }
    }
}