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
using com.espertech.esper.common.@internal.context.activator;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.expression.subquery;
using com.espertech.esper.common.@internal.epl.expression.table;
using com.espertech.esper.common.@internal.epl.subselect;
using com.espertech.esper.common.@internal.epl.table.strategy;
using com.espertech.esper.common.@internal.@event.core;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.context.aifactory.ontrigger.core
{
    public abstract class StatementAgentInstanceFactoryOnTriggerBaseForge
    {
        private readonly ViewableActivatorForge activator;
        private readonly EventType resultEventType;
        private readonly IDictionary<ExprSubselectNode, SubSelectFactoryForge> subselects;
        private readonly IDictionary<ExprTableAccessNode, ExprTableEvalStrategyFactoryForge> tableAccesses;

        public StatementAgentInstanceFactoryOnTriggerBaseForge(
            ViewableActivatorForge activator,
            EventType resultEventType,
            IDictionary<ExprSubselectNode, SubSelectFactoryForge> subselects,
            IDictionary<ExprTableAccessNode, ExprTableEvalStrategyFactoryForge> tableAccesses)
        {
            this.activator = activator;
            this.resultEventType = resultEventType;
            this.subselects = subselects;
            this.tableAccesses = tableAccesses;
        }

        public EventType ResultEventType => resultEventType;

        public abstract Type TypeOfSubclass();

        public abstract void InlineInitializeOnTriggerBase(
            CodegenExpressionRef saiff,
            CodegenMethod method,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope);

        public CodegenMethod InitializeCodegen(
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            var method = parent.MakeChild(TypeOfSubclass(), GetType(), classScope);
            method.Block
                .DeclareVarNewInstance(TypeOfSubclass(), "saiff")
                .SetProperty(Ref("saiff"), "Activator", activator.MakeCodegen(method, symbols, classScope))
                .SetProperty(
                    Ref("saiff"),
                    "ResultEventType",
                    EventTypeUtility.ResolveTypeCodegen(resultEventType, symbols.GetAddInitSvc(method)))
                .SetProperty(
                    Ref("saiff"),
                    "Subselects",
                    SubSelectFactoryForge.CodegenInitMap(subselects, GetType(), method, symbols, classScope))
                .SetProperty(
                    Ref("saiff"),
                    "TableAccesses",
                    ExprTableEvalStrategyUtil.CodegenInitMap(tableAccesses, GetType(), method, symbols, classScope));
            InlineInitializeOnTriggerBase(Ref("saiff"), method, symbols, classScope);
            method.Block.MethodReturn(Ref("saiff"));
            return method;
        }
    }
} // end of namespace