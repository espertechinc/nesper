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
using static com.espertech.esper.common.@internal.epl.expression.core.ExprNodeUtilityCodegen;

namespace com.espertech.esper.common.@internal.view.timelengthbatch
{
    public class TimeLengthBatchViewForge : ViewFactoryForgeBase,
        DataWindowViewForge,
        DataWindowViewForgeWithPrevious,
        ScheduleHandleCallbackProvider,
        DataWindowBatchingViewForge
    {
        internal bool isForceUpdate;
        internal bool isStartEager;
        internal int scheduleCallbackId;

        /// <summary>
        ///     Number of events to collect before batch fires.
        /// </summary>
        internal ExprForge sizeForge;

        internal TimePeriodComputeForge timePeriodCompute;

        public override string ViewName => "Time-Length-Batch";

        public int ScheduleCallbackId {
            set => scheduleCallbackId = value;
        }

        public override void SetViewParameters(
            IList<ExprNode> parameters,
            ViewForgeEnv viewForgeEnv,
            int streamNumber)
        {
            var validated = ViewForgeSupport.Validate(ViewName, parameters, viewForgeEnv, streamNumber);
            var errorMessage =
                ViewName +
                " view requires a numeric or time period parameter as a time interval size, and an integer parameter as a maximal number-of-events, and an optional list of control keywords as a string parameter (please see the documentation)";
            if (validated.Length != 2 && validated.Length != 3) {
                throw new ViewParameterException(errorMessage);
            }

            timePeriodCompute = ViewFactoryTimePeriodHelper.ValidateAndEvaluateTimeDeltaFactory(
                ViewName,
                parameters[0],
                errorMessage,
                0,
                viewForgeEnv,
                streamNumber);

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
            int streamNumber,
            ViewForgeEnv viewForgeEnv,
            bool grouped)
        {
            eventType = parentEventType;
        }

        public override Type TypeOfFactory()
        {
            return typeof(TimeLengthBatchViewFactory);
        }

        public override string FactoryMethod()
        {
            return "Timelengthbatch";
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
                .SetProperty(factory, "SizeEvaluator", CodegenEvaluator(sizeForge, method, GetType(), classScope))
                .SetProperty(factory, "TimePeriodCompute", Ref("eval"))
                .SetProperty(factory, "ScheduleCallbackId", Constant(scheduleCallbackId))
                .SetProperty(factory, "ForceUpdate", Constant(isForceUpdate))
                .SetProperty(factory, "StartEager", Constant(isStartEager));
        }

        public override AppliesTo AppliesTo()
        {
            return client.annotation.AppliesTo.WINDOW_TIMELENGTHBATCH;
        }
    }
} // end of namespace