///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
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
    /// <summary>
    /// Context dimension information for keyed segmented context.
    /// </summary>
    [Serializable]
    public class ContextDescriptorKeyedSegmented : ContextDescriptor
    {
        /// <summary>Ctor. </summary>
        public ContextDescriptorKeyedSegmented()
        {
            Items = new List<ContextDescriptorKeyedSegmentedItem>();
        }

        /// <summary>Ctor. </summary>
        /// <param name="items">key set descriptions</param>
        public ContextDescriptorKeyedSegmented(IList<ContextDescriptorKeyedSegmentedItem> items)
        {
            Items = items;
        }

        /// <summary>Returns the key set descriptions </summary>
        /// <value>list</value>
        public IList<ContextDescriptorKeyedSegmentedItem> Items { get; set; }

        public void ToEPL(TextWriter writer, EPStatementFormatter formatter)
        {
            writer.Write("partition by ");
            String delimiter = "";
            foreach (ContextDescriptorKeyedSegmentedItem item in Items)
            {
                writer.Write(delimiter);
                item.ToEPL(writer, formatter);
                delimiter = ", ";
            }
        }
    }
}