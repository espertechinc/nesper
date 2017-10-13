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
    /// Category-segmented context.
    /// </summary>
    [Serializable]
    public class ContextDescriptorCategory : ContextDescriptor
    {
        /// <summary>Ctor. </summary>
        public ContextDescriptorCategory()
        {
            Items = new List<ContextDescriptorCategoryItem>();
        }

        /// <summary>Ctor. </summary>
        /// <param name="items">categories</param>
        /// <param name="filter">event type and predicate</param>
        public ContextDescriptorCategory(IList<ContextDescriptorCategoryItem> items,
                                         Filter filter)
        {
            Items = items;
            Filter = filter;
        }

        /// <summary>Returns categories. </summary>
        /// <value>categories</value>
        public IList<ContextDescriptorCategoryItem> Items { get; set; }

        /// <summary>Returns type name and predicate expressions (filter) </summary>
        /// <value>filter</value>
        public Filter Filter { get; set; }

        #region ContextDescriptor Members

        public void ToEPL(TextWriter writer,
                          EPStatementFormatter formatter)
        {
            String delimiter = "";
            foreach (ContextDescriptorCategoryItem item in Items)
            {
                writer.Write(delimiter);
                item.ToEPL(writer, formatter);
                delimiter = ", ";
            }
            writer.Write(" from ");
            Filter.ToEPL(writer, formatter);
        }

        #endregion
    }
}