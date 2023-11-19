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

using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.client.soda
{
    /// <summary>
    ///     Context dimension information for keyed segmented context.
    /// </summary>
    public class ContextDescriptorKeyedSegmented : ContextDescriptor
    {
        private IList<ContextDescriptorConditionFilter> initiationConditions;
        private IList<ContextDescriptorKeyedSegmentedItem> items;
        private ContextDescriptorCondition terminationCondition;

        /// <summary>
        ///     Ctor.
        /// </summary>
        public ContextDescriptorKeyedSegmented()
        {
            items = new List<ContextDescriptorKeyedSegmentedItem>();
        }

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="items">key set descriptions</param>
        public ContextDescriptorKeyedSegmented(IList<ContextDescriptorKeyedSegmentedItem> items)
        {
            this.items = items;
        }

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="items">key set descriptions</param>
        /// <param name="initiationConditions">initialization conditions</param>
        /// <param name="terminationCondition">termination condition</param>
        public ContextDescriptorKeyedSegmented(
            IList<ContextDescriptorKeyedSegmentedItem> items,
            IList<ContextDescriptorConditionFilter> initiationConditions,
            ContextDescriptorCondition terminationCondition)
        {
            this.items = items;
            this.initiationConditions = initiationConditions;
            this.terminationCondition = terminationCondition;
        }

        /// <summary>
        ///     Returns the key set descriptions
        /// </summary>
        /// <returns>list</returns>
        public IList<ContextDescriptorKeyedSegmentedItem> Items {
            get => items;
            set => items = value;
        }

        /// <summary>
        ///     Returns the terminating condition or null if there is none
        /// </summary>
        /// <returns>condition</returns>
        public ContextDescriptorCondition TerminationCondition {
            get => terminationCondition;
            set => terminationCondition = value;
        }

        /// <summary>
        ///     Returns the initiation conditions, if any.
        /// </summary>
        /// <returns>null or list of filters for initiation</returns>
        public IList<ContextDescriptorConditionFilter> InitiationConditions {
            get => initiationConditions;
            set => initiationConditions = value;
        }

        public void ToEPL(
            TextWriter writer,
            EPStatementFormatter formatter)
        {
            writer.Write("partition by ");
            var delimiter = "";
            foreach (var item in items) {
                writer.Write(delimiter);
                item.ToEPL(writer, formatter);
                delimiter = ", ";
            }

            if (initiationConditions != null && !initiationConditions.IsEmpty()) {
                writer.Write(" initiated by ");
                var delimiterInit = "";
                foreach (var filter in initiationConditions) {
                    writer.Write(delimiterInit);
                    filter.ToEPL(writer, formatter);
                    delimiterInit = ", ";
                }
            }

            if (terminationCondition != null) {
                writer.Write(" terminated by ");
                terminationCondition.ToEPL(writer, formatter);
            }
        }
    }
} // end of namespace