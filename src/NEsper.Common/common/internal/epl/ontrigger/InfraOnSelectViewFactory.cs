///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.lookupplansubord;
using com.espertech.esper.common.@internal.epl.namedwindow.core;
using com.espertech.esper.common.@internal.epl.resultset.core;
using com.espertech.esper.common.@internal.epl.table.core;
using com.espertech.esper.compat.collections;


namespace com.espertech.esper.common.@internal.epl.ontrigger
{
    /// <summary>
    /// View for the on-select statement that handles selecting events from a named window.
    /// </summary>
    public class InfraOnSelectViewFactory : InfraOnExprBaseViewFactory
    {
        private readonly bool addToFront;
        private readonly bool isDistinct;
        private readonly EventPropertyValueGetter distinctKeyGetter;
        private readonly bool selectAndDelete;
        private readonly StreamSelector? optionalStreamSelector;
        private readonly Table optionalInsertIntoTable;
        private readonly bool insertInto;
        private readonly ResultSetProcessorFactoryProvider resultSetProcessorPrototype;
        private readonly ExprEvaluator eventPrecedence;

        public InfraOnSelectViewFactory(
            EventType infraEventType,
            bool addToFront,
            bool isDistinct,
            EventPropertyValueGetter distinctKeyGetter,
            bool selectAndDelete,
            StreamSelector? optionalStreamSelector,
            Table optionalInsertIntoTable,
            bool insertInto,
            ResultSetProcessorFactoryProvider resultSetProcessorPrototype,
            ExprEvaluator eventPrecedence) : base(infraEventType)
        {
            this.addToFront = addToFront;
            this.isDistinct = isDistinct;
            this.distinctKeyGetter = distinctKeyGetter;
            this.selectAndDelete = selectAndDelete;
            this.optionalStreamSelector = optionalStreamSelector;
            this.optionalInsertIntoTable = optionalInsertIntoTable;
            this.insertInto = insertInto;
            this.resultSetProcessorPrototype = resultSetProcessorPrototype;
            this.eventPrecedence = eventPrecedence;
        }

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
                selectAndDelete,
                tableInstanceInsertInto,
                eventPrecedence);
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
                selectAndDelete,
                tableInstanceInsertInto,
                eventPrecedence);
            return new InfraOnExprBaseViewResult(selectView, pair.Second);
        }

        public bool IsAddToFront => addToFront;

        public bool IsDistinct => isDistinct;

        public bool IsSelectAndDelete => selectAndDelete;

        public bool IsInsertInto => insertInto;

        public EventPropertyValueGetter DistinctKeyGetter => distinctKeyGetter;

        public StreamSelector? OptionalStreamSelector => optionalStreamSelector;
    }
} // end of namespace