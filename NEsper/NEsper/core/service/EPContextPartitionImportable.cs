///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.compat.collections;
using com.espertech.esper.core.context.mgr;

namespace com.espertech.esper.core.service
{
    [Serializable]
    public class EPContextPartitionImportable
    {
        public EPContextPartitionImportable(OrderedDictionary<ContextStatePathKey, ContextStatePathValue> paths)
        {
            Paths = paths;
        }

        public OrderedDictionary<ContextStatePathKey, ContextStatePathValue> Paths { get; private set; }
    }
}
