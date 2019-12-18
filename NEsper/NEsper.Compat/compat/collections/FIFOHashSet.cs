///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace com.espertech.esper.collection
{
    /// <summary>
    /// FIFOHashSet is a collection that : "set" principals.  Members of a set
    /// are unique and can only occur once.  Additionally, iteration of the set is
    /// governed by first-in first-out principal.  This means that the order in which
    /// items are added to the set is preserved through iteration.
    /// </summary>

    [Serializable]
    public sealed class FIFOHashSet<T> : ISet<T>
    {
        private int _primeIndex;

        /// <summary>
        /// NodeTable of nodes ...
        /// </summary>
        private Node[] _nodeTable;

        /// <summary>
        /// NodeTable that is indexed by hash code and points to the first node
        /// in the chain for that hash code.
        /// </summary>
        private int[] _hashIndex;

        /// <summary>
        /// MapIndex that represents the first valid node in the ordered list.  Value should
        /// be -1 if there is no head value.
        /// </summary>
        private int _headIndex;

        /// <summary>
        /// MapIndex that represents the last valid node in the ordered list.  Value should be
        /// -1 if there is no last value.
        /// </summary>
        private int _tailIndex;

        /// <summary>
        /// Head of the free node chain
        /// </summary>
        private int _freeListHead;

        /// <summary>
        /// Version of the collection.
        /// </summary>
        private int _version;

        /// <summary>
        /// Total number of entries in the set.
        /// </summary>
        private int _nodeCount;

        /// <summary>
        /// # of nodes that overlap in the same bucket
        /// </summary>
        private int _collisions;

        public double Load
        {
            get { return (_collisions * 100.0) / _hashIndex.Length; }
        }

        private static readonly int[] PrimeTable =
        {
            67,
            131,
            257,
            521,
            1031,
            2053,
            4099,
            8209,
            16411,
            32771,
            65537,
            131101,
            262147,
            524309,
            1048583,
            2097169,
            4194319,
            8388617,
            16777259,
            33554467,
            67108879,
            134217757,
            268435459,
            536870923,
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="FIFOHashSet&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="sourceCollection">The source collection.</param>
        public FIFOHashSet(ICollection<T> sourceCollection)
            : this(sourceCollection.Count)
        {
            foreach (var item in sourceCollection) {
                Add(item);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FIFOHashSet&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="minCapacity">The minimum capacity.</param>
        public FIFOHashSet(int minCapacity)
        {
            for (int ii = 0; ii < PrimeTable.Length; ii++)
            {
                if (PrimeTable[ii] > minCapacity)
                {
                    _primeIndex = ii;
                    break;
                }
            }

            var tableSize = PrimeTable[_primeIndex];

            _headIndex = -1;
            _tailIndex = -1;
            _freeListHead = -1;

            _hashIndex = new int[tableSize];
            for (int ii = 0; ii < tableSize; ii++)
                _hashIndex[ii] = -1;

            _nodeTable = new Node[tableSize];
            _nodeCount = 0;
            _collisions = 0;
            _version = 0;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FIFOHashSet&lt;T&gt;"/> class.
        /// </summary>
        public FIFOHashSet()
        {
            _primeIndex = 0;

            var tableSize = PrimeTable[_primeIndex];

            _headIndex = -1;
            _tailIndex = -1;
            _freeListHead = -1;

            _hashIndex = new int[tableSize];
            for (int ii = 0; ii < tableSize; ii++)
                _hashIndex[ii] = -1;

            _nodeTable = new Node[tableSize];
            _nodeCount = 0;
            _collisions = 0;
            _version = 0;
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<T> GetEnumerator()
        {
            var versionAtHead = _version;
            var nodeIndex = _nodeTable;

            for (var index = _headIndex; index != -1; index = nodeIndex[index].NextNodeInOrder)
            {
                if (versionAtHead != _version)
                {
                    throw new InvalidOperationException("Collection modified");
                }

                yield return nodeIndex[index].Value;
            }
        }

        /// <summary>
        /// Iterates over the collection performing one operation on each element in
        /// the set.
        /// </summary>
        /// <param name="action">The action.</param>
        public void ForEach(Action<T> action)
        {
            var versionAtHead = _version;
            var nodeIndex = _nodeTable;

            for (var index = _headIndex; index != -1; index = nodeIndex[index].NextNodeInOrder)
            {
                if (versionAtHead != _version)
                {
                    throw new InvalidOperationException("Collection modified");
                }

                action.Invoke(nodeIndex[index].Value);
            }
        }

        /// <summary>
        /// Removes all items from the <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </summary>
        /// <exception cref="T:System.NotSupportedException">
        /// The <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only.
        /// </exception>
        public void Clear()
        {
#if DIAGNOSTICS
            System.Diagnostics.Debug.Print("Clear: {0}", id);
#endif

            _primeIndex = 0;

            var tableSize = PrimeTable[_primeIndex];

            _headIndex = -1;
            _tailIndex = -1;
            _freeListHead = -1;

            _hashIndex = new int[tableSize];
            for (int ii = 0; ii < tableSize; ii++)
                _hashIndex[ii] = -1;

            _nodeAllocIndex = 0;
            _nodeTable = new Node[tableSize];
            _nodeCount = 0;
            _collisions = 0;
            ++_version;
        }

        /// <summary>
        /// Gets an enumeration of all items in a chain.
        /// </summary>
        /// <param name="hashIndexIndex">MapIndex of the hash index.</param>
        /// <returns></returns>
        internal IEnumerable<Node> GetChain(int hashIndexIndex)
        {
            var nodeTable = _nodeTable;
            for (var nodeIndex = _hashIndex[hashIndexIndex]; nodeIndex != -1; nodeIndex = nodeTable[nodeIndex].NextNodeInChain)
            {
                yield return nodeTable[nodeIndex];
            }
        }

        /// <summary>
        /// Gets the histogram.
        /// </summary>
        /// <returns></returns>
        public String DebugHashDistribution()
        {
            var basicHistogram = new SortedDictionary<int, int[]>();

            var length = _hashIndex.Length;
            for (int ii = 0; ii < length; ii++)
            {
                int chainCount = GetChain(ii).Count();
                int[] chainCountMatch;

                if (basicHistogram.TryGetValue(chainCount, out chainCountMatch))
                {
                    chainCountMatch[0]++;
                }
                else
                {
                    basicHistogram[chainCount] = new[] { 1 };
                }
            }

            var writer = new StringWriter();
            foreach (var entry in basicHistogram)
            {
                writer.WriteLine("{0}\t{1}", entry.Key, entry.Value[0]);
            }

            return writer.ToString();
        }

        /// <summary>
        /// Indicates the index where the next node needs to be allocated from.
        /// </summary>
        private int _nodeAllocIndex;

        /// <summary>
        /// Allocates a node for use and return the index of the node.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="hashCode">The hash code.</param>
        /// <returns></returns>
        private int AllocNode(T item, int hashCode)
        {
            // Allocate from the freeList first.  If there are no nodes available on
            // the freeList then allocate from the current "count" on.  If the freeList
            // is empty then we have a completely allocated list from 0 to count - 1.
            if (_freeListHead == -1)
            {
                // No items on the freeList.
                // Space must be allocated from the existing node table.
                int index = _nodeAllocIndex;
                if (index == _nodeTable.Length)
                {
                    var newTableSize = _nodeTable.Length * 2;
                    var newTable = new Node[newTableSize];
                    Array.Copy(_nodeTable, 0, newTable, 0, _nodeTable.Length);
                    _nodeTable = newTable;
                    ReIndex();
                }

                _nodeTable[index].SetValues(item, hashCode);
                _nodeAllocIndex++;
                return index;
            }
            else
            {
                int index = _freeListHead;
                _freeListHead = _nodeTable[index].NextNodeInChain;
                _nodeTable[index].SetValues(item, hashCode);
                return index;
            }
        }

        /// <summary>
        /// Reindexes the internal bucket table.
        /// </summary>
        private void ReIndex()
        {
            _primeIndex++;
            var newHashIndexLength = PrimeTable[_primeIndex];
            var newHashIndex = new int[newHashIndexLength];
            for (int ii = 0; ii < newHashIndexLength; ii++)
            {
                newHashIndex[ii] = -1;
            }

            var collisions = 0;
            var nodeTable = _nodeTable;

            for (var nodeIndex = _headIndex; nodeIndex != -1; nodeIndex = nodeTable[nodeIndex].NextNodeInOrder)
            {
                var node = nodeTable[nodeIndex];
                // Modulus the hash code with new table size 
                var bucket = node.HashCode % newHashIndexLength;
                // Get the current entry at the bucket
                var entry = newHashIndex[bucket];
                // Attach the node at the head of the bucket chain
                nodeTable[nodeIndex].NextNodeInChain = entry;
                newHashIndex[bucket] = nodeIndex;
                // Increment the collision space if appropriate
                if (entry != -1) collisions++;
            }

            _hashIndex = newHashIndex;
            _collisions = collisions;
        }

        public void UnionWith(IEnumerable<T> other)
        {
            throw new NotSupportedException();
        }

        public void IntersectWith(IEnumerable<T> other)
        {
            throw new NotSupportedException();
        }

        public void ExceptWith(IEnumerable<T> other)
        {
            throw new NotSupportedException();
        }

        public void SymmetricExceptWith(IEnumerable<T> other)
        {
            throw new NotSupportedException();
        }

        public bool IsSubsetOf(IEnumerable<T> other)
        {
            throw new NotSupportedException();
        }

        public bool IsSupersetOf(IEnumerable<T> other)
        {
            throw new NotSupportedException();
        }

        public bool IsProperSupersetOf(IEnumerable<T> other)
        {
            throw new NotSupportedException();
        }

        public bool IsProperSubsetOf(IEnumerable<T> other)
        {
            throw new NotSupportedException();
        }

        public bool Overlaps(IEnumerable<T> other)
        {
            throw new NotSupportedException();
        }

        public bool SetEquals(IEnumerable<T> other)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Adds the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        void ICollection<T>.Add(T item)
        {
            Add(item);
        }

        /// <summary>
        /// Adds an item to the <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </summary>
        /// <param name="item">The object to add to the <see cref="T:System.Collections.Generic.ICollection`1"/>.</param>
        /// <exception cref="T:System.NotSupportedException">
        /// The <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only.
        /// </exception>
        public bool Add(T item)
        {
            var nodeTable = _nodeTable;
            var hashIndex = _hashIndex;

            // Look for the node in the current space
            var hashCode = item.GetHashCode() & 0x7fffffff;
            // Get the appropriate node index - remember there are no direct node references
            var chainIndex = hashIndex[hashCode % hashIndex.Length];

            // Scan the chain
            int currIndex;
            for (currIndex = chainIndex;
                 currIndex >= 0;
                 currIndex = nodeTable[currIndex].NextNodeInChain)
            {
                // Skip entries that do not share the same hashcode
                if (nodeTable[currIndex].HashCode == hashCode)
                {
                    // Check for node equality
                    if (Equals(item, nodeTable[currIndex].Value))
                    {
                        return true;
                    }
                }
            }

            // Add the node to our current collection
            var nodeIndex = AllocNode(item, hashCode);

            nodeTable = _nodeTable;
            hashIndex = _hashIndex;
            chainIndex = hashIndex[hashCode % hashIndex.Length];

            if (_tailIndex != -1)
            {
                nodeTable[_tailIndex].NextNodeInOrder = nodeIndex;
            }
            if (_headIndex == -1)
            {
                _headIndex = nodeIndex;
            }

            nodeTable[nodeIndex].SetReferences(_tailIndex, -1, chainIndex);

#if DIAGNOSTICS
            System.Diagnostics.Debug.Print("Add: {0} => {1} / {2} / {3}",
                                           id,
                                           hashCode%hashIndex.Length,
                                           nodeIndex,
                                           chainIndex);
#endif

            _tailIndex = nodeIndex;

            // Add the node to the current chain for the hash
            if (chainIndex != -1) _collisions++;
            hashIndex[hashCode % hashIndex.Length] = nodeIndex;

            ++_nodeCount;
            ++_version;

            return false;
        }

        /// <summary>
        /// Determines whether [contains] [the specified item].
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns>
        /// 	<c>true</c> if [contains] [the specified item]; otherwise, <c>false</c>.
        /// </returns>
        public bool Contains(T item)
        {
            var nodeTable = _nodeTable;
            var hashIndex = _hashIndex;

            // Look for the node in the current space
            var hashCode = item.GetHashCode() & 0x7fffffff;

            // Get the appropriate bucket - this indexes into HashIndex
            var hashIndexIndex = hashCode % hashIndex.Length;
            // Get the appropriate node index - remember there are no direct node references
            var headIndex = hashIndex[hashIndexIndex];

            int currIndex;
            for (currIndex = headIndex;
                 currIndex >= 0;
                 currIndex = nodeTable[currIndex].NextNodeInChain)
            {
                // Skip entries that do not share the same hashcode
                if (nodeTable[currIndex].HashCode != hashCode) continue;
                // Check for node equality
                if (Equals(item, nodeTable[currIndex].Value))
                {
                    return true; // Node already exists in set
                }
            }

            return false;
        }

        /// <summary>
        /// Copies the elements of the <see cref="T:System.Collections.Generic.ICollection`1" /> to an <see cref="T:System.Array" />, starting at a particular <see cref="T:System.Array" /> index.
        /// </summary>
        /// <param name="array">The one-dimensional <see cref="T:System.Array" /> that is the destination of the elements copied from <see cref="T:System.Collections.Generic.ICollection`1" />. The <see cref="T:System.Array" /> must have zero-based indexing.</param>
        /// <param name="arrayIndex">The zero-based index in <paramref name="array" /> at which copying begins.</param>
        /// <exception cref="System.InvalidOperationException">Collection modified</exception>
        public void CopyTo(T[] array, int arrayIndex)
        {
            var arrCount = array.Length - arrayIndex;
            var maxCount = _nodeCount;
            if (maxCount < arrCount)
                maxCount = arrCount;

            var arrLast = arrayIndex + maxCount;
            var versionAtHead = _version;
            var nodeIndex = _nodeTable;

            for (var index = _headIndex; index != -1; index = nodeIndex[index].NextNodeInOrder) {
                if (versionAtHead != _version) {
                    throw new InvalidOperationException("Collection modified");
                }

                array[arrayIndex] = nodeIndex[index].Value;
                arrayIndex++;

                if (arrayIndex >= arrLast ) {
                    return;
                }
            }
        }

        /// <summary>
        /// Removes the first occurrence of a specific object from the <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </summary>
        /// <param name="item">The object to remove from the <see cref="T:System.Collections.Generic.ICollection`1"/>.</param>
        /// <returns>
        /// true if <paramref name="item"/> was successfully removed from the <see cref="T:System.Collections.Generic.ICollection`1"/>; otherwise, false. This method also returns false if <paramref name="item"/> is not found in the original <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </returns>
        /// <exception cref="T:System.NotSupportedException">
        /// The <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only.
        /// </exception>
        public bool Remove(T item)
        {
            var nodeTable = _nodeTable;
            var hashIndex = _hashIndex;

            // Look for the node in the current space
            var hashCode = item.GetHashCode() & 0x7fffffff;
            // Get the appropriate bucket - this indexes into HashIndex
            var hashIndexIndex = hashCode % hashIndex.Length;
            // Get the appropriate node index - remember there are no direct node references
            var chainIndex = hashIndex[hashIndexIndex];

            // Scan the chain
            int currIndex, prevIndex;
            for (prevIndex = -1, currIndex = chainIndex;
                 currIndex >= 0;
                 prevIndex = currIndex, currIndex = nodeTable[currIndex].NextNodeInChain)
            {
                // Skip entries that do not share the same hashcode
                if (nodeTable[currIndex].HashCode == hashCode)
                {
                    // Check for node equality
                    if (Equals(item, nodeTable[currIndex].Value))
                    {
                        // Node found ...
                        var prevInOrder = nodeTable[currIndex].PrevNodeInOrder;
                        var nextInOrder = nodeTable[currIndex].NextNodeInOrder;
                        var nextInChain = nodeTable[currIndex].NextNodeInChain;
                        if (prevInOrder != -1) nodeTable[prevInOrder].NextNodeInOrder = nextInOrder;
                        if (nextInOrder != -1) nodeTable[nextInOrder].PrevNodeInOrder = prevInOrder;
                        if (_tailIndex == currIndex) _tailIndex = prevInOrder;
                        if (_headIndex == currIndex) _headIndex = nextInOrder;
                        if (chainIndex == currIndex) hashIndex[hashIndexIndex] = nextInChain;
                        if (prevIndex != -1) nodeTable[prevIndex].NextNodeInChain = nextInChain;

                        nodeTable[currIndex].SetValues(default(T), -1);
                        nodeTable[currIndex].SetReferences(-1, _freeListHead, -1);
                        _freeListHead = currIndex;

                        _nodeCount--;

#if DIAGNOSTICS
                        System.Diagnostics.Debug.Print("Del: {0} => {1} / {2} / {3} / {4}",
                               id,
                               hashIndexIndex,
                               currIndex,
                               chainIndex,
                               nextInChain);
#endif

                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </summary>
        /// <value></value>
        /// <returns>
        /// The number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </returns>
        public int Count
        {
            get { return _nodeCount; }
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only.
        /// </summary>
        /// <value></value>
        /// <returns>true if the <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only; otherwise, false.
        /// </returns>
        public bool IsReadOnly
        {
            get { return false; }
        }

        /// <summary>
        /// Each node contains the content for the node and references to
        /// the next node in it's respective chain and order.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        [Serializable]
        internal struct Node
        {
            /// <summary>
            /// Gets or sets the next node in chain.
            /// </summary>
            /// <value>The next node in chain.</value>
            internal int NextNodeInChain;

            /// <summary>
            /// Gets or sets the hash code.
            /// </summary>
            /// <value>The hash code.</value>
            internal int HashCode;

            /// <summary>
            /// Gets or sets the value.
            /// </summary>
            /// <value>The value.</value>
            internal T Value;

            /// <summary>
            /// Gets or sets the next node in order.
            /// </summary>
            /// <value>The next node in order.</value>
            internal int NextNodeInOrder;

            /// <summary>
            /// Gets or sets the previous node in order.
            /// </summary>
            internal int PrevNodeInOrder;

            internal void SetReferences(int prevInOrder, int nextInOrder, int nextInChain)
            {
                PrevNodeInOrder = prevInOrder;
                NextNodeInOrder = nextInOrder;
                NextNodeInChain = nextInChain;
            }

            internal void SetValues(T value, int hashCode)
            {
                Value = value;
                HashCode = hashCode;
                NextNodeInChain = -1;
                NextNodeInOrder = -1;
                PrevNodeInOrder = -1;
            }
        }
    }
}

