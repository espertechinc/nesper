///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.compat;

namespace com.espertech.esper.collection
{
    public class SortedVector<T> where T : IComparable
    {
        private readonly List<T> _values;
        private readonly IComparer<T> _comparer;

        /// <summary>
        /// Constructor.
        /// </summary>

        public SortedVector()
        {
            _values = new List<T>();
            _comparer = new DefaultComparer();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SortedVector{T}"/> class.
        /// </summary>
        /// <param name="values">The values.</param>
        public SortedVector(List<T> values)
        {
            _values = values;
            _comparer = new DefaultComparer();
        }

        /// <summary> Returns the number of items in the collection.</summary>
        /// <returns> size
        /// </returns>
        public virtual int Count
        {
            get { return _values.Count; }
        }

        /// <summary> Returns the value at a given index.</summary>
        /// <param name="index">for which to return value for
        /// </param>
        /// <returns> value at index
        /// </returns>
        public virtual T this[int index]
        {
            get { return _values[index]; }
        }

        /// <summary> Add a value to the collection.</summary>
        /// <param name="val">is the double-type value to add
        /// </param>
        public virtual void Add(T val)
        {
            int index = FindInsertIndex(val);

            if (index == -1)
            {
                _values.Add(val);
            }
            else
            {
                _values.Insert(index, val);
            }
        }

        /// <summary> Remove a value from the collection.</summary>
        /// <param name="val">to remove
        /// </param>
        /// <throws>  IllegalStateException if the value has not been added </throws>

        public virtual void Remove(T val)
        {
            int index = FindInsertIndex(val);
            if (index == -1)
            {
                throw new IllegalStateException("Value not found in collection");
            }

            T valueAtIndex = _values[index];
            if (IsEQ(valueAtIndex, val))
            {
                throw new IllegalStateException("Value not found in collection");
            }
            _values.RemoveAt(index);
        }

        /// <summary>
        /// Clear out the collection.
        /// </summary>
        public virtual void Clear()
        {
            _values.Clear();
        }

        /// <summary>
        /// Returns underlying vector, for testing purposes only.
        /// </summary>
        /// <returns>vector with double values</returns>

        public IList<T> Values
        {
            get { return _values; }
        }

        private bool IsLT(T x, T y)
        {
            return _comparer.Compare(x, y) < 0;
        }

        private bool IsLTE(T x, T y)
        {
            return _comparer.Compare(x, y) <= 0;
        }

        private bool IsGT(T x, T y)
        {
            return _comparer.Compare(x, y) > 0;
        }

        private bool IsGTE(T x, T y)
        {
            return _comparer.Compare(x, y) >= 0;
        }

        private bool IsEQ(T x, T y)
        {
            return _comparer.Compare(x, y) == 0;
        }

        /// <summary> Returns the index into which to insert to.
        /// Proptected access level for convenient testing.
        /// </summary>
        /// <param name="val">to find insert index
        /// </param>
        /// <returns> position to insert the value to, or -1 to indicate to add to the end.
        /// </returns>
        public virtual int FindInsertIndex(T val)
        {
            if (_values.Count > 2)
            {
                var startIndex = _values.Count >> 1;
                var startValue = _values[startIndex];
                var insertAt = -1;

                if (IsLT(val, startValue))
                {
                    // find in lower half
                    insertAt = FindInsertIndex(0, startIndex - 1, val);
                }
                else if (IsGT(val, startValue))
                {
                    // find in upper half
                    insertAt = FindInsertIndex(startIndex + 1, _values.Count - 1, val);
                }
                else
                {
                    // we hit the value
                    insertAt = startIndex;
                }

                if (insertAt == _values.Count)
                {
                    return -1;
                }
                return insertAt;
            }

            if (_values.Count == 2)
            {
                if (IsGT(val, _values[1]))
                {
                    return -1;
                }
                else if (IsLTE(val, _values[0]))
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }

            if (_values.Count == 1)
            {
                if (IsGT(val, _values[0]))
                {
                    return -1;
                }
                else
                {
                    return 0;
                }
            }

            return -1;
        }

        private int FindInsertIndex(int lowerBound, int upperBound, T val)
        {
            while (true)
            {
                if (upperBound == lowerBound)
                {
                    T valueLowerBound = _values[lowerBound];
                    if (IsLTE(val, valueLowerBound))
                    {
                        return lowerBound;
                    }
                    else
                    {
                        return lowerBound + 1;
                    }
                }

                if (upperBound - lowerBound == 1)
                {
                    T valueLowerBound = _values[lowerBound];
                    if (IsLTE(val, valueLowerBound))
                    {
                        return lowerBound;
                    }

                    T valueUpperBound = _values[upperBound];
                    if (IsGT(val, valueUpperBound))
                    {
                        return upperBound + 1;
                    }

                    return upperBound;
                }

                int nextMiddle = lowerBound + ((upperBound - lowerBound) >> 1);
                T valueAtMiddle = _values[nextMiddle];

                if (IsLT(val, valueAtMiddle))
                {
                    // find in lower half
                    upperBound = nextMiddle - 1;
                }
                else if (IsGT(val, valueAtMiddle))
                {
                    // find in upper half
                    lowerBound = nextMiddle;
                }
                else
                {
                    return nextMiddle;
                }
            }
        }

        internal class DefaultComparer : Comparer<T>
        {
            #region Overrides of Comparer<T>

            public override int Compare(T x, T y)
            {
                return ((IComparable)x).CompareTo(y);
            }

            #endregion
        }
    }
}
