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
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.core;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.compile.multikey;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.agg.rollup;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.output.polled;
using com.espertech.esper.common.@internal.epl.resultset.codegen;
using com.espertech.esper.common.@internal.epl.resultset.core;
using com.espertech.esper.common.@internal.epl.resultset.grouped;
using com.espertech.esper.common.@internal.epl.resultset.rowforall;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.function;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.epl.expression.codegen.ExprForgeCodegenNames;
using static com.espertech.esper.common.@internal.epl.resultset.codegen.ResultSetProcessorCodegenNames;

namespace com.espertech.esper.common.@internal.epl.resultset.rowpergrouprollup
{
    /// <summary>
    ///     Result set processor prototype for the fully-grouped case:
    ///     there is a group-by and all non-aggregation event properties in the select clause are listed in the group by,
    ///     and there are aggregation functions.
    /// </summary>
    public class ResultSetProcessorRowPerGroupRollupForge : ResultSetProcessorFactoryForge
    {
        private readonly bool unbounded;

        public ResultSetProcessorRowPerGroupRollupForge(
            EventType resultEventType,
            GroupByRollupPerLevelForge perLevelForges,
            ExprNode[] groupKeyNodeExpressions,
            bool isSelectRStream,
            bool isUnidirectional,
            OutputLimitSpec outputLimitSpec,
            bool isSorting,
            bool noDataWindowSingleStream,
            AggregationGroupByRollupDescForge groupByRollupDesc,
            bool isJoin,
            bool isHistoricalOnly,
            bool iterateUnbounded,
            ResultSetProcessorOutputConditionType? outputConditionType,
            OutputConditionPolledFactoryForge optionalOutputFirstConditionFactory,
            EventType[] eventTypes,
            MultiKeyClassRef multiKeyClassRef,
            Supplier<StateMgmtSetting> outputFirstSettings,
            Supplier<StateMgmtSetting> outputAllSettings,
            Supplier<StateMgmtSetting> outputLastSettings,
            Supplier<StateMgmtSetting> outputSnapshotSettings)
        {
            ResultEventType = resultEventType;
            GroupKeyNodeExpressions = groupKeyNodeExpressions;
            PerLevelForges = perLevelForges;
            IsSorting = isSorting;
            IsSelectRStream = isSelectRStream;
            IsUnidirectional = isUnidirectional;
            OutputLimitSpec = outputLimitSpec;
            var noDataWindowSingleSnapshot = iterateUnbounded ||
                                             outputLimitSpec != null &&
                                             outputLimitSpec.DisplayLimit == OutputLimitLimitType.SNAPSHOT &&
                                             noDataWindowSingleStream;
            unbounded = noDataWindowSingleSnapshot && !isHistoricalOnly;
            GroupByRollupDesc = groupByRollupDesc;
            IsJoin = isJoin;
            IsHistoricalOnly = isHistoricalOnly;
            OutputConditionType = outputConditionType;
            OptionalOutputFirstConditionFactory = optionalOutputFirstConditionFactory;
            EventTypes = eventTypes;
            GroupKeyTypes = ExprNodeUtilityQuery.GetExprResultTypes(groupKeyNodeExpressions);
            MultiKeyClassRef = multiKeyClassRef;
            OutputFirstSettings = outputFirstSettings;
            OutputAllSettings = outputAllSettings;
            OutputLastSettings = outputLastSettings;
            OutputSnapshotSettings = outputSnapshotSettings;
        }

        public MultiKeyClassRef MultiKeyClassRef { get; }

        public CodegenMethod GenerateGroupKeySingle { get; set; }

        public EventType ResultEventType { get; }

        public OutputLimitSpec OutputLimitSpec { get; }

        public AggregationGroupByRollupDescForge GroupByRollupDesc { get; }

        public GroupByRollupPerLevelForge PerLevelForges { get; }

        public ResultSetProcessorOutputConditionType? OutputConditionType { get; }

        public int NumStreams => EventTypes.Length;

        public Type InterfaceClass => typeof(ResultSetProcessorRowPerGroupRollup);

        public OutputConditionPolledFactoryForge OptionalOutputFirstConditionFactory { get; }

        public string InstrumentedQName => "ResultSetProcessGroupedRowPerGroup";

        public bool IsSorting { get; }

        public bool IsSelectRStream { get; }

        public bool IsUnidirectional { get; }

        public ExprNode[] GroupKeyNodeExpressions { get; }

        public bool IsJoin { get; }

        public bool IsHistoricalOnly { get; }

        public EventType[] EventTypes { get; }

        public Type[] GroupKeyTypes { get; }

        public Supplier<StateMgmtSetting> OutputFirstSettings { get; }

        public Supplier<StateMgmtSetting> OutputAllSettings { get; }

        public Supplier<StateMgmtSetting> OutputLastSettings { get; }

        public Supplier<StateMgmtSetting> OutputSnapshotSettings { get; }

        public void InstanceCodegen(
            CodegenInstanceAux instance,
            CodegenClassScope classScope,
            CodegenCtor factoryCtor,
            IList<CodegenTypedParam> factoryMembers)
        {
            instance.Properties.AddProperty(
                typeof(AggregationService),
                "AggregationService",
                GetType(),
                classScope,
                node => node.GetterBlock.BlockReturn(MEMBER_AGGREGATIONSVC));
            instance.Properties.AddProperty(
                typeof(ExprEvaluatorContext),
                "ExprEvaluatorContext",
                GetType(),
                classScope,
                node => node.GetterBlock.BlockReturn(MEMBER_EXPREVALCONTEXT));
            instance.Properties.AddProperty(
                typeof(bool),
                "IsSelectRStream",
                typeof(ResultSetProcessorRowForAll),
                classScope,
                node => node.GetterBlock.BlockReturn(Constant(IsSelectRStream)));

            var rollupDesc = classScope.AddDefaultFieldUnshared(
                true,
                typeof(AggregationGroupByRollupDesc),
                GroupByRollupDesc.Codegen(classScope.NamespaceScope.InitMethod, classScope));

            instance.Properties.AddProperty(
                typeof(AggregationGroupByRollupDesc),
                "GroupByRollupDesc",
                typeof(ResultSetProcessorRowPerGroupRollup),
                classScope,
                node => node.GetterBlock.BlockReturn(rollupDesc));

            GenerateGroupKeySingle = ResultSetProcessorGroupedUtil.GenerateGroupKeySingleCodegen(GroupKeyNodeExpressions, MultiKeyClassRef, classScope, instance);
            ResultSetProcessorRowPerGroupRollupImpl.RemovedAggregationGroupKeyCodegen(classScope, instance);
            ResultSetProcessorRowPerGroupRollupImpl.GenerateOutputBatchedMapUnsortedCodegen(this, instance, classScope);
            ResultSetProcessorRowPerGroupRollupImpl.GenerateOutputBatchedCodegen(this, instance, classScope);

            // generate having clauses
            var havingForges = PerLevelForges.OptionalHavingForges;
            if (havingForges != null) {
                factoryMembers.Add(
                    new CodegenTypedParam(typeof(HavingClauseEvaluator[]), NAME_HAVINGEVALUATOR_ARRAYNONMEMBER));
                factoryCtor.Block.AssignRef(
                    NAME_HAVINGEVALUATOR_ARRAYNONMEMBER,
                    NewArrayByLength(typeof(HavingClauseEvaluator), Constant(havingForges.Length)));
                for (var i = 0; i < havingForges.Length; i++) {
                    var evaluateHaving = new CodegenExpressionLambda(factoryCtor.Block)
                        .WithParams(PARAMS)
                        .WithBody(block => block.BlockReturn(
                            CodegenLegoMethodExpression.CodegenBooleanExpressionReturnTrueFalse(
                                havingForges[i],
                                classScope,
                                factoryCtor,
                                REF_EPS,
                                ExprForgeCodegenNames.REF_ISNEWDATA,
                                REF_EXPREVALCONTEXT)));

                    var impl = NewInstance<ProxyHavingClauseEvaluator>(evaluateHaving);

                    //var evaluateHaving = CodegenMethod.MakeMethod(typeof(bool), GetType(), classScope)
                    //    .AddParam(PARAMS);
                    //impl.AddMethod("EvaluateHaving", evaluateHaving);

                    factoryCtor.Block.AssignArrayElement(NAME_HAVINGEVALUATOR_ARRAYNONMEMBER, Constant(i), impl);
                }
            }
        }

        public void ProcessViewResultCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            if (unbounded) {
                ResultSetProcessorRowPerGroupRollupUnbound.ProcessViewResultUnboundCodegen(
                    this,
                    classScope,
                    method,
                    instance);
            }
            else {
                ResultSetProcessorRowPerGroupRollupImpl.ProcessViewResultCodegen(this, classScope, method, instance);
            }
        }

        public void ProcessJoinResultCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            ResultSetProcessorRowPerGroupRollupImpl.ProcessJoinResultCodegen(this, classScope, method, instance);
        }

        public void GetEnumeratorViewCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            if (unbounded) {
                ResultSetProcessorRowPerGroupRollupUnbound.GetEnumeratorViewUnboundCodegen(
                    this,
                    classScope,
                    method,
                    instance);
            }
            else {
                ResultSetProcessorRowPerGroupRollupImpl.GetEnumeratorViewCodegen(this, classScope, method, instance);
            }
        }

        public void GetEnumeratorJoinCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            ResultSetProcessorRowPerGroupRollupImpl.GetEnumeratorJoinCodegen(this, classScope, method, instance);
        }

        public void ProcessOutputLimitedViewCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            ResultSetProcessorRowPerGroupRollupImpl.ProcessOutputLimitedViewCodegen(this, classScope, method, instance);
        }

        public void ProcessOutputLimitedJoinCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            ResultSetProcessorRowPerGroupRollupImpl.ProcessOutputLimitedJoinCodegen(this, classScope, method, instance);
        }

        public void ApplyViewResultCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            if (unbounded) {
                ResultSetProcessorRowPerGroupRollupUnbound.ApplyViewResultUnboundCodegen(
                    this,
                    classScope,
                    method,
                    instance);
            }
            else {
                ResultSetProcessorRowPerGroupRollupImpl.ApplyViewResultCodegen(this, classScope, method, instance);
            }
        }

        public void ApplyJoinResultCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            ResultSetProcessorRowPerGroupRollupImpl.ApplyJoinResultCodegen(this, classScope, method, instance);
        }

        public void ContinueOutputLimitedLastAllNonBufferedViewCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            ResultSetProcessorRowPerGroupRollupImpl.ContinueOutputLimitedLastAllNonBufferedViewCodegen(this, method);
        }

        public void ContinueOutputLimitedLastAllNonBufferedJoinCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            ResultSetProcessorRowPerGroupRollupImpl.ContinueOutputLimitedLastAllNonBufferedJoinCodegen(this, method);
        }

        public void ProcessOutputLimitedLastAllNonBufferedViewCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            ResultSetProcessorRowPerGroupRollupImpl.ProcessOutputLimitedLastAllNonBufferedViewCodegen(
                this,
                classScope,
                method,
                instance);
        }

        public void ProcessOutputLimitedLastAllNonBufferedJoinCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            ResultSetProcessorRowPerGroupRollupImpl.ProcessOutputLimitedLastAllNonBufferedJoinCodegen(
                this,
                classScope,
                method,
                instance);
        }

        public void AcceptHelperVisitorCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            ResultSetProcessorRowPerGroupRollupImpl.AcceptHelperVisitorCodegen(method, instance);
        }

        public void StopMethodCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            if (unbounded) {
                ResultSetProcessorRowPerGroupRollupUnbound.StopMethodUnboundCodegen(this, classScope, method, instance);
            }
            else {
                ResultSetProcessorRowPerGroupRollupImpl.StopMethodCodegenBound(method, instance);
            }
        }

        public void ClearMethodCodegen(
            CodegenClassScope classScope,
            CodegenMethod method)
        {
            ResultSetProcessorRowPerGroupRollupImpl.ClearMethodCodegen(method);
        }
    }
} // end of namespace