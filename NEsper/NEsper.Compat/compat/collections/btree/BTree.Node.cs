using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace com.espertech.esper.compat.collections.btree
{
    public partial class BTree<TK, TV>
    {
        public class Node
        {
            public long Id = DebugId<Node>.NewId();
            
            private Func<TV,TK> _accessor;
            private Node _parent;
            
            // ------------------------------------------------------------
            // BaseFields
            // ------------------------------------------------------------

            internal bool Leaf;
            internal int Position;
            internal readonly int MaxCount;
            internal int Count;

            internal Node Parent {
                get => _parent ?? this;
                set => _parent = value;
            }

            // ------------------------------------------------------------
            // LeafFields
            // ------------------------------------------------------------

            /// <summary>
            /// The array of values. Only the first count of these values have been constructed and are valid.
            /// </summary>
            private TV[] _values;

            // ------------------------------------------------------------
            // InternalFields
            // ------------------------------------------------------------

            // The array of child pointers. The keys in children_[i] are all less than
            // key(i). The keys in children_[i + 1] are all greater than key(i). There
            // are always count + 1 children.
            private Node[] _children;

            // ------------------------------------------------------------
            // RootFields
            // ------------------------------------------------------------

            public Node RightMost;
            public int Size;

            // ------------------------------------------------------------
            // Constructors & Builders
            // ------------------------------------------------------------

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="keyAccessor"></param>
            /// <param name="leaf"></param>
            /// <param name="maxCount"></param>
            /// <param name="parent"></param>
            internal Node(
                Func<TV,TK> keyAccessor,
                bool leaf,
                int maxCount,
                Node parent) : this(keyAccessor, leaf, maxCount, parent, null, 0)
            {
            }

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="keyAccessor"></param>
            /// <param name="leaf"></param>
            /// <param name="maxCount"></param>
            /// <param name="parent"></param>
            /// <param name="rightMost"></param>
            /// <param name="size"></param>
            internal Node(
                Func<TV,TK> keyAccessor,
                bool leaf,
                int maxCount,
                Node parent,
                Node rightMost,
                int size)
            {
                _accessor = keyAccessor;
                
                Leaf = leaf;
                Position = 0;
                MaxCount = maxCount;
                Count = 0;
                Parent = parent;
                RightMost = rightMost;
                Size = size;

                _values = new TV[MaxCount];
                _children = new Node[MaxCount + 1];
                
                Array.Clear(_values, 0, _values.Length);
                Array.Clear(_children, 0, _children.Length);
            }

            // ------------------------------------------------------------
            // Node::Properties
            // ------------------------------------------------------------

            public bool IsLeaf => Leaf;

            public bool IsRoot => Parent.IsLeaf;

            public void MakeRoot()
            {
                Debug.Assert(Parent.IsRoot);
                Parent = Parent.Parent;
            }

            /// <summary>
            /// Retrieves the key in the specified index.
            /// </summary>
            /// <param name="i"></param>
            /// <returns></returns>
            internal TK Key(int i)
            {
                return _accessor(_values[i]);
            }

            /// <summary>
            /// Retrieves the value in the specified index.
            /// </summary>
            /// <param name="i"></param>
            /// <returns></returns>
            internal TV GetValue(int i)
            {
                return _values[i];
            }

            /// <summary>
            /// Sets the value at the specified index.
            /// </summary>
            /// <param name="i"></param>
            /// <param name="value"></param>
            /// <returns></returns>
            internal TV SetValue(
                int i,
                TV value)
            {
                // Add proper atomic or synchronized behavior.
                var original = _values[i];
                _values[i] = value;
                return original;
            }

            /// <summary>
            /// Swap value i in this node with value j in node x.
            /// </summary>
            /// <param name="i"></param>
            /// <param name="that"></param>
            /// <param name="j"></param>
            public void SwapValues(
                int i,
                Node that,
                int j)
            {
                var lvalue = this.GetValue(i);
                var rvalue = that.GetValue(j);
                this.SetValue(i, rvalue);
                that.SetValue(j, lvalue);
            }

            void DestroyValue(int i)
            {
                _values[i] = default;
            }

            /// <summary>
            /// Getters the child at a position in the node.
            /// </summary>
            public Node GetChild(int index)
            {
                return _children[index];
            }

            /// <summary>
            /// Sets the child at a position in the node.
            /// </summary>
            /// <param name="index"></param>
            /// <param name="node"></param>
            public void SetChild(
                int index,
                Node node)
            {
                _children[index] = node;
                if (node != null) {
                    node.Parent = this;
                    node.Position = index;
                }
            }

            /// <summary>
            /// Returns the position of the first value whose key is not less than k.
            /// (aka) key(value) >= k 
            /// </summary>
            /// <param name="k"></param>
            /// <param name="comp"></param>
            /// <returns></returns>
            public int LowerBound(
                TK k,
                IComparer<TK> comp,
                out bool isExactMatch)
            {
                // The C++ implementation allows different search types on the data in a
                // node.  For simplicity, we've chosen to normalize to a linear search.  We
                // may revisit this decision and allow a variable search mechanism.

                isExactMatch = false;
                
                for (int ii = 0; ii < Count; ii++) {
                    var key = _accessor(_values[ii]);
                    var cmp = comp.Compare(k, key);
                    if (cmp == 0) {
                        isExactMatch = true;
                        return ii;
                    } else if (cmp < 0) {
                        //isExactMatch = false;
                        return ii;
                    }
                }

                //isExactMatch = false;
                return Count;
            }

            /// <summary>
            /// Returns the position of the first value whose key is greater than k.
            /// </summary>
            /// <param name="k"></param>
            /// <param name="comp"></param>
            /// <returns></returns>
            public int UpperBound(
                TK k,
                IComparer<TK> comp)
            {
                // The C++ implementation allows different search types on the data in a
                // node.  For simplicity, we've chosen to normalize to a linear search.  We
                // may revisit this decision and allow a variable search mechanism.

                for (int ii = 0; ii < _values.Length; ii++) {
                    var key = _accessor(_values[ii]);
                    var cmp = comp.Compare(k, key);
                    if (cmp < 0) {
                        return ii;
                    }
                }

                return -1;
            }

            /// <summary>
            /// Inserts the value x at position i, shifting all existing values and
            /// children at positions >= i to the right by 1.
            /// </summary>
            /// <param name="i"></param>
            /// <param name="x"></param>
            internal void InsertValue(
                int i,
                TV x)
            {
                Debug.Assert(i <= Count);
                _values[Count] = x;
                for (int j = Count; j > i; --j) {
                    SwapValues(j, this, j - 1);
                }

                Count++;

                if (!IsLeaf) {
                    ++i;
                    for (int j = Count; j > i; --j) {
                        SetChild(j, GetChild(j - 1));
                        GetChild(j).Position = j;
                    }

                    SetChild(i, default);
                }
            }

            /// <summary>
            /// Removes the value at position i, shifting all existing values and children
            /// at positions > i to the left by 1.
            /// </summary>
            /// <param name="i"></param>
            internal void RemoveValue(int i)
            {
                if (!IsLeaf) {
                    Debug.Assert(GetChild(i + 1).Count == 0);
                    for (int j = i + 1; j < Count; ++j) {
                        SetChild(j, GetChild(j + 1));
                        //GetChild(j).Position = j;
                    }

                    SetChild(Count, default);
                }

                Count--;
                for (; i < Count; ++i) {
                    SwapValues(i, this, i + 1);
                }

                DestroyValue(i);
            }

            /// <summary>
            /// Rebalances a node with its right sibling.
            /// </summary>
            /// <param name="src"></param>
            /// <param name="toMove"></param>
            internal void RebalanceRTL(
                Node src,
                int toMove)
            {
                Debug.Assert(Parent == src.Parent);
                Debug.Assert(Position + 1 == src.Position);
                Debug.Assert(src.Count >= Count);
                Debug.Assert(toMove >= 1);
                Debug.Assert(toMove <= src.Count);

                // Make room in the left node for the new values.
                for (int i = 0; i < toMove; ++i) {
                    SetValue(i + Count, default);
                }

                // Move the delimiting value to the left node and the new delimiting value
                // from the right node.
                SwapValues(Count, Parent, Position);
                Parent.SwapValues(Position, src, toMove - 1);

                // Move the values from the right to the left node.
                for (int i = 1; i < toMove; ++i) {
                    SwapValues(Count + i, src, i - 1);
                }

                // Shift the values in the right node to their correct position.
                for (int i = toMove; i < src.Count; ++i) {
                    src.SwapValues(i - toMove, src, i);
                }

                for (int i = 1; i <= toMove; ++i) {
                    src.DestroyValue(src.Count - i);
                }

                if (!IsLeaf) {
                    // Move the child pointers from the right to the left node.
                    for (int i = 0; i < toMove; ++i) {
                        SetChild(1 + Count + i, src.GetChild(i));
                    }

                    for (int i = 0; i <= src.Count - toMove; ++i) {
                        Debug.Assert(i + toMove <= src.MaxCount);
                        src.SetChild(i, src.GetChild(i + toMove));
                        src.SetChild(i + toMove, default);
                    }
                }

                // Fixup the counts on the src and dest nodes.
                Count += toMove;
                src.Count -= toMove;
            }

            internal void RebalanceLTR(
                Node dest,
                int toMove)
            {
                Debug.Assert(Parent == dest.Parent);
                Debug.Assert(Position + 1 == dest.Position);
                Debug.Assert(Count >= dest.Count);
                Debug.Assert(toMove >= 1);
                Debug.Assert(toMove <= Count);

                // Make room in the right node for the new values.
                for (int i = 0; i < toMove; ++i) {
                    dest.SetValue(i + dest.Count, default);
                }

                for (int i = dest.Count - 1; i >= 0; --i) {
                    dest.SwapValues(i, dest, i + toMove);
                }

                // Move the delimiting value to the right node and the new delimiting value
                // from the left node.
                dest.SwapValues(toMove - 1, Parent, Position);
                Parent.SwapValues(Position, this, Count - toMove);
                DestroyValue(Count - toMove);

                // Move the values from the left to the right node.
                for (int i = 1; i < toMove; ++i) {
                    SwapValues(Count - toMove + i, dest, i - 1);
                    DestroyValue(Count - toMove + i);
                }

                if (!IsLeaf) {
                    // Move the child pointers from the left to the right node.
                    for (int i = dest.Count; i >= 0; --i) {
                        dest.SetChild(i + toMove, dest.GetChild(i));
                        dest.SetChild(i, null);
                    }

                    for (int i = 1; i <= toMove; ++i) {
                        dest.SetChild(i - 1, GetChild(Count - toMove + i));
                        SetChild(Count - toMove + i, null);
                    }
                }

                // Fixup the counts on the src and dest nodes.
                Count -= toMove;
                dest.Count += toMove;
            }

            /// <summary>
            /// Splits a node, moving a portion of the node's values to its right sibling. 
            /// </summary>
            /// <param name="dest"></param>
            /// <param name="insertPosition"></param>
            internal void Split(
                Node dest,
                int insertPosition)
            {
                Debug.Assert(dest.Count == 0);

                // We bias the split based on the position being inserted. If we're
                // inserting at the beginning of the left node then bias the split to put
                // more values on the right node. If we're inserting at the end of the
                // right node then bias the split to put more values on the left node.
                if (insertPosition == 0) {
                    dest.Count = Count - 1;
                }
                else if (insertPosition == MaxCount) {
                    dest.Count = 0;
                }
                else {
                    dest.Count = Count / 2;
                }

                Count -= dest.Count;
                Debug.Assert(Count >= 1);

                // Move values from the left sibling to the right sibling.
                for (int i = 0; i < dest.Count; ++i) {
                    dest.SetValue(i, default);
                    SwapValues(Count + i, dest, i);
                    DestroyValue(Count + i);
                }

                // The split key is the largest value in the left sibling.
                Count -= 1;
                Parent.InsertValue(Position, default(TV));
                SwapValues(Count, Parent, Position);
                DestroyValue(Count);
                Parent.SetChild(Position + 1, dest);

                if (!IsLeaf) {
                    for (int i = 0; i <= dest.Count; ++i) {
                        Debug.Assert(GetChild(Count + i + 1) != null);
                        dest.SetChild(i, GetChild(Count + i + 1));
                        SetChild(Count + i + 1, null);
                    }
                }
            }

            /// <summary>
            /// Merges a node with its right sibling, moving all of the values and the
            /// delimiting key in the parent node onto itself. 
            /// </summary>
            /// <param name="src"></param>
            internal void Merge(Node src)
            {
                Debug.Assert(Parent == src.Parent);
                Debug.Assert(Position + 1 == src.Position);

                // Move the delimiting value to the left node.
                SetValue(Count, default);
                SwapValues(Count, Parent, Position);

                // Move the values from the right to the left node.
                for (int i = 0; i < src.Count; ++i) {
                    SetValue(1 + Count + i, default);
                    SwapValues(1 + Count + i, src, i);
                    src.DestroyValue(i);
                }

                if (!IsLeaf) {
                    // Move the child pointers from the right to the left node.
                    for (int i = 0; i <= src.Count; ++i) {
                        SetChild(1 + Count + i, src.GetChild(i));
                        src.SetChild(i, null);
                    }
                }

                // Fixup the counts on the src and dest nodes.
                Count = 1 + Count + src.Count;
                src.Count = 0;

                // Remove the value on the parent node.
                Parent.RemoveValue(Position);
            }

            /// <summary>
            /// Swap the contents of "this" and "that". 
            /// </summary>
            /// <param name="that"></param>
            internal void Swap(Node that)
            {
                Debug.Assert(IsLeaf == that.IsLeaf);

                // Swap the values.
                for (int i = Count; i < that.Count; ++i) {
                    this.SetValue(i, default);
                }

                for (int i = that.Count; i < Count; ++i) {
                    that.SetValue(i, default);
                }

                int n = Math.Max(Count, that.Count);
                for (int i = 0; i < n; ++i) {
                    this.SwapValues(i, that, i);
                }

                for (int i = Count; i < that.Count; ++i) {
                    that.DestroyValue(i);
                }

                for (int i = that.Count; i < Count; ++i) {
                    this.DestroyValue(i);
                }

                if (!IsLeaf) {
                    // Swap the child pointers.
                    for (int i = 0; i <= n; ++i) {
                        var lvalue = this._children[i];
                        var rvalue = that._children[i];
                        this.SetChild(i, rvalue);
                        that.SetChild(i, lvalue);
                        // btree_swap_helper(*mutable_child(i), *src.mutable_Child(i));
                    }

                    for (int i = 0; i <= this.Count; ++i) {
                        that.GetChild(i).Parent = that;
                    }

                    for (int i = 0; i <= that.Count; ++i) {
                        this.GetChild(i).Parent = this;
                    }
                }

                // Swap the counts.
                RefSwap(ref this.Count, ref that.Count);
            }

            /// <summary>
            /// Destroys the node and recursively destroys any children.
            /// </summary>
            internal void DestroyRecursive()
            {
                if (!IsLeaf) {
                    for (int ii = 0; ii < Count; ++ii) {
                        _children[ii]?.DestroyRecursive();
                    }
                }

                DestroyValues();
            }
            
            /// <summary>
            /// Destroys the values associated with the node ONLY.
            /// </summary>
            internal void DestroyValues()
            {
                for (int i = 0; i < Count; ++i) {
                    DestroyValue(i);
                }
            }

            public override string ToString()
            {
                return
                    $"{nameof(Id)}: {Id}, " +
                    $"{nameof(Position)}: {Position}, {nameof(MaxCount)}: {MaxCount}, {nameof(Count)}: {Count}, " +
                    $"{nameof(Size)}: {Size}, {nameof(IsLeaf)}: {IsLeaf}, {nameof(IsRoot)}: {IsRoot}";
            }

            public void Dump(
                TextWriter textWriter,
                int level)
            {
                for (int i = 0; i < Count; ++i) {
                    if (!IsLeaf) {
                        _children[i].Dump(textWriter, level + 1);
                    }

                    for (int j = 0; j < level; ++j) {
                        textWriter.Write("  ");
                    }

                    textWriter.Write(Key(i));
                    textWriter.Write(" (");
                    textWriter.Write(i);
                    textWriter.Write(") [");
                    textWriter.Write(level);
                    textWriter.Write("]: ");
                    textWriter.Write(Id);
                    textWriter.Write("\n");
                }

                if (!IsLeaf) {
                    GetChild(Count).Dump(textWriter, level + 1);
                }
            }
        }

#if false
        public interface Mutable<T>
        {
            T Value { get; set; }
            T Get();
            void Set(T value);
        }

        public class MutableImpl<T> : Mutable<T>
        {
            private Func<T> accessor;
            private Action<T> mutator;

            public MutableImpl(Func<T> accessor,
                Action<T> mutator)
            {
                this.accessor = accessor;
                this.mutator = mutator;
            }

            public T Value {
                get => accessor();
                set => mutator(value);
            }

            public T Get() => accessor();

            public void Set(T value) => mutator(value);
        }
        #endif
    }
}