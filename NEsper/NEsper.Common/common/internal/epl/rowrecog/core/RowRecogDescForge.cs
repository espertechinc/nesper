///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using com.espertech.esper.collection;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.bytecodemodel.name;
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
    using LocalPair = Pair<int, bool>;
    using LocalMap = LinkedHashMap<string, Pair<int, bool>>;

    public class RowRecogDescForge
    {
        private readonly AggregationServiceForgeDesc[] _aggregationServices;
        private readonly bool _allMatches;
        private readonly RowRecogNFAStateForge[] _allStates;
        private readonly bool _collectMultimatches;
        private readonly ExprNode[] _columnEvaluators;
        private readonly string[] _columnNames;
        private readonly EventType _compositeEventType;
        private readonly bool _defineAsksMultimatches;
        private readonly bool _hasInterval;
        private readonly TimePeriodComputeForge _intervalCompute;
        private readonly bool _iterateOnly;
        private readonly EventType _multimatchEventType;
        private readonly int[] _multimatchStreamNumToVariable;
        private readonly string[] _multimatchVariablesArray;
        private readonly int[] _multimatchVariableToStreamNum;
        private readonly int _numEventsEventsPerStreamDefine;
        private readonly bool _orTerminated;
        private readonly EventType _parentEventType;
        private readonly ExprNode[] _partitionBy;
        private readonly int[] _previousRandomAccessIndexes;
        private readonly MatchRecognizeSkipEnum _skip;
        private readonly RowRecogNFAStateForge[] _startStates;
        private readonly bool _unbound;
        private readonly LinkedHashMap<string, Pair<int, bool>> _variableStreams;

        public RowRecogDescForge(
            EventType parentEventType,
            EventType rowEventType,
            EventType compositeEventType,
            EventType multimatchEventType,
            int[] multimatchStreamNumToVariable,
            int[] multimatchVariableToStreamNum,
            ExprNode[] partitionBy,
            LinkedHashMap<string, Pair<int, bool>> variableStreams,
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
            AggregationServiceForgeDesc[] aggregationServices)
        {
            this._parentEventType = parentEventType;
            RowEventType = rowEventType;
            this._compositeEventType = compositeEventType;
            this._multimatchEventType = multimatchEventType;
            this._multimatchStreamNumToVariable = multimatchStreamNumToVariable;
            this._multimatchVariableToStreamNum = multimatchVariableToStreamNum;
            this._partitionBy = partitionBy;
            this._variableStreams = variableStreams;
            this._hasInterval = hasInterval;
            this._iterateOnly = iterateOnly;
            this._unbound = unbound;
            this._orTerminated = orTerminated;
            this._collectMultimatches = collectMultimatches;
            this._defineAsksMultimatches = defineAsksMultimatches;
            this._numEventsEventsPerStreamDefine = numEventsEventsPerStreamDefine;
            this._multimatchVariablesArray = multimatchVariablesArray;
            this._startStates = startStates;
            this._allStates = allStates;
            this._allMatches = allMatches;
            this._skip = skip;
            this._columnEvaluators = columnEvaluators;
            this._columnNames = columnNames;
            this._intervalCompute = intervalCompute;
            this._previousRandomAccessIndexes = previousRandomAccessIndexes;
            this._aggregationServices = aggregationServices;
        }

        public EventType RowEventType { get; }

        public CodegenExpression Make(
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            var method = parent.MakeChild(typeof(RowRecogDesc), GetType(), classScope);
            var desc = Ref("desc");
            CodegenExpression init = symbols.GetAddInitSvc(method);

            var startStateNums = new int[_startStates.Length];
            for (var i = 0; i < _startStates.Length; i++) {
                startStateNums[i] = _startStates[i].NodeNumFlat;
            }

            var aggregationServiceFactories = ConstantNull();
            if (_aggregationServices != null) {
                var initAggsSvcs = new CodegenExpression[_aggregationServices.Length];
                for (var i = 0; i < _aggregationServices.Length; i++) {
                    initAggsSvcs[i] = ConstantNull();
                    if (_aggregationServices[i] != null) {
                        var aggSvc = _aggregationServices[i];
                        var aggregationClassNames = new AggregationClassNames("_mra" + i);
                        var result = AggregationServiceFactoryCompiler.MakeInnerClassesAndInit(
                            false, aggSvc.AggregationServiceFactoryForge, method, classScope,
                            classScope.OutermostClassName, aggregationClassNames);
                        classScope.AddInnerClasses(result.InnerClasses);
                        initAggsSvcs[i] = LocalMethod(result.InitMethod, symbols.GetAddInitSvc(parent));
                    }
                }

                aggregationServiceFactories = NewArrayWithInit(typeof(AggregationServiceFactory), initAggsSvcs);
            }

            method.Block
                .DeclareVar(typeof(RowRecogDesc), desc.Ref, NewInstance(typeof(RowRecogDesc)))
                .SetProperty(desc, "ParentEventType", EventTypeUtility.ResolveTypeCodegen(_parentEventType, init))
                .SetProperty(desc, "RowEventType", EventTypeUtility.ResolveTypeCodegen(RowEventType, init))
                .SetProperty(desc, "CompositeEventType", EventTypeUtility.ResolveTypeCodegen(_compositeEventType, init))
                .SetProperty(desc, "MultimatchEventType",
                    _multimatchEventType == null
                        ? ConstantNull()
                        : EventTypeUtility.ResolveTypeCodegen(_multimatchEventType, init))
                .SetProperty(desc, "MultimatchStreamNumToVariable", Constant(_multimatchStreamNumToVariable))
                .SetProperty(desc, "MultimatchVariableToStreamNum", Constant(_multimatchVariableToStreamNum))
                .SetProperty(desc, "PartitionEvalMayNull",
                    _partitionBy == null
                        ? ConstantNull()
                        : ExprNodeUtilityCodegen.CodegenEvaluatorMayMultiKeyWCoerce(
                            ExprNodeUtilityQuery.GetForges(_partitionBy), null, method, GetType(), classScope))
                .SetProperty(desc, "PartitionEvalTypes",
                    _partitionBy == null
                        ? ConstantNull()
                        : Constant(ExprNodeUtilityQuery.GetExprResultTypes(_partitionBy)))
                .SetProperty(desc, "VariableStreams", MakeVariableStreams(method, symbols, classScope))
                .SetProperty(desc, "HasInterval", Constant(_hasInterval))
                .SetProperty(desc, "IterateOnly", Constant(_iterateOnly))
                .SetProperty(desc, "Unbound", Constant(_unbound))
                .SetProperty(desc, "OrTerminated", Constant(_orTerminated))
                .SetProperty(desc, "CollectMultimatches", Constant(_collectMultimatches))
                .SetProperty(desc, "DefineAsksMultimatches", Constant(_defineAsksMultimatches))
                .SetProperty(desc, "NumEventsEventsPerStreamDefine", Constant(_numEventsEventsPerStreamDefine))
                .SetProperty(desc, "MultimatchVariablesArray", Constant(_multimatchVariablesArray))
                .SetProperty(desc, "StatesOrdered", MakeStates(method, symbols, classScope))
                .SetProperty(desc, "NextStatesPerState", MakeNextStates(method, classScope))
                .SetProperty(desc, "StartStates", Constant(startStateNums))
                .SetProperty(desc, "AllMatches", Constant(_allMatches))
                .SetProperty(desc, "Skip", Constant(_skip))
                .SetProperty(desc, "ColumnEvaluators",
                    ExprNodeUtilityCodegen.CodegenEvaluators(_columnEvaluators, method, GetType(), classScope))
                .SetProperty(desc, "ColumnNames", Constant(_columnNames))
                .SetProperty(desc, "IntervalCompute",
                    _intervalCompute == null ? ConstantNull() : _intervalCompute.MakeEvaluator(method, classScope))
                .SetProperty(desc, "PreviousRandomAccessIndexes", Constant(_previousRandomAccessIndexes))
                .SetProperty(desc, "AggregationServiceFactories", aggregationServiceFactories)
                .SetProperty(desc, "AggregationResultFutureAssignables",
                    _aggregationServices == null ? ConstantNull() : MakeAggAssignables(method, classScope))
                .MethodReturn(desc);
            return LocalMethod(method);
        }

        private CodegenExpression MakeAggAssignables(
            CodegenMethodScope parent,
            CodegenClassScope classScope)
        {
            var method = parent.MakeChild(typeof(AggregationResultFutureAssignable[]), GetType(), classScope);
            method.Block
                .DeclareVar(
                    typeof(AggregationResultFutureAssignable[]), "assignables",
                    NewArrayByLength(typeof(AggregationResultFutureAssignable), Constant(_aggregationServices.Length)));

            for (var i = 0; i < _aggregationServices.Length; i++) {
                if (_aggregationServices[i] != null) {
                    var anonymousClass = NewAnonymousClass(method.Block, typeof(AggregationResultFutureAssignable));
                    var assign = CodegenMethod.MakeParentNode(typeof(void), GetType(), classScope)
                        .AddParam(typeof(AggregationResultFuture), "future");
                    anonymousClass.AddMethod("assign", assign);

                    CodegenExpression field = classScope.NamespaceScope.AddOrGetFieldWellKnown(
                        new CodegenFieldNameMatchRecognizeAgg(i), typeof(AggregationResultFuture));
                    assign.Block.AssignRef(field, Ref("future"));

                    method.Block.AssignArrayElement(Ref("assignables"), Constant(i), anonymousClass);
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
            method.Block.DeclareVar(
                typeof(RowRecogNFAStateBase[]), "states",
                NewArrayByLength(typeof(RowRecogNFAStateBase), Constant(_allStates.Length)));
            for (var i = 0; i < _allStates.Length; i++) {
                method.Block.AssignArrayElement("states", Constant(i), _allStates[i].Make(method, symbols, classScope));
            }

            method.Block.MethodReturn(Ref("states"));
            return LocalMethod(method);
        }

        private CodegenExpression MakeNextStates(
            CodegenMethodScope parent,
            CodegenClassScope classScope)
        {
            IList<Pair<int, int[]>> nextStates = new List<Pair<int, int[]>>();
            foreach (var state in _allStates) {
                var next = new int[state.NextStates.Count];
                for (var i = 0; i < next.Length; i++) {
                    next[i] = state.NextStates[i].NodeNumFlat;
                }

                nextStates.Add(new Pair<int, int[]>(state.NodeNumFlat, next));
            }

            var method = parent.MakeChild(typeof(IList<object>), GetType(), classScope);
            method.Block.DeclareVar(
                typeof(IList<object>), "next", NewInstance<List<object>>(Constant(nextStates.Count)));
            foreach (var pair in nextStates) {
                method.Block.ExprDotMethod(
                    Ref("next"), "add", NewInstance(typeof(Pair<int, int[]>), Constant(pair.First), Constant(pair.Second)));
            }

            method.Block.MethodReturn(Ref("next"));
            return LocalMethod(method);
        }

        private CodegenExpression MakeVariableStreams(
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            var method = parent.MakeChild(typeof(LinkedHashMap<string, Pair<int, bool>>), GetType(), classScope);
            method.Block
                .DeclareVar(
                    typeof(LinkedHashMap<string, Pair<int, bool>>), "vars",
                    NewInstance(
                        typeof(LinkedHashMap<string, Pair<int, bool>>),
                        Constant(CollectionUtil.CapacityHashMap(_variableStreams.Count))));
            foreach (var entry in _variableStreams) {
                method.Block.ExprDotMethod(
                    Ref("vars"), "put", Constant(entry.Key),
                    NewInstance(typeof(Pair<int, bool>), Constant(entry.Value.First), Constant(entry.Value.Second)));
            }

            method.Block.MethodReturn(Ref("vars"));
            return LocalMethod(method);
        }
    }
} // end of namespace