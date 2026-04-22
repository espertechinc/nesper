///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.common.@internal.filterspec
{
    /// <summary>
    ///     Holds a range of string values with a minimum (start) value and a maximum (end) value.
    /// </summary>
    public readonly struct StringRange : Range
    {
        private readonly int _hashCode;

        /// <summary>
        ///     Constructor - takes range endpoints.
        /// </summary>
        /// <param name="min">is the low endpoint</param>
        /// <param name="max">is the high endpoint</param>
        public StringRange(
            string min,
            string max)
        {
            if (min != null && max != null && min.CompareTo(max) > 0) {
                Min = max;
                Max = min;
            }
            else {
                Min = min;
                Max = max;
            }

            _hashCode = 7;
            if (Min != null) {
                _hashCode = 31 * _hashCode;
                _hashCode ^= Min.GetHashCode();
            }

            if (Max != null) {
                _hashCode = 31 * _hashCode;
                _hashCode ^= Max.GetHashCode();
            }
        }

        /// <summary>
        ///     Returns low endpoint.
        /// </summary>
        /// <returns>low endpoint</returns>
        public string Min { get; }

        /// <summary>
        ///     Returns high endpoint.
        /// </summary>
        /// <returns>high endpoint</returns>
        public string Max { get; }

        public object LowEndpoint => Min;

        public object HighEndpoint => Max;

        public override bool Equals(object o)
        {
            if (!(o is StringRange that)) {
                return false;
            }

            if (_hashCode != that._hashCode) {
                return false;
            }

            return string.Equals(Max, that.Max) && string.Equals(Min, that.Min);
        }

        public bool Equals(StringRange that)
        {
            if (_hashCode != that._hashCode) {
                return false;
            }

            return string.Equals(Max, that.Max) && string.Equals(Min, that.Min);
        }

        public override int GetHashCode()
        {
            return _hashCode;
        }

        public override string ToString()
        {
            return "StringRange" +
                   " min=" +
                   Min +
                   " max=" +
                   Max;
        }
    }
} // end of namespace