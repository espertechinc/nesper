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
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.time.eval;
using com.espertech.esper.common.@internal.schedule;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.common.@internal.view.util;
using com.espertech.esper.compat;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.view.timewin
{
    public class TimeWindowViewForge
        : ViewFactoryForgeBase,
            DataWindowViewForge,
            DataWindowViewForgeWithPrevious,
            ScheduleHandleCallbackProvider
    {
        private int scheduleCallbackId = -1;
        internal TimePeriodComputeForge timePeriodComputeForge;

        public override string ViewName => "Time";

        private string ViewParamMessage => ViewName + " view requires a single numeric or time period parameter";

        public int ScheduleCallbackId {
            set => scheduleCallbackId = value;
        }

        public override void SetViewParameters(IList<ExprNode> parameters, ViewForgeEnv viewForgeEnv, int streamNumber)
        {
            if (parameters.Count != 1) {
                throw new ViewParameterException(ViewParamMessage);
            }

            timePeriodComputeForge = ViewFactoryTimePeriodHelper.ValidateAndEvaluateTimeDeltaFactory(
                ViewName, parameters[0], ViewParamMessage, 0, viewForgeEnv, streamNumber);
        }

        public override void Attach(EventType parentEventType, int streamNumber, ViewForgeEnv viewForgeEnv)
        {
            eventType = parentEventType;
        }

        internal override Type TypeOfFactory()
        {
            return typeof(TimeWindowViewFactory);
        }

        internal override string FactoryMethod()
        {
            return "time";
        }

        internal override void Assign(
            CodegenMethod method, CodegenExpressionRef factory, SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            if (scheduleCallbackId == -1) {
                throw new IllegalStateException("No schedule callback id");
            }

            method.Block
                .DeclareVar(typeof(TimePeriodCompute), "eval", timePeriodComputeForge.MakeEvaluator(method, classScope))
                .ExprDotMethod(factory, "setTimePeriodCompute", Ref("eval"))
                .ExprDotMethod(factory, "setScheduleCallbackId", Constant(scheduleCallbackId));
        }
    }
} // end of namespace