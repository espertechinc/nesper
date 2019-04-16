///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.namedwindow.core;
using com.espertech.esper.common.@internal.epl.ontrigger;
using com.espertech.esper.common.@internal.epl.table.core;
using com.espertech.esper.common.@internal.epl.updatehelper;

namespace com.espertech.esper.common.@internal.context.aifactory.ontrigger.ontrigger
{
    public class StatementAgentInstanceFactoryOnTriggerInfraUpdate : StatementAgentInstanceFactoryOnTriggerInfraBase
    {
        private EventBeanUpdateHelperWCopy updateHelperNamedWindow;
        private EventBeanUpdateHelperNoCopy updateHelperTable;

        public EventBeanUpdateHelperWCopy UpdateHelperNamedWindow {
            set => updateHelperNamedWindow = value;
        }

        public EventBeanUpdateHelperNoCopy UpdateHelperTable {
            set => updateHelperTable = value;
        }

        protected override bool IsSelect => false;

        protected override InfraOnExprBaseViewFactory SetupFactory(
            EventType infraEventType,
            NamedWindow namedWindow,
            Table table,
            StatementContext statementContext)
        {
            return new InfraOnUpdateViewFactory(
                infraEventType, updateHelperNamedWindow, updateHelperTable, table, statementContext);
        }

        public override StatementAgentInstanceLock ObtainAgentInstanceLock(
            StatementContext statementContext,
            int agentInstanceId)
        {
            return StatementAgentInstanceFactoryOnTriggerUtil.ObtainAgentInstanceLock(
                this, statementContext, agentInstanceId);
        }
    }
} // end of namespace