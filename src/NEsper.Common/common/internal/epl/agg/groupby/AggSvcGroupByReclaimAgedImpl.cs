///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
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
    /// Implementation for handling aggregation with grouping by group-keys.
    /// </summary>
    public class AggSvcGroupByReclaimAgedImpl
    {
        private static readonly CodegenExpressionRef REF_NEXTSWEEPTIME = Ref("nextSweepTime");
        private static readonly CodegenExpressionRef REF_REMOVEDCALLBACK = Ref("removedCallback");
        private static readonly CodegenExpressionRef REF_CURRENTMAXAGE = Ref("currentMaxAge");
        private static readonly CodegenExpressionRef REF_CURRENTRECLAIMFREQUENCY = Ref("currentReclaimFrequency");
        private static readonly CodegenExpressionRef REF_EVALUATORFUNCTIONMAXAGE = Ref("evaluationFunctionMaxAge");

        private static readonly CodegenExpressionRef REF_EVALUATIONFUNCTIONFREQUENCY =
            Ref("evaluationFunctionFrequency");

        public const long DEFAULT_MAX_AGE_MSEC = 60000L;

        public static void RowCtorCodegen(
            CodegenNamedMethods namedMethods,
            CodegenClassScope classScope,
            IList<CodegenTypedParam> rowMembers)
        {
            rowMembers.Add(new CodegenTypedParam(typeof(long), "lastUpdateTime"));
            namedMethods
                .AddMethod(
                typeof(void),
                "SetLastUpdateTime",
                CodegenNamedParam.From(typeof(long), "time"),
                typeof(AggSvcGroupByReclaimAgedImpl),
                classScope,
                method => method.Block.AssignRef("lastUpdateTime", Ref("time")));
            namedMethods.AddMethod(
                typeof(long),
                "GetLastUpdateTime",
                EmptyList<CodegenNamedParam>.Instance, 
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
            explicitMembers.Add(
                new CodegenTypedParam(typeof(long?), REF_NEXTSWEEPTIME.Ref).WithFinal(false));
            explicitMembers.Add(
                new CodegenTypedParam(typeof(AggregationRowRemovedCallback), REF_REMOVEDCALLBACK.Ref).WithFinal(false));
            explicitMembers.Add(
                new CodegenTypedParam(typeof(long), REF_CURRENTMAXAGE.Ref).WithFinal(false));
            explicitMembers.Add(
                new CodegenTypedParam(typeof(long), REF_CURRENTRECLAIMFREQUENCY.Ref).WithFinal(false));
            explicitMembers.Add(
                new CodegenTypedParam(typeof(AggSvcGroupByReclaimAgedEvalFunc), REF_EVALUATORFUNCTIONMAXAGE.Ref));
            explicitMembers.Add(
                new CodegenTypedParam(typeof(AggSvcGroupByReclaimAgedEvalFunc), REF_EVALUATIONFUNCTIONFREQUENCY.Ref));
            ctor.Block.AssignRef(REF_CURRENTMAXAGE, Constant(DEFAULT_MAX_AGE_MSEC))
                .AssignRef(REF_CURRENTRECLAIMFREQUENCY, Constant(DEFAULT_MAX_AGE_MSEC))
                .AssignRef(REF_EVALUATORFUNCTIONMAXAGE, ExprDotMethod(maxAgeFactory, "Make", MEMBER_EXPREVALCONTEXT))
                .AssignRef(
                    REF_EVALUATIONFUNCTIONFREQUENCY,
                    ExprDotMethod(frequencyFactory, "Make", MEMBER_EXPREVALCONTEXT));
        }

        public static void ApplyEnterCodegenSweep(
            CodegenMethod method,
            CodegenClassScope classScope,
            AggregationClassNames classNames)
        {
            var timeAbacus = classScope.AddOrGetDefaultFieldSharable(TimeAbacusField.INSTANCE);
            method.Block.DeclareVar(
                    typeof(long),
                    "currentTime",
                    ExprDotMethodChain(REF_EXPREVALCONTEXT).Get("TimeProvider").Get("Time"))
                .IfCondition(Or(EqualsNull(REF_NEXTSWEEPTIME), Relational(REF_NEXTSWEEPTIME, LE, Ref("currentTime"))))
                .AssignRef(
                    REF_CURRENTMAXAGE,
                    StaticMethod(
                        typeof(AggSvcGroupByReclaimAgedImpl),
                        "ComputeTimeReclaimAgeFreq",
                        REF_CURRENTMAXAGE,
                        REF_EVALUATORFUNCTIONMAXAGE,
                        timeAbacus))
                .AssignRef(
                    REF_CURRENTRECLAIMFREQUENCY,
                    StaticMethod(
                        typeof(AggSvcGroupByReclaimAgedImpl),
                        "ComputeTimeReclaimAgeFreq",
                        REF_CURRENTRECLAIMFREQUENCY,
                        REF_EVALUATIONFUNCTIONFREQUENCY,
                        timeAbacus))
                .AssignRef(REF_NEXTSWEEPTIME, Op(Ref("currentTime"), "+", REF_CURRENTRECLAIMFREQUENCY))
                .LocalMethod(SweepCodegen(method, classScope, classNames), Ref("currentTime"), REF_CURRENTMAXAGE);
        }

        /// <summary>
        /// NOTE: Code-generation-invoked method, method name and parameter order matters
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
            var method = parent.MakeChild(typeof(void), typeof(AggSvcGroupByReclaimAgedImpl), classScope)
                .AddParam(typeof(long), "currentTime")
                .AddParam(typeof(long), REF_CURRENTMAXAGE.Ref);
            method.Block.DeclareVar(
                    typeof(ArrayDeque<object>),
                    "removed",
                    NewInstance(typeof(ArrayDeque<object>)))
                .ForEach(typeof(KeyValuePair<object, object>), "entry", MEMBER_AGGREGATORSPERGROUP)
                .DeclareVar(
                    typeof(long),
                    "age",
                    Op(
                        Ref("currentTime"),
                        "-",
                        ExprDotMethod(
                            Cast(classNames.RowTop, ExprDotName(Ref("entry"), "Value")),
                            "GetLastUpdateTime")))
                .IfCondition(Relational(Ref("age"), GT, REF_CURRENTMAXAGE))
                .ExprDotMethod(Ref("removed"), "Add", ExprDotName(Ref("entry"), "Key"))
                .BlockEnd()
                .BlockEnd()
                .ForEach(typeof(object), "key", Ref("removed"))
                .ExprDotMethod(MEMBER_AGGREGATORSPERGROUP, "Remove", Ref("key"))
                .ExprDotMethod(REF_REMOVEDCALLBACK, "RemovedAggregationGroupKey", Ref("key"));
            
            return method;
        }
    }
} // end of namespace