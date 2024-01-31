///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.namedwindow.core;
using com.espertech.esper.common.@internal.epl.ontrigger;
using com.espertech.esper.common.@internal.epl.resultset.core;
using com.espertech.esper.common.@internal.epl.table.core;
using com.espertech.esper.compat.threading.locks;

namespace com.espertech.esper.common.@internal.context.aifactory.ontrigger.ontrigger
{
    public class StatementAgentInstanceFactoryOnTriggerInfraSelect : StatementAgentInstanceFactoryOnTriggerInfraBase
    {
        private bool addToFront;
        private EventPropertyValueGetter distinctKeyGetter;
        private ExprEvaluator eventPrecedence;
        private bool insertInto;
        private bool isDistinct;
        private Table optionalInsertIntoTable;
        private ResultSetProcessorFactoryProvider resultSetProcessorFactoryProvider;
        private bool selectAndDelete;

        protected override bool IsSelect => true;

        public ResultSetProcessorFactoryProvider ResultSetProcessorFactoryProvider {
            set => resultSetProcessorFactoryProvider = value;
        }

        public bool InsertInto {
            set => insertInto = value;
        }

        public Table OptionalInsertIntoTable {
            set => optionalInsertIntoTable = value;
        }

        public bool SelectAndDelete {
            set => selectAndDelete = value;
        }

        public bool IsDistinct {
            set => isDistinct = value;
        }

        public EventPropertyValueGetter DistinctKeyGetter {
            set => distinctKeyGetter = value;
        }

        public bool AddToFront {
            set => addToFront = value;
        }

        public ExprEvaluator EventPrecedence {
            set => eventPrecedence = value;
        }

        protected override InfraOnExprBaseViewFactory SetupFactory(
            EventType infraEventType,
            NamedWindow namedWindow,
            Table table,
            StatementContext statementContext)
        {
            return new InfraOnSelectViewFactory(
                infraEventType,
                addToFront,
                isDistinct,
                distinctKeyGetter,
                selectAndDelete,
                null,
                optionalInsertIntoTable,
                insertInto,
                resultSetProcessorFactoryProvider,
                eventPrecedence);
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