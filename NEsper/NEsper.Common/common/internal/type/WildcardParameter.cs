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
    /// Represents a wildcard as a parameter.
    /// </summary>
    [Serializable]
    public class WildcardParameter : NumberSetParameter
    {
        /// <summary>
        /// Returns true if all values between and including min and max are supplied by the parameter.
        /// </summary>
        /// <param name="min">lower end of range</param>
        /// <param name="max">upper end of range</param>
        /// <returns>
        /// true if parameter specifies all int values between min and max, false if not
        /// </returns>
        public bool IsWildcard(
            int min,
            int max)
        {
            return true;
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
            ICollection<int> result = new HashSet<int>();
            for (int i = min; i <= max; i++) {
                result.Add(i);
            }

            return result;
        }

        public bool ContainsPoint(int point)
        {
            return true;
        }

        public string Formatted()
        {
            return "*";
        }
    }
}