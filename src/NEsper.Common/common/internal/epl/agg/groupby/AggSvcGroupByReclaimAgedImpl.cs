///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.core;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.expression.time.abacus;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionRelational.
    CodegenRelational;
using static com.espertech.esper.common.@internal.epl.agg.groupby.AggregationServiceGroupByForge;
using static com.espertech.esper.common.@internal.epl.expression.codegen.ExprForgeCodegenNames;
using static com.espertech.esper.common.@internal.epl.resultset.codegen.ResultSetProcessorCodegenNames;

namespace com.espertech.esper.common.@internal.epl.agg.groupby
{
    /// <summary>
    ///     Implementation for handling aggregation with grouping by group-keys.
    /// </summary>
    public class AggSvcGroupByReclaimAgedImpl
    {
        public const long DEFAULT_MAX_AGE_MSEC = 60000L;
        private static readonly CodegenExpressionRef RefNextsweeptime = Ref("nextSweepTime");
        private static readonly CodegenExpressionRef RefRemovedcallback = Ref("removedCallback");
        private static readonly CodegenExpressionRef RefCurrentmaxage = Ref("currentMaxAge");
        private static readonly CodegenExpressionRef RefCurrentreclaimfrequency = Ref("currentReclaimFrequency");
        private static readonly CodegenExpressionRef RefEvaluatorfunctionmaxage = Ref("evaluationFunctionMaxAge");

        private static readonly CodegenExpressionRef RefEvaluationfunctionfrequency =
            Ref("evaluationFunctionFrequency");

        public static void RowCtorCodegen(
            CodegenNamedMethods namedMethods,
            CodegenClassScope classScope,
            IList<CodegenTypedParam> rowMembers)
        {
            rowMembers.Add(new CodegenTypedParam(typeof(long), "lastUpdateTime"));
            namedMethods.AddMethod(
                typeof(void),
                "SetLastUpdateTime",
                CodegenNamedParam.From(typeof(long), "time"),
                typeof(AggSvcGroupByReclaimAgedImpl),
                classScope,
                method => method.Block.AssignRef("lastUpdateTime", Ref("time")));
            namedMethods.AddMethod(
                typeof(long),
                "GetLastUpdateTime",
                Collections.GetEmptyList<CodegenNamedParam>(),
                typeof(AggSvcGroupByReclaimAgedImpl),
                classScope,
                method => method.Block.MethodReturn(Ref("lastUpdateTime")));
        }

        public static void CtorCodegenReclaim(
            CodegenCtor ctor,
            IList<CodegenTypedParam> explicitMembers,
            CodegenClassScope classScope,
            CodegenExpression maxAgeFactory,
            CodegenExpression frequencyFactory)
        {
            explicitMembers.Add(new CodegenTypedParam(typeof(long?), RefNextsweeptime.Ref));
            explicitMembers.Add(new CodegenTypedParam(typeof(AggregationRowRemovedCallback), RefRemovedcallback.Ref));
            explicitMembers.Add(new CodegenTypedParam(typeof(long), RefCurrentmaxage.Ref));
            explicitMembers.Add(new CodegenTypedParam(typeof(long), RefCurrentreclaimfrequency.Ref));
            explicitMembers.Add(
                new CodegenTypedParam(typeof(AggSvcGroupByReclaimAgedEvalFunc), RefEvaluatorfunctionmaxage.Ref));
            explicitMembers.Add(
                new CodegenTypedParam(typeof(AggSvcGroupByReclaimAgedEvalFunc), RefEvaluationfunctionfrequency.Ref));
            ctor.Block.AssignRef(RefCurrentmaxage, Constant(DEFAULT_MAX_AGE_MSEC))
                .AssignRef(RefCurrentreclaimfrequency, Constant(DEFAULT_MAX_AGE_MSEC))
                .AssignRef(RefEvaluatorfunctionmaxage, ExprDotMethod(maxAgeFactory, "Make", MEMBER_EXPREVALCONTEXT))
                .AssignRef(
                    RefEvaluationfunctionfrequency,
                    ExprDotMethod(frequencyFactory, "Make", MEMBER_EXPREVALCONTEXT));
        }

        public static void ApplyEnterCodegenSweep(
            CodegenMethod method,
            CodegenClassScope classScope,
            AggregationClassNames classNames)
        {
            var timeAbacus = classScope.AddOrGetDefaultFieldSharable(TimeAbacusField.INSTANCE);
            method.Block.DeclareVar<long>(
                    "currentTime",
                    ExprDotMethodChain(REF_EXPREVALCONTEXT).Get("TimeProvider").Get("Time"))
                .IfCondition(Or(EqualsNull(RefNextsweeptime), Relational(RefNextsweeptime, LE, Ref("currentTime"))))
                .AssignRef(
                    RefCurrentmaxage,
                    StaticMethod(
                        typeof(AggSvcGroupByReclaimAgedImpl),
                        "ComputeTimeReclaimAgeFreq",
                        RefCurrentmaxage,
                        RefEvaluatorfunctionmaxage,
                        timeAbacus))
                .AssignRef(
                    RefCurrentreclaimfrequency,
                    StaticMethod(
                        typeof(AggSvcGroupByReclaimAgedImpl),
                        "ComputeTimeReclaimAgeFreq",
                        RefCurrentreclaimfrequency,
                        RefEvaluationfunctionfrequency,
                        timeAbacus))
                .AssignRef(RefNextsweeptime, Op(Ref("currentTime"), "+", RefCurrentreclaimfrequency))
                .LocalMethod(SweepCodegen(method, classScope, classNames), Ref("currentTime"), RefCurrentmaxage);
        }

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="current">current</param>
        /// <param name="func">func</param>
        /// <param name="timeAbacus">abacus</param>
        /// <returns>delta</returns>
        public static long ComputeTimeReclaimAgeFreq(
            long current,
            AggSvcGroupByReclaimAgedEvalFunc func,
            TimeAbacus timeAbacus)
        {
            var maxAge = func.LongValue;
            if (maxAge == null || maxAge <= 0) {
                return current;
            }

            return timeAbacus.DeltaForSecondsDouble(maxAge.Value);
        }

        private static CodegenMethod SweepCodegen(
            CodegenMethodScope parent,
            CodegenClassScope classScope,
            AggregationClassNames classNames)
        {
            var method = parent
                .MakeChild(typeof(void), typeof(AggSvcGroupByReclaimAgedImpl), classScope)
                .AddParam(typeof(long), "currentTime")
                .AddParam(typeof(long), RefCurrentmaxage.Ref);

            method.Block.DeclareVar<ArrayDeque<object>>("removed", NewInstance(typeof(ArrayDeque<object>)))
                .ForEach(
                    typeof(KeyValuePair<object, object>),
                    "entry",
                    MEMBER_AGGREGATORSPERGROUP)
                .DeclareVar<long>(
                    "age",
                    Op(
                        Ref("currentTime"),
                        "-",
                        ExprDotMethod(
                            Cast(classNames.RowTop, ExprDotName(Ref("entry"), "Value")),
                            "GetLastUpdateTime")))
                .IfCondition(Relational(Ref("age"), GT, RefCurrentmaxage))
                .ExprDotMethod(Ref("removed"), "Add", ExprDotName(Ref("entry"), "Key"))
                .BlockEnd()
                .BlockEnd()
                .ForEach(typeof(object), "key", Ref("removed"))
                .ExprDotMethod(MEMBER_AGGREGATORSPERGROUP, "Remove", Ref("key"))
                .ExprDotMethod(RefRemovedcallback, "RemovedAggregationGroupKey", Ref("key"));

            return method;
        }
    }
} // end of namespace