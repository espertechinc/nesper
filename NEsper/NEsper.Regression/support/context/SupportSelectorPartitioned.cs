///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.context;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.regressionlib.support.context
{
    public class SupportSelectorPartitioned : ContextPartitionSelectorSegmented
    {
        public SupportSelectorPartitioned(IList<object[]> keys)
        {
            PartitionKeys = keys;
        }

        public SupportSelectorPartitioned(object[] keys)
        {
            PartitionKeys = Collections.SingletonList(keys);
        }

        public SupportSelectorPartitioned(object singleKey)
        {
            PartitionKeys = Collections.SingletonList(new[] {singleKey});
        }

        public IList<object[]> PartitionKeys { get; }
    }
} // end of namespace