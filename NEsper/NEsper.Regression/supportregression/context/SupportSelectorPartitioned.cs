///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client.context;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;

using NUnit.Framework;

namespace com.espertech.esper.supportregression.context
{
    public class SupportSelectorPartitioned : ContextPartitionSelectorSegmented
    {
        public SupportSelectorPartitioned(IList<object[]> keys) {
            this.PartitionKeys = keys;
        }
    
        public SupportSelectorPartitioned(object[] keys) {
            this.PartitionKeys = Collections.SingletonList(keys);
        }
    
        public SupportSelectorPartitioned(Object singleKey) {
            this.PartitionKeys = Collections.SingletonList(new object[]{singleKey});
        }

        public IList<object[]> PartitionKeys { get; }
    }
    
} // end of namespace
