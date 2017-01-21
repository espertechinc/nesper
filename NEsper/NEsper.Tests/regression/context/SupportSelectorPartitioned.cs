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
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.regression.context
{
    public class SupportSelectorPartitioned : ContextPartitionSelectorSegmented
    {
        private readonly IList<Object[]> _keys;
    
        public SupportSelectorPartitioned(IList<Object[]> keys)
        {
            _keys = keys;
        }
    
        public SupportSelectorPartitioned(Object[] keys)
        {
            _keys = Collections.SingletonList(keys);
        }
    
        public SupportSelectorPartitioned(Object singleKey) 
        {
            _keys = Collections.SingletonList(new Object[] {singleKey});
        }

        public IList<object[]> PartitionKeys
        {
            get { return _keys; }
        }
    }
    
}
