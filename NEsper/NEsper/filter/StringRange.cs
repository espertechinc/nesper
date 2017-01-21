///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.filter
{
    /// <summary>Holds a range of double values with a minimum (start) value and a maximum (end) value. </summary>
    public sealed class StringRange : Range
    {
        private readonly String _min;
        private readonly String _max;
        private readonly int _hashCode;

        /// <summary>Constructor - takes range endpoints. </summary>
        /// <param name="min">is the low endpoint</param>
        /// <param name="max">is the high endpoint</param>
        public StringRange(String min, String max)
        {
            _min = min;
            _max = max;

            if ((min != null) && (max != null))
            {
                if (min.CompareTo(max) > 0)
                {
                    _max = min;
                    _min = max;
                }
            }

            unchecked
            {
                _hashCode = (min != null ? min.GetHashCode() : 0);
                _hashCode = (_hashCode * 397) ^ (max != null ? max.GetHashCode() : 0);
                _hashCode = (_hashCode * 397) ^ _hashCode;
            }
        }

        public object LowEndpoint
        {
            get { return _min; }
        }

        public object HighEndpoint
        {
            get { return _max; }
        }

        /// <summary>Returns low endpoint. </summary>
        /// <value>low endpoint</value>
        public string Min
        {
            get { return _min; }
        }

        /// <summary>Returns high endpoint. </summary>
        /// <value>high endpoint</value>
        public string Max
        {
            get { return _max; }
        }

        public bool Equals(StringRange other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other._min, _min) && Equals(other._max, _max);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof (StringRange)) return false;
            return Equals((StringRange) obj);
        }

        public override int GetHashCode()
        {
            unchecked {
                int result = (_min != null ? _min.GetHashCode() : 0);
                result = (result*397) ^ (_max != null ? _max.GetHashCode() : 0);
                result = (result*397) ^ _hashCode;
                return result;
            }
        }

        public override String ToString()
        {
            return string.Format("StringRange min={0} max={1}", _min, _max);
        }
    }
}