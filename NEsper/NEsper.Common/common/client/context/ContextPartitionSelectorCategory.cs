///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

namespace com.espertech.esper.common.client.context
{
    /// <summary>
    /// Selects context partitions for use with a category context by providing a set of labels.
    /// </summary>
    public interface ContextPartitionSelectorCategory : ContextPartitionSelector
    {
        /// <summary>Returns a set of category label names. </summary>
        /// <value>label names</value>
        ICollection<string> Labels { get; }
    }

    public class ProxyContextPartitionSelectorCategory : ContextPartitionSelectorCategory
    {
        public Func<ICollection<string>> ProcLabels { get; set; }

        public ProxyContextPartitionSelectorCategory()
        {
        }

        public ProxyContextPartitionSelectorCategory(Func<ICollection<string>> procLabels)
        {
            ProcLabels = procLabels;
        }

        public ICollection<string> Labels {
            get { return ProcLabels.Invoke(); }
        }
    }
}