///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.common.@internal.view.util;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.epl.expression.core.ExprNodeUtilityCodegen;

namespace com.espertech.esper.common.@internal.view.timetolive
{
    public class TimeToLiveViewForge : ViewFactoryForgeBase,
        DataWindowViewForge,
        DataWindowViewForgeWithPrevious,
        ScheduleHandleCallbackProvider
    {
        private IList<ExprNode> viewParameters;
        protected ExprNode timestampExpression;
        private int scheduleCallbackId = -1;

        public override void SetViewParameters(
            IList<ExprNode> parameters,
            ViewForgeEnv viewForgeEnv,
            int streamNumber)
        {
            viewParameters = parameters;
        }

        public override void AttachValidate(
            EventType parentEventType,
            ViewForgeEnv viewForgeEnv)
        {
            var validated = ViewForgeSupport.Validate(ViewName, parentEventType, viewParameters, true, viewForgeEnv);
            if (viewParameters.Count != 1) {
                throw new ViewParameterException(ViewParamMessage);
            }

            if (!validated[0].Forge.EvaluationType.IsTypeInt64()) {
                throw new ViewParameterException(ViewParamMessage);
            }

            timestampExpression = validated[0];
            eventType = parentEventType;
        }

        internal override Type TypeOfFactory => typeof(TimeOrderViewFactory);
        internal override string FactoryMethod => "Timeorder";

        internal override void Assign(
            CodegenMethod method,
            CodegenExpressionRef factory,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            method.Block
                .DeclareVar<TimePeriodCompute>(
                    "eval",
                    new TimePeriodComputeConstGivenDeltaForge(0).MakeEvaluator(method, classScope))
                .SetProperty(
                    factory,
                    "TimestampEval",
                    CodegenEvaluator(timestampExpression.Forge, method, typeof(TimeOrderViewForge), classScope))
                .SetProperty(factory, "TimePeriodCompute", Ref("eval"))
                .SetProperty(factory, "ScheduleCallbackId", Constant(scheduleCallbackId))
                .SetProperty(factory, "TimeToLive", ConstantTrue());
        }

        public override AppliesTo AppliesTo()
        {
            return client.annotation.AppliesTo.WINDOW_TIMETOLIVE;
        }

        public override T Accept<T>(ViewFactoryForgeVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override string ViewName => "Time-To-Live";

        public int ScheduleCallbackId {
            get => scheduleCallbackId;

            set => scheduleCallbackId = value;
        }

        public string ViewParamMessage => ViewName +
                                          " view requires a single expression supplying long-type timestamp values as a parameter";
    }
} // end of namespace