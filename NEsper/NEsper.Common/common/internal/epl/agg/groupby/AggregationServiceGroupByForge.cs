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
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.time.abacus;
using com.espertech.esper.common.@internal.serde;

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
        public static readonly CodegenExpressionRef REF_CURRENTROW = new CodegenExpressionRef("currentRow");
        public static readonly CodegenExpressionRef REF_CURRENTGROUPKEY = new CodegenExpressionRef("currentGroupKey");
        public static readonly CodegenExpressionRef REF_AGGREGATORSPERGROUP = Ref("aggregatorsPerGroup");
        public static readonly CodegenExpressionRef REF_REMOVEDKEYS = Ref("removedKeys");

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

            var timeAbacus = classScope.AddOrGetFieldSharable(TimeAbacusField.INSTANCE);
            method.Block
                .DeclareVar<AggregationRowFactory>(
                    "rowFactory",
                    NewInstance(classNames.RowFactoryTop, Ref("this")))
                .DeclareVar<DataInputOutputSerdeWCollation<AggregationRow>>(
                    "rowSerde",
                    NewInstance(classNames.RowSerdeTop, Ref("this")))
                .DeclareVar<AggregationServiceFactory>(
                    "svcFactory",
                    NewInstance(classNames.ServiceFactory, Ref("this")))
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
                            timeAbacus));
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
            method.Block.MethodReturn(NewInstance(classNames.Service, Ref("o"), REF_AGENTINSTANCECONTEXT));
        }

        public void CtorCodegen(
            CodegenCtor ctor,
            IList<CodegenTypedParam> explicitMembers,
            CodegenClassScope classScope,
            AggregationClassNames classNames)
        {
            ctor.CtorParams.Add(new CodegenTypedParam(typeof(AgentInstanceContext), NAME_AGENTINSTANCECONTEXT));
            explicitMembers.Add(
                new CodegenTypedParam(typeof(IDictionary<object, object>), REF_AGGREGATORSPERGROUP.Ref));
            explicitMembers.Add(new CodegenTypedParam(typeof(object), REF_CURRENTGROUPKEY.Ref));
            explicitMembers.Add(new CodegenTypedParam(classNames.RowTop, REF_CURRENTROW.Ref));
            ctor.Block.AssignRef(REF_AGGREGATORSPERGROUP, NewInstance(typeof(Dictionary<object, object>)));
            if (aggGroupByDesc.IsReclaimAged) {
                AggSvcGroupByReclaimAgedImpl.CtorCodegenReclaim(
                    ctor,
                    explicitMembers,
                    classScope,
                    reclaimAge,
                    reclaimFreq);
            }

            if (HasRefCounting) {
                explicitMembers.Add(new CodegenTypedParam(typeof(IList<object>), REF_REMOVEDKEYS.Ref));
                ctor.Block.AssignRef(REF_REMOVEDKEYS, NewInstance<List<object>>(Constant(4)));
            }
        }

        public void GetValueCodegen(
            CodegenMethod method,
            CodegenClassScope classScope,
            CodegenNamedMethods namedMethods)
        {
            method.Block.MethodReturn(
                ExprDotMethod(
                    REF_CURRENTROW,
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
                    REF_CURRENTROW,
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
                    REF_CURRENTROW,
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
                    REF_CURRENTROW,
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
                method.Block.InstanceMethod(HandleRemovedKeysCodegen(method, classScope));
            }

            var block = method.Block.AssignRef(
                REF_CURRENTROW,
                Cast(classNames.RowTop, ExprDotMethod(REF_AGGREGATORSPERGROUP, "Get", REF_GROUPKEY)));
            block.IfCondition(EqualsNull(REF_CURRENTROW))
                .AssignRef(REF_CURRENTROW, NewInstance(classNames.RowTop))
                .ExprDotMethod(REF_AGGREGATORSPERGROUP, "Put", REF_GROUPKEY, REF_CURRENTROW);

            if (HasRefCounting) {
                block.ExprDotMethod(REF_CURRENTROW, "IncreaseRefcount");
            }

            if (aggGroupByDesc.IsReclaimAged) {
                block.ExprDotMethod(REF_CURRENTROW, "SetLastUpdateTime", Ref("currentTime"));
            }

            block.ExprDotMethod(REF_CURRENTROW, "ApplyEnter", REF_EPS, REF_EXPREVALCONTEXT)
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
                    REF_CURRENTROW,
                    Cast(classNames.RowTop, ExprDotMethod(REF_AGGREGATORSPERGROUP, "Get", REF_GROUPKEY)))
                .IfCondition(EqualsNull(REF_CURRENTROW))
                .AssignRef(REF_CURRENTROW, NewInstance(classNames.RowTop))
                .ExprDotMethod(REF_AGGREGATORSPERGROUP, "Put", REF_GROUPKEY, REF_CURRENTROW);

            if (HasRefCounting) {
                method.Block.ExprDotMethod(REF_CURRENTROW, "DecreaseRefcount");
            }

            if (aggGroupByDesc.IsReclaimAged) {
                method.Block.SetProperty(
                    REF_CURRENTROW,
                    "LastUpdateTime",
                    ExprDotMethodChain(REF_EXPREVALCONTEXT).Get("TimeProvider").Get("Time"));
            }

            method.Block.ExprDotMethod(REF_CURRENTROW, "ApplyLeave", REF_EPS, REF_EXPREVALCONTEXT);

            if (HasRefCounting) {
                method.Block.IfCondition(Relational(ExprDotMethod(REF_CURRENTROW, "GetRefcount"), LE, Constant(0)))
                    .ExprDotMethod(REF_REMOVEDKEYS, "Add", REF_GROUPKEY);
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
            method.Block.AssignRef(REF_CURRENTGROUPKEY, REF_GROUPKEY)
                .AssignRef(
                    REF_CURRENTROW,
                    Cast(classNames.RowTop, ExprDotMethod(REF_AGGREGATORSPERGROUP, "Get", REF_GROUPKEY)))
                .IfCondition(EqualsNull(REF_CURRENTROW))
                .AssignRef(REF_CURRENTROW, NewInstance(classNames.RowTop));
        }

        public void ClearResultsCodegen(
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            method.Block.ExprDotMethod(REF_AGGREGATORSPERGROUP, "Clear");
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
                ExprDotName(REF_AGGREGATORSPERGROUP, "Count"),
                REF_AGGREGATORSPERGROUP);
        }

        public void GetGroupKeysCodegen(
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            if (aggGroupByDesc.IsRefcounted) {
                method.Block.InstanceMethod(HandleRemovedKeysCodegen(method, classScope));
            }

            method.Block.MethodReturn(ExprDotName(REF_AGGREGATORSPERGROUP, "Keys"));
        }

        public void GetGroupKeyCodegen(
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            method.Block.MethodReturn(REF_CURRENTGROUPKEY);
        }

        public void AcceptGroupDetailCodegen(
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            method.Block.ExprDotMethod(REF_AGGVISITOR, "VisitGrouped", ExprDotName(REF_AGGREGATORSPERGROUP, "Count"))
                .ForEach(
                    typeof(KeyValuePair<object, object>),
                    "entry",
                    REF_AGGREGATORSPERGROUP)
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

        private CodegenMethod HandleRemovedKeysCodegen(
            CodegenMethod scope,
            CodegenClassScope classScope)
        {
            var method = scope.MakeChild(typeof(void), typeof(AggregationServiceGroupByForge), classScope);
            method.Block.IfCondition(Not(ExprDotMethod(REF_REMOVEDKEYS, "IsEmpty")))
                .ForEach(typeof(object), "removedKey", REF_REMOVEDKEYS)
                .ExprDotMethod(REF_AGGREGATORSPERGROUP, "Remove", Ref("removedKey"))
                .BlockEnd()
                .ExprDotMethod(REF_REMOVEDKEYS, "Clear");
            return method;
        }
    }
} // end of namespaceiceGroupBy