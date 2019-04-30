///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System;
using System.Collections.Generic;

namespace com.espertech.esper.common.@internal.type
{
    /// <summary>
    /// Parameter supplying a single int value is a set of numbers.
    /// </summary>
    [Serializable]
    public class IntParameter : NumberSetParameter
    {
        /// <summary> Returns int value.</summary>
        /// <returns> int value
        /// </returns>
        public int IntValue { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="IntParameter"/> class.
        /// </summary>
        public IntParameter()
        {
        }

        /// <summary> Ctor.</summary>
        /// <param name="intValue">single in value
        /// </param>
        public IntParameter(int intValue)
        {
            IntValue = intValue;
        }

        /// <summary>
        /// Returns true if all values between and including min and max are supplied by the parameter.
        /// </summary>
        /// <param name="min">lower end of range</param>
        /// <param name="max">upper end of range</param>
        /// <returns>
        /// true if parameter specifies all int values between min and max, false if not
        /// </returns>
        public virtual bool IsWildcard(
            int min,
            int max)
        {
            if ((IntValue == min) && (IntValue == max)) {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Return a set of int values representing the value of the parameter for the given range.
        /// </summary>
        /// <param name="min">lower end of range</param>
        /// <param name="max">upper end of range</param>
        /// <returns>set of integer</returns>
        public ICollection<int> GetValuesInRange(
            int min,
            int max)
        {
            ICollection<int> values = new HashSet<int>();

            if ((IntValue >= min) && (IntValue <= max)) {
                values.Add(IntValue);
            }

            return values;
        }

        public bool ContainsPoint(int point)
        {
            return IntValue == point;
        }

        public string Formatted()
        {
            return IntValue.ToString();
        }
    }
}