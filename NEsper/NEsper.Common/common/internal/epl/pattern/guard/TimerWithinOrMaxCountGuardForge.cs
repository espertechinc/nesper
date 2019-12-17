///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
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
    public class TimerWithinOrMaxCountGuardForge : GuardForge,
        ScheduleHandleCallbackProvider
    {
        /// <summary>
        ///     For converting matched-events maps to events-per-stream.
        /// </summary>
        [NonSerialized] protected MatchedEventConvertorForge convertor;

        private ExprNode numCountToExpr;
        private int scheduleCallbackId = -1;
        private TimeAbacus timeAbacus;

        private ExprNode timeExpr;

        public void SetGuardParameters(
            IList<ExprNode> parameters,
            MatchedEventConvertorForge convertor,
            StatementCompileTimeServices services)
        {
            var message = "Timer-within-or-max-count guard requires two parameters: " +
                          "numeric or time period parameter and an integer-value expression parameter";

            if (parameters.Count != 2) {
                throw new GuardParameterException(message);
            }

            if (!parameters[0].Forge.EvaluationType.IsNumeric()) {
                throw new GuardParameterException(message);
            }

            var paramOneType = parameters[1].Forge.EvaluationType;
            if (paramOneType != typeof(int) && paramOneType != typeof(int)) {
                throw new GuardParameterException(message);
            }

            timeExpr = parameters[0];
            numCountToExpr = parameters[1];
            this.convertor = convertor;
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

            var method = parent.MakeChild(typeof(TimerWithinOrMaxCountGuardFactory), GetType(), classScope);
            var patternDelta = PatternDeltaComputeUtil.MakePatternDeltaLambda(
                timeExpr,
                convertor,
                timeAbacus,
                method,
                classScope);

            CodegenExpression convertorExpr;
            if (numCountToExpr.Forge.ForgeConstantType.IsCompileTimeConstant) {
                convertorExpr = ConstantNull();
            }
            else {
                convertorExpr = ExprNodeUtilityCodegen.CodegenEvaluator(
                    numCountToExpr.Forge,
                    method,
                    GetType(),
                    classScope);
            }

            method.Block
                .DeclareVar<TimerWithinOrMaxCountGuardFactory>(
                    "factory",
                    ExprDotMethodChain(symbols.GetAddInitSvc(method))
                        .Get(EPStatementInitServicesConstants.PATTERNFACTORYSERVICE)
                        .Add("GuardTimerWithinOrMax"))
                .SetProperty(Ref("factory"), "ScheduleCallbackId", Constant(scheduleCallbackId))
                .SetProperty(Ref("factory"), "DeltaCompute", patternDelta)
                .SetProperty(Ref("factory"), "OptionalConvertor", convertorExpr)
                .SetProperty(
                    Ref("factory"),
                    "CountEval",
                    ExprNodeUtilityCodegen.CodegenEvaluator(numCountToExpr.Forge, method, GetType(), classScope))
                .MethodReturn(Ref("factory"));
            return LocalMethod(method);
        }

        public int ScheduleCallbackId {
            set => scheduleCallbackId = value;
        }
    }
} // end of namespace