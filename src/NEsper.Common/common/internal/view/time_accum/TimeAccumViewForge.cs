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
using com.espertech.esper.common.client.annotation;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.time.eval;
using com.espertech.esper.common.@internal.schedule;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.common.@internal.view.util;
using com.espertech.esper.compat;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.view.time_accum
{
    public class TimeAccumViewForge
        : ViewFactoryForgeBase,
            DataWindowViewForge,
            DataWindowViewForgeWithPrevious,
            ScheduleHandleCallbackProvider
    {
        private int scheduleCallbackId = -1;
        internal TimePeriodComputeForge timePeriodCompute;

        public override void SetViewParameters(
            IList<ExprNode> parameters,
            ViewForgeEnv viewForgeEnv,
            int streamNumber)
        {
            if (parameters.Count != 1) {
                throw new ViewParameterException(ViewParamMessage);
            }

            timePeriodCompute = ViewFactoryTimePeriodHelper.ValidateAndEvaluateTimeDeltaFactory(
                ViewName,
                parameters[0],
                ViewParamMessage,
                0,
                viewForgeEnv,
                streamNumber);
        }

        public override void AttachValidate(
            EventType parentEventType,
            int streamNumber,
            ViewForgeEnv viewForgeEnv,
            bool grouped)
        {
            eventType = parentEventType;
        }

        public override Type TypeOfFactory()
        {
            return typeof(TimeAccumViewFactory);
        }

        public override string FactoryMethod()
        {
            return "Timeaccum";
        }

        internal override void Assign(
            CodegenMethod method,
            CodegenExpressionRef factory,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            if (scheduleCallbackId == -1) {
                throw new IllegalStateException("No schedule callback id");
            }

            method.Block
                .DeclareVar<TimePeriodCompute>("eval", timePeriodCompute.MakeEvaluator(method, classScope))
                .SetProperty(factory, "TimePeriodCompute", Ref("eval"))
                .SetProperty(factory, "ScheduleCallbackId", Constant(scheduleCallbackId));
        }

        public override string ViewName => "Time-Accumulative-Batch";

        public int ScheduleCallbackId {
            set => scheduleCallbackId = value;
        }

        private string ViewParamMessage =>
            ViewName + " view requires a single numeric parameter or time period parameter";

        public override AppliesTo AppliesTo()
        {
            return client.annotation.AppliesTo.WINDOW_TIMEACCUM;
        }
    }
} // end of namespace