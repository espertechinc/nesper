///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client.context;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.core.context.mgr
{
    public class ContextStatePathDescriptor
    {
        public ContextStatePathDescriptor(
            OrderedDictionary<ContextStatePathKey, ContextStatePathValue> paths,
            IDictionary<int, ContextPartitionDescriptor> contextPartitionInformation)
        {
            Paths = paths;
            ContextPartitionInformation = contextPartitionInformation;
        }

        public OrderedDictionary<ContextStatePathKey, ContextStatePathValue> Paths { get; private set; }

        public IDictionary<int, ContextPartitionDescriptor> ContextPartitionInformation { get; private set; }
    }
}