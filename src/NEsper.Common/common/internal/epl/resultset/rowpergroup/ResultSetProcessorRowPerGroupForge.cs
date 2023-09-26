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
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.output.polled;
using com.espertech.esper.common.@internal.epl.resultset.core;
using com.espertech.esper.common.@internal.epl.resultset.grouped;
using com.espertech.esper.common.@internal.epl.resultset.rowforall;
using com.espertech.esper.common.@internal.epl.resultset.select.core;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.fabric;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.epl.resultset.codegen.ResultSetProcessorCodegenNames;
using static com.espertech.esper.common.@internal.epl.resultset.core.ResultSetProcessorOutputConditionType;
using static com.espertech.esper.common.@internal.epl.resultset.grouped.ResultSetProcessorGroupedUtil;

namespace com.espertech.esper.common.@internal.epl.resultset.rowpergroup
{
    /// <summary>
    /// Result set processor prototype for the fully-grouped case:
    /// there is a group-by and all non-aggregation event properties in the select clause are listed in the group by,
    /// and there are aggregation functions.
    /// </summary>
    public class ResultSetProcessorRowPerGroupForge : ResultSetProcessorFactoryForgeBase
    {
        private const string NAME_GROUPREPS = "groupReps";
        private readonly ExprNode[] groupKeyNodeExpressions;
        private readonly ExprForge optionalHavingNode;
        private readonly bool isSorting;
        private readonly bool isSelectRStream;
        private readonly bool isUnidirectional;
        private readonly OutputLimitSpec outputLimitSpec;
        private readonly bool unboundedProcessor;
        private readonly bool isHistoricalOnly;
        private readonly ResultSetProcessorOutputConditionType outputConditionType;
        private readonly OutputConditionPolledFactoryForge optionalOutputFirstConditionFactory;
        private readonly Type[] groupKeyTypes;
        private readonly MultiKeyClassRef multiKeyClassRef;
        private StateMgmtSetting unboundGroupRepSettings;
        private StateMgmtSetting outputFirstHelperSettings;
        private StateMgmtSetting outputAllHelperSettings;
        private StateMgmtSetting outputAllOptHelperSettings;
        private StateMgmtSetting outputLastOptHelperSettings;
        private CodegenMethod generateGroupKeySingle;
        private CodegenMethod generateGroupKeyArrayView;
        private CodegenMethod generateGroupKeyArrayJoin;

        public ResultSetProcessorRowPerGroupForge(
            EventType resultEventType,
            EventType[] typesPerStream,
            ExprNode[] groupKeyNodeExpressions,
            ExprForge optionalHavingNode,
            bool isSelectRStream,
            bool isUnidirectional,
            OutputLimitSpec outputLimitSpec,
            bool isSorting,
            bool isHistoricalOnly,
            ResultSetProcessorOutputConditionType outputConditionType,
            OutputConditionPolledFactoryForge optionalOutputFirstConditionFactory,
            MultiKeyClassRef multiKeyClassRef,
            bool unboundedProcessor) : base(resultEventType, typesPerStream)
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
            this.unboundedProcessor = unboundedProcessor;
        }

        public bool IsSorting => isSorting;

        public bool IsSelectRStream => isSelectRStream;

        public bool IsUnidirectional => isUnidirectional;

        public bool IsHistoricalOnly => isHistoricalOnly;

        public bool IsOutputLast()
        {
            return IsLimitSpec(OutputLimitLimitType.LAST);
        }

        public bool IsOutputAll()
        {
            return IsLimitSpec(OutputLimitLimitType.ALL);
        }

        public bool IsOutputFirst()
        {
            return IsLimitSpec(OutputLimitLimitType.FIRST);
        }

        public override void InstanceCodegen(
            CodegenInstanceAux instance,
            CodegenClassScope classScope,
            CodegenCtor factoryCtor,
            IList<CodegenTypedParam> factoryMembers)
        {
            instance.Methods.AddMethod(
                typeof(SelectExprProcessor),
                "getSelectExprProcessor",
                EmptyList<CodegenNamedParam>.Instance, 
                GetType(),
                classScope,
                methodNode => methodNode.Block.MethodReturn(MEMBER_SELECTEXPRPROCESSOR));
            instance.Methods.AddMethod(
                typeof(AggregationService),
                "getAggregationService",
                EmptyList<CodegenNamedParam>.Instance,
                GetType(),
                classScope,
                methodNode => methodNode.Block.MethodReturn(MEMBER_AGGREGATIONSVC));
            instance.Methods.AddMethod(
                typeof(ExprEvaluatorContext),
                "getExprEvaluatorContext",
                EmptyList<CodegenNamedParam>.Instance,
                GetType(),
                classScope,
                methodNode => methodNode.Block.MethodReturn(MEMBER_EXPREVALCONTEXT));
            instance.Methods.AddMethod(
                typeof(bool),
                "hasHavingClause",
                EmptyList<CodegenNamedParam>.Instance,
                GetType(),
                classScope,
                methodNode => methodNode.Block.MethodReturn(Constant(optionalHavingNode != null)));
            instance.Methods.AddMethod(
                typeof(bool),
                "isSelectRStream",
                EmptyList<CodegenNamedParam>.Instance,
                typeof(ResultSetProcessorRowForAll),
                classScope,
                methodNode => methodNode.Block.MethodReturn(Constant(isSelectRStream)));
            ResultSetProcessorUtil.EvaluateHavingClauseCodegen(optionalHavingNode, classScope, instance);
            generateGroupKeySingle = GenerateGroupKeySingleCodegen(
                GroupKeyNodeExpressions,
                multiKeyClassRef,
                classScope,
                instance);
            generateGroupKeyArrayView = GenerateGroupKeyArrayViewCodegen(generateGroupKeySingle, classScope, instance);
            generateGroupKeyArrayJoin = GenerateGroupKeyArrayJoinCodegen(generateGroupKeySingle, classScope, instance);
            ResultSetProcessorRowPerGroupImpl.GenerateOutputBatchedNoSortWMapCodegen(this, classScope, instance);
            ResultSetProcessorRowPerGroupImpl.GenerateOutputBatchedArrFromEnumeratorCodegen(this, classScope, instance);
            ResultSetProcessorRowPerGroupImpl.RemovedAggregationGroupKeyCodegen(classScope, instance);
            if (unboundedProcessor) {
                var factory =
                    classScope.AddOrGetDefaultFieldSharable(ResultSetProcessorHelperFactoryField.INSTANCE);
                instance.AddMember(NAME_GROUPREPS, typeof(ResultSetProcessorRowPerGroupUnboundHelper));
                var groupKeySerde = MultiKeyClassRef.GetExprMKSerde(classScope.NamespaceScope.InitMethod, classScope);
                var eventType = classScope.AddDefaultFieldUnshared(
                    true,
                    typeof(EventType),
                    EventTypeUtility.ResolveTypeCodegen(typesPerStream[0], EPStatementInitServicesConstants.REF));
                instance.ServiceCtor.Block.AssignRef(
                        NAME_GROUPREPS,
                        ExprDotMethod(
                            factory,
                            "MakeRSRowPerGroupUnboundGroupRep",
                            Constant(groupKeyTypes),
                            groupKeySerde,
                            eventType,
                            unboundGroupRepSettings.ToExpression(),
                            MEMBER_EXPREVALCONTEXT))
                    .ExprDotMethod(MEMBER_AGGREGATIONSVC, "setRemovedCallback", Member(NAME_GROUPREPS));
            }
            else {
                instance.ServiceCtor.Block.ExprDotMethod(MEMBER_AGGREGATIONSVC, "setRemovedCallback", Ref("this"));
            }
        }

        public override void ProcessViewResultCodegen(
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

        public override void ProcessJoinResultCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            ResultSetProcessorRowPerGroupImpl.ProcessJoinResultCodegen(this, classScope, method, instance);
        }

        public override void GetEnumeratorViewCodegen(
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
                ResultSetProcessorRowPerGroupImpl.GetEnumeratorViewCodegen(this, classScope, method, instance);
            }
        }

        public override void GetEnumeratorJoinCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            ResultSetProcessorRowPerGroupImpl.GetEnumeratorJoinCodegen(this, classScope, method, instance);
        }

        public override void ProcessOutputLimitedViewCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            ResultSetProcessorRowPerGroupImpl.ProcessOutputLimitedViewCodegen(this, classScope, method, instance);
        }

        public override void ProcessOutputLimitedJoinCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            ResultSetProcessorRowPerGroupImpl.ProcessOutputLimitedJoinCodegen(this, classScope, method, instance);
        }

        public override void ApplyViewResultCodegen(
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

        public override void ApplyJoinResultCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            ResultSetProcessorRowPerGroupImpl.ApplyJoinResultCodegen(this, classScope, method, instance);
        }

        public override void ContinueOutputLimitedLastAllNonBufferedViewCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            ResultSetProcessorRowPerGroupImpl.ContinueOutputLimitedLastAllNonBufferedViewCodegen(this, method);
        }

        public override void ContinueOutputLimitedLastAllNonBufferedJoinCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            ResultSetProcessorRowPerGroupImpl.ContinueOutputLimitedLastAllNonBufferedJoinCodegen(this, method);
        }

        public override void ProcessOutputLimitedLastAllNonBufferedViewCodegen(
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

        public override void ProcessOutputLimitedLastAllNonBufferedJoinCodegen(
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

        public override void AcceptHelperVisitorCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            ResultSetProcessorRowPerGroupImpl.AcceptHelperVisitorCodegen(method, instance);
        }

        public override void StopMethodCodegen(
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

        public override void ClearMethodCodegen(
            CodegenClassScope classScope,
            CodegenMethod method)
        {
            ResultSetProcessorRowPerGroupImpl.ClearMethodCodegen(method);
        }

        public void PlanStateSettings(
            FabricCharge fabricCharge,
            StatementRawInfo statementRawInfo,
            ResultSetProcessorFlags flags,
            StatementCompileTimeServices services)
        {
            if (IsOutputFirst()) {
                outputFirstHelperSettings =
                    services.StateMgmtSettingsProvider.ResultSet.RowPerGroupOutputFirst(
                        fabricCharge,
                        statementRawInfo,
                        this);
            }
            else if (IsOutputAll()) {
                if (flags.OutputConditionType == POLICY_LASTALL_UNORDERED) {
                    outputAllOptHelperSettings =
                        services.StateMgmtSettingsProvider.ResultSet.RowPerGroupOutputAllOpt(
                            fabricCharge,
                            statementRawInfo,
                            this);
                }
                else {
                    outputAllHelperSettings =
                        services.StateMgmtSettingsProvider.ResultSet.RowPerGroupOutputAll(
                            fabricCharge,
                            statementRawInfo,
                            this);
                }
            }
            else if (IsOutputLast()) {
                if (flags.OutputConditionType == POLICY_LASTALL_UNORDERED) {
                    outputLastOptHelperSettings =
                        services.StateMgmtSettingsProvider.ResultSet.RowPerGroupOutputLast(
                            fabricCharge,
                            statementRawInfo,
                            this);
                }
            }

            if (unboundedProcessor) {
                unboundGroupRepSettings =
                    services.StateMgmtSettingsProvider.ResultSet.RowPerGroupUnbound(
                        fabricCharge,
                        statementRawInfo,
                        this);
            }
        }

        private bool IsLimitSpec(OutputLimitLimitType expected)
        {
            return outputLimitSpec != null && outputLimitSpec.DisplayLimit == expected;
        }

        public ExprForge OptionalHavingNode => optionalHavingNode;

        public OutputLimitSpec OutputLimitSpec => outputLimitSpec;

        public ExprNode[] GroupKeyNodeExpressions => groupKeyNodeExpressions;

        public OutputConditionPolledFactoryForge OptionalOutputFirstConditionFactory =>
            optionalOutputFirstConditionFactory;

        public ResultSetProcessorOutputConditionType OutputConditionType => outputConditionType;

        public int NumStreams => typesPerStream.Length;

        public override Type InterfaceClass => typeof(ResultSetProcessorRowPerGroup);

        public MultiKeyClassRef MultiKeyClassRef => multiKeyClassRef;

        public Type[] GroupKeyTypes => groupKeyTypes;

        public override string InstrumentedQName => "ResultSetProcessGroupedRowPerGroup";

        public CodegenMethod GenerateGroupKeySingle => generateGroupKeySingle;

        public CodegenMethod GenerateGroupKeyArrayView => generateGroupKeyArrayView;

        public CodegenMethod GenerateGroupKeyArrayJoin => generateGroupKeyArrayJoin;

        public StateMgmtSetting OutputFirstHelperSettings => outputFirstHelperSettings;

        public StateMgmtSetting OutputAllHelperSettings => outputAllHelperSettings;

        public StateMgmtSetting OutputAllOptHelperSettings => outputAllOptHelperSettings;

        public StateMgmtSetting OutputLastOptHelperSettings => outputLastOptHelperSettings;
    }
} // end of namespace