///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.compat.collections;
using com.espertech.esper.core.context.util;

namespace com.espertech.esper.epl.named
{
    public class NamedWindowUtil
    {
        public static IDictionary<EPStatementAgentInstanceHandle, IList<NamedWindowConsumerView>> CreateConsumerMap(bool isPrioritized)
        {
            if (!isPrioritized)
            {
                return new LinkedHashMap<EPStatementAgentInstanceHandle, IList<NamedWindowConsumerView>>();
            }
            
            return new OrderedDictionary<EPStatementAgentInstanceHandle, IList<NamedWindowConsumerView>>(
                EPStatementAgentInstanceHandleComparator.Instance);
        }
    }
}
