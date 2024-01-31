///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
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
    /// <summary>Hash-segmented context. </summary>
    public class ContextDescriptorHashSegmented : ContextDescriptor
    {
        /// <summary>Ctor. </summary>
        public ContextDescriptorHashSegmented()
        {
            Items = new List<ContextDescriptorHashSegmentedItem>();
        }

        /// <summary>Ctor. </summary>
        /// <param name="items">list of hash code functions and event types to apply to</param>
        /// <param name="granularity">a number between 1 and Integer.MAX for parallelism</param>
        /// <param name="preallocate">true to allocate each context partition at time of statement creation</param>
        public ContextDescriptorHashSegmented(
            IList<ContextDescriptorHashSegmentedItem> items,
            int granularity,
            bool preallocate)
        {
            Items = items;
            Granularity = granularity;
            IsPreallocate = preallocate;
        }

        /// <summary>Returns hash items. </summary>
        /// <value>hash items</value>
        public IList<ContextDescriptorHashSegmentedItem> Items { get; set; }

        /// <summary>Returns the granularity. </summary>
        /// <value>granularity</value>
        public int Granularity { get; set; }

        /// <summary>Returns flag indicating whether to allocate context partitions upon statement creation, or only when actually referred to </summary>
        /// <value>preallocation flag</value>
        public bool IsPreallocate { get; set; }

        public void ToEPL(
            TextWriter writer,
            EPStatementFormatter formatter)
        {
            writer.Write("coalesce ");
            var delimiter = "";
            foreach (var item in Items) {
                writer.Write(delimiter);
                item.ToEPL(writer, formatter);
                delimiter = ", ";
            }

            writer.Write(" granularity ");
            writer.Write(Convert.ToString(Granularity));
            if (IsPreallocate) {
                writer.Write(" preallocate");
            }
        }
    }
}