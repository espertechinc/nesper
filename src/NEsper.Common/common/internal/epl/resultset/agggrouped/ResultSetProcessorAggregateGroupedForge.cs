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
using com.espertech.esper.common.@internal.compile.multikey;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.output.polled;
using com.espertech.esper.common.@internal.epl.resultset.core;
using com.espertech.esper.common.@internal.epl.resultset.grouped;
using com.espertech.esper.common.@internal.epl.resultset.rowforall;
using com.espertech.esper.common.@internal.epl.resultset.select.core;
using com.espertech.esper.common.@internal.fabric;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.epl.resultset.codegen.ResultSetProcessorCodegenNames;
using static com.espertech.esper.common.@internal.epl.resultset.core.ResultSetProcessorOutputConditionType;
using static com.espertech.esper.common.@internal.epl.resultset.grouped.ResultSetProcessorGroupedUtil;

namespace com.espertech.esper.common.@internal.epl.resultset.agggrouped
{
    /// <summary>
    /// Result-set processor prototype for the aggregate-grouped case:
    /// there is a group-by and one or more non-aggregation event properties in the select clause are not listed in the group by,
    /// and there are aggregation functions.
    /// </summary>
    public class ResultSetProcessorAggregateGroupedForge : ResultSetProcessorFactoryForgeBase
    {
        private readonly ExprNode[] groupKeyNodeExpressions;
        private readonly ExprForge optionalHavingNode;
        private readonly bool isSorting;
        private readonly bool isSelectRStream;
        private readonly bool isUnidirectional;
        private readonly OutputLimitSpec outputLimitSpec;
        private readonly bool isHistoricalOnly;
        private readonly ResultSetProcessorOutputConditionType? outputConditionType;
        private readonly OutputConditionPolledFactoryForge optionalOutputFirstConditionFactory;
        private readonly Type[] groupKeyTypes;
        private readonly MultiKeyClassRef multiKeyClassRef;
        private StateMgmtSetting outputFirstHelperSettings;
        private StateMgmtSetting outputAllHelperSettings;
        private StateMgmtSetting outputAllOptSettings;
        private StateMgmtSetting outputLastOptSettings;

        private CodegenMethod generateGroupKeySingle;
        private CodegenMethod generateGroupKeyArrayView;
        private CodegenMethod generateGroupKeyArrayJoin;

        public ResultSetProcessorAggregateGroupedForge(
            EventType resultEventType,
            EventType[] typesPerStream,
            ExprNode[] groupKeyNodeExpressions,
            ExprForge optionalHavingNode,
            bool isSelectRStream,
            bool isUnidirectional,
            OutputLimitSpec outputLimitSpec,
            bool isSorting,
            bool isHistoricalOnly,
            ResultSetProcessorOutputConditionType? outputConditionType,
            OutputConditionPolledFactoryForge optionalOutputFirstConditionFactory,
            MultiKeyClassRef multiKeyClassRef) : base(resultEventType, typesPerStream)
        {
            this.groupKeyNodeExpressions = groupKeyNodeExpressions;
            this.optionalHavingNode = optionalHavingNode;
            this.isSorting = isSorting;
            this.isSelectRStream = isSelectRStream;
            this.isUnidirectional = isUnidirectional;
            this.outputLimitSpec = outputLimitSpec;
            this.isHistoricalOnly = isHistoricalOnly;
            this.outputConditionType = outputConditionType;
            this.optionalOutputFirstConditionFactory = optionalOutputFirstConditionFactory;
            groupKeyTypes = ExprNodeUtilityQuery.GetExprResultTypes(groupKeyNodeExpressions);
            this.multiKeyClassRef = multiKeyClassRef;
            //this.outputLastOptSettings = outputLastOptSettings;
        }

        public ExprForge OptionalHavingNode => optionalHavingNode;

        public bool IsSorting => isSorting;

        public bool IsSelectRStream => isSelectRStream;

        public bool IsUnidirectional => isUnidirectional;

        public OutputLimitSpec OutputLimitSpec => outputLimitSpec;

        public ExprNode[] GroupKeyNodeExpressions => groupKeyNodeExpressions;

        public bool IsHistoricalOnly => isHistoricalOnly;

        public OutputConditionPolledFactoryForge OptionalOutputFirstConditionFactory =>
            optionalOutputFirstConditionFactory;

        public bool IsOutputLast =>
            outputLimitSpec != null && outputLimitSpec.DisplayLimit == OutputLimitLimitType.LAST;

        public bool IsOutputAll => outputLimitSpec != null && outputLimitSpec.DisplayLimit == OutputLimitLimitType.ALL;

        public bool IsOutputFirst =>
            outputLimitSpec != null && outputLimitSpec.DisplayLimit == OutputLimitLimitType.FIRST;

        public ResultSetProcessorOutputConditionType? OutputConditionType => outputConditionType;

        public int NumStreams => typesPerStream.Length;

        public Type[] GroupKeyTypes => groupKeyTypes;

        public override Type InterfaceClass => typeof(ResultSetProcessorAggregateGrouped);

        public override void InstanceCodegen(
            CodegenInstanceAux instance,
            CodegenClassScope classScope,
            CodegenCtor factoryCtor,
            IList<CodegenTypedParam> factoryMembers)
        {
            instance.Properties.AddProperty(
                typeof(SelectExprProcessor),
                "SelectExprProcessor",
                GetType(),
                classScope,
                property => property.GetterBlock.BlockReturn(MEMBER_SELECTEXPRPROCESSOR));
            instance.Properties.AddProperty(
                typeof(AggregationService),
                "AggregationService",
                GetType(),
                classScope,
                property => property.GetterBlock.BlockReturn(MEMBER_AGGREGATIONSVC));
            
#if DEFINED_IN_BASECLASS
            instance.Properties.AddProperty(
                typeof(ExprEvaluatorContext),
                "ExprEvaluatorContext",
                GetType(),
                classScope,
                property => {
                    property
                        .GetterBlock
                        .Debug("ResultSetProcessorAggregateGroupedForge.InstanceCodegen")
                        .BlockReturn(MEMBER_EXPREVALCONTEXT);
                });
#endif
            
            instance.Properties.AddProperty(
                typeof(bool),
                "HasHavingClause",
                GetType(),
                classScope,
                property => property.GetterBlock.BlockReturn(Constant(optionalHavingNode != null)));
            instance.Properties.AddProperty(
                typeof(bool),
                "IsSelectRStream",
                typeof(ResultSetProcessorRowForAll),
                classScope,
                property => property.GetterBlock.BlockReturn(Constant(isSelectRStream)));
            ResultSetProcessorUtil.EvaluateHavingClauseCodegen(optionalHavingNode, classScope, instance);
            ResultSetProcessorAggregateGroupedImpl.RemovedAggregationGroupKeyCodegen(classScope, instance);

            generateGroupKeySingle = GenerateGroupKeySingleCodegen(
                groupKeyNodeExpressions,
                multiKeyClassRef,
                classScope,
                instance);
            generateGroupKeyArrayView = GenerateGroupKeyArrayViewCodegen(generateGroupKeySingle, classScope, instance);
            generateGroupKeyArrayJoin =
                GenerateGroupKeyArrayJoinCodegen(
                    generateGroupKeySingle,
                    classScope,
                    instance);

            ResultSetProcessorAggregateGroupedImpl.GenerateOutputBatchedSingleCodegen(this, classScope, instance);
            ResultSetProcessorAggregateGroupedImpl.GenerateOutputBatchedViewUnkeyedCodegen(this, classScope, instance);
            ResultSetProcessorAggregateGroupedImpl.GenerateOutputBatchedJoinUnkeyedCodegen(this, classScope, instance);
            ResultSetProcessorAggregateGroupedImpl.GenerateOutputBatchedJoinPerKeyCodegen(this, classScope, instance);
            ResultSetProcessorAggregateGroupedImpl.GenerateOutputBatchedViewPerKeyCodegen(this, classScope, instance);
        }

        public override void ProcessViewResultCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            ResultSetProcessorAggregateGroupedImpl.ProcessViewResultCodegen(this, classScope, method, instance);
        }

        public override void ProcessJoinResultCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            ResultSetProcessorAggregateGroupedImpl.ProcessJoinResultCodegen(this, classScope, method, instance);
        }

        public override void GetEnumeratorViewCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            ResultSetProcessorAggregateGroupedImpl.GetEnumeratorViewCodegen(this, classScope, method, instance);
        }

        public override void GetEnumeratorJoinCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            ResultSetProcessorAggregateGroupedImpl.GetEnumeratorJoinCodegen(this, classScope, method, instance);
        }

        public override void ProcessOutputLimitedViewCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            ResultSetProcessorAggregateGroupedImpl.ProcessOutputLimitedViewCodegen(this, classScope, method, instance);
        }

        public override void ProcessOutputLimitedJoinCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            ResultSetProcessorAggregateGroupedImpl.ProcessOutputLimitedJoinCodegen(this, classScope, method, instance);
        }

        public override void ApplyViewResultCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            ResultSetProcessorAggregateGroupedImpl.ApplyViewResultCodegen(this, classScope, method, instance);
        }

        public override void ApplyJoinResultCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            ResultSetProcessorAggregateGroupedImpl.ApplyJoinResultCodegen(this, classScope, method, instance);
        }

        public override void ContinueOutputLimitedLastAllNonBufferedViewCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            ResultSetProcessorAggregateGroupedImpl.ContinueOutputLimitedLastAllNonBufferedViewCodegen(this, method);
        }

        public override void ContinueOutputLimitedLastAllNonBufferedJoinCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            ResultSetProcessorAggregateGroupedImpl.ContinueOutputLimitedLastAllNonBufferedJoinCodegen(this, method);
        }

        public override void ProcessOutputLimitedLastAllNonBufferedViewCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            ResultSetProcessorAggregateGroupedImpl.ProcessOutputLimitedLastAllNonBufferedViewCodegen(
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
            ResultSetProcessorAggregateGroupedImpl.ProcessOutputLimitedLastAllNonBufferedJoinCodegen(
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
            ResultSetProcessorAggregateGroupedImpl.AcceptHelperVisitorCodegen(method, instance);
        }

        public override void StopMethodCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            ResultSetProcessorAggregateGroupedImpl.StopMethodCodegen(method, instance);
        }

        public override void ClearMethodCodegen(
            CodegenClassScope classScope,
            CodegenMethod method)
        {
            ResultSetProcessorAggregateGroupedImpl.ClearMethodCodegen(method);
        }

        public override string InstrumentedQName => "ResultSetProcessGroupedRowPerEvent";

        public CodegenMethod GenerateGroupKeySingle => generateGroupKeySingle;

        public CodegenMethod GenerateGroupKeyArrayView => generateGroupKeyArrayView;

        public CodegenMethod GenerateGroupKeyArrayJoin => generateGroupKeyArrayJoin;

        public MultiKeyClassRef MultiKeyClassRef => multiKeyClassRef;

        public StateMgmtSetting OutputFirstHelperSettings => outputFirstHelperSettings;

        public StateMgmtSetting OutputAllHelperSettings => outputAllHelperSettings;

        public StateMgmtSetting OutputAllOptSettings => outputAllOptSettings;

        public StateMgmtSetting OutputLastOptSettings => outputLastOptSettings;

        public void PlanStateSettings(
            FabricCharge fabricCharge,
            StatementRawInfo statementRawInfo,
            ResultSetProcessorFlags flags,
            StatementCompileTimeServices services)
        {
            if (IsOutputFirst) {
                outputFirstHelperSettings = services.StateMgmtSettingsProvider.ResultSet
                    .AggGroupedOutputFirst(fabricCharge, statementRawInfo, this);
            }
            else if (IsOutputAll) {
                if (flags.OutputConditionType == POLICY_LASTALL_UNORDERED) {
                    outputAllOptSettings = services.StateMgmtSettingsProvider.ResultSet
                        .AggGroupedOutputAllOpt(fabricCharge, statementRawInfo, this);
                }
                else {
                    outputAllHelperSettings = services.StateMgmtSettingsProvider.ResultSet
                        .AggGroupedOutputAll(fabricCharge, statementRawInfo, this);
                }
            }
            else if (IsOutputLast) {
                if (flags.OutputConditionType == POLICY_LASTALL_UNORDERED) {
                    outputLastOptSettings = services.StateMgmtSettingsProvider.ResultSet
                        .AggGroupedOutputLast(fabricCharge, statementRawInfo, this);
                }
            }
        }
    }
} // end of namespace