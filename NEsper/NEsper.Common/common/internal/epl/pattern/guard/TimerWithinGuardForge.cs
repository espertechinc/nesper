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
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.time.abacus;
using com.espertech.esper.common.@internal.epl.pattern.core;
using com.espertech.esper.common.@internal.schedule;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.pattern.guard
{
    /// <summary>
    ///     Forge for <seealso cref="TimerWithinGuard" /> instances.
    /// </summary>
    public class TimerWithinGuardForge : GuardForge,
        ScheduleHandleCallbackProvider
    {
        private MatchedEventConvertorForge convertor;
        private int scheduleCallbackId = -1;
        private TimeAbacus timeAbacus;

        private ExprNode timeExpr;

        public void SetGuardParameters(
            IList<ExprNode> parameters,
            MatchedEventConvertorForge convertor,
            StatementCompileTimeServices services)
        {
            var errorMessage = "Timer-within guard requires a single numeric or time period parameter";
            if (parameters.Count != 1) {
                throw new GuardParameterException(errorMessage);
            }

            if (!parameters[0].Forge.EvaluationType.IsNumeric()) {
                throw new GuardParameterException(errorMessage);
            }

            this.convertor = convertor;
            timeExpr = parameters[0];
            timeAbacus = services.ImportServiceCompileTime.TimeAbacus;
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

            var method = parent.MakeChild(typeof(TimerWithinGuardFactory), GetType(), classScope);
            var patternDelta = PatternDeltaComputeUtil.MakePatternDeltaAnonymous(
                timeExpr,
                convertor,
                timeAbacus,
                method,
                classScope);

            method.Block
                .DeclareVar<TimerWithinGuardFactory>(
                    "factory",
                    ExprDotMethodChain(symbols.GetAddInitSvc(method))
                        .Get(EPStatementInitServicesConstants.PATTERNFACTORYSERVICE)
                        .Add("GuardTimerWithin"))
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