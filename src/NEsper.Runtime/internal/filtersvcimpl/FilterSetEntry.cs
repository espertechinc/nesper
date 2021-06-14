///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.IO;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.filterspec;
using com.espertech.esper.common.@internal.filtersvc;

namespace com.espertech.esper.runtime.@internal.filtersvcimpl
{
    /// <summary>
    ///     Entry to a <seealso cref="FilterSet" /> filter set taken from a <seealso cref="FilterService" />.
    /// </summary>
    public class FilterSetEntry
    {
        public FilterSetEntry(
            FilterHandle handle,
            EventType eventType,
            FilterValueSetParam[][] valueSet)
        {
            Handle = handle;
            EventType = eventType;
            ValueSet = valueSet;
        }

        /// <summary>
        ///     Returns the handle.
        /// </summary>
        /// <returns>handle</returns>
        public FilterHandle Handle { get; }

        public EventType EventType { get; }

        public FilterValueSetParam[][] ValueSet { get; }

        public void AppendTo(StringWriter writer)
        {
            writer.Write(EventType.Name);
            writer.Write("(");
            var delimiter = "";
            foreach (var param in ValueSet)
            {
                writer.Write(delimiter);
                AppendTo(writer, param);
                delimiter = " or ";
            }

            writer.Write(")");
        }

        private void AppendTo(
            StringWriter writer,
            FilterValueSetParam[] parameters)
        {
            var delimiter = "";
            foreach (var param in parameters)
            {
                writer.Write(delimiter);
                param.AppendTo(writer);
                delimiter = ",";
            }
        }
    }
} // end of namespace