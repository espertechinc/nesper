///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.namedwindow.consume;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.namedwindow.core
{
    public class NamedWindowUtil
    {
        protected internal static IDictionary<EPStatementAgentInstanceHandle, IList<NamedWindowConsumerView>>
            CreateConsumerMap(
                bool isPrioritized)
        {
            if (!isPrioritized) {
                return new LinkedHashMap<EPStatementAgentInstanceHandle, IList<NamedWindowConsumerView>>();
            }

            return new SortedDictionary<EPStatementAgentInstanceHandle, IList<NamedWindowConsumerView>>(
                EPStatementAgentInstanceHandleComparer.INSTANCE);
        }
    }
} // end of namespace