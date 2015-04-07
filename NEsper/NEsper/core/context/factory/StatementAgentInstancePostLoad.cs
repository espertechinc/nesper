///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.core.context.factory
{
    public interface StatementAgentInstancePostLoad
    {
        void ExecutePostLoad();
        void AcceptIndexVisitor(StatementAgentInstancePostLoadIndexVisitor visitor);
    }

    public class ProxyStatementAgentInstancePostLoad : StatementAgentInstancePostLoad
    {
        public Action ProcExecutePostLoad;
        public Action<StatementAgentInstancePostLoadIndexVisitor> ProcAcceptIndexVisitor;

        public void ExecutePostLoad()
        {
            ProcExecutePostLoad();
        }

        public void AcceptIndexVisitor(StatementAgentInstancePostLoadIndexVisitor visitor)
        {
            ProcAcceptIndexVisitor(visitor);
        }
    }
}
