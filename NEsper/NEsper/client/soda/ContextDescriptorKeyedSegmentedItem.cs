///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;

namespace com.espertech.esper.client.soda
{
    /// <summary>Context detail for a key-filter pair for the keyed segmented context. </summary>
    public class ContextDescriptorKeyedSegmentedItem : ContextDescriptor
    {
        /// <summary>Ctor. </summary>
        public ContextDescriptorKeyedSegmentedItem()
        {
        }

        /// <summary>Ctor. </summary>
        /// <param name="propertyNames">list of property names</param>
        /// <param name="filter">event type name and optional filter predicates</param>
        public ContextDescriptorKeyedSegmentedItem(IList<string> propertyNames,
                                                   Filter filter)
        {
            PropertyNames = propertyNames;
            Filter = filter;
        }

        /// <summary>Returns the filter. </summary>
        /// <value>filter</value>
        public Filter Filter { get; set; }

        /// <summary>Returns the property names. </summary>
        /// <value>list</value>
        public IList<string> PropertyNames { get; set; }

        #region ContextDescriptor Members

        public void ToEPL(TextWriter writer,
                          EPStatementFormatter formatter)
        {
            String delimiter = "";
            foreach (String prop in PropertyNames)
            {
                writer.Write(delimiter);
                writer.Write(prop);
                delimiter = " and ";
            }
            writer.Write(" from ");
            Filter.ToEPL(writer, formatter);
        }

        #endregion
    }
}