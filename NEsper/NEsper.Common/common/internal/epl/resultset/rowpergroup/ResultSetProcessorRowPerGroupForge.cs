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
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.core;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.output.polled;
using com.espertech.esper.common.@internal.epl.resultset.core;
using com.espertech.esper.common.@internal.epl.resultset.grouped;
using com.espertech.esper.common.@internal.epl.resultset.rowforall;
using com.espertech.esper.common.@internal.epl.resultset.@select.core;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.epl.resultset.codegen.ResultSetProcessorCodegenNames;

namespace com.espertech.esper.common.@internal.epl.resultset.rowpergroup
{
    /// <summary>
    ///     Result set processor prototype for the fully-grouped case:
    ///     there is a group-by and all non-aggregation event properties in the select clause are listed in the group by,
    ///     and there are aggregation functions.
    /// </summary>
    public class ResultSetProcessorRowPerGroupForge : ResultSetProcessorFactoryForge
    {
        private const string NAME_GROUPREPS = "groupReps";
        private readonly Type[] groupKeyTypes;

        private readonly SelectExprProcessorForge selectExprProcessorForge;
        private readonly EventType[] typesPerStream;
        private readonly bool unboundedProcessor;

        public ResultSetProcessorRowPerGroupForge(
            EventType resultEventType,
            EventType[] typesPerStream,
            SelectExprProcessorForge selectExprProcessorForge,
            ExprNode[] groupKeyNodeExpressions,
            ExprForge optionalHavingNode,
            bool isSelectRStream,
            bool isUnidirectional,
            OutputLimitSpec outputLimitSpec,
            bool isSorting,
            bool noDataWindowSingleStream,
            bool isHistoricalOnly,
            bool iterateUnbounded,
            ResultSetProcessorOutputConditionType? outputConditionType,
            EventType[] eventTypes,
            OutputConditionPolledFactoryForge optionalOutputFirstConditionFactory)
        {
            ResultEventType = resultEventType;
            this.typesPerStream = typesPerStream;
            GroupKeyNodeExpressions = groupKeyNodeExpressions;
            this.selectExprProcessorForge = selectExprProcessorForge;
            OptionalHavingNode = optionalHavingNode;
            IsSorting = isSorting;
            IsSelectRStream = isSelectRStream;
            IsUnidirectional = isUnidirectional;
            OutputLimitSpec = outputLimitSpec;
            var noDataWindowSingleSnapshot = iterateUnbounded ||
                                             outputLimitSpec != null &&
                                             outputLimitSpec.DisplayLimit == OutputLimitLimitType.SNAPSHOT &&
                                             noDataWindowSingleStream;
            unboundedProcessor = noDataWindowSingleSnapshot && !isHistoricalOnly;
            IsHistoricalOnly = isHistoricalOnly;
            OutputConditionType = outputConditionType;
            EventTypes = eventTypes;
            OptionalOutputFirstConditionFactory = optionalOutputFirstConditionFactory;
            groupKeyTypes = ExprNodeUtilityQuery.GetExprResultTypes(groupKeyNodeExpressions);
        }

        public EventType ResultEventType { get; }

        public ExprForge OptionalHavingNode { get; }

        public bool IsSorting { get; }

        public bool IsSelectRStream { get; }

        public bool IsUnidirectional { get; }

        public OutputLimitSpec OutputLimitSpec { get; }

        public ExprNode[] GroupKeyNodeExpressions { get; }

        public bool IsHistoricalOnly { get; }

        public bool IsOutputLast =>
            OutputLimitSpec != null && OutputLimitSpec.DisplayLimit == OutputLimitLimitType.LAST;

        public bool IsOutputAll => OutputLimitSpec != null && OutputLimitSpec.DisplayLimit == OutputLimitLimitType.ALL;

        public OutputConditionPolledFactoryForge OptionalOutputFirstConditionFactory { get; }

        public ResultSetProcessorOutputConditionType? OutputConditionType { get; }

        public int NumStreams => EventTypes.Length;

        public EventType[] EventTypes { get; }

        public Type InterfaceClass => typeof(ResultSetProcessorRowPerGroup);

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
                propertyNode => propertyNode.GetterBlock.BlockReturn(REF_SELECTEXPRPROCESSOR));
            instance.Properties.AddProperty(
                typeof(AggregationService),
                "AggregationService",
                GetType(),
                classScope,
                propertyNode => propertyNode.GetterBlock.BlockReturn(REF_AGGREGATIONSVC));
            instance.Methods.AddMethod(
                typeof(ExprEvaluatorContext),
                "GetAgentInstanceContext",
                new EmptyList<CodegenNamedParam>(),
                GetType(),
                classScope,
                node => node.Block.ReturnMethodOrBlock(REF_AGENTINSTANCECONTEXT));
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
            ResultSetProcessorGroupedUtil.GenerateGroupKeySingleCodegen(GroupKeyNodeExpressions, classScope, instance);
            ResultSetProcessorRowPerGroupImpl.GenerateOutputBatchedNoSortWMapCodegen(this, classScope, instance);
            ResultSetProcessorRowPerGroupImpl.GenerateOutputBatchedArrFromIteratorCodegen(this, classScope, instance);
            ResultSetProcessorRowPerGroupImpl.RemovedAggregationGroupKeyCodegen(classScope, instance);

            if (unboundedProcessor) {
                var factory = classScope.AddOrGetFieldSharable(ResultSetProcessorHelperFactoryField.INSTANCE);
                instance.AddMember(NAME_GROUPREPS, typeof(ResultSetProcessorRowPerGroupUnboundHelper));
                var eventType = classScope.AddFieldUnshared(
                    true,
                    typeof(EventType),
                    EventTypeUtility.ResolveTypeCodegen(typesPerStream[0], EPStatementInitServicesConstants.REF));
                instance.ServiceCtor.Block.AssignRef(
                        NAME_GROUPREPS,
                        ExprDotMethod(
                            factory,
                            "MakeRSRowPerGroupUnboundGroupRep",
                            Constant(groupKeyTypes),
                            eventType,
                            REF_AGENTINSTANCECONTEXT))
                    .ExprDotMethod(REF_AGGREGATIONSVC, "SetRemovedCallback", Ref(NAME_GROUPREPS));
            }
            else {
                instance.ServiceCtor.Block.ExprDotMethod(REF_AGGREGATIONSVC, "SetRemovedCallback", Ref("this"));
            }
        }

        public void ProcessViewResultCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            if (unboundedProcessor) {
                ResultSetProcessorRowPerGroupUnbound.ProcessViewResultUnboundCodegen(
                    this,
                    classScope,
                    method,
                    instance);
            }
            else {
                ResultSetProcessorRowPerGroupImpl.ProcessViewResultCodegen(this, classScope, method, instance);
            }
        }

        public void ProcessJoinResultCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            ResultSetProcessorRowPerGroupImpl.ProcessJoinResultCodegen(this, classScope, method, instance);
        }

        public void GetIteratorViewCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            if (unboundedProcessor) {
                ResultSetProcessorRowPerGroupUnbound.GetIteratorViewUnboundedCodegen(
                    this,
                    classScope,
                    method,
                    instance);
            }
            else {
                ResultSetProcessorRowPerGroupImpl.GetIteratorViewCodegen(this, classScope, method, instance);
            }
        }

        public void GetIteratorJoinCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            ResultSetProcessorRowPerGroupImpl.GetIteratorJoinCodegen(this, classScope, method, instance);
        }

        public void ProcessOutputLimitedViewCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            ResultSetProcessorRowPerGroupImpl.ProcessOutputLimitedViewCodegen(this, classScope, method, instance);
        }

        public void ProcessOutputLimitedJoinCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            ResultSetProcessorRowPerGroupImpl.ProcessOutputLimitedJoinCodegen(this, classScope, method, instance);
        }

        public void ApplyViewResultCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            if (unboundedProcessor) {
                ResultSetProcessorRowPerGroupUnbound.ApplyViewResultCodegen(this, classScope, method, instance);
            }
            else {
                ResultSetProcessorRowPerGroupImpl.ApplyViewResultCodegen(this, classScope, method, instance);
            }
        }

        public void ApplyJoinResultCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            ResultSetProcessorRowPerGroupImpl.ApplyJoinResultCodegen(this, classScope, method, instance);
        }

        public void ContinueOutputLimitedLastAllNonBufferedViewCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            ResultSetProcessorRowPerGroupImpl.ContinueOutputLimitedLastAllNonBufferedViewCodegen(this, method);
        }

        public void ContinueOutputLimitedLastAllNonBufferedJoinCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            ResultSetProcessorRowPerGroupImpl.ContinueOutputLimitedLastAllNonBufferedJoinCodegen(this, method);
        }

        public void ProcessOutputLimitedLastAllNonBufferedViewCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            ResultSetProcessorRowPerGroupImpl.ProcessOutputLimitedLastAllNonBufferedViewCodegen(
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
            ResultSetProcessorRowPerGroupImpl.ProcessOutputLimitedLastAllNonBufferedJoinCodegen(
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
            ResultSetProcessorRowPerGroupImpl.AcceptHelperVisitorCodegen(method, instance);
        }

        public void StopMethodCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            if (unboundedProcessor) {
                ResultSetProcessorRowPerGroupUnbound.StopMethodCodegenUnbound(this, classScope, method, instance);
            }
            else {
                ResultSetProcessorRowPerGroupImpl.StopMethodCodegenBound(method, instance);
            }
        }

        public void ClearMethodCodegen(
            CodegenClassScope classScope,
            CodegenMethod method)
        {
            ResultSetProcessorRowPerGroupImpl.ClearMethodCodegen(method);
        }

        public string InstrumentedQName => "ResultSetProcessGroupedRowPerGroup";

        public Type[] GroupKeyTypes {
            get { return groupKeyTypes; }
        }
    }
} // end of namespace