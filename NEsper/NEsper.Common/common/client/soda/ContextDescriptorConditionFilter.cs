///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

namespace com.espertech.esper.common.client.soda
{
    /// <summary>
    /// Context condition that start/initiated or ends/terminates context partitions based on a filter expression.
    /// </summary>
    [Serializable]
    public class ContextDescriptorConditionFilter : ContextDescriptorCondition
    {
        /// <summary>Ctor. </summary>
        public ContextDescriptorConditionFilter()
        {
        }

        /// <summary>Ctor. </summary>
        /// <param name="filter">event filter</param>
        /// <param name="optionalAsName">tag name of the filtered events</param>
        public ContextDescriptorConditionFilter(
            Filter filter,
            string optionalAsName)
        {
            Filter = filter;
            OptionalAsName = optionalAsName;
        }

        /// <summary>Returns the event stream filter. </summary>
        /// <value>filter</value>
        public Filter Filter { get; set; }

        /// <summary>Returns the tag name assigned, if any. </summary>
        /// <value>tag name</value>
        public string OptionalAsName { get; set; }

        public void ToEPL(
            TextWriter writer,
            EPStatementFormatter formatter)
        {
            Filter.ToEPL(writer, formatter);
            if (OptionalAsName != null)
            {
                writer.Write(" as ");
                writer.Write(OptionalAsName);
            }
        }
    }
}