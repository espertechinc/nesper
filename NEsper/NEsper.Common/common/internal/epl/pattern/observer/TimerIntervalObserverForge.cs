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
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.time.abacus;
using com.espertech.esper.common.@internal.epl.expression.time.node;
using com.espertech.esper.common.@internal.epl.pattern.core;
using com.espertech.esper.common.@internal.schedule;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.pattern.observer
{
    /// <summary>
    ///     Factory for making observer instances.
    /// </summary>
    public class TimerIntervalObserverForge : ObserverForge,
        ScheduleHandleCallbackProvider
    {
        private const string NAME = "Timer-interval observer";
        internal MatchedEventConvertorForge convertor;

        internal ExprNode parameter;
        internal int scheduleCallbackId = -1;
        internal TimeAbacus timeAbacus;

        public void SetObserverParameters(
            IList<ExprNode> parameters,
            MatchedEventConvertorForge convertor,
            ExprValidationContext validationContext)
        {
            ObserverParameterUtil.ValidateNoNamedParameters(NAME, parameters);
            var errorMessage = NAME + " requires a single numeric or time period parameter";
            if (parameters.Count != 1) {
                throw new ObserverParameterException(errorMessage);
            }

            if (!(parameters[0] is ExprTimePeriod)) {
                var returnType = parameters[0].Forge.EvaluationType;
                if (!returnType.IsNumeric()) {
                    throw new ObserverParameterException(errorMessage);
                }
            }

            parameter = parameters[0];
            this.convertor = convertor;
            timeAbacus = validationContext.ImportService.TimeAbacus;
        }

        public void CollectSchedule(IList<ScheduleHandleCallbackProvider> schedules)
        {
            schedules.Add(this);
        }

        public CodegenExpression MakeCodegen(
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            if (scheduleCallbackId == -1) {
                throw new IllegalStateException("Unassigned schedule callback id");
            }

            var method = parent.MakeChild(
                typeof(TimerIntervalObserverFactory),
                typeof(TimerIntervalObserverForge),
                classScope);
            var patternDelta = PatternDeltaComputeUtil.MakePatternDeltaAnonymous(
                parameter,
                convertor,
                timeAbacus,
                method,
                classScope);

            method.Block
                .DeclareVar<TimerIntervalObserverFactory>(
                    "factory",
                    ExprDotMethodChain(symbols.GetAddInitSvc(method))
                        .Add(EPStatementInitServicesConstants.GETPATTERNFACTORYSERVICE)
                        .Add("observerTimerInterval"))
                .SetProperty(Ref("factory"), "ScheduleCallbackId", Constant(scheduleCallbackId))
                .SetProperty(Ref("factory"), "DeltaCompute", patternDelta)
                .MethodReturn(Ref("factory"));
            return LocalMethod(method);
        }

        public int ScheduleCallbackId {
            set => scheduleCallbackId = value;
        }
    }
} // end of namespace