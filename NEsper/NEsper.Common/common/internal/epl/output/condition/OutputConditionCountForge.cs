///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
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
        internal readonly int eventRate;
        internal readonly VariableMetaData variableMetaData;

        public OutputConditionCountForge(
            int eventRate,
            VariableMetaData variableMetaData)
        {
            if (eventRate < 1 && variableMetaData == null) {
                throw new ArgumentException(
                    "Limiting output by event count requires an event count of at least 1 or a variable name");
            }

            this.eventRate = eventRate;
            this.variableMetaData = variableMetaData;
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
                        .Add(EPStatementInitServicesConstants.GETRESULTSETPROCESSORHELPERFACTORY)
                        .Add("makeOutputConditionCount", Constant(eventRate), variableExpression));
            return LocalMethod(method);
        }

        public void CollectSchedules(IList<ScheduleHandleCallbackProvider> scheduleHandleCallbackProviders)
        {
        }
    }
} // end of namespace