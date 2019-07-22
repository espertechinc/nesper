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
using com.espertech.esper.common.@internal.context.aifactory.ontrigger.core;
using com.espertech.esper.common.@internal.epl.expression.subquery;
using com.espertech.esper.common.@internal.epl.expression.table;
using com.espertech.esper.common.@internal.epl.subselect;
using com.espertech.esper.common.@internal.epl.table.strategy;
using com.espertech.esper.common.@internal.epl.variable.core;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.context.aifactory.ontrigger.onset
{
    public class StatementAgentInstanceFactoryOnTriggerSetForge : StatementAgentInstanceFactoryOnTriggerBaseForge
    {
        private readonly string resultSetProcessorProviderClassName;
        private readonly VariableReadWritePackageForge variableReadWrite;

        public StatementAgentInstanceFactoryOnTriggerSetForge(
            ViewableActivatorForge activator,
            EventType resultEventType,
            IDictionary<ExprSubselectNode, SubSelectFactoryForge> subselects,
            IDictionary<ExprTableAccessNode, ExprTableEvalStrategyFactoryForge> tableAccesses,
            VariableReadWritePackageForge variableReadWrite,
            string resultSetProcessorProviderClassName)
            : base(activator, resultEventType, subselects, tableAccesses)
        {
            this.variableReadWrite = variableReadWrite;
            this.resultSetProcessorProviderClassName = resultSetProcessorProviderClassName;
        }

        public override Type TypeOfSubclass()
        {
            return typeof(StatementAgentInstanceFactoryOnTriggerSet);
        }

        public override void InlineInitializeOnTriggerBase(
            CodegenExpressionRef saiff,
            CodegenMethod method,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            method.Block
                .SetProperty(saiff, "VariableReadWrite", variableReadWrite.Make(method, symbols, classScope))
                .SetProperty(
                    saiff,
                    "ResultSetProcessorFactoryProvider",
                    NewInstance(resultSetProcessorProviderClassName, symbols.GetAddInitSvc(method)));
        }
    }
} // end of namespace