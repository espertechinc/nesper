///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.epl.expression.time.eval;
using com.espertech.esper.common.@internal.epl.expression.time.node;
using com.espertech.esper.common.@internal.schedule;
using com.espertech.esper.compat;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.output.condition
{
    public class OutputConditionTimeForge : OutputConditionFactoryForge,
        ScheduleHandleCallbackProvider
    {
        private readonly ExprTimePeriod timePeriod;
        private readonly bool isStartConditionOnCreation;
        private int scheduleCallbackId = -1;

        public OutputConditionTimeForge(
            ExprTimePeriod timePeriod,
            bool isStartConditionOnCreation)
        {
            this.timePeriod = timePeriod;
            this.isStartConditionOnCreation = isStartConditionOnCreation;
        }

        public CodegenExpression Make(
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            if (scheduleCallbackId == -1) {
                throw new IllegalStateException("Unassigned callback id");
            }

            CodegenMethod method = parent.MakeChild(typeof(OutputConditionFactory), this.GetType(), classScope);
            method.Block
                .DeclareVar(typeof(TimePeriodCompute), "delta", timePeriod.TimePeriodComputeForge.MakeEvaluator(method, classScope))
                .MethodReturn(
                    ExprDotMethodChain(symbols.GetAddInitSvc(method)).Add(EPStatementInitServicesConstants.GETRESULTSETPROCESSORHELPERFACTORY)
                        .Add(
                            "makeOutputConditionTime", Constant(timePeriod.HasVariable), @Ref("delta"), Constant(isStartConditionOnCreation),
                            Constant(scheduleCallbackId)));
            return LocalMethod(method);
        }

        public int ScheduleCallbackId {
            set { this.scheduleCallbackId = value; }
        }

        public void CollectSchedules(IList<ScheduleHandleCallbackProvider> scheduleHandleCallbackProviders)
        {
            scheduleHandleCallbackProviders.Add(this);
        }
    }
} // end of namespace