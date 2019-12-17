///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.soda;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.namedwindow.core;
using com.espertech.esper.common.@internal.epl.ontrigger;
using com.espertech.esper.common.@internal.epl.resultset.core;
using com.espertech.esper.common.@internal.epl.table.core;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.compat.threading.locks;

namespace com.espertech.esper.common.@internal.context.aifactory.ontrigger.ontrigger
{
    public class StatementAgentInstanceFactoryOnTriggerInfraSelect : StatementAgentInstanceFactoryOnTriggerInfraBase
    {
        private bool addToFront;
        private bool insertInto;
        private bool isDistinct;
        private Table optionalInsertIntoTable;
        private ResultSetProcessorFactoryProvider resultSetProcessorFactoryProvider;
        private bool selectAndDelete;

        public ResultSetProcessorFactoryProvider ResultSetProcessorFactoryProvider {
            set => resultSetProcessorFactoryProvider = value;
        }

        public bool IsInsertInto {
            set => insertInto = value;
        }

        public Table OptionalInsertIntoTable {
            set => optionalInsertIntoTable = value;
        }

        public bool IsSelectAndDelete {
            set => selectAndDelete = value;
        }

        protected override bool IsSelect => true;

        public bool IsDistinct {
            set => isDistinct = value;
        }

        public bool IsAddToFront {
            set => addToFront = value;
        }

        protected override InfraOnExprBaseViewFactory SetupFactory(
            EventType infraEventType,
            NamedWindow namedWindow,
            Table table,
            StatementContext statementContext)
        {
            EventBeanReader eventBeanReader = null;
            StreamSelector? optionalStreamSelector = null;

            if (isDistinct) {
                var outputEventType = resultSetProcessorFactoryProvider.ResultEventType;
                if (outputEventType is EventTypeSPI) {
                    eventBeanReader = ((EventTypeSPI) outputEventType).Reader;
                }

                if (eventBeanReader == null) {
                    eventBeanReader = new EventBeanReaderDefaultImpl(outputEventType);
                }
            }

            return new InfraOnSelectViewFactory(
                infraEventType,
                addToFront,
                eventBeanReader,
                isDistinct,
                selectAndDelete,
                optionalStreamSelector,
                optionalInsertIntoTable,
                insertInto,
                resultSetProcessorFactoryProvider);
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