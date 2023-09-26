///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.compat;
using com.espertech.esper.compat.io;

namespace com.espertech.esper.common.@internal.collection
{
    public class SortedDoubleVector
    {
        private readonly List<double> _values;

        /// <summary>
        /// Constructor.
        /// </summary>
        public SortedDoubleVector()
        {
            _values = new List<double>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SortedDoubleVector"/> class.
        /// </summary>
        /// <param name="values">The values.</param>
        public SortedDoubleVector(IList<double> values)
        {
            _values = new List<double>(values);
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
        public virtual double this[int index] => _values[index];

        /// <summary> Add a value to the collection.</summary>
        /// <param name="val">is the double-type value to add
        /// </param>
        public virtual void Add(double val)
        {
            if (double.IsNaN(val)) {
                return;
            }

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
        public virtual void Remove(double val)
        {
            if (double.IsNaN(val)) {
                return;
            }

            var index = FindInsertIndex(val);
            if (index == -1) {
                return;
            }

            double? valueAtIndex = _values[index];
            if (valueAtIndex != null && valueAtIndex.Value != val) {
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

        public IList<double> Values => _values;

        /// <summary> Returns the index into which to insert to.</summary>
        /// <param name="val">to find insert index
        /// </param>
        /// <returns> position to insert the value to, or -1 to indicate to add to the end.
        /// </returns>
        public virtual int FindInsertIndex(double val)
        {
            if (_values.Count > 2) {
                var startIndex = _values.Count >> 1;
                var startValue = _values[startIndex];
                var insertAt = -1;

                if (val < startValue) {
                    // find in lower half
                    insertAt = FindInsertIndex(0, startIndex - 1, val);
                }
                else if (val > startValue) {
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
                if (val > _values[1]) {
                    return -1;
                }
                else if (val <= _values[0]) {
                    return 0;
                }
                else {
                    return 1;
                }
            }

            if (_values.Count == 1) {
                if (val > _values[0]) {
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
            double val)
        {
            while (true) {
                if (upperBound == lowerBound) {
                    var valueLowerBound = _values[lowerBound];
                    if (val <= valueLowerBound) {
                        return lowerBound;
                    }
                    else {
                        return lowerBound + 1;
                    }
                }

                if (upperBound - lowerBound == 1) {
                    var valueLowerBound = _values[lowerBound];
                    if (val <= valueLowerBound) {
                        return lowerBound;
                    }

                    var valueUpperBound = _values[upperBound];
                    if (val > valueUpperBound) {
                        return upperBound + 1;
                    }

                    return upperBound;
                }

                var nextMiddle = lowerBound + ((upperBound - lowerBound) >> 1);
                var valueAtMiddle = _values[nextMiddle];

                if (val < valueAtMiddle) {
                    // find in lower half
                    upperBound = nextMiddle - 1;
                }
                else if (val > valueAtMiddle) {
                    // find in upper half
                    lowerBound = nextMiddle;
                }
                else {
                    return nextMiddle;
                }
            }
        }

        /// <summary>
        /// NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        public static void WritePoints(
            DataOutput output,
            SortedDoubleVector vector)
        {
            output.WriteInt(vector.Values.Count);
            foreach (var num in vector.Values) {
                output.WriteDouble(num);
            }
        }

        /// <summary>
        /// NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        public static SortedDoubleVector ReadPoints(DataInput input)
        {
            var points = new SortedDoubleVector();
            var size = input.ReadInt();
            for (var i = 0; i < size; i++) {
                points.Add(input.ReadDouble());
            }

            return points;
        }
    }
}