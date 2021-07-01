namespace com.espertech.esper.compat.collections.btree
{
    public partial class BTree<TK, TV>
    {
        /// <summary>
        /// Returns an iterator pointing to the first value >= the value "iter" is
        /// pointing at. Note that "iter" might be pointing to an invalid location as
        /// iter.position == iter.node->count(). This routine simply moves iter up in
        /// the tree to a valid location.
        /// </summary>
        /// <param name="cursor"></param>
        /// <returns></returns>
        internal Cursor InternalLast(Cursor cursor)
        {
            cursor = new Cursor(cursor);
            while ((cursor.Node != null) && (cursor.Position == cursor.Node.Count)) {
                cursor.Position = cursor.Node.Position;
                cursor.Node = cursor.Node.Parent;
                if (cursor.Node != null && cursor.Node.IsLeaf) {
                    cursor.Node = null;
                }
            }

            return cursor;
        }
        
        /// <summary>
        /// Returns true if the tree contains the given key.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>

        public bool ContainsKey(TK key)
        {
            var result = Locate(key, RootCursor);
            return result.IsExactMatch;
        }

        /// <summary>
        /// Returns true if the tree contains the given value.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>

        public bool ContainsValue(TV value)
        {
            var key = _keyAccessor(value);
            var result = Locate(key, RootCursor);
            return result.IsExactMatch && Equals(value, result.Cursor.Value);
        }

        /// <summary>
        /// Attempts to locate an item.  Returns a locate with the best location for the item.
        /// </summary>
        /// <param name="key">the key to locate</param>
        /// <param name="cursor">enumerator indicating where to start our search</param>
        /// <returns></returns>
        public LocateResult Locate(
            TK key,
            Cursor cursor)
        {
            var cursorX = new Cursor(cursor);
            for (;;) {
                cursorX.Position = cursorX.Node.LowerBound(key, _comparer, out var isExactMatch);
                if (isExactMatch) {
                    // We have located the exact item at this position.  Return immediately.
                    return new LocateResult(cursorX, true);
                }
                
                // We didnt the item, but we have identified the node that precedes the key
                // we are looking for.  If the node is a leaf, we can return immediately.  Leaves
                // do not have children and as such there is no further depth.

                if (cursorX.Node.IsLeaf) {
                    return new LocateResult(cursorX, false);
                }

                // Reset the position if its -1 but not a leaf.
                if (cursorX.Position == -1) {
                    cursorX.Position = 0;
                }

                // This is an internal node.  Internal nodes have children.  The posn that we
                // received back is the "lower-bound" meaning it should set the enumerator to
                // the position just before or equal to the key we are looking for.  Change the
                // node on the enumerator (proceed down into the child).
                
                cursorX.Node = cursorX.Node.GetChild(cursorX.Position);
            }
        }

        public Cursor InternalLowerBound(
            TK key,
            Cursor cursor)
        {
            if (cursor.Node != null) {
                for (;;) {
                    cursor.Position = cursor.Node.LowerBound(key, _comparer, out var isExactMatch);
                    
                    // We are either at the node & position that represents the key or we are just before
                    // the node & position at this depth in the tree.  If we are at leaf node, then we
                    // can stop our depth scan.
                    
                    if (cursor.Node.IsLeaf) {
                        break;
                    }
                    
                    // Internal node: continue
                    
                    cursor.Node = cursor.Node.GetChild(cursor.Position);
                }

                cursor = InternalLast(cursor);
            }

            return cursor;
        }

        public Cursor InternalUpperBound(
            TK key,
            Cursor cursor)
        {
            if (cursor.Node != null) {
                for (;;) {
                    cursor.Position = cursor.Node.UpperBound(key, _comparer);

                    // We are either at the node & position that is just greater than our key at this depth in
                    // the tree.  If we are at leaf node, then we can stop our depth scan.
                    
                    if (cursor.Node.IsLeaf) {
                        break;
                    }

                    cursor.Node = cursor.Node.GetChild(cursor.Position);
                }

                cursor = InternalLast(cursor);
            }

            return cursor;
        }

        // --------------------------------------------------------------------------------
        // Relative
        // --------------------------------------------------------------------------------

        /// <summary>
        /// Returns a cursor where key(node) &gt;= key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>

        public Cursor GreaterThanOrEqual(TK key, Cursor cursor)
        {
            var res = Locate(key, cursor);
            if (!res.IsExactMatch) {
                res.Cursor.MoveNext(); // Cursor is below
            }

            return res.Cursor;
        }
        
        /// <summary>
        /// Returns a cursor where key(node) &lt;= key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>

        public Cursor LessThanOrEqual(TK key, Cursor cursor)
        {
            var res = Locate(key, cursor);
            return res.Cursor;
        }

        /// <summary>
        /// Returns a cursor where key(node) &gt; key.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public Cursor GreaterThan(TK key, Cursor cursor)
        {
            var res = Locate(key, cursor);
            res.Cursor.MoveNext();
            return res.Cursor;
        }

        /// <summary>
        /// Returns a cursor where key(node) &lt; key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public Cursor LessThan(TK key, Cursor cursor)
        {
            var res = Locate(key, cursor);
            if (res.IsExactMatch) {
                res.Cursor.MovePrevious();
            }

            return res.Cursor;
        }
        
        // --------------------------------------------------------------------------------
        // FindUnique
        // --------------------------------------------------------------------------------

        public Cursor FindUnique(
            TK key,
            Cursor cursor)
        {
            if (cursor.Node != null) {
                var res = Locate(key, cursor);
                if (res.IsExactMatch) {
                    return res.Cursor;
                }

                if (cursor.Position == 0) {
                    cursor = InternalLast(res.Cursor);
                    if ((cursor.Node != null) && !CompareKeys(key, cursor.Key)) {
                        return cursor;
                    }
                }
            }

            return new Cursor(null, 0);
        }

        public Cursor FindUnique(TK key)
        {
            return FindUnique(key, RootCursor);
        }

        // --------------------------------------------------------------------------------
        // FindMulti
        // --------------------------------------------------------------------------------

        public Cursor FindMulti(
            TK key,
            Cursor cursor)
        {
            if (cursor.Node != null) {
                cursor = InternalLowerBound(key, cursor);
                if (cursor.Node != null) {
                    cursor = InternalLast(cursor);
                    if ((cursor.Node != null) && !CompareKeys(key, cursor.Key)) {
                        return cursor;
                    }
                }
            }

            return new Cursor(null, 0);
        }

        public Cursor FindMulti(TK key)
        {
            return FindMulti(key, RootCursor);
        }
        
        // --------------------------------------------------------------------------------
        
        // --------------------------------------------------------------------------------

        public int InternalVerify(
            Node node,
            TK lo,
            TK hi)
        {
            Assert(node.Count > 0);
            Assert(node.Count <= node.MaxCount);
            if (lo != null) {
                Assert(!CompareKeys(node.Key(0), lo));
            }

            if (hi != null) {
                Assert(!CompareKeys(hi, node.Key(node.Count - 1)));
            }

            for (var i = 1; i < node.Count; ++i) {
                Assert(!CompareKeys(node.Key(i), node.Key(i - 1)));
            }

            var count = node.Count;
            if (!node.IsLeaf) {
                for (var i = 0; i <= node.Count; ++i) {
                    Assert(node.GetChild(i) != null);
                    Assert(node.GetChild(i).Parent == node);
                    Assert(node.GetChild(i).Position == i);
                    count += InternalVerify(
                        node.GetChild(i),
                        (i == 0) ? lo : node.Key(i - 1),
                        (i == node.Count) ? hi : node.Key(i));
                }
            }

            return count;
        }

        /// <summary>
        /// Represents the result of a locate request.
        /// </summary>
        public struct LocateResult
        {
            public Cursor Cursor;
            public bool IsExactMatch;

            public LocateResult(
                Cursor cursor,
                bool isExactMatch)
            {
                Cursor = cursor;
                IsExactMatch = isExactMatch;
            }
        }
    }
}