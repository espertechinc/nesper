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

namespace com.espertech.esper.common.@internal.collection
{
    public class SortedDoubleVector
    {
        private readonly List<Double> _values;

        /// <summary>
        /// Constructor.
        /// </summary>
        public SortedDoubleVector()
        {
            _values = new List<Double>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SortedDoubleVector"/> class.
        /// </summary>
        /// <param name="values">The values.</param>
        public SortedDoubleVector(List<Double> values)
        {
            _values = values;
        }

        /// <summary> Returns the number of items in the collection.</summary>
        /// <returns> size
        /// </returns>
        public virtual int Count {
            get { return _values.Count; }
        }

        /// <summary> Returns the value at a given index.</summary>
        /// <param name="index">for which to return value for
        /// </param>
        /// <returns> value at index
        /// </returns>
        public virtual double this[int index] {
            get { return _values[index]; }
        }

        /// <summary> Add a value to the collection.</summary>
        /// <param name="val">is the double-type value to add
        /// </param>
        public virtual void Add(double val)
        {
            if (Double.IsNaN(val)) {
                return;
            }

            int index = FindInsertIndex(val);

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
            if (Double.IsNaN(val)) {
                return;
            }

            int index = FindInsertIndex(val);
            if (index == -1) {
                throw new IllegalStateException("Value not found in collection");
            }

            double? valueAtIndex = _values[index];
            if ((valueAtIndex != null) && (valueAtIndex != val)) {
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

        public IList<Double> Values {
            get { return _values; }
        }

        /// <summary> Returns the index into which to insert to.
        /// Proptected access level for convenient testing.
        /// </summary>
        /// <param name="val">to find insert index
        /// </param>
        /// <returns> position to insert the value to, or -1 to indicate to add to the end.
        /// </returns>
        public virtual int FindInsertIndex(double val)
        {
            if (_values.Count > 2) {
                int startIndex = _values.Count >> 1;
                double startValue = _values[startIndex];
                int insertAt = -1;

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
                    double valueLowerBound = _values[lowerBound];
                    if (val <= valueLowerBound) {
                        return lowerBound;
                    }
                    else {
                        return lowerBound + 1;
                    }
                }

                if (upperBound - lowerBound == 1) {
                    double valueLowerBound = _values[lowerBound];
                    if (val <= valueLowerBound) {
                        return lowerBound;
                    }

                    double valueUpperBound = _values[upperBound];
                    if (val > valueUpperBound) {
                        return upperBound + 1;
                    }

                    return upperBound;
                }

                int nextMiddle = lowerBound + ((upperBound - lowerBound) >> 1);
                double valueAtMiddle = _values[nextMiddle];

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
    }
}