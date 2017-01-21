///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;


namespace com.espertech.esper.core.context.util
{
    public class EPStatementAgentInstanceHandleComparator : IComparer<EPStatementAgentInstanceHandle>
    {
        public static EPStatementAgentInstanceHandleComparator Instance = new EPStatementAgentInstanceHandleComparator();
    
        public int Compare(EPStatementAgentInstanceHandle o1, EPStatementAgentInstanceHandle o2)
        {
            if (o1.Priority == o2.Priority) {
                if (o1 == o2 || o1.Equals(o2)) {
                    return 0;
                }
                if (o1.StatementId != o2.StatementId) {
                    return o1.StatementId.CompareTo(o2.StatementId);
                }
                return o1.AgentInstanceId < o2.AgentInstanceId ? -1 : 1;
            }
            else {
                return o1.Priority > o2.Priority ? -1 : 1;
            }
        }
    }
}
