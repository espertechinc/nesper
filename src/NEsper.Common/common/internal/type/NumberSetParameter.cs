///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

namespace com.espertech.esper.common.@internal.type
{
    /// <summary>
    /// Interface to generate a set of integers from parameters that include ranges, lists and frequencies.
    /// </summary>
    public interface NumberSetParameter
    {
        /// <summary> Returns true if all values between and including min and max are supplied by the parameter.</summary>
        /// <param name="min">lower end of range
        /// </param>
        /// <param name="max">upper end of range
        /// </param>
        /// <returns> true if parameter specifies all int values between min and max, false if not
        /// </returns>
        bool IsWildcard(
            int min,
            int max);

        /// <summary> Return a set of int values representing the value of the parameter for the given range.</summary>
        /// <param name="min">lower end of range
        /// </param>
        /// <param name="max">upper end of range
        /// </param>
        /// <returns> set of integer
        /// </returns>
        ICollection<int> GetValuesInRange(
            int min,
            int max);

        bool ContainsPoint(int point);

        string Formatted();
    }
}