using System;
using System.Collections.Generic;

namespace com.espertech.esper.compat.collections.btree
{
    public partial class BTree<TK, TV>
    {
        /// <summary>
        /// Returns a cursor to the root of the tree.
        /// </summary>
        public Cursor RootCursor => new Cursor(Root, 0);
        
        /// <summary>
        /// Returns a cursor set to the start of the tree.
        /// </summary>
        /// <returns></returns>
        public Cursor Begin()
        {
            return new Cursor(LeftMost, 0);
        }

        /// <summary>
        /// Returns a cursor set to the end of the tree.
        /// </summary>
        /// <returns></returns>
        public Cursor End()
        {
            var rightMost = RightMost;
            return new Cursor(
                rightMost,
                rightMost?.Count ?? 0);
        }

        /// <summary>
        /// Returns a cursor representing the current position *if* it represents a valid
        /// node, otherwise it returns the end.
        /// </summary>
        /// <param name="cursor"></param>
        /// <returns></returns>
        public Cursor InternalEnd(Cursor cursor)
        {
            return cursor.Node != null ? cursor : End();
        }

        // --------------------------------------------------------------------------------
        // Cursor
        // --------------------------------------------------------------------------------
        
        public class Cursor
        {
            /// <summary>
            /// The node in the tree the iterator is pointing at.
            /// </summary>
            internal Node Node;

            /// <summary>
            /// The position within the node of the tree the iterator is pointing at.
            /// </summary>
            internal int Position;

            /// <summary>
            /// Returns the key at the current enumerator position.
            /// </summary>
            public TK Key => Node.Key(Position);

            /// <summary>
            /// Returns the value at the current enumerator position.
            /// </summary>
            public TV Value {
                get => Node.GetValue(Position);
                // NOTE: this accessor is public but callers must be aware that we will not
                //   be performing any checks on the key associated with the value.
                set => Node.SetValue(Position, value);
            }
            
            /// <summary>
            /// Returns true if the cursor represents the end of the tree.
            /// </summary>
            public bool IsEnd => Node == null || Position >= Node.Count;

            /// <summary>
            /// Returns true if the cursor is not at the end (mostly for while loop convenience).
            /// </summary>
            public bool IsNotEnd => Node != null && Position < Node.Count;

            /// <summary>
            /// Constructor.
            /// </summary>
            /// <param name="node"></param>
            /// <param name="position"></param>
            public Cursor(
                Node node,
                int position)
            {
                this.Node = node;
                this.Position = position;
            }

            /// <summary>
            /// Constructor (copy)
            /// </summary>
            /// <param name="source"></param>
            public Cursor(Cursor source) : this(source.Node, source.Position)
            {
            }

            /// <summary>
            /// Increments the enumerator.
            /// </summary>
            public void Increment()
            {
                if (Node.Leaf && ++Position < Node.Count) {
                    return;
                }

                IncrementSlow();
            }

            public void IncrementBy(int count)
            {
                while (count > 0) {
                    if (Node.Leaf) {
                        var rest = Node.Count - Position;
                        Position += Math.Min(rest, count);
                        count = count - rest;
                        if (Position < Node.Count) {
                            return;
                        }
                    }
                    else {
                        --count;
                    }

                    IncrementSlow();
                }
            }

            public void IncrementSlow()
            {
                if (Node.Leaf) {
                    if (Position < Node.Count) {
                        throw new IllegalStateException("Cursor.Position < Node.Count");
                    }

                    // Save the current state of the enumerator
                    var saveNode = this.Node;
                    var savePosn = this.Position;

                    while (Position == Node.Count && !Node.IsRoot) {
                        //Assert(Node.Parent.GetChild(Node.Position) == Node);
                        if (Node.Parent.GetChild(Node.Position) != Node) {
                            throw new IllegalStateException("GetChild != Node");
                        }

                        Position = Node.Position;
                        Node = Node.Parent;
                    }

                    if (Position == Node.Count) {
                        this.Node = saveNode;
                        this.Position = savePosn;
                    }
                }
                else {
                    if (Position >= Node.Count) {
                        throw new IllegalStateException("Cursor.Position >= Node.Count");
                    }

                    Node = Node.GetChild(Position + 1);
                    while (!Node.Leaf) {
                        Node = Node.GetChild(0);
                    }

                    Position = 0;
                }
            }

            public void Decrement()
            {
                if (Node.Leaf && --Position >= 0) {
                    return;
                }

                DecrementSlow();
            }

            public void DecrementSlow()
            {
                if (Node.Leaf) {
                    //Assert(Position <= -1);
                    if (Position > -1) {
                        throw new IllegalStateException("Position > -1");
                    }

                    // Save the current state of the enumerator
                    var saveNode = this.Node;
                    var savePosn = this.Position;

                    while (Position < 0 && !Node.IsRoot) {
                        //Assert(Node.Parent.GetChild(Node.Position) == Node);
                        if (Node.Parent.GetChild(Node.Position) != Node) {
                            throw new IllegalStateException("GetChild != Node");
                        }
                        Position = Node.Position - 1;
                        Node = Node.Parent;
                    }

                    if (Position < 0) {
                        this.Node = saveNode;
                        this.Position = savePosn;
                    }
                }
                else {
                    // Assert(Position >= 0);
                    if (Position < 0) {
                        throw new IllegalStateException("Cursor.Position < 0");
                    }
                    Node = Node.GetChild(Position);
                    while (!Node.Leaf) {
                        Node = Node.GetChild(Node.Count);
                    }

                    Position = Node.Count - 1;
                }
            }

            /// <summary>
            /// Moves the enumerator forward in the tree.
            /// </summary>
            /// <returns></returns>
            public Cursor MoveNext()
            {
                Increment();
                return this;
            }

            /// <summary>
            /// Moves the enumerator backwards in the tree.
            /// </summary>
            /// <returns></returns>
            public Cursor MovePrevious()
            {
                Decrement();
                return this;
            }
            
            /// <summary>
            /// Converts the cursor into an enumerator.
            /// </summary>
            /// <returns>the enumerator</returns>

            public IEnumerator<TV> ToEnumerator()
            {
                while (IsNotEnd) {
                    yield return Value;
                    MoveNext();
                }
            }
        
            /// <summary>
            /// Converts the cursor into an enumerable.
            /// </summary>
            /// <returns>the enumerable</returns>

            public IEnumerable<TV> ToEnumerable()
            {
                while (IsNotEnd) {
                    yield return Value;
                    MoveNext();
                }
            }
        }
    }
}