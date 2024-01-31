///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.common.@internal.context.util
{
    public interface StatementFinalizeCallback
    {
        void StatementDestroyed(StatementContext context);
    }

    public class ProxyStatementFinalizeCallback : StatementFinalizeCallback
    {
        public Action<StatementContext> ProcStatementDestroyed;

        public ProxyStatementFinalizeCallback()
        {
        }

        public ProxyStatementFinalizeCallback(Action<StatementContext> procStatementDestroyed)
        {
            ProcStatementDestroyed = procStatementDestroyed;
        }

        public void StatementDestroyed(StatementContext context)
        {
            ProcStatementDestroyed.Invoke(context);
        }
    }
} // end of namespace