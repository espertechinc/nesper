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
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.common.@internal.view.util;
using com.espertech.esper.compat;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.epl.expression.core.ExprNodeUtilityCodegen;

namespace com.espertech.esper.common.@internal.view.timetolive
{
    public class TimeOrderViewForge : ViewFactoryForgeBase,
        DataWindowViewForge,
        DataWindowViewForgeWithPrevious,
        ScheduleHandleCallbackProvider
    {
        internal int scheduleCallbackId = -1;
        internal TimePeriodComputeForge timePeriodCompute;
        private ExprNode timestampExpression;
        private IList<ExprNode> viewParameters;

        public override string ViewName => "Time-Order";

        private string ViewParamMessage => ViewName +
                                           " view requires the expression supplying timestamp values, and a numeric or time period parameter for interval size";

        public int ScheduleCallbackId {
            set => scheduleCallbackId = value;
        }

        public override void SetViewParameters(
            IList<ExprNode> parameters,
            ViewForgeEnv viewForgeEnv,
            int streamNumber)
        {
            viewParameters = parameters;
        }

        public override void Attach(
            EventType parentEventType,
            int streamNumber,
            ViewForgeEnv viewForgeEnv)
        {
            var validated = ViewForgeSupport.Validate(
                ViewName,
                parentEventType,
                viewParameters,
                true,
                viewForgeEnv,
                streamNumber);

            if (viewParameters.Count != 2) {
                throw new ViewParameterException(ViewParamMessage);
            }

            if (!validated[0].Forge.EvaluationType.IsNumeric()) {
                throw new ViewParameterException(ViewParamMessage);
            }

            timestampExpression = validated[0];
            timePeriodCompute = ViewFactoryTimePeriodHelper.ValidateAndEvaluateTimeDeltaFactory(
                ViewName,
                viewParameters[1],
                ViewParamMessage,
                1,
                viewForgeEnv,
                streamNumber);
            eventType = parentEventType;
        }

        internal override Type TypeOfFactory()
        {
            return typeof(TimeOrderViewFactory);
        }

        internal override string FactoryMethod()
        {
            return "Timeorder";
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
                .SetProperty(
                    factory,
                    "TimestampEval",
                    CodegenEvaluator(timestampExpression.Forge, method, typeof(TimeOrderViewForge), classScope))
                .SetProperty(factory, "TimePeriodCompute", Ref("eval"))
                .SetProperty(factory, "ScheduleCallbackId", Constant(scheduleCallbackId));
        }
    }
} // end of namespace