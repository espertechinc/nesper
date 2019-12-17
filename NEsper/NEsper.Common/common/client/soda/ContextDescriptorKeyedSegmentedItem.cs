///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;

namespace com.espertech.esper.common.client.soda
{
    /// <summary>
    /// Context detail for a key-filter pair for the keyed segmented context.
    /// </summary>
    [Serializable]
    public class ContextDescriptorKeyedSegmentedItem : ContextDescriptor
    {
        private IList<string> propertyNames;
        private Filter filter;
        private string streamName;

        /// <summary>
        /// Ctor.
        /// </summary>
        public ContextDescriptorKeyedSegmentedItem()
        {
        }

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="propertyNames">list of property names</param>
        /// <param name="filter">event type name and optional filter predicates</param>
        public ContextDescriptorKeyedSegmentedItem(
            IList<string> propertyNames,
            Filter filter)
        {
            this.propertyNames = propertyNames;
            this.filter = filter;
        }

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="propertyNames">list of property names</param>
        /// <param name="filter">event type name and optional filter predicates</param>
        /// <param name="streamName">alias name</param>
        public ContextDescriptorKeyedSegmentedItem(
            IList<string> propertyNames,
            Filter filter,
            string streamName)
        {
            this.propertyNames = propertyNames;
            this.filter = filter;
            this.streamName = streamName;
        }

        /// <summary>
        /// Returns the filter.
        /// </summary>
        /// <returns>filter</returns>
        public Filter Filter
        {
            get => filter;
            set => filter = value;
        }

        /// <summary>
        /// Sets the filter.
        /// </summary>
        /// <param name="filter">filter</param>
        public ContextDescriptorKeyedSegmentedItem WithFilter(Filter filter)
        {
            this.filter = filter;
            return this;
        }

        /// <summary>
        /// Returns the property names.
        /// </summary>
        /// <returns>list</returns>
        public IList<string> PropertyNames
        {
            get => propertyNames;
            set => propertyNames = value;
        }

        /// <summary>
        /// Sets the property names.
        /// </summary>
        /// <param name="propertyNames">list</param>
        public ContextDescriptorKeyedSegmentedItem WithPropertyNames(IList<string> propertyNames)
        {
            this.propertyNames = propertyNames;
            return this;
        }

        /// <summary>
        /// Returns the stream name.
        /// </summary>
        /// <returns>stream name</returns>
        public string StreamName
        {
            get => streamName;
            set => streamName = value;
        }

        /// <summary>
        /// Sets the stream name.
        /// </summary>
        /// <param name="streamName">stream name</param>
        public ContextDescriptorKeyedSegmentedItem WithStreamName(string streamName)
        {
            this.streamName = streamName;
            return this;
        }

        public void ToEPL(
            TextWriter writer,
            EPStatementFormatter formatter)
        {
            string delimiter = "";
            foreach (string prop in propertyNames)
            {
                writer.Write(delimiter);
                writer.Write(prop);
                delimiter = " and ";
            }

            writer.Write(" from ");
            filter.ToEPL(writer, formatter);
            if (streamName != null)
            {
                writer.Write(" as ");
                writer.Write(streamName);
            }
        }
    }
} // end of namespace