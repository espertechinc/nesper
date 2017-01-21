///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

namespace com.espertech.esper.filter
{
    /// <summary>
    /// Comparator for DoubleRange values. 
    /// <para />
    /// Sorts double ranges as this:
    ///      sort by min asc, max asc. I.e. same minimum value sorts maximum value ascending.
    /// </summary>
    public sealed class StringRangeComparator : IComparer<StringRange>
    {
        public int Compare(StringRange r1, StringRange r2)
        {
            if (r1.Min == null)
            {
                if (r2.Min != null)
                {
                    return -1;
                }
            }
            else
            {
                if (r2.Min == null)
                {
                    return 1;
                }
                int comp = r1.Min.CompareTo(r2.Min);
                if (comp != 0)
                {
                    return comp;
                }
            }

            if (r1.Max == null)
            {
                if (r2.Max != null)
                {
                    return 1;
                }
                return 0;
            }
            else
            {
                if (r2.Max == null)
                {
                    return 0;
                }
                return r1.Max.CompareTo(r2.Max);
            }
        }
    }
}
