///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using com.espertech.esper.common.@internal.context.util;

namespace com.espertech.esper.runtime.@internal.kernel.service
{
    [Serializable]
    public class EPStatementAgentInstanceHandleComparator : IComparer<EPStatementAgentInstanceHandle>
    {
        public static readonly EPStatementAgentInstanceHandleComparator INSTANCE = new EPStatementAgentInstanceHandleComparator();

        public int Compare(
            EPStatementAgentInstanceHandle o1,
            EPStatementAgentInstanceHandle o2)
        {
            if (o1.Priority == o2.Priority) {
                if (o1 == o2 || o1.Equals(o2)) {
                    return 0;
                }

                if (o1.StatementId != o2.StatementId) {
                    return Compare(o1.StatementId, o2.StatementId);
                }

                return o1.AgentInstanceId < o2.AgentInstanceId ? -1 : 1;
            }

            return o1.Priority > o2.Priority ? -1 : 1;
        }

        public static int Compare(
            int x,
            int y)
        {
            return x < y ? -1 : x == y ? 0 : 1;
        }
    }
} // end of namespace