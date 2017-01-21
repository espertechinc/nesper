///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.IO;

namespace com.espertech.esper.filter
{
    /// <summary>
    /// Record to a <seealso cref="FilterSet"/> filter set taken from a <seealso cref="FilterService"/>.
    /// </summary>
    public class FilterSetEntry
    {
        /// <summary>Ctor. </summary>
        /// <param name="handle">handle</param>
        /// <param name="filterValueSet">values</param>
        public FilterSetEntry(FilterHandle handle,
                              FilterValueSet filterValueSet)
        {
            Handle = handle;
            FilterValueSet = filterValueSet;
        }

        /// <summary>Returns the handle. </summary>
        /// <value>handle</value>
        public FilterHandle Handle { get; private set; }

        /// <summary>Returns filters. </summary>
        /// <value>filters</value>
        public FilterValueSet FilterValueSet { get; private set; }

        public void AppendTo(TextWriter writer)
        {
            FilterValueSet.AppendTo(writer);
        }
    }
}