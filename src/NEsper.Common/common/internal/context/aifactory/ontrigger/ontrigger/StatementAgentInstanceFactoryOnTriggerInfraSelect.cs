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
using com.espertech.esper.common.@internal.epl.resultset.core;
using com.espertech.esper.common.@internal.epl.table.core;
using com.espertech.esper.compat.threading.locks;

namespace com.espertech.esper.common.@internal.context.aifactory.ontrigger.ontrigger
{
    public class StatementAgentInstanceFactoryOnTriggerInfraSelect : StatementAgentInstanceFactoryOnTriggerInfraBase
    {
        public ResultSetProcessorFactoryProvider ResultSetProcessorFactoryProvider { get; set; }

        public bool IsInsertInto { get; set; }

        public Table OptionalInsertIntoTable { get; set; }

        public bool IsSelectAndDelete { get; set; }

        protected override bool IsSelect => true;

        public bool IsDistinct { get; set; }

        public bool IsAddToFront { get; set; }

        public EventPropertyValueGetter DistinctKeyGetter { get; set; }

        protected override InfraOnExprBaseViewFactory SetupFactory(
            EventType infraEventType,
            NamedWindow namedWindow,
            Table table,
            StatementContext statementContext)
        {
            return new InfraOnSelectViewFactory(
                infraEventType,
                IsAddToFront,
                IsDistinct,
                DistinctKeyGetter,
                IsSelectAndDelete,
                null,
                OptionalInsertIntoTable,
                IsInsertInto,
                ResultSetProcessorFactoryProvider);
        }

        public override IReaderWriterLock ObtainAgentInstanceLock(
            StatementContext statementContext,
            int agentInstanceId)
        {
            return StatementAgentInstanceFactoryOnTriggerUtil.ObtainAgentInstanceLock(
                this,
                statementContext,
                agentInstanceId);
        }
    }
} // end of namespace