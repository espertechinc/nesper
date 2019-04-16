///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.common.@internal.filterspec
{
    /// <summary>
    ///     Holds a range of double values with a minimum (start) value and a maximum (end) value.
    /// </summary>
    [Serializable]
    public class StringRange : Range
    {
        private readonly int hashCode;

        /// <summary>
        ///     Constructor - takes range endpoints.
        /// </summary>
        /// <param name="min">is the low endpoint</param>
        /// <param name="max">is the high endpoint</param>
        public StringRange(
            string min,
            string max)
        {
            Min = min;
            Max = max;

            if (min != null && max != null) {
                if (min.CompareTo(max) > 0) {
                    Max = min;
                    Min = max;
                }
            }

            hashCode = 7;
            if (min != null) {
                hashCode = 31 * hashCode;
                hashCode ^= min.GetHashCode();
            }

            if (max != null) {
                hashCode = 31 * hashCode;
                hashCode ^= max.GetHashCode();
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
            if (this == o) {
                return true;
            }

            if (o == null || GetType() != o.GetType()) {
                return false;
            }

            var that = (StringRange) o;

            if (hashCode != that.hashCode) {
                return false;
            }

            if (Max != null ? !Max.Equals(that.Max) : that.Max != null) {
                return false;
            }

            if (Min != null ? !Min.Equals(that.Min) : that.Min != null) {
                return false;
            }

            return true;
        }

        public override int GetHashCode()
        {
            return hashCode;
        }

        public override string ToString()
        {
            return "StringRange" +
                   " min=" + Min +
                   " max=" + Max;
        }
    }
} // end of namespace