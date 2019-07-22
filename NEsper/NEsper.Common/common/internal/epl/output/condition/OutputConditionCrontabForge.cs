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
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.schedule;
using com.espertech.esper.compat;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.output.condition
{
    /// <summary>
    ///     Output condition handling crontab-at schedule output.
    /// </summary>
    public class OutputConditionCrontabForge : OutputConditionFactoryForge,
        ScheduleHandleCallbackProvider
    {
        internal readonly bool isStartConditionOnCreation;
        internal readonly ExprForge[] scheduleSpecEvaluators;
        private int scheduleCallbackId = -1;

        public OutputConditionCrontabForge(
            IList<ExprNode> scheduleSpecExpressionList,
            bool isStartConditionOnCreation,
            StatementRawInfo statementRawInfo,
            StatementCompileTimeServices services)
        {
            scheduleSpecEvaluators = ScheduleExpressionUtil.CrontabScheduleValidate(
                ExprNodeOrigin.OUTPUTLIMIT,
                scheduleSpecExpressionList,
                false,
                statementRawInfo,
                services);
            this.isStartConditionOnCreation = isStartConditionOnCreation;
        }

        public CodegenExpression Make(
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            if (scheduleCallbackId == -1) {
                throw new IllegalStateException("Unassigned schedule");
            }

            var method = parent.MakeChild(typeof(OutputConditionFactory), GetType(), classScope);
            method.Block.DeclareVar<ExprEvaluator[]>(
                "evals",
                NewArrayByLength(typeof(ExprEvaluator), Constant(scheduleSpecEvaluators.Length)));
            for (var i = 0; i < scheduleSpecEvaluators.Length; i++) {
                method.Block.AssignArrayElement(
                    "evals",
                    Constant(i),
                    ExprNodeUtilityCodegen.CodegenEvaluatorNoCoerce(
                        scheduleSpecEvaluators[i],
                        method,
                        GetType(),
                        classScope));
            }

            method.Block.MethodReturn(
                ExprDotMethodChain(symbols.GetAddInitSvc(method))
                    .Add(EPStatementInitServicesConstants.GETRESULTSETPROCESSORHELPERFACTORY)
                    .Add(
                        "makeOutputConditionCrontab",
                        Ref("evals"),
                        Constant(isStartConditionOnCreation),
                        Constant(scheduleCallbackId)));
            return LocalMethod(method);
        }

        public void CollectSchedules(IList<ScheduleHandleCallbackProvider> scheduleHandleCallbackProviders)
        {
            scheduleHandleCallbackProviders.Add(this);
        }

        public int ScheduleCallbackId {
            set => scheduleCallbackId = value;
        }
    }
} // end of namespace