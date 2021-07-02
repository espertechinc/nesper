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
using com.espertech.esper.common.@internal.compile.multikey;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.output.polled;
using com.espertech.esper.common.@internal.epl.resultset.core;
using com.espertech.esper.common.@internal.epl.resultset.grouped;
using com.espertech.esper.common.@internal.epl.resultset.rowforall;
using com.espertech.esper.common.@internal.epl.resultset.select.core;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.function;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.epl.resultset.codegen.ResultSetProcessorCodegenNames;

namespace com.espertech.esper.common.@internal.epl.resultset.agggrouped
{   
    /// <summary>
    ///     Result-set processor prototype for the aggregate-grouped case:
    ///     there is a group-by and one or more non-aggregation event properties in the select clause are not listed in the
    ///     group by, and there are aggregation functions.
    /// </summary>
    public class ResultSetProcessorAggregateGroupedForge : ResultSetProcessorFactoryForge
    {
        public ResultSetProcessorAggregateGroupedForge(
            EventType resultEventType,
            ExprNode[] groupKeyNodeExpressions,
            ExprForge optionalHavingNode,
            bool isSelectRStream,
            bool isUnidirectional,
            OutputLimitSpec outputLimitSpec,
            bool isSorting,
            bool isHistoricalOnly,
            ResultSetProcessorOutputConditionType? outputConditionType,
            OutputConditionPolledFactoryForge optionalOutputFirstConditionFactory,
            EventType[] eventTypes,
            MultiKeyClassRef multiKeyClassRef,
            Supplier<StateMgmtSetting> outputFirstHelperSettings,
            Supplier<StateMgmtSetting> outputAllHelperSettings,
            Supplier<StateMgmtSetting> outputAllOptSettings,
            Supplier<StateMgmtSetting> outputLastOptSettings)
        {
            ResultEventType = resultEventType;
            GroupKeyNodeExpressions = groupKeyNodeExpressions;
            OptionalHavingNode = optionalHavingNode;
            IsSorting = isSorting;
            IsSelectRStream = isSelectRStream;
            IsUnidirectional = isUnidirectional;
            OutputLimitSpec = outputLimitSpec;
            IsHistoricalOnly = isHistoricalOnly;
            OutputConditionType = outputConditionType;
            OptionalOutputFirstConditionFactory = optionalOutputFirstConditionFactory;
            EventTypes = eventTypes;
            GroupKeyTypes = ExprNodeUtilityQuery.GetExprResultTypes(groupKeyNodeExpressions);
            MultiKeyClassRef = multiKeyClassRef;
            this.OutputFirstHelperSettings = outputFirstHelperSettings;
            this.OutputAllHelperSettings = outputAllHelperSettings;
            this.OutputAllOptSettings = outputAllOptSettings;
            this.OutputLastOptSettings = outputLastOptSettings;
        }

        public EventType ResultEventType { get; }

        public ExprForge OptionalHavingNode { get; }

        public OutputLimitSpec OutputLimitSpec { get; }

        public OutputConditionPolledFactoryForge OptionalOutputFirstConditionFactory { get; }

        public ResultSetProcessorOutputConditionType? OutputConditionType { get; }

        public int NumStreams => EventTypes.Length;

        public bool IsSorting { get; }

        public bool IsSelectRStream { get; }

        public bool IsUnidirectional { get; }

        public ExprNode[] GroupKeyNodeExpressions { get; }

        public bool IsHistoricalOnly { get; }

        public bool IsOutputLast =>
            OutputLimitSpec != null && OutputLimitSpec.DisplayLimit == OutputLimitLimitType.LAST;

        public bool IsOutputAll => OutputLimitSpec != null && OutputLimitSpec.DisplayLimit == OutputLimitLimitType.ALL;

        public EventType[] EventTypes { get; }

        public Type[] GroupKeyTypes { get; }

        public Type InterfaceClass => typeof(ResultSetProcessorAggregateGrouped);

        public string InstrumentedQName => "ResultSetProcessGroupedRowPerEvent";
        
        public MultiKeyClassRef MultiKeyClassRef { get; }

        public CodegenMethod GenerateGroupKeySingle { get; private set; }
        public CodegenMethod GenerateGroupKeyArrayView { get; private set; }
        public CodegenMethod GenerateGroupKeyArrayJoin { get; private set; }

        public Supplier<StateMgmtSetting> OutputFirstHelperSettings { get; }
        public Supplier<StateMgmtSetting> OutputAllHelperSettings { get; }
        public Supplier<StateMgmtSetting> OutputAllOptSettings { get; }
        public Supplier<StateMgmtSetting> OutputLastOptSettings { get; }

        public void InstanceCodegen(
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
                propertyNode => propertyNode.GetterBlock.BlockReturn(MEMBER_SELECTEXPRPROCESSOR));
            instance.Properties.AddProperty(
                typeof(AggregationService),
                "AggregationService",
                GetType(),
                classScope,
                propertyNode => propertyNode.GetterBlock.BlockReturn(MEMBER_AGGREGATIONSVC));
            instance.Properties.AddProperty(
                typeof(ExprEvaluatorContext),
                "ExprEvaluatorContext",
                GetType(),
                classScope,
                propertyNode => propertyNode.GetterBlock.ReturnMethodOrBlock(MEMBER_EXPREVALCONTEXT));
            instance.Properties.AddProperty(
                typeof(bool),
                "HasHavingClause",
                GetType(),
                classScope,
                propertyNode => propertyNode.GetterBlock.BlockReturn(Constant(OptionalHavingNode != null)));
            instance.Properties.AddProperty(
                typeof(bool),
                "IsSelectRStream",
                typeof(ResultSetProcessorRowForAll),
                classScope,
                propertyNode => propertyNode.GetterBlock.BlockReturn(Constant(IsSelectRStream)));
            ResultSetProcessorUtil.EvaluateHavingClauseCodegen(OptionalHavingNode, classScope, instance);
            ResultSetProcessorAggregateGroupedImpl.RemovedAggregationGroupKeyCodegen(classScope, instance);

            GenerateGroupKeySingle = ResultSetProcessorGroupedUtil.GenerateGroupKeySingleCodegen(
                GroupKeyNodeExpressions, MultiKeyClassRef, classScope, instance);
            GenerateGroupKeyArrayView = ResultSetProcessorGroupedUtil.GenerateGroupKeyArrayViewCodegen(
                GenerateGroupKeySingle, classScope, instance);
            GenerateGroupKeyArrayJoin = ResultSetProcessorGroupedUtil.GenerateGroupKeyArrayJoinCodegen(
                GenerateGroupKeySingle, classScope, instance);

            ResultSetProcessorAggregateGroupedImpl.GenerateOutputBatchedSingleCodegen(this, classScope, instance);
            ResultSetProcessorAggregateGroupedImpl.GenerateOutputBatchedViewUnkeyedCodegen(this, classScope, instance);
            ResultSetProcessorAggregateGroupedImpl.GenerateOutputBatchedJoinUnkeyedCodegen(this, classScope, instance);
            ResultSetProcessorAggregateGroupedImpl.GenerateOutputBatchedJoinPerKeyCodegen(this, classScope, instance);
            ResultSetProcessorAggregateGroupedImpl.GenerateOutputBatchedViewPerKeyCodegen(this, classScope, instance);
        }

        public void ProcessViewResultCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            ResultSetProcessorAggregateGroupedImpl.ProcessViewResultCodegen(this, classScope, method, instance);
        }

        public void ProcessJoinResultCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            ResultSetProcessorAggregateGroupedImpl.ProcessJoinResultCodegen(this, classScope, method, instance);
        }

        public void GetEnumeratorViewCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            ResultSetProcessorAggregateGroupedImpl.GetEnumeratorViewCodegen(this, classScope, method, instance);
        }

        public void GetEnumeratorJoinCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            ResultSetProcessorAggregateGroupedImpl.GetEnumeratorJoinCodegen(this, classScope, method, instance);
        }

        public void ProcessOutputLimitedViewCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            ResultSetProcessorAggregateGroupedImpl.ProcessOutputLimitedViewCodegen(this, classScope, method, instance);
        }

        public void ProcessOutputLimitedJoinCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            ResultSetProcessorAggregateGroupedImpl.ProcessOutputLimitedJoinCodegen(this, classScope, method, instance);
        }

        public void ApplyViewResultCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            ResultSetProcessorAggregateGroupedImpl.ApplyViewResultCodegen(this, classScope, method, instance);
        }

        public void ApplyJoinResultCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            ResultSetProcessorAggregateGroupedImpl.ApplyJoinResultCodegen(this, classScope, method, instance);
        }

        public void ContinueOutputLimitedLastAllNonBufferedViewCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            ResultSetProcessorAggregateGroupedImpl.ContinueOutputLimitedLastAllNonBufferedViewCodegen(this, method);
        }

        public void ContinueOutputLimitedLastAllNonBufferedJoinCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            ResultSetProcessorAggregateGroupedImpl.ContinueOutputLimitedLastAllNonBufferedJoinCodegen(this, method);
        }

        public void ProcessOutputLimitedLastAllNonBufferedViewCodegen(
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

        public void ProcessOutputLimitedLastAllNonBufferedJoinCodegen(
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

        public void AcceptHelperVisitorCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            ResultSetProcessorAggregateGroupedImpl.AcceptHelperVisitorCodegen(method, instance);
        }

        public void StopMethodCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            ResultSetProcessorAggregateGroupedImpl.StopMethodCodegen(method, instance);
        }

        public void ClearMethodCodegen(
            CodegenClassScope classScope,
            CodegenMethod method)
        {
            ResultSetProcessorAggregateGroupedImpl.ClearMethodCodegen(method);
        }
    }
} // end of namespace