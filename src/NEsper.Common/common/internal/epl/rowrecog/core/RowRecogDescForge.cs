///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.bytecodemodel.name;
using com.espertech.esper.common.@internal.compile.multikey;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.time.eval;
using com.espertech.esper.common.@internal.epl.rowrecog.nfa;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.rowrecog.core
{
    public class RowRecogDescForge
    {
        private readonly EventType parentEventType;
        private readonly EventType rowEventType;
        private readonly EventType compositeEventType;
        private readonly EventType multimatchEventType;
        private readonly int[] multimatchStreamNumToVariable;
        private readonly int[] multimatchVariableToStreamNum;
        private readonly ExprNode[] partitionBy;
        private readonly MultiKeyClassRef partitionByMultiKey;
        private readonly IDictionary<string, Pair<int, bool>> variableStreams;
        private readonly bool hasInterval;
        private readonly bool iterateOnly;
        private readonly bool unbound;
        private readonly bool orTerminated;
        private readonly bool collectMultimatches;
        private readonly bool defineAsksMultimatches;
        private readonly int numEventsEventsPerStreamDefine;
        private readonly string[] multimatchVariablesArray;
        private readonly RowRecogNFAStateForge[] startStates;
        private readonly RowRecogNFAStateForge[] allStates;
        private readonly bool allMatches;
        private readonly MatchRecognizeSkipEnum skip;
        private readonly ExprNode[] columnEvaluators;
        private readonly string[] columnNames;
        private readonly TimePeriodComputeForge intervalCompute;
        private readonly int[] previousRandomAccessIndexes;
        private readonly AggregationServiceForgeDesc[] aggregationServices;
        private readonly bool isTargetHA;
        private StateMgmtSetting partitionMgmtStateMgmtSettings;
        private StateMgmtSetting scheduleMgmtStateMgmtSettings;

        public RowRecogDescForge(
            EventType parentEventType,
            EventType rowEventType,
            EventType compositeEventType,
            EventType multimatchEventType,
            int[] multimatchStreamNumToVariable,
            int[] multimatchVariableToStreamNum,
            ExprNode[] partitionBy,
            MultiKeyClassRef partitionByMultiKey,
            IDictionary<string, Pair<int, bool>> variableStreams,
            bool hasInterval,
            bool iterateOnly,
            bool unbound,
            bool orTerminated,
            bool collectMultimatches,
            bool defineAsksMultimatches,
            int numEventsEventsPerStreamDefine,
            string[] multimatchVariablesArray,
            RowRecogNFAStateForge[] startStates,
            RowRecogNFAStateForge[] allStates,
            bool allMatches,
            MatchRecognizeSkipEnum skip,
            ExprNode[] columnEvaluators,
            string[] columnNames,
            TimePeriodComputeForge intervalCompute,
            int[] previousRandomAccessIndexes,
            AggregationServiceForgeDesc[] aggregationServices,
            bool isTargetHA)
        {
            this.parentEventType = parentEventType;
            this.rowEventType = rowEventType;
            this.compositeEventType = compositeEventType;
            this.multimatchEventType = multimatchEventType;
            this.multimatchStreamNumToVariable = multimatchStreamNumToVariable;
            this.multimatchVariableToStreamNum = multimatchVariableToStreamNum;
            this.partitionBy = partitionBy;
            this.partitionByMultiKey = partitionByMultiKey;
            this.variableStreams = variableStreams;
            this.hasInterval = hasInterval;
            this.iterateOnly = iterateOnly;
            this.unbound = unbound;
            this.orTerminated = orTerminated;
            this.collectMultimatches = collectMultimatches;
            this.defineAsksMultimatches = defineAsksMultimatches;
            this.numEventsEventsPerStreamDefine = numEventsEventsPerStreamDefine;
            this.multimatchVariablesArray = multimatchVariablesArray;
            this.startStates = startStates;
            this.allStates = allStates;
            this.allMatches = allMatches;
            this.skip = skip;
            this.columnEvaluators = columnEvaluators;
            this.columnNames = columnNames;
            this.intervalCompute = intervalCompute;
            this.previousRandomAccessIndexes = previousRandomAccessIndexes;
            this.aggregationServices = aggregationServices;
            this.isTargetHA = isTargetHA;
        }

        public CodegenExpression Make(
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            var method = parent.MakeChild(typeof(RowRecogDesc), GetType(), classScope);
            var desc = Ref("desc");
            CodegenExpression init = symbols.GetAddInitSvc(method);
            var startStateNums = new int[startStates.Length];
            for (var i = 0; i < startStates.Length; i++) {
                startStateNums[i] = startStates[i].NodeNumFlat;
            }

            var aggregationServiceFactories = ConstantNull();
            if (aggregationServices != null) {
                var initAggsSvcs = new CodegenExpression[aggregationServices.Length];
                for (var i = 0; i < aggregationServices.Length; i++) {
                    initAggsSvcs[i] = ConstantNull();
                    if (aggregationServices[i] != null) {
                        var aggSvc = aggregationServices[i];
                        var aggregationClassNames = new AggregationClassNames("_mra" + i);
                        var result = AggregationServiceFactoryCompiler.MakeInnerClassesAndInit(
                            aggSvc.AggregationServiceFactoryForge,
                            method,
                            classScope,
                            classScope.OutermostClassName,
                            aggregationClassNames,
                            isTargetHA);
                        classScope.AddInnerClasses(result.InnerClasses);
                        initAggsSvcs[i] = LocalMethod(result.InitMethod, symbols.GetAddInitSvc(parent));
                    }
                }

                aggregationServiceFactories = NewArrayWithInit(typeof(AggregationServiceFactory), initAggsSvcs);
            }

            method.Block
                .DeclareVarNewInstance(typeof(RowRecogDesc), desc.Ref)
                .SetProperty(desc, "ParentEventType", EventTypeUtility.ResolveTypeCodegen(parentEventType, init))
                .SetProperty(desc, "RowEventType", EventTypeUtility.ResolveTypeCodegen(rowEventType, init))
                .SetProperty(
                    desc,
                    "CompositeEventType",
                    EventTypeUtility.ResolveTypeCodegen(compositeEventType, init))
                .SetProperty(
                    desc,
                    "MultimatchEventType",
                    multimatchEventType == null
                        ? ConstantNull()
                        : EventTypeUtility.ResolveTypeCodegen(multimatchEventType, init))
                .SetProperty(desc, "MultimatchStreamNumToVariable", Constant(multimatchStreamNumToVariable))
                .SetProperty(desc, "MultimatchVariableToStreamNum", Constant(multimatchVariableToStreamNum))
                .SetProperty(
                    desc,
                    "PartitionEvalMayNull",
                    MultiKeyCodegen.CodegenExprEvaluatorMayMultikey(
                        partitionBy,
                        null,
                        partitionByMultiKey,
                        method,
                        classScope))
                .SetProperty(
                    desc,
                    "PartitionEvalTypes",
                    partitionBy == null
                        ? ConstantNull()
                        : Constant(ExprNodeUtilityQuery.GetExprResultTypes(partitionBy)))
                .SetProperty(
                    desc,
                    "PartitionEvalSerde",
                    partitionBy == null ? ConstantNull() : partitionByMultiKey.GetExprMKSerde(method, classScope))
                .SetProperty(desc, "VariableStreams", MakeVariableStreams(method, classScope))
                .SetProperty(desc, "HasInterval", Constant(hasInterval))
                .SetProperty(desc, "IsIterateOnly", Constant(iterateOnly))
                .SetProperty(desc, "IsUnbound", Constant(unbound))
                .SetProperty(desc, "IsOrTerminated", Constant(orTerminated))
                .SetProperty(desc, "IsCollectMultimatches", Constant(collectMultimatches))
                .SetProperty(desc, "IsDefineAsksMultimatches", Constant(defineAsksMultimatches))
                .SetProperty(desc, "NumEventsEventsPerStreamDefine", Constant(numEventsEventsPerStreamDefine))
                .SetProperty(desc, "MultimatchVariablesArray", Constant(multimatchVariablesArray))
                .SetProperty(desc, "StatesOrdered", MakeStates(method, symbols, classScope))
                .SetProperty(desc, "NextStatesPerState", MakeNextStates(method, classScope))
                .SetProperty(desc, "StartStates", Constant(startStateNums))
                .SetProperty(desc, "IsAllMatches", Constant(allMatches))
                .SetProperty(desc, "Skip", Constant(skip))
                .SetProperty(
                    desc,
                    "ColumnEvaluators",
                    ExprNodeUtilityCodegen.CodegenEvaluators(columnEvaluators, method, GetType(), classScope))
                .SetProperty(desc, "ColumnNames", Constant(columnNames))
                .SetProperty(
                    desc,
                    "IntervalCompute",
                    intervalCompute == null ? ConstantNull() : intervalCompute.MakeEvaluator(method, classScope))
                .SetProperty(desc, "PreviousRandomAccessIndexes", Constant(previousRandomAccessIndexes))
                .SetProperty(desc, "AggregationServiceFactories", aggregationServiceFactories)
                .SetProperty(
                    desc,
                    "AggregationResultFutureAssignables",
                    aggregationServices == null ? ConstantNull() : MakeAggAssignables(method, classScope))
                .SetProperty(desc, "PartitionMgmtStateMgmtSettings", partitionMgmtStateMgmtSettings.ToExpression())
                .SetProperty(desc, "ScheduleMgmtStateMgmtSettings", scheduleMgmtStateMgmtSettings.ToExpression())
                .MethodReturn(desc);
            return LocalMethod(method);
        }

        public bool IsHasInterval => hasInterval;

        public bool IsIterateOnly => iterateOnly;

        public bool IsUnbound => unbound;

        public bool IsOrTerminated => orTerminated;

        public bool IsCollectMultimatches => collectMultimatches;

        public bool IsDefineAsksMultimatches => defineAsksMultimatches;

        public bool IsAllMatches => allMatches;

        public bool IsTargetHA => isTargetHA;

        private CodegenExpression MakeAggAssignables(
            CodegenMethodScope parent,
            CodegenClassScope classScope)
        {
            var method = parent.MakeChild(typeof(AggregationResultFutureAssignable[]), GetType(), classScope);
            method.Block.DeclareVar<AggregationResultFutureAssignable[]>(
                "assignables",
                NewArrayByLength(typeof(AggregationResultFutureAssignable), Constant(aggregationServices.Length)));
            for (var i = 0; i < aggregationServices.Length; i++) {
                if (aggregationServices[i] != null) {
                    // var anonymousClass = NewAnonymousClass(method.Block, typeof(AggregationResultFutureAssignable));
                    // var assign = CodegenMethod.MakeParentNode(typeof(void), GetType(), classScope)
                    //     .AddParam<AggregationResultFuture>("future");
                    // anonymousClass.AddMethod("assign", assign);
                    
                    var assignLambda = new CodegenExpressionLambda(method.Block)
                        .WithParam<AggregationResultFuture>("future");
                    var assignProxy = NewInstance<ProxyAggregationResultFutureAssignable>(assignLambda);
                    
                    var field = classScope.NamespaceScope.AddOrGetDefaultFieldWellKnown(
                        new CodegenFieldNameMatchRecognizeAgg(i),
                        typeof(AggregationResultFuture));
                    assignLambda.Block.AssignRef(field, Ref("future"));
                    method.Block.AssignArrayElement(Ref("assignables"), Constant(i), assignProxy);
                }
            }

            method.Block.MethodReturn(Ref("assignables"));
            return LocalMethod(method);
        }

        private CodegenExpression MakeStates(
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            var method = parent.MakeChild(typeof(RowRecogNFAStateBase[]), GetType(), classScope);
            method.Block.DeclareVar<RowRecogNFAStateBase[]>(
                "states",
                NewArrayByLength(typeof(RowRecogNFAStateBase), Constant(allStates.Length)));
            for (var i = 0; i < allStates.Length; i++) {
                method.Block.AssignArrayElement("states", Constant(i), allStates[i].Make(method, symbols, classScope));
            }

            method.Block.MethodReturn(Ref("states"));
            return LocalMethod(method);
        }

        private CodegenExpression MakeNextStates(
            CodegenMethodScope parent,
            CodegenClassScope classScope)
        {
            IList<Pair<int, int[]>> nextStates = new List<Pair<int, int[]>>();
            foreach (var state in allStates) {
                var next = new int[state.NextStates.Count];
                for (var i = 0; i < next.Length; i++) {
                    next[i] = state.NextStates[i].NodeNumFlat;
                }

                nextStates.Add(new Pair<int, int[]>(state.NodeNumFlat, next));
            }

            var method = parent.MakeChild(typeof(IList<Pair<int, int[]>>), GetType(), classScope);
            method.Block.DeclareVar(
                typeof(IList<Pair<int, int[]>>),
                "next",
                NewInstance(typeof(List<Pair<int, int[]>>), Constant(nextStates.Count)));
            foreach (var pair in nextStates) {
                method.Block.ExprDotMethod(
                    Ref("next"),
                    "Add",
                    NewInstance(typeof(Pair<int, int[]>), Constant(pair.First), Constant(pair.Second)));
            }

            method.Block.MethodReturn(Ref("next"));
            return LocalMethod(method);
        }

        private CodegenExpression MakeVariableStreams(
            CodegenMethodScope parent,
            CodegenClassScope classScope)
        {
            var method = parent
                .MakeChild(typeof(IDictionary<string, Pair<int, bool>>), GetType(), classScope);
            method.Block.DeclareVar(
                typeof(IDictionary<string, Pair<int, bool>>),
                "vars",
                NewInstance(typeof(Dictionary<string, Pair<int, bool>>)));
            foreach (var entry in variableStreams) {
                method.Block.ExprDotMethod(
                    Ref("vars"),
                    "Put",
                    Constant(entry.Key),
                    NewInstance(typeof(Pair<int, bool>), Constant(entry.Value.First), Constant(entry.Value.Second)));
            }

            method.Block.MethodReturn(Ref("vars"));
            return LocalMethod(method);
        }

        public EventType RowEventType => rowEventType;

        public StateMgmtSetting PartitionMgmtStateMgmtSettings {
            get => partitionMgmtStateMgmtSettings;
            set => partitionMgmtStateMgmtSettings = value;
        }

        public StateMgmtSetting ScheduleMgmtStateMgmtSettings {
            get => scheduleMgmtStateMgmtSettings;
            set => scheduleMgmtStateMgmtSettings = value;
        }

        public EventType ParentEventType => parentEventType;

        public EventType CompositeEventType => compositeEventType;

        public EventType MultimatchEventType => multimatchEventType;

        public int[] MultimatchStreamNumToVariable => multimatchStreamNumToVariable;

        public int[] MultimatchVariableToStreamNum => multimatchVariableToStreamNum;

        public ExprNode[] PartitionBy => partitionBy;

        public MultiKeyClassRef PartitionByMultiKey => partitionByMultiKey;

        public IDictionary<string, Pair<int, bool>> VariableStreams => variableStreams;

        public int NumEventsEventsPerStreamDefine => numEventsEventsPerStreamDefine;

        public string[] MultimatchVariablesArray => multimatchVariablesArray;

        public RowRecogNFAStateForge[] StartStates => startStates;

        public RowRecogNFAStateForge[] AllStates => allStates;

        public MatchRecognizeSkipEnum Skip => skip;

        public ExprNode[] ColumnEvaluators => columnEvaluators;

        public string[] ColumnNames => columnNames;

        public TimePeriodComputeForge IntervalCompute => intervalCompute;

        public int[] PreviousRandomAccessIndexes => previousRandomAccessIndexes;

        public AggregationServiceForgeDesc[] AggregationServices => aggregationServices;
    }
} // end of namespace