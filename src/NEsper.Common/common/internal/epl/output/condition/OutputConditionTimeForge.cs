///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.compile.util;
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
        protected readonly StateMgmtSetting stateMgmtSetting;

        public OutputConditionTimeForge(
            ExprTimePeriod timePeriod,
            bool isStartConditionOnCreation,
            StateMgmtSetting stateMgmtSetting)
        {
            this.timePeriod = timePeriod;
            this.isStartConditionOnCreation = isStartConditionOnCreation;
            this.stateMgmtSetting = stateMgmtSetting;
        }

        public CodegenExpression Make(
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            if (scheduleCallbackId == -1) {
                throw new IllegalStateException("Unassigned callback id");
            }

            var method = parent.MakeChild(typeof(OutputConditionFactory), GetType(), classScope);
            method.Block
                .DeclareVar<
                    TimePeriodCompute>("delta", timePeriod.TimePeriodComputeForge.MakeEvaluator(method, classScope))
                .MethodReturn(
                    ExprDotMethodChain(symbols.GetAddInitSvc(method))
                        .Get(EPStatementInitServicesConstants.RESULTSETPROCESSORHELPERFACTORY)
                        .Add(
                            "MakeOutputConditionTime",
                            Constant(timePeriod.HasVariable),
                            Ref("delta"),
                            Constant(isStartConditionOnCreation),
                            Constant(scheduleCallbackId),
                            stateMgmtSetting.ToExpression()));
            return LocalMethod(method);
        }

        public void CollectSchedules(
            CallbackAttributionOutputRate callbackAttribution,
            IList<ScheduleHandleTracked> scheduleHandleCallbackProviders)
        {
            scheduleHandleCallbackProviders.Add(new ScheduleHandleTracked(callbackAttribution, this));
        }

        public int ScheduleCallbackId {
            get => scheduleCallbackId;

            set => scheduleCallbackId = value;
        }
    }
} // end of namespace