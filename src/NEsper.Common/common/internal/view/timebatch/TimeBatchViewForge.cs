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
using com.espertech.esper.compat;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.view.timebatch
{
    /// <summary>
    /// Factory for <seealso cref = "TimeBatchView"/>.
    /// </summary>
    public class TimeBatchViewForge : ViewFactoryForgeBase,
        DataWindowViewForge,
        DataWindowViewForgeWithPrevious,
        ScheduleHandleCallbackProvider,
        DataWindowBatchingViewForge
    {
        /// <summary>
        /// The reference point, or null if none supplied.
        /// </summary>
        protected long? optionalReferencePoint;

        protected bool isForceUpdate;
        protected bool isStartEager;
        protected TimePeriodComputeForge timePeriodCompute;
        protected int scheduleCallbackId;

        public override void SetViewParameters(
            IList<ExprNode> parameters,
            ViewForgeEnv viewForgeEnv,
            int streamNumber)
        {
            if (parameters.Count < 1 || parameters.Count > 3) {
                throw new ViewParameterException(ViewParamMessage);
            }

            var viewParamValues = new object[parameters.Count];
            for (var i = 1; i < viewParamValues.Length; i++) {
                viewParamValues[i] = ViewForgeSupport.ValidateAndEvaluate(ViewName, parameters[i], viewForgeEnv);
            }

            timePeriodCompute = ViewFactoryTimePeriodHelper.ValidateAndEvaluateTimeDeltaFactory(
                ViewName,
                parameters[0],
                ViewParamMessage,
                0,
                viewForgeEnv);
            var timeBatchFlags = new TimeBatchFlags(false, false);
            if (viewParamValues.Length == 2 && viewParamValues[1] is string) {
                timeBatchFlags = TimeBatchFlags.ProcessKeywords(viewParamValues[1], ViewParamMessage);
            }
            else {
                if (viewParamValues.Length >= 2) {
                    var paramRef = viewParamValues[1];
                    if (!(paramRef.IsNumber()) || paramRef.IsFloatingPointNumber()) {
                        throw new ViewParameterException(
                            ViewName + " view requires a Long-typed reference point in msec as a second parameter");
                    }

                    optionalReferencePoint = paramRef.AsInt64();
                }

                if (viewParamValues.Length == 3) {
                    timeBatchFlags = TimeBatchFlags.ProcessKeywords(viewParamValues[2], ViewParamMessage);
                }
            }

            isForceUpdate = timeBatchFlags.IsForceUpdate;
            isStartEager = timeBatchFlags.IsStartEager;
        }

        public override void AttachValidate(
            EventType parentEventType,
            ViewForgeEnv viewForgeEnv)
        {
            eventType = parentEventType;
        }

        internal override Type TypeOfFactory => typeof(TimeBatchViewFactory);
        internal override string FactoryMethod => "Timebatch";

        internal override void Assign(
            CodegenMethod method,
            CodegenExpressionRef factory,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            if (scheduleCallbackId == -1) {
                throw new IllegalStateException("No schedule callback id");
            }

            method.Block.DeclareVar<TimePeriodCompute>("eval", timePeriodCompute.MakeEvaluator(method, classScope))
                .SetProperty(factory, "TimePeriodCompute", Ref("eval"))
                .SetProperty(factory, "ScheduleCallbackId", Constant(scheduleCallbackId))
                .SetProperty(factory, "IsForceUpdate", Constant(isForceUpdate))
                .SetProperty(factory, "IsStartEager", Constant(isStartEager))
                .SetProperty(factory, "OptionalReferencePoint", Constant(optionalReferencePoint));
        }

        public override AppliesTo AppliesTo()
        {
            return client.annotation.AppliesTo.WINDOW_TIMEBATCH;
        }

        public override T Accept<T>(ViewFactoryForgeVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public string ViewParamMessage => ViewName +
                                          " view requires a single numeric or time period parameter, and an optional long-typed reference point in msec, and an optional list of control keywords as a string parameter (please see the documentation)";

        public override string ViewName => "Time-Batch";

        public int ScheduleCallbackId {
            get => scheduleCallbackId;

            set => scheduleCallbackId = value;
        }
    }
} // end of namespace