///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.IO;

namespace com.espertech.esper.filter
{
    /// <summary>Holder object for a set of filters for one or more statements. </summary>
    public class FilterSet
    {
        /// <summary>Ctor. </summary>
        /// <param name="filters">set of filters</param>
        public FilterSet(IList<FilterSetEntry> filters)
        {
            Filters = filters;
        }

        /// <summary>Returns the filters. </summary>
        /// <value>filters</value>
        public IList<FilterSetEntry> Filters { get; private set; }

        public override string ToString()
        {
            var filterTexts = new List<string>();
            foreach (FilterSetEntry entry in Filters)
            {
                var writer = new StringWriter();
                entry.AppendTo(writer);
                filterTexts.Add(writer.ToString());
            }

            filterTexts.Sort();

            var writerX = new StringWriter();
            var delimiter = "";
            foreach (var filterText in filterTexts)
            {
                writerX.Write(delimiter);
                writerX.Write(filterText);
                delimiter = ",";
            }
            return writerX.ToString();
        }
    }
}