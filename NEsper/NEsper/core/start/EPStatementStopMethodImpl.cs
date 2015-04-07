///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.core.context.util;
using com.espertech.esper.core.service;
using com.espertech.esper.util;

namespace com.espertech.esper.core.start
{
    /// <summary>
    /// Method to call to stop an EPStatement.
    /// </summary>
    public class EPStatementStopMethodImpl
    {
        private readonly StatementContext _statementContext;
        private readonly StopCallback[] _stopCallbacks;

        public static EPStatementStopMethod New(StatementContext statementContext, IList<StopCallback> stopCallbacks)
        {
            return (new EPStatementStopMethodImpl(statementContext, stopCallbacks)).Stop;
        }

        public EPStatementStopMethodImpl(StatementContext statementContext, IList<StopCallback> stopCallbacks)
        {
            _statementContext = statementContext;
            _stopCallbacks = stopCallbacks.ToArray();
        }
    
        public void Stop()
        {
            foreach (StopCallback stopCallback in _stopCallbacks)
            {
                StatementAgentInstanceUtil.StopSafe(stopCallback, _statementContext);
            }
        }
    }
}
