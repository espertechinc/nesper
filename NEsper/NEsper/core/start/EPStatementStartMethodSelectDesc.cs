///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.core.context.factory;
using com.espertech.esper.core.context.subselect;
using com.espertech.esper.epl.core;

namespace com.espertech.esper.core.start
{
    /// <summary>
    /// Starts and provides the stop method for EPL statements.
    /// </summary>
    public class EPStatementStartMethodSelectDesc
    {
        public EPStatementStartMethodSelectDesc(
            StatementAgentInstanceFactorySelect statementAgentInstanceFactorySelect,
            SubSelectStrategyCollection subSelectStrategyCollection,
            ViewResourceDelegateUnverified viewResourceDelegateUnverified,
            ResultSetProcessorFactoryDesc resultSetProcessorPrototypeDesc,
            EPStatementStopMethod stopMethod,
            EPStatementDestroyCallbackList destroyCallbacks)
        {
            StatementAgentInstanceFactorySelect = statementAgentInstanceFactorySelect;
            SubSelectStrategyCollection = subSelectStrategyCollection;
            ViewResourceDelegateUnverified = viewResourceDelegateUnverified;
            ResultSetProcessorPrototypeDesc = resultSetProcessorPrototypeDesc;
            StopMethod = stopMethod;
            DestroyCallbacks = destroyCallbacks;
        }

        public StatementAgentInstanceFactorySelect StatementAgentInstanceFactorySelect { get; private set; }

        public SubSelectStrategyCollection SubSelectStrategyCollection { get; private set; }

        public ViewResourceDelegateUnverified ViewResourceDelegateUnverified { get; private set; }

        public ResultSetProcessorFactoryDesc ResultSetProcessorPrototypeDesc { get; private set; }

        public EPStatementStopMethod StopMethod { get; private set; }

        public EPStatementDestroyCallbackList DestroyCallbacks { get; private set; }
    }
}