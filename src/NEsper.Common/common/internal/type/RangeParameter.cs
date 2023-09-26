///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

namespace com.espertech.esper.common.@internal.type
{
    /// <summary>
    /// Represents a range of numbers as a parameter.
    /// </summary>
    public class RangeParameter : NumberSetParameter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RangeParameter"/> class.
        /// </summary>
        public RangeParameter()
        {
        }

        /// <summary>Ctor. </summary>
        /// <param name="low">start of range</param>
        /// <param name="high">end of range</param>
        public RangeParameter(
            int low,
            int high)
        {
            Low = low;
            High = high;
        }

        /// <summary>Returns start of range. </summary>
        /// <value>start of range</value>
        public int Low { get; set; }

        /// <summary>Returns end of range. </summary>
        /// <value>end of range</value>
        public int High { get; set; }

        public bool IsWildcard(
            int min,
            int max)
        {
            if (min >= Low && max <= High) {
                return true;
            }

            return false;
        }

        public ICollection<int> GetValuesInRange(
            int min,
            int max)
        {
            ICollection<int> values = new HashSet<int>();

            var start = min > Low ? min : Low;
            var end = max > High ? High : max;

            while (start <= end) {
                values.Add(start);
                start++;
            }

            return values;
        }

        public bool ContainsPoint(int point)
        {
            return Low <= point && point <= High;
        }

        public string Formatted()
        {
            return Low + "-" + High;
        }
    }
}