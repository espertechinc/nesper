///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.serde;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.core;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.time.abacus;
using com.espertech.esper.common.@internal.epl.resultset.codegen;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionRelational.
    CodegenRelational;
using static com.espertech.esper.common.@internal.epl.agg.core.AggregationServiceCodegenNames;
using static com.espertech.esper.common.@internal.epl.expression.codegen.ExprForgeCodegenNames;
using static com.espertech.esper.common.@internal.epl.resultset.codegen.ResultSetProcessorCodegenNames;
using static com.espertech.esper.common.@internal.metrics.instrumentation.InstrumentationCode;

namespace com.espertech.esper.common.@internal.epl.agg.groupby
{
    /// <summary>
    ///     Implementation for handling aggregation with grouping by group-keys.
    /// </summary>
    public class AggregationServiceGroupByForge : AggregationServiceFactoryForgeWMethodGen
    {
        public static readonly CodegenExpressionMember MEMBER_CURRENTROW = Member("currentRow");
        public static readonly CodegenExpressionMember MEMBER_CURRENTGROUPKEY = Member("currentGroupKey");
        public static readonly CodegenExpressionMember MEMBER_AGGREGATORSPERGROUP = Member("aggregatorsPerGroup");
        public static readonly CodegenExpressionMember MEMBER_REMOVEDKEYS = Member("removedKeys");

        protected internal readonly AggGroupByDesc aggGroupByDesc;
        protected internal readonly TimeAbacus timeAbacus;

        protected internal CodegenExpression reclaimAge;
        protected internal CodegenExpression reclaimFreq;

        public AggregationServiceGroupByForge(
            AggGroupByDesc aggGroupByDesc,
            TimeAbacus timeAbacus)
        {
            this.aggGroupByDesc = aggGroupByDesc;
            this.timeAbacus = timeAbacus;
        }

        private bool HasRefCounting => aggGroupByDesc.IsRefcounted || aggGroupByDesc.IsReclaimAged;

        public void ProviderCodegen(
            CodegenMethod method,
            CodegenClassScope classScope,
            AggregationClassNames classNames)
        {
            var groupByTypes = ExprNodeUtilityQuery.GetExprResultTypes(aggGroupByDesc.GroupByNodes);

            if (aggGroupByDesc.IsReclaimAged) {
                reclaimAge = aggGroupByDesc.ReclaimEvaluationFunctionMaxAge.Make(classScope);
                reclaimFreq = aggGroupByDesc.ReclaimEvaluationFunctionFrequency.Make(classScope);
            }
            else {
                reclaimAge = ConstantNull();
                reclaimFreq = ConstantNull();
            }

            var stmtFields = ResultSetProcessorCodegenNames.REF_STATEMENT_FIELDS;
            var timeAbacus = classScope.AddOrGetDefaultFieldSharable(TimeAbacusField.INSTANCE);

            method.Block
                .DeclareVar<AggregationRowFactory>(
                    "rowFactory",
                    NewInstanceInner(classNames.RowFactoryTop, Ref("this")))
                .DeclareVar<DataInputOutputSerde<AggregationRow>>(
                    "rowSerde",
                    NewInstanceInner(classNames.RowSerdeTop, Ref("this")))
                .DeclareVar<AggregationServiceFactory>(
                    "svcFactory",
                    NewInstanceInner(classNames.ServiceFactory, Ref("this")))
                .DeclareVar<DataInputOutputSerde>(
                    "serde", aggGroupByDesc.GroupByMultiKey.GetExprMKSerde(method, classScope))
                .MethodReturn(
                    ExprDotMethodChain(EPStatementInitServicesConstants.REF)
                        .Get(EPStatementInitServicesConstants.AGGREGATIONSERVICEFACTORYSERVICE)
                        .Add(
                            "GroupBy",
                            Ref("svcFactory"),
                            Ref("rowFactory"),
                            aggGroupByDesc.RowStateForgeDescs.UseFlags.ToExpression(),
                            Ref("rowSerde"),
                            Constant(groupByTypes),
                            reclaimAge,
                            reclaimFreq,
                            timeAbacus,
                            Ref("serde")));
        }

        public void RowCtorCodegen(AggregationRowCtorDesc rowCtorDesc)
        {
            AggregationServiceCodegenUtil.GenerateIncidentals(
                HasRefCounting,
                aggGroupByDesc.IsReclaimAged,
                rowCtorDesc);
        }

        public void RowWriteMethodCodegen(
            CodegenMethod method,
            int level)
        {
            if (HasRefCounting) {
                method.Block.ExprDotMethod(Ref("output"), "WriteInt", Ref("row.refcount"));
            }

            if (aggGroupByDesc.IsReclaimAged) {
                method.Block.ExprDotMethod(Ref("output"), "WriteLong", Ref("row.lastUpd"));
            }
        }

        public void RowReadMethodCodegen(
            CodegenMethod method,
            int level)
        {
            if (HasRefCounting) {
                method.Block.AssignRef("row.refcount", ExprDotMethod(Ref("input"), "ReadInt"));
            }

            if (aggGroupByDesc.IsReclaimAged) {
                method.Block.AssignRef("row.lastUpd", ExprDotMethod(Ref("input"), "ReadLong"));
            }
        }

        public void MakeServiceCodegen(
            CodegenMethod method,
            CodegenClassScope classScope,
            AggregationClassNames classNames)
        {
            method.Block.MethodReturn(NewInstanceInner(classNames.Service, Ref("o"), MEMBER_AGENTINSTANCECONTEXT));
        }

        public void CtorCodegen(
            CodegenCtor ctor,
            IList<CodegenTypedParam> explicitMembers,
            CodegenClassScope classScope,
            AggregationClassNames classNames)
        {
            ctor.CtorParams.Add(new CodegenTypedParam(typeof(AgentInstanceContext), NAME_AGENTINSTANCECONTEXT));
            explicitMembers.Add(
                new CodegenTypedParam(typeof(IDictionary<object, object>), MEMBER_AGGREGATORSPERGROUP.Ref));
            explicitMembers.Add(new CodegenTypedParam(typeof(object), MEMBER_CURRENTGROUPKEY.Ref));
            explicitMembers.Add(new CodegenTypedParam(classNames.RowTop, MEMBER_CURRENTROW.Ref));
            ctor.Block.AssignRef(MEMBER_AGGREGATORSPERGROUP, NewInstance(typeof(Dictionary<object, object>)));
            if (aggGroupByDesc.IsReclaimAged) {
                AggSvcGroupByReclaimAgedImpl.CtorCodegenReclaim(
                    ctor,
                    explicitMembers,
                    classScope,
                    reclaimAge,
                    reclaimFreq);
            }

            if (HasRefCounting) {
                explicitMembers.Add(new CodegenTypedParam(typeof(IList<object>), MEMBER_REMOVEDKEYS.Ref));
                ctor.Block.AssignRef(MEMBER_REMOVEDKEYS, NewInstance<List<object>>(Constant(4)));
            }
        }

        public void GetValueCodegen(
            CodegenMethod method,
            CodegenClassScope classScope,
            CodegenNamedMethods namedMethods)
        {
            method.Block.DebugStack();
            method.Block.MethodReturn(
                ExprDotMethod(
                    MEMBER_CURRENTROW,
                    "GetValue",
                    REF_COLUMN,
                    REF_EPS,
                    ExprForgeCodegenNames.REF_ISNEWDATA,
                    REF_EXPREVALCONTEXT));
        }

        public void GetEventBeanCodegen(
            CodegenMethod method,
            CodegenClassScope classScope,
            CodegenNamedMethods namedMethods)
        {
            method.Block.MethodReturn(
                ExprDotMethod(
                    MEMBER_CURRENTROW,
                    "GetEventBean",
                    REF_COLUMN,
                    REF_EPS,
                    ExprForgeCodegenNames.REF_ISNEWDATA,
                    REF_EXPREVALCONTEXT));
        }

        public void GetCollectionScalarCodegen(
            CodegenMethod method,
            CodegenClassScope classScope,
            CodegenNamedMethods namedMethods)
        {
            method.Block.MethodReturn(
                ExprDotMethod(
                    MEMBER_CURRENTROW,
                    "GetCollectionScalar",
                    REF_COLUMN,
                    REF_EPS,
                    ExprForgeCodegenNames.REF_ISNEWDATA,
                    REF_EXPREVALCONTEXT));
        }

        public void GetCollectionOfEventsCodegen(
            CodegenMethod method,
            CodegenClassScope classScope,
            CodegenNamedMethods namedMethods)
        {
            method.Block.MethodReturn(
                ExprDotMethod(
                    MEMBER_CURRENTROW,
                    "GetCollectionOfEvents",
                    REF_COLUMN,
                    REF_EPS,
                    ExprForgeCodegenNames.REF_ISNEWDATA,
                    REF_EXPREVALCONTEXT));
        }

        public void ApplyEnterCodegen(
            CodegenMethod method,
            CodegenClassScope classScope,
            CodegenNamedMethods namedMethods,
            AggregationClassNames classNames)
        {
            method.Block
                .Apply(
                    Instblock(
                        classScope,
                        "qAggregationGroupedApplyEnterLeave",
                        ConstantTrue(),
                        Constant(aggGroupByDesc.NumMethods),
                        Constant(aggGroupByDesc.NumAccess),
                        REF_GROUPKEY));

            if (aggGroupByDesc.IsReclaimAged) {
                AggSvcGroupByReclaimAgedImpl.ApplyEnterCodegenSweep(method, classScope, classNames);
            }

            if (HasRefCounting) {
                method.Block.LocalMethod(HandleRemovedKeysCodegen(method, classScope));
            }

            var block = method.Block.AssignRef(
                MEMBER_CURRENTROW,
                Cast(classNames.RowTop, ExprDotMethod(MEMBER_AGGREGATORSPERGROUP, "Get", REF_GROUPKEY)));
            block.IfCondition(EqualsNull(MEMBER_CURRENTROW))
                .AssignRef(MEMBER_CURRENTROW, NewInstanceInner(classNames.RowTop, Ref("o")))
                .ExprDotMethod(MEMBER_AGGREGATORSPERGROUP, "Put", REF_GROUPKEY, MEMBER_CURRENTROW);

            if (HasRefCounting) {
                block.ExprDotMethod(MEMBER_CURRENTROW, "IncreaseRefcount");
            }

            if (aggGroupByDesc.IsReclaimAged) {
                block.ExprDotMethod(MEMBER_CURRENTROW, "SetLastUpdateTime", Ref("currentTime"));
            }

            block.ExprDotMethod(MEMBER_CURRENTROW, "ApplyEnter", REF_EPS, REF_EXPREVALCONTEXT)
                .Apply(Instblock(classScope, "aAggregationGroupedApplyEnterLeave", ConstantTrue()));
        }

        public void ApplyLeaveCodegen(
            CodegenMethod method,
            CodegenClassScope classScope,
            CodegenNamedMethods namedMethods,
            AggregationClassNames classNames)
        {
            method.Block
                .Apply(
                    Instblock(
                        classScope,
                        "qAggregationGroupedApplyEnterLeave",
                        ConstantFalse(),
                        Constant(aggGroupByDesc.NumMethods),
                        Constant(aggGroupByDesc.NumAccess),
                        REF_GROUPKEY))
                .AssignRef(
                    MEMBER_CURRENTROW,
                    Cast(classNames.RowTop, ExprDotMethod(MEMBER_AGGREGATORSPERGROUP, "Get", REF_GROUPKEY)))
                .IfCondition(EqualsNull(MEMBER_CURRENTROW))
                .AssignRef(MEMBER_CURRENTROW, NewInstanceInner(classNames.RowTop, Ref("o")))
                .ExprDotMethod(MEMBER_AGGREGATORSPERGROUP, "Put", REF_GROUPKEY, MEMBER_CURRENTROW);

            if (HasRefCounting) {
                method.Block.ExprDotMethod(MEMBER_CURRENTROW, "DecreaseRefcount");
            }

            if (aggGroupByDesc.IsReclaimAged) {
                method.Block.ExprDotMethod(
                    MEMBER_CURRENTROW,
                    "SetLastUpdateTime",
                    ExprDotMethodChain(REF_EXPREVALCONTEXT).Get("TimeProvider").Get("Time"));
            }

            method.Block.ExprDotMethod(MEMBER_CURRENTROW, "ApplyLeave", REF_EPS, REF_EXPREVALCONTEXT);

            if (HasRefCounting) {
                method.Block.IfCondition(Relational(ExprDotMethod(MEMBER_CURRENTROW, "GetRefcount"), LE, Constant(0)))
                    .ExprDotMethod(MEMBER_REMOVEDKEYS, "Add", REF_GROUPKEY);
            }

            method.Block.Apply(Instblock(classScope, "aAggregationGroupedApplyEnterLeave", ConstantFalse()));
        }

        public void StopMethodCodegen(
            AggregationServiceFactoryForgeWMethodGen forge,
            CodegenMethod method)
        {
            // no code
        }

        public void SetRemovedCallbackCodegen(CodegenMethod method)
        {
            if (aggGroupByDesc.IsReclaimAged) {
                method.Block.AssignRef("removedCallback", REF_CALLBACK);
            }
        }

        public void SetCurrentAccessCodegen(
            CodegenMethod method,
            CodegenClassScope classScope,
            AggregationClassNames classNames)
        {
            method.Block.AssignRef(MEMBER_CURRENTGROUPKEY, REF_GROUPKEY)
                .AssignRef(
                    MEMBER_CURRENTROW,
                    Cast(classNames.RowTop, ExprDotMethod(MEMBER_AGGREGATORSPERGROUP, "Get", REF_GROUPKEY)))
                .IfCondition(EqualsNull(MEMBER_CURRENTROW))
                .AssignRef(MEMBER_CURRENTROW, NewInstanceInner(classNames.RowTop, Ref("o")));
        }

        public void ClearResultsCodegen(
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            method.Block.ExprDotMethod(MEMBER_AGGREGATORSPERGROUP, "Clear");
        }

        public AggregationCodegenRowLevelDesc RowLevelDesc =>
            AggregationCodegenRowLevelDesc.FromTopOnly(aggGroupByDesc.RowStateForgeDescs);

        public void AcceptCodegen(
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            method.Block.ExprDotMethod(
                REF_AGGVISITOR,
                "VisitAggregations",
                ExprDotName(MEMBER_AGGREGATORSPERGROUP, "Count"),
                MEMBER_AGGREGATORSPERGROUP);
        }

        public void GetGroupKeysCodegen(
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            if (aggGroupByDesc.IsRefcounted) {
                method.Block.LocalMethod(HandleRemovedKeysCodegen(method, classScope));
            }

            method.Block.MethodReturn(ExprDotName(MEMBER_AGGREGATORSPERGROUP, "Keys"));
        }

        public void GetGroupKeyCodegen(
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            method.Block.MethodReturn(MEMBER_CURRENTGROUPKEY);
        }

        public void AcceptGroupDetailCodegen(
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            method.Block.ExprDotMethod(REF_AGGVISITOR, "VisitGrouped", ExprDotName(MEMBER_AGGREGATORSPERGROUP, "Count"))
                .ForEach(
                    typeof(KeyValuePair<object, object>),
                    "entry",
                    MEMBER_AGGREGATORSPERGROUP)
                .ExprDotMethod(
                    REF_AGGVISITOR,
                    "VisitGroup",
                    ExprDotName(Ref("entry"), "Key"),
                    ExprDotName(Ref("entry"), "Value"));
        }

        public void IsGroupedCodegen(
            CodegenProperty property,
            CodegenClassScope classScope)
        {
            property.GetterBlock.BlockReturn(ConstantTrue());
        }

        public void GetRowCodegen(
            CodegenMethod method,
            CodegenClassScope classScope,
            CodegenNamedMethods namedMethods)
        {
            method.Block.MethodReturn(MEMBER_CURRENTROW);
        }

        private CodegenMethod HandleRemovedKeysCodegen(
            CodegenMethod scope,
            CodegenClassScope classScope)
        {
            var method = scope.MakeChild(typeof(void), typeof(AggregationServiceGroupByForge), classScope);
            method.Block.IfCondition(Not(ExprDotMethod(MEMBER_REMOVEDKEYS, "IsEmpty")))
                .ForEach(typeof(object), "removedKey", MEMBER_REMOVEDKEYS)
                .ExprDotMethod(MEMBER_AGGREGATORSPERGROUP, "Remove", Ref("removedKey"))
                .BlockEnd()
                .ExprDotMethod(MEMBER_REMOVEDKEYS, "Clear");
            return method;
        }
    }
} // end of namespaceiceGroupBy