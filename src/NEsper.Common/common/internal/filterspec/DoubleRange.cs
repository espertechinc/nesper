///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.filterspec
{
    /// <summary>
    ///     Holds a range of double values with a minimum (start) value and a maximum (end) value.
    /// </summary>
    [Serializable]
    public class DoubleRange : Range
    {
        private readonly int _hashCode;

        /// <summary>
        ///     Constructor - takes range endpoints.
        /// </summary>
        /// <param name="min">is the low endpoint</param>
        /// <param name="max">is the high endpoint</param>
        public DoubleRange(
            double? min,
            double? max)
        {
            Min = min;
            Max = max;

            if (min != null && max != null) {
                if (min > max) {
                    Max = min;
                    Min = max;
                }
            }

            _hashCode = 7;
            if (min != null) {
                _hashCode = 31 * _hashCode;
                _hashCode ^= min.GetHashCode();
            }

            if (max != null) {
                _hashCode = 31 * _hashCode;
                _hashCode ^= max.GetHashCode();
            }
        }

        /// <summary>
        ///     Returns low endpoint.
        /// </summary>
        /// <returns>low endpoint</returns>
        public double? Min { get; }

        /// <summary>
        ///     Returns high endpoint.
        /// </summary>
        /// <returns>high endpoint</returns>
        public double? Max { get; }

        public object HighEndpoint => Max;

        public object LowEndpoint => Min;

        public override bool Equals(object other)
        {
            if (other == this) {
                return true;
            }

            if (!(other is DoubleRange)) {
                return false;
            }

            var otherRange = (DoubleRange) other;

            return otherRange.Max.AsDouble() == Max && otherRange.Min.AsDouble() == Min;
        }

        public override int GetHashCode()
        {
            return _hashCode;
        }

        public override string ToString()
        {
            return "DoubleRange" +
                   " min=" +
                   Min +
                   " max=" +
                   Max;
        }
    }
} // end of namespace