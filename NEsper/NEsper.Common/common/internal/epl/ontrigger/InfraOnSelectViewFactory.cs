///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.annotation;
using com.espertech.esper.common.client.soda;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.lookupplansubord;
using com.espertech.esper.common.@internal.epl.namedwindow.core;
using com.espertech.esper.common.@internal.epl.resultset.core;
using com.espertech.esper.common.@internal.epl.table.core;
using com.espertech.esper.common.@internal.@event.core;

namespace com.espertech.esper.common.@internal.epl.ontrigger
{
    /// <summary>
    ///     View for the on-select statement that handles selecting events from a named window.
    /// </summary>
    public class InfraOnSelectViewFactory : InfraOnExprBaseViewFactory
    {
        private readonly Table optionalInsertIntoTable;
        private readonly ResultSetProcessorFactoryProvider resultSetProcessorPrototype;

        public InfraOnSelectViewFactory(
            EventType infraEventType,
            bool addToFront,
            bool isDistinct,
            EventPropertyValueGetter distinctKeyGetter,
            bool selectAndDelete,
            StreamSelector? optionalStreamSelector,
            Table optionalInsertIntoTable,
            bool insertInto,
            ResultSetProcessorFactoryProvider resultSetProcessorPrototype)
            : base(infraEventType)
        {
            IsAddToFront = addToFront;
            IsDistinct = isDistinct;
            DistinctKeyGetter = distinctKeyGetter;
            IsSelectAndDelete = selectAndDelete;
            OptionalStreamSelector = optionalStreamSelector;
            this.optionalInsertIntoTable = optionalInsertIntoTable;
            IsInsertInto = insertInto;
            this.resultSetProcessorPrototype = resultSetProcessorPrototype;
        }

        public EventPropertyValueGetter DistinctKeyGetter { get; }

        public StreamSelector? OptionalStreamSelector { get; }

        public bool IsAddToFront { get; }

        public bool IsDistinct { get; }

        public bool IsSelectAndDelete { get; }

        public bool IsInsertInto { get; }

        public override InfraOnExprBaseViewResult MakeNamedWindow(
            SubordWMatchExprLookupStrategy lookupStrategy,
            NamedWindowRootViewInstance namedWindowRootViewInstance,
            AgentInstanceContext agentInstanceContext)
        {
            var pair = StatementAgentInstanceFactoryUtil.StartResultSetAndAggregation(
                resultSetProcessorPrototype,
                agentInstanceContext,
                false,
                null);

            var audit = AuditEnum.INSERT.GetAudit(agentInstanceContext.Annotations) != null;
            TableInstance tableInstanceInsertInto = null;
            if (optionalInsertIntoTable != null) {
                tableInstanceInsertInto =
                    optionalInsertIntoTable.GetTableInstance(agentInstanceContext.AgentInstanceId);
            }

            var selectView = new OnExprViewNamedWindowSelect(
                lookupStrategy,
                namedWindowRootViewInstance,
                agentInstanceContext,
                this,
                pair.First,
                audit,
                IsSelectAndDelete,
                tableInstanceInsertInto);
            return new InfraOnExprBaseViewResult(selectView, pair.Second);
        }

        public override InfraOnExprBaseViewResult MakeTable(
            SubordWMatchExprLookupStrategy lookupStrategy,
            TableInstance tableInstance,
            AgentInstanceContext agentInstanceContext)
        {
            var pair = StatementAgentInstanceFactoryUtil.StartResultSetAndAggregation(
                resultSetProcessorPrototype,
                agentInstanceContext,
                false,
                null);

            var audit = AuditEnum.INSERT.GetAudit(agentInstanceContext.Annotations) != null;
            TableInstance tableInstanceInsertInto = null;
            if (optionalInsertIntoTable != null) {
                tableInstanceInsertInto =
                    optionalInsertIntoTable.GetTableInstance(agentInstanceContext.AgentInstanceId);
            }

            var selectView = new OnExprViewTableSelect(
                lookupStrategy,
                tableInstance,
                agentInstanceContext,
                pair.First,
                this,
                audit,
                IsSelectAndDelete,
                tableInstanceInsertInto);
            return new InfraOnExprBaseViewResult(selectView, pair.Second);
        }
    }
} // end of namespace