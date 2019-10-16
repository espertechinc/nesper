///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.activator;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.context.aifactory.ontrigger.core;
using com.espertech.esper.common.@internal.epl.expression.subquery;
using com.espertech.esper.common.@internal.epl.expression.table;
using com.espertech.esper.common.@internal.epl.lookupplansubord;
using com.espertech.esper.common.@internal.epl.namedwindow.core;
using com.espertech.esper.common.@internal.epl.namedwindow.path;
using com.espertech.esper.common.@internal.epl.subselect;
using com.espertech.esper.common.@internal.epl.table.compiletime;
using com.espertech.esper.common.@internal.epl.table.core;
using com.espertech.esper.common.@internal.epl.table.strategy;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.context.aifactory.ontrigger.ontrigger
{
    public abstract class StatementAgentInstanceFactoryOnTriggerInfraBaseForge :
        StatementAgentInstanceFactoryOnTriggerBaseForge
    {
        internal readonly NamedWindowMetaData namedWindow;
        private readonly string nonSelectRSPProviderClassName;
        private readonly SubordinateWMatchExprQueryPlanForge queryPlanForge;
        internal readonly TableMetaData table;

        public StatementAgentInstanceFactoryOnTriggerInfraBaseForge(
            ViewableActivatorForge activator,
            EventType resultEventType,
            IDictionary<ExprSubselectNode, SubSelectFactoryForge> subselects,
            IDictionary<ExprTableAccessNode, ExprTableEvalStrategyFactoryForge> tableAccesses,
            string nonSelectRSPProviderClassName,
            NamedWindowMetaData namedWindow,
            TableMetaData table,
            SubordinateWMatchExprQueryPlanForge queryPlanForge)
            : base(activator, resultEventType, subselects, tableAccesses)

        {
            this.nonSelectRSPProviderClassName = nonSelectRSPProviderClassName;
            this.namedWindow = namedWindow;
            this.table = table;
            this.queryPlanForge = queryPlanForge;
        }

        protected abstract void InlineInitializeOnTriggerSpecific(
            CodegenExpressionRef saiff,
            CodegenMethod method,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope);

        public override void InlineInitializeOnTriggerBase(
            CodegenExpressionRef saiff,
            CodegenMethod method,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            method.Block
                .SetProperty(
                    saiff,
                    "NamedWindow",
                    namedWindow == null
                        ? ConstantNull()
                        : NamedWindowDeployTimeResolver.MakeResolveNamedWindow(
                            namedWindow,
                            symbols.GetAddInitSvc(method)))
                .SetProperty(
                    saiff,
                    "Table",
                    table == null
                        ? ConstantNull()
                        : TableDeployTimeResolver.MakeResolveTable(table, symbols.GetAddInitSvc(method)))
                .SetProperty(saiff, "QueryPlan", queryPlanForge.Make(method, symbols, classScope))
                .SetProperty(
                    saiff,
                    "NonSelectRSPFactoryProvider",
                    nonSelectRSPProviderClassName == null
                        ? ConstantNull()
                        : NewInstance(nonSelectRSPProviderClassName, symbols.GetAddInitSvc(method), Ref("statementFields")))
                .ExprDotMethod(symbols.GetAddInitSvc(method), "AddReadyCallback", saiff); // add ready-callback

            InlineInitializeOnTriggerSpecific(saiff, method, symbols, classScope);
        }
    }
} // end of namespace