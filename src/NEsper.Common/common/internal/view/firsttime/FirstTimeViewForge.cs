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
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.common.@internal.view.util;
using com.espertech.esper.compat;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.view.firsttime
{
    /// <summary>
    /// Factory for <seealso cref = "FirstTimeView"/>.
    /// </summary>
    public class FirstTimeViewForge : ViewFactoryForgeBase,
        ScheduleHandleCallbackProvider,
        AsymetricDataWindowViewForge,
        DataWindowBatchingViewForge
    {
        protected TimePeriodComputeForge timePeriodComputeForge;
        private int scheduleCallbackId = -1;

        public override void SetViewParameters(
            IList<ExprNode> parameters,
            ViewForgeEnv viewForgeEnv,
            int streamNumber)
        {
            if (parameters.Count != 1) {
                throw new ViewParameterException(ViewParamMessage);
            }

            timePeriodComputeForge = ViewFactoryTimePeriodHelper.ValidateAndEvaluateTimeDeltaFactory(
                ViewName,
                parameters[0],
                ViewParamMessage,
                0,
                viewForgeEnv);
        }

        public override void AttachValidate(
            EventType parentEventType,
            ViewForgeEnv viewForgeEnv)
        {
            eventType = parentEventType;
        }

        internal override Type TypeOfFactory => typeof(FirstTimeViewFactory);
        internal override string FactoryMethod => "firsttime";

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
                .DeclareVar<TimePeriodCompute>("eval", timePeriodComputeForge.MakeEvaluator(method, classScope))
                .ExprDotMethod(factory, "setTimePeriodCompute", Ref("eval"))
                .ExprDotMethod(factory, "setScheduleCallbackId", Constant(scheduleCallbackId));
        }

        public override AppliesTo AppliesTo()
        {
            return client.annotation.AppliesTo.WINDOW_FIRSTTIME;
        }

        public override T Accept<T>(ViewFactoryForgeVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override string ViewName => "First-Time";

        public string ViewParamMessage => ViewName + " view requires a single numeric or time period parameter";

        public int ScheduleCallbackId {
            get => scheduleCallbackId;

            set => scheduleCallbackId = value;
        }
    }
} // end of namespace