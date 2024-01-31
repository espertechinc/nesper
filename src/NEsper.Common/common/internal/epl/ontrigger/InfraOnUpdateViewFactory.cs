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
using com.espertech.esper.common.@internal.epl.lookupplansubord;
using com.espertech.esper.common.@internal.epl.namedwindow.core;
using com.espertech.esper.common.@internal.epl.table.core;
using com.espertech.esper.common.@internal.epl.table.update;
using com.espertech.esper.common.@internal.epl.updatehelper;

namespace com.espertech.esper.common.@internal.epl.ontrigger
{
    /// <summary>
    ///     View for the on-delete statement that handles removing events from a named window.
    /// </summary>
    public class InfraOnUpdateViewFactory : InfraOnExprBaseViewFactory,
        TableUpdateStrategyRedoCallback
    {
        private readonly Table table;

        public InfraOnUpdateViewFactory(
            EventType infraEventType,
            EventBeanUpdateHelperWCopy updateHelperNamedWindow,
            EventBeanUpdateHelperNoCopy updateHelperTable,
            Table table,
            StatementContext statementContext)
            : base(
                infraEventType)
        {
            UpdateHelperNamedWindow = updateHelperNamedWindow;
            UpdateHelperTable = updateHelperTable;
            this.table = table;

            if (table != null) {
                InitTableUpdateStrategy(table);
                table.AddUpdateStrategyCallback(this);
                statementContext.AddFinalizeCallback(
                    new ProxyStatementFinalizeCallback {
                        ProcStatementDestroyed = context => { this.table.RemoveUpdateStrategyCallback(this); }
                    });
            }
        }

        public EventBeanUpdateHelperWCopy UpdateHelperNamedWindow { get; }

        public EventBeanUpdateHelperNoCopy UpdateHelperTable { get; }

        public TableUpdateStrategy TableUpdateStrategy { get; private set; }

        public bool IsMerge => false;

        public string[] TableUpdatedProperties => UpdateHelperTable.UpdatedProperties;

        public void InitTableUpdateStrategy(Table table)
        {
            try {
                TableUpdateStrategy =
                    TableUpdateStrategyFactory.ValidateGetTableUpdateStrategy(table.MetaData, UpdateHelperTable, false);
            }
            catch (ExprValidationException ex) {
                throw new EPException(ex.Message, ex);
            }
        }

        public override InfraOnExprBaseViewResult MakeNamedWindow(
            SubordWMatchExprLookupStrategy lookupStrategy,
            NamedWindowRootViewInstance namedWindowRootViewInstance,
            AgentInstanceContext agentInstanceContext)
        {
            var view = new OnExprViewNamedWindowUpdate(
                lookupStrategy,
                namedWindowRootViewInstance,
                agentInstanceContext,
                this);
            return new InfraOnExprBaseViewResult(view, null);
        }

        public override InfraOnExprBaseViewResult MakeTable(
            SubordWMatchExprLookupStrategy lookupStrategy,
            TableInstance tableInstance,
            AgentInstanceContext agentInstanceContext)
        {
            var view = new OnExprViewTableUpdate(lookupStrategy, tableInstance, agentInstanceContext, this);
            return new InfraOnExprBaseViewResult(view, null);
        }
    }
} // end of namespace