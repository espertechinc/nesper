///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.collection
{
    public class SortedVector<T>
    {
        private readonly List<T> _values;
        private readonly IComparer<T> _comparer;

        /// <summary>
        /// Constructor.
        /// </summary>
        public SortedVector(IComparer<T> comparer)
        {
            _values = new List<T>();
            _comparer = comparer;
        }

        /// <summary>
        /// Initializes a new instance of the <see>
        ///     <cref>SortedVector</cref>
        /// </see>
        /// class.
        /// </summary>
        /// <param name="values">The values.</param>
        /// <param name="comparer"></param>
        public SortedVector(
            IList<T> values,
            IComparer<T> comparer)
        {
            _values = new List<T>(values);
            _comparer = comparer;
        }

        /// <summary> Returns the number of items in the collection.</summary>
        /// <returns> size
        /// </returns>
        public virtual int Count => _values.Count;

        /// <summary> Returns the value at a given index.</summary>
        /// <param name="index">for which to return value for
        /// </param>
        /// <returns> value at index
        /// </returns>
        public virtual T this[int index] => _values[index];

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool IsNaN(T value)
        {
            return false;
        }

        /// <summary> Add a value to the collection.</summary>
        /// <param name="val">is the double-type value to add
        /// </param>
        public virtual void Add(T val)
        {
            var index = FindInsertIndex(val);
            if (index == -1) {
                _values.Add(val);
            }
            else {
                _values.Insert(index, val);
            }
        }

        /// <summary> Remove a value from the collection.</summary>
        /// <param name="val">to remove
        /// </param>
        /// <throws>  IllegalStateException if the value has not been added </throws>
        public virtual void Remove(T val)
        {
            var index = FindInsertIndex(val);
            if (index == -1) {
                return;
            }

            var valueAtIndex = _values[index];
            if (valueAtIndex != null && _comparer.Compare(valueAtIndex, val) != 0) {
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

        public IList<T> Values => _values;

        /// <summary>
        /// Performs a comparison using the internal comparer.
        /// </summary>
        /// <param name="value1"></param>
        /// <param name="value2"></param>
        /// <returns></returns>
        public int Compare(
            T value1,
            T value2)
        {
            return _comparer.Compare(value1, value2);
        }

        /// <summary> Returns the index into which to insert to.</summary>
        /// <param name="val">to find insert index
        /// </param>
        /// <returns> position to insert the value to, or -1 to indicate to add to the end.
        /// </returns>
        public virtual int FindInsertIndex(T val)
        {
            if (_values.Count > 2) {
                var startIndex = _values.Count >> 1;
                var startValue = _values[startIndex];
                var insertAt = -1;

                var cmp = _comparer.Compare(val, startValue);
                if (cmp < 0) {
                    // find in lower half
                    insertAt = FindInsertIndex(0, startIndex - 1, val);
                }
                else if (cmp > 0) {
                    // find in upper half
                    insertAt = FindInsertIndex(startIndex + 1, _values.Count - 1, val);
                }
                else {
                    // we hit the value
                    insertAt = startIndex;
                }

                if (insertAt == _values.Count) {
                    return -1;
                }

                return insertAt;
            }

            if (_values.Count == 2) {
                if (_comparer.Compare(val, _values[1]) < 0) {
                    return -1;
                }
                else if (_comparer.Compare(val, _values[0]) <= 0) {
                    return 0;
                }
                else {
                    return 1;
                }
            }

            if (_values.Count == 1) {
                if (_comparer.Compare(val, _values[0]) > 0) {
                    return -1;
                }
                else {
                    return 0;
                }
            }

            return -1;
        }

        private int FindInsertIndex(
            int lowerBound,
            int upperBound,
            T val)
        {
            while (true) {
                if (upperBound == lowerBound) {
                    var valueLowerBound = _values[lowerBound];
                    if (_comparer.Compare(val, valueLowerBound) <= 0) {
                        return lowerBound;
                    }
                    else {
                        return lowerBound + 1;
                    }
                }

                if (upperBound - lowerBound == 1) {
                    var valueLowerBound = _values[lowerBound];
                    if (_comparer.Compare(val, valueLowerBound) <= 0) {
                        return lowerBound;
                    }

                    var valueUpperBound = _values[upperBound];
                    if (_comparer.Compare(val, valueUpperBound) > 0) {
                        return upperBound + 1;
                    }

                    return upperBound;
                }

                var nextMiddle = lowerBound + ((upperBound - lowerBound) >> 1);
                var valueAtMiddle = _values[nextMiddle];

                if (_comparer.Compare(val, valueAtMiddle) < 0) {
                    // find in lower half
                    upperBound = nextMiddle - 1;
                }
                else if (_comparer.Compare(val, valueAtMiddle) > 0) {
                    // find in upper half
                    lowerBound = nextMiddle;
                }
                else {
                    return nextMiddle;
                }
            }
        }
    }
}