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
using static com.espertech.esper.common.@internal.epl.expression.core.ExprNodeUtilityCodegen;

namespace com.espertech.esper.common.@internal.view.timelengthbatch
{
    public class TimeLengthBatchViewForge : ViewFactoryForgeBase,
        DataWindowViewForge,
        DataWindowViewForgeWithPrevious,
        ScheduleHandleCallbackProvider,
        DataWindowBatchingViewForge
    {
        /// <summary>
        /// Number of events to collect before batch fires.
        /// </summary>
        protected ExprForge sizeForge;

        protected bool isForceUpdate;
        protected bool isStartEager;
        protected TimePeriodComputeForge timePeriodCompute;
        protected int scheduleCallbackId;

        public override void SetViewParameters(
            IList<ExprNode> parameters,
            ViewForgeEnv viewForgeEnv,
            int streamNumber)
        {
            var validated = ViewForgeSupport.Validate(ViewName, parameters, viewForgeEnv, streamNumber);
            var errorMessage = ViewName +
                               " view requires a numeric or time period parameter as a time interval size, and an integer parameter as a maximal number-of-events, and an optional list of control keywords as a string parameter (please see the documentation)";
            if (validated.Length != 2 && validated.Length != 3) {
                throw new ViewParameterException(errorMessage);
            }

            timePeriodCompute = ViewFactoryTimePeriodHelper.ValidateAndEvaluateTimeDeltaFactory(
                ViewName,
                parameters[0],
                errorMessage,
                0,
                viewForgeEnv);
            sizeForge = ViewForgeSupport.ValidateSizeParam(ViewName, validated[1], 1);
            if (validated.Length > 2) {
                var keywords = ViewForgeSupport.Evaluate(validated[2].Forge.ExprEvaluator, 2, ViewName);
                var flags = TimeBatchFlags.ProcessKeywords(keywords, errorMessage);
                isForceUpdate = flags.IsForceUpdate;
                isStartEager = flags.IsStartEager;
            }
        }

        public override void AttachValidate(
            EventType parentEventType,
            ViewForgeEnv viewForgeEnv)
        {
            eventType = parentEventType;
        }

        internal override Type TypeOfFactory => typeof(TimeLengthBatchViewFactory);
        internal override string FactoryMethod => "Timelengthbatch";

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
                .DeclareVar(typeof(TimePeriodCompute), "eval", timePeriodCompute.MakeEvaluator(method, classScope))
                .SetProperty(factory, "Size", CodegenEvaluator(sizeForge, method, GetType(), classScope))
                .SetProperty(factory, "TimePeriodCompute", Ref("eval"))
                .SetProperty(factory, "ScheduleCallbackId", Constant(scheduleCallbackId))
                .SetProperty(factory, "IsForceUpdate", Constant(isForceUpdate))
                .SetProperty(factory, "IsStartEager", Constant(isStartEager));
        }

        public override AppliesTo AppliesTo()
        {
            return client.annotation.AppliesTo.WINDOW_TIMELENGTHBATCH;
        }

        public override T Accept<T>(ViewFactoryForgeVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public int ScheduleCallbackId {
            get => scheduleCallbackId;

            set => scheduleCallbackId = value;
        }

        public override string ViewName => "Time-Length-Batch";
    }
} // end of namespace