///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.agg.rollup;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.output.polled;
using com.espertech.esper.common.@internal.epl.resultset.codegen;
using com.espertech.esper.common.@internal.epl.resultset.core;
using com.espertech.esper.common.@internal.epl.resultset.rowforall;
using com.espertech.esper.common.@internal.fabric;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.epl.expression.codegen.ExprForgeCodegenNames;
using static com.espertech.esper.common.@internal.epl.resultset.codegen.ResultSetProcessorCodegenNames;
using static com.espertech.esper.common.@internal.epl.resultset.grouped.ResultSetProcessorGroupedUtil;

namespace com.espertech.esper.common.@internal.epl.resultset.rowpergrouprollup
{
    /// <summary>
    /// Result set processor prototype for the fully-grouped case:
    /// there is a group-by and all non-aggregation event properties in the select clause are listed in the group by,
    /// and there are aggregation functions.
    /// </summary>
    public class ResultSetProcessorRowPerGroupRollupForge : ResultSetProcessorFactoryForgeBase
    {
        private readonly GroupByRollupPerLevelForge perLevelForges;
        private readonly ExprNode[] groupKeyNodeExpressions;
        private readonly bool isSorting;
        private readonly bool isSelectRStream;
        private readonly bool isUnidirectional;
        private readonly OutputLimitSpec outputLimitSpec;
        private readonly AggregationGroupByRollupDescForge groupByRollupDesc;
        private readonly bool isJoin;
        private readonly bool isHistoricalOnly;
        private readonly ResultSetProcessorOutputConditionType? outputConditionType;
        private readonly OutputConditionPolledFactoryForge optionalOutputFirstConditionFactory;
        private readonly EventType[] eventTypes;
        private readonly Type[] groupKeyTypes;
        private readonly bool unbounded;
        private readonly MultiKeyClassRef multiKeyClassRef;
        private StateMgmtSetting outputFirstSettings;
        private StateMgmtSetting outputAllSettings;
        private StateMgmtSetting outputLastSettings;
        private StateMgmtSetting outputSnapshotSettings;
        private CodegenMethod generateGroupKeySingle;

        public ResultSetProcessorRowPerGroupRollupForge(
            EventType resultEventType,
            EventType[] typesPerStream,
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
            MultiKeyClassRef multiKeyClassRef) : base(resultEventType, typesPerStream)
        {
            this.groupKeyNodeExpressions = groupKeyNodeExpressions;
            this.perLevelForges = perLevelForges;
            this.isSorting = isSorting;
            this.isSelectRStream = isSelectRStream;
            this.isUnidirectional = isUnidirectional;
            this.outputLimitSpec = outputLimitSpec;
            var noDataWindowSingleSnapshot = iterateUnbounded ||
                                             (outputLimitSpec != null &&
                                              outputLimitSpec.DisplayLimit == OutputLimitLimitType.SNAPSHOT &&
                                              noDataWindowSingleStream);
            unbounded = noDataWindowSingleSnapshot && !isHistoricalOnly;
            this.groupByRollupDesc = groupByRollupDesc;
            this.isJoin = isJoin;
            this.isHistoricalOnly = isHistoricalOnly;
            this.outputConditionType = outputConditionType;
            this.optionalOutputFirstConditionFactory = optionalOutputFirstConditionFactory;
            this.eventTypes = eventTypes;
            groupKeyTypes = ExprNodeUtilityQuery.GetExprResultTypes(groupKeyNodeExpressions);
            this.multiKeyClassRef = multiKeyClassRef;
        }

        public bool IsSorting => isSorting;

        public bool IsSelectRStream => isSelectRStream;

        public bool IsUnidirectional => isUnidirectional;

        public bool IsJoin => isJoin;

        public bool IsHistoricalOnly => isHistoricalOnly;

        public override void InstanceCodegen(
            CodegenInstanceAux instance,
            CodegenClassScope classScope,
            CodegenCtor factoryCtor,
            IList<CodegenTypedParam> factoryMembers)
        {
            instance.Methods.AddMethod(
                typeof(AggregationService),
                "GetAggregationService",
                EmptyList<CodegenNamedParam>.Instance,
                GetType(),
                classScope,
                methodNode => methodNode.Block.MethodReturn(MEMBER_AGGREGATIONSVC));
            instance.Properties.AddProperty(
                typeof(ExprEvaluatorContext),
                "ExprEvaluatorContext",
                GetType(),
                classScope,
                property => property.GetterBlock.BlockReturn(MEMBER_EXPREVALCONTEXT));
            instance.Properties.AddProperty(
                typeof(bool),
                "IsSelectRStream",
                typeof(ResultSetProcessorRowForAll),
                classScope,
                property => property.GetterBlock.BlockReturn(Constant(isSelectRStream)));
            var rollupDesc = classScope.AddDefaultFieldUnshared(
                true,
                typeof(AggregationGroupByRollupDesc),
                groupByRollupDesc.Codegen(classScope.NamespaceScope.InitMethod, classScope));
            instance.Methods.AddMethod(
                typeof(AggregationGroupByRollupDesc),
                "GetGroupByRollupDesc",
                EmptyList<CodegenNamedParam>.Instance,
                typeof(ResultSetProcessorRowPerGroupRollup),
                classScope,
                methodNode => methodNode.Block.MethodReturn(rollupDesc));
            generateGroupKeySingle = GenerateGroupKeySingleCodegen(
                GroupKeyNodeExpressions,
                multiKeyClassRef,
                classScope,
                instance);
            ResultSetProcessorRowPerGroupRollupImpl.RemovedAggregationGroupKeyCodegen(classScope, instance);
            ResultSetProcessorRowPerGroupRollupImpl.GenerateOutputBatchedMapUnsortedCodegen(this, instance, classScope);
            ResultSetProcessorRowPerGroupRollupImpl.GenerateOutputBatchedCodegen(this, instance, classScope);
            // generate having clauses
            var havingForges = perLevelForges.OptionalHavingForges;
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
                                REF_ISNEWDATA,
                                REF_EXPREVALCONTEXT)));
                    
                    var impl = NewInstance<ProxyHavingClauseEvaluator>(evaluateHaving);
                    
                    // CodegenExpressionNewAnonymousClass impl = NewAnonymousClass(
                    //     factoryCtor.Block,
                    //     typeof(HavingClauseEvaluator));
                    // var evaluateHaving = CodegenMethod.MakeParentNode(typeof(bool), GetType(), classScope)
                    //     .AddParam(PARAMS);
                    // impl.AddMethod("evaluateHaving", evaluateHaving);
                    
                    // evaluateHaving.Block.MethodReturn(
                    //     CodegenLegoMethodExpression.CodegenBooleanExpressionReturnTrueFalse(
                    //         havingForges[i],
                    //         classScope,
                    //         factoryCtor,
                    //         REF_EPS,
                    //         REF_ISNEWDATA,
                    //         REF_EXPREVALCONTEXT));
                    
                    factoryCtor.Block.AssignArrayElement(NAME_HAVINGEVALUATOR_ARRAYNONMEMBER, Constant(i), impl);
                }
            }
        }

        public override void ProcessViewResultCodegen(
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

        public override void ProcessJoinResultCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            ResultSetProcessorRowPerGroupRollupImpl.ProcessJoinResultCodegen(this, classScope, method, instance);
        }

        public override void GetEnumeratorViewCodegen(
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

        public override void GetEnumeratorJoinCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            ResultSetProcessorRowPerGroupRollupImpl.GetEnumeratorJoinCodegen(this, classScope, method, instance);
        }

        public override void ProcessOutputLimitedViewCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            ResultSetProcessorRowPerGroupRollupImpl.ProcessOutputLimitedViewCodegen(this, classScope, method, instance);
        }

        public override void ProcessOutputLimitedJoinCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            ResultSetProcessorRowPerGroupRollupImpl.ProcessOutputLimitedJoinCodegen(this, classScope, method, instance);
        }

        public override void ApplyViewResultCodegen(
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

        public override void ApplyJoinResultCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            ResultSetProcessorRowPerGroupRollupImpl.ApplyJoinResultCodegen(this, classScope, method, instance);
        }

        public override void ContinueOutputLimitedLastAllNonBufferedViewCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            ResultSetProcessorRowPerGroupRollupImpl.ContinueOutputLimitedLastAllNonBufferedViewCodegen(this, method);
        }

        public override void ContinueOutputLimitedLastAllNonBufferedJoinCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            ResultSetProcessorRowPerGroupRollupImpl.ContinueOutputLimitedLastAllNonBufferedJoinCodegen(this, method);
        }

        public override void ProcessOutputLimitedLastAllNonBufferedViewCodegen(
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

        public override void ProcessOutputLimitedLastAllNonBufferedJoinCodegen(
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

        public override void AcceptHelperVisitorCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            ResultSetProcessorRowPerGroupRollupImpl.AcceptHelperVisitorCodegen(method, instance);
        }

        public override void StopMethodCodegen(
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

        public override void ClearMethodCodegen(
            CodegenClassScope classScope,
            CodegenMethod method)
        {
            ResultSetProcessorRowPerGroupRollupImpl.ClearMethodCodegen(method);
        }

        public void PlanStateSettings(
            FabricCharge fabricCharge,
            StatementRawInfo statementRawInfo,
            ResultSetProcessorFlags flags,
            StatementCompileTimeServices services)
        {
            if (IsOutputLast) {
                outputLastSettings =
                    services.StateMgmtSettingsProvider.ResultSet.RollupOutputLast(fabricCharge, statementRawInfo, this);
            }
            else if (IsOutputAll) {
                outputAllSettings =
                    services.StateMgmtSettingsProvider.ResultSet.RollupOutputAll(fabricCharge, statementRawInfo, this);
            }
            else if (IsOutputSnapshot) {
                outputSnapshotSettings =
                    services.StateMgmtSettingsProvider.ResultSet.RollupOutputSnapshot(
                        fabricCharge,
                        statementRawInfo,
                        this);
            }
            else if (IsOutputFirst) {
                outputFirstSettings =
                    services.StateMgmtSettingsProvider.ResultSet.RollupOutputFirst(
                        fabricCharge,
                        statementRawInfo,
                        this);
            }
        }

        public bool IsOutputFirst => IsOutputLimit(OutputLimitLimitType.FIRST);

        public bool IsOutputLast => IsOutputLimit(OutputLimitLimitType.LAST);

        public bool IsOutputAll => IsOutputLimit(OutputLimitLimitType.ALL);

        public bool IsOutputSnapshot => IsOutputLimit(OutputLimitLimitType.SNAPSHOT);

        private bool IsOutputLimit(OutputLimitLimitType type)
        {
            return outputLimitSpec != null && outputLimitSpec.DisplayLimit == type;
        }

        public bool IsOutputDefault => IsOutputLimit(OutputLimitLimitType.DEFAULT);

        public ExprNode[] GroupKeyNodeExpressions => groupKeyNodeExpressions;

        public AggregationGroupByRollupDescForge GroupByRollupDesc => groupByRollupDesc;

        public GroupByRollupPerLevelForge PerLevelForges => perLevelForges;

        public ResultSetProcessorOutputConditionType? OutputConditionType => outputConditionType;

        public int NumStreams => eventTypes.Length;

        public EventType[] EventTypes => eventTypes;

        public override Type InterfaceClass => typeof(ResultSetProcessorRowPerGroupRollup);

        public OutputConditionPolledFactoryForge OptionalOutputFirstConditionFactory => optionalOutputFirstConditionFactory;

        public MultiKeyClassRef MultiKeyClassRef => multiKeyClassRef;

        public Type[] GroupKeyTypes => groupKeyTypes;

        public override string InstrumentedQName => "ResultSetProcessGroupedRowPerGroup";

        public CodegenMethod GenerateGroupKeySingle => generateGroupKeySingle;

        public StateMgmtSetting OutputFirstSettings => outputFirstSettings;

        public StateMgmtSetting OutputAllSettings => outputAllSettings;

        public StateMgmtSetting OutputLastSettings => outputLastSettings;

        public StateMgmtSetting OutputSnapshotSettings => outputSnapshotSettings;
    }
} // end of namespace