///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.core.context.factory;
using com.espertech.esper.core.service.resource;
using com.espertech.esper.core.start;
using com.espertech.esper.util;

namespace com.espertech.esper.core.service
{
    /// <summary>
    /// Statement-level extension services.
    /// </summary>
    public interface StatementExtensionSvcContext
    {
        StatementResourceService StmtResources { get; }
        StatementResourceHolder ExtractStatementResourceHolder(StatementAgentInstanceFactoryResult resultOfStart);
        void PreStartWalk(EPStatementStartMethodSelectDesc selectDesc);
        void PostProcessStart(StatementAgentInstanceFactoryResult resultOfStart, bool isRecoveringResilient);
        void ContributeStopCallback(StatementAgentInstanceFactoryResult selectResult, IList<StopCallback> stopCallbacks);
    }

    public class ProxyStatementExtensionSvcContext : StatementExtensionSvcContext
    {
        public Func<StatementResourceService> ProcStmtResources { get; set; }
        public Func<StatementAgentInstanceFactoryResult, StatementResourceHolder> ProcExtractStatementResourceHolder { get; set; }
        public Action<EPStatementStartMethodSelectDesc> ProcPreStartWalk { get; set; }
        public Action<StatementAgentInstanceFactoryResult, bool> ProcPostProcessStart { get; set; }
        public Action<StatementAgentInstanceFactoryResult, IList<StopCallback>> ProcContributeStopCallback { get; set; }

        public StatementResourceService StmtResources
        {
            get { return ProcStmtResources.Invoke(); }
        }

        public StatementResourceHolder ExtractStatementResourceHolder(StatementAgentInstanceFactoryResult resultOfStart)
        {
            return ProcExtractStatementResourceHolder.Invoke(resultOfStart);
        }

        public void PreStartWalk(EPStatementStartMethodSelectDesc selectDesc)
        {
            if (ProcPreStartWalk != null)
            {
                ProcPreStartWalk.Invoke(selectDesc);
            }
        }

        public void PostProcessStart(StatementAgentInstanceFactoryResult resultOfStart, bool isRecoveringResilient)
        {
            if (ProcPostProcessStart != null)
            {
                ProcPostProcessStart.Invoke(resultOfStart, isRecoveringResilient);
            }
        }

        public void ContributeStopCallback(
            StatementAgentInstanceFactoryResult selectResult,
            IList<StopCallback> stopCallbacks)
        {
            if (ProcContributeStopCallback != null)
            {
                ProcContributeStopCallback.Invoke(selectResult, stopCallbacks);
            }
        }
    }
}
