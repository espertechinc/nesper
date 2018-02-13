///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

namespace com.espertech.esper.client.context
{
    /// <summary>
    /// Selector of context partitions for use with segmented contexts, provides a set
    /// of partition keys to select.
    /// </summary>
    public interface ContextPartitionSelectorSegmented : ContextPartitionSelector
    {
        /// <summary>Returns the partition keys. </summary>
        /// <value>key set</value>
        IList<object[]> PartitionKeys { get; }
    }

    public class ProxyContextPartitionSelectorSegmented : ContextPartitionSelectorSegmented
    {
        public Func<IList<object[]>> ProcPartitionKeys { get; set; }

        public IList<object[]> PartitionKeys
        {
            get { return ProcPartitionKeys.Invoke(); }
        }

        public ProxyContextPartitionSelectorSegmented()
        {
        }

        public ProxyContextPartitionSelectorSegmented(Func<IList<object[]>> procPartitionKeys)
        {
            ProcPartitionKeys = procPartitionKeys;
        }
    }
}