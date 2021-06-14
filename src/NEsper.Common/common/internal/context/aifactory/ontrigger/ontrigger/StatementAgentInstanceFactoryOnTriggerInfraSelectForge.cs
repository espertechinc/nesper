///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.compile.multikey;
using com.espertech.esper.common.@internal.context.activator;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.expression.subquery;
using com.espertech.esper.common.@internal.epl.expression.table;
using com.espertech.esper.common.@internal.epl.lookupplansubord;
using com.espertech.esper.common.@internal.epl.namedwindow.path;
using com.espertech.esper.common.@internal.epl.subselect;
using com.espertech.esper.common.@internal.epl.table.compiletime;
using com.espertech.esper.common.@internal.epl.table.core;
using com.espertech.esper.common.@internal.epl.table.strategy;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.context.aifactory.ontrigger.ontrigger
{
    public class StatementAgentInstanceFactoryOnTriggerInfraSelectForge :
        StatementAgentInstanceFactoryOnTriggerInfraBaseForge
    {
        private readonly bool addToFront;
        private readonly bool distinct;
        private readonly bool insertInto;
        private readonly TableMetaData optionalInsertIntoTable;
        private readonly string resultSetProcessorProviderClassName;
        private readonly bool selectAndDelete;
        private readonly MultiKeyClassRef distinctMultiKey;

        public StatementAgentInstanceFactoryOnTriggerInfraSelectForge(
            ViewableActivatorForge activator,
            EventType resultEventType,
            IDictionary<ExprSubselectNode, SubSelectFactoryForge> subselects,
            IDictionary<ExprTableAccessNode, ExprTableEvalStrategyFactoryForge> tableAccesses,
            NamedWindowMetaData namedWindow,
            TableMetaData table,
            SubordinateWMatchExprQueryPlanForge queryPlanForge,
            string resultSetProcessorProviderClassName,
            bool insertInto,
            bool addToFront,
            TableMetaData optionalInsertIntoTable,
            bool selectAndDelete,
            bool distinct,
            MultiKeyClassRef distinctMultiKey)
            : base(activator, resultEventType, subselects, tableAccesses, null, namedWindow, table, queryPlanForge)

        {
            this.resultSetProcessorProviderClassName = resultSetProcessorProviderClassName;
            this.insertInto = insertInto;
            this.addToFront = addToFront;
            this.optionalInsertIntoTable = optionalInsertIntoTable;
            this.selectAndDelete = selectAndDelete;
            this.distinct = distinct;
            this.distinctMultiKey = distinctMultiKey;
        }

        public override Type TypeOfSubclass()
        {
            return typeof(StatementAgentInstanceFactoryOnTriggerInfraSelect);
        }

        protected override void InlineInitializeOnTriggerSpecific(
            CodegenExpressionRef saiff,
            CodegenMethod method,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            method.Block
                .SetProperty(
                    saiff,
                    "ResultSetProcessorFactoryProvider",
                    NewInstanceInner(
                        resultSetProcessorProviderClassName,
                        symbols.GetAddInitSvc(method),
                        Ref("statementFields")))
                .SetProperty(saiff, "IsInsertInto", Constant(insertInto))
                .SetProperty(saiff, "IsAddToFront", Constant(addToFront))
                .SetProperty(saiff, "IsSelectAndDelete", Constant(selectAndDelete))
                .SetProperty(saiff, "IsDistinct", Constant(distinct))
                .SetProperty(
                    saiff,
                    "DistinctKeyGetter",
                    MultiKeyCodegen.CodegenGetterEventDistinct(
                        distinct,
                        ResultEventType,
                        distinctMultiKey,
                        method,
                        classScope))
                .SetProperty(
                    saiff,
                    "OptionalInsertIntoTable",
                    optionalInsertIntoTable == null
                        ? ConstantNull()
                        : TableDeployTimeResolver.MakeResolveTable(
                            optionalInsertIntoTable,
                            symbols.GetAddInitSvc(method)));
        }
    }
} // end of namespace