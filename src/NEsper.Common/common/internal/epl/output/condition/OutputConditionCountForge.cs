///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.compile.util;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.epl.variable.compiletime;
using com.espertech.esper.common.@internal.epl.variable.core;
using com.espertech.esper.common.@internal.schedule;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.output.condition
{
    public class OutputConditionCountForge : OutputConditionFactoryForge
    {
        protected readonly int eventRate;
        protected readonly VariableMetaData variableMetaData;
        protected readonly StateMgmtSetting stateMgmtSetting;

        public OutputConditionCountForge(
            int eventRate,
            VariableMetaData variableMetaData,
            StateMgmtSetting stateMgmtSetting)
        {
            if (eventRate < 1 && variableMetaData == null) {
                throw new ArgumentException(
                    "Limiting output by event count requires an event count of at least 1 or a variable name");
            }

            this.eventRate = eventRate;
            this.variableMetaData = variableMetaData;
            this.stateMgmtSetting = stateMgmtSetting;
        }

        public CodegenExpression Make(
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            // resolve variable at init-time via field
            var variableExpression = ConstantNull();
            if (variableMetaData != null) {
                variableExpression =
                    VariableDeployTimeResolver.MakeVariableField(variableMetaData, classScope, GetType());
            }

            var method = parent.MakeChild(typeof(OutputConditionFactory), GetType(), classScope);
            method.Block
                .MethodReturn(
                    ExprDotMethodChain(symbols.GetAddInitSvc(method))
                        .Get(EPStatementInitServicesConstants.RESULTSETPROCESSORHELPERFACTORY)
                        .Add(
                            "MakeOutputConditionCount",
                            Constant(eventRate),
                            variableExpression,
                            stateMgmtSetting.ToExpression()));
            return LocalMethod(method);
        }

        public void CollectSchedules(
            CallbackAttributionOutputRate callbackAttribution,
            IList<ScheduleHandleTracked> scheduleHandleCallbackProviders)
        {
        }
    }
} // end of namespace