///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.core.context.factory
{
    public interface StatementAgentInstancePreload
    {
        void ExecutePreload();
    }

    public class ProxyStatementAgentInstancePreload : StatementAgentInstancePreload
    {
        public Action ProcExecutePreload { get; set; }

        /// <summary>
        /// Executes the preload.
        /// </summary>
        public void ExecutePreload()
        {
            if (ProcExecutePreload != null)
                ProcExecutePreload();
        }
    }
}