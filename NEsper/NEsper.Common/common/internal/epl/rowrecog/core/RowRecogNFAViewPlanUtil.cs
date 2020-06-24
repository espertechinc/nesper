///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using com.espertech.esper.collection;
using com.espertech.esper.common.client;
using com.espertech.esper.common.client.annotation;
using com.espertech.esper.common.client.meta;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.compile.multikey;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.expression.agg.@base;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.prev;
using com.espertech.esper.common.@internal.epl.expression.time.eval;
using com.espertech.esper.common.@internal.epl.expression.time.node;
using com.espertech.esper.common.@internal.epl.expression.visitor;
using com.espertech.esper.common.@internal.epl.rowrecog.expr;
using com.espertech.esper.common.@internal.epl.rowrecog.nfa;
using com.espertech.esper.common.@internal.epl.streamtype;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.container;

namespace com.espertech.esper.common.@internal.epl.rowrecog.core
{
    /// <summary>
    ///     Plan match-recognize.
    /// </summary>
    public class RowRecogNFAViewPlanUtil
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static RowRecogPlan ValidateAndPlan(
            IContainer container,
            EventType parentEventType,
            bool unbound,
            StatementBaseInfo @base,
            StatementCompileTimeServices services)
        {
            var statementRawInfo = @base.StatementRawInfo;
            var matchRecognizeSpec = @base.StatementSpec.Raw.MatchRecognizeSpec;
            var annotations = statementRawInfo.Annotations;
            var iterateOnly = HintEnum.ITERATE_ONLY.GetHint(annotations) != null;
            var additionalForgeables = new List<StmtClassForgeableFactory>();

            // Expanded pattern already there
            RowRecogExprNode expandedPatternNode = matchRecognizeSpec.Pattern;

            // Determine single-row and multiple-row variables
            var variablesSingle = new LinkedHashSet<string>();
            var variablesMultiple = new LinkedHashSet<string>();
            RowRecogHelper.RecursiveInspectVariables(expandedPatternNode, false, variablesSingle, variablesMultiple);

            // each variable gets associated with a stream number (multiple-row variables as well to hold the current event for the expression).
            var streamNum = 0;
            var variableStreams = new LinkedHashMap<string, Pair<int, bool>>();
            foreach (var variableSingle in variablesSingle) {
                variableStreams.Put(variableSingle, new Pair<int, bool>(streamNum, false));
                streamNum++;
            }

            foreach (var variableMultiple in variablesMultiple) {
                variableStreams.Put(variableMultiple, new Pair<int, bool>(streamNum, true));
                streamNum++;
            }

            // mapping of stream to variable
            var streamVariables = new OrderedDictionary<int, string>();
            foreach (var entry in variableStreams) {
                streamVariables.Put(entry.Value.First, entry.Key);
            }

            // determine visibility rules
            var visibility = RowRecogHelper.DetermineVisibility(expandedPatternNode);

            // assemble all single-row variables for expression validation
            var allStreamNames = new string[variableStreams.Count];
            var allTypes = new EventType[variableStreams.Count];

            streamNum = 0;
            foreach (var variableSingle in variablesSingle) {
                allStreamNames[streamNum] = variableSingle;
                allTypes[streamNum] = parentEventType;
                streamNum++;
            }

            foreach (var variableMultiple in variablesMultiple) {
                allStreamNames[streamNum] = variableMultiple;
                allTypes[streamNum] = parentEventType;
                streamNum++;
            }

            // determine type service for use with DEFINE
            // validate each DEFINE clause expression
            ISet<string> definedVariables = new HashSet<string>();
            IList<ExprAggregateNode> aggregateNodes = new List<ExprAggregateNode>();
            var isExprRequiresMultimatchState = new bool[variableStreams.Count];
            var previousNodes = new OrderedDictionary<int, IList<ExprPreviousMatchRecognizeNode>>();

            for (var defineIndex = 0; defineIndex < matchRecognizeSpec.Defines.Count; defineIndex++) {
                MatchRecognizeDefineItem defineItem = matchRecognizeSpec.Defines[defineIndex];
                if (definedVariables.Contains(defineItem.Identifier)) {
                    throw new ExprValidationException(
                        "Variable '" + defineItem.Identifier + "' has already been defined");
                }

                definedVariables.Add(defineItem.Identifier);

                // stream-type visibilities handled here
                var typeServiceDefines = BuildDefineStreamTypeServiceDefine(
                    defineIndex,
                    variableStreams,
                    defineItem,
                    visibility,
                    parentEventType,
                    statementRawInfo,
                    services);

                var exprNodeResult = HandlePreviousFunctions(defineItem.Expression, previousNodes);
                var validationContext = new ExprValidationContextBuilder(typeServiceDefines, statementRawInfo, services)
                    .WithAllowBindingConsumption(true)
                    .WithDisablePropertyExpressionEventCollCache(true)
                    .Build();

                ExprNode validated;
                try {
                    // validate
                    validated = ExprNodeUtilityValidate.GetValidatedSubtree(
                        ExprNodeOrigin.MATCHRECOGDEFINE,
                        exprNodeResult,
                        validationContext);

                    // check aggregates
                    defineItem.Expression = validated;
                    ExprAggregateNodeUtil.GetAggregatesBottomUp(validated, aggregateNodes);
                    if (!aggregateNodes.IsEmpty()) {
                        throw new ExprValidationException("An aggregate function may not appear in a DEFINE clause");
                    }
                }
                catch (ExprValidationException ex) {
                    throw new ExprValidationException(
                        "Failed to validate condition expression for variable '" +
                        defineItem.Identifier +
                        "': " +
                        ex.Message,
                        ex);
                }

                // determine access to event properties from multi-matches
                var visitor = new ExprNodeStreamRequiredVisitor();
                validated.Accept(visitor);
                var streamsRequired = visitor.StreamsRequired;
                foreach (var streamRequired in streamsRequired) {
                    if (streamRequired >= variableStreams.Count) {
                        var streamNumIdent = variableStreams.Get(defineItem.Identifier).First;
                        isExprRequiresMultimatchState[streamNumIdent] = true;
                        break;
                    }
                }
            }

            var defineAsksMultimatches = CollectionUtil.IsAnySet(isExprRequiresMultimatchState);

            // determine type service for use with MEASURE
            IDictionary<string, object> measureTypeDef = new LinkedHashMap<string, object>();
            foreach (var variableSingle in variablesSingle) {
                measureTypeDef.Put(variableSingle, parentEventType);
            }

            foreach (var variableMultiple in variablesMultiple) {
                measureTypeDef.Put(variableMultiple, new[] {parentEventType});
            }

            var compositeTypeName = services.EventTypeNameGeneratorStatement.AnonymousRowrecogCompositeName;
            var compositeTypeMetadata = new EventTypeMetadata(
                compositeTypeName,
                @base.ModuleName,
                EventTypeTypeClass.MATCHRECOGDERIVED,
                EventTypeApplicationType.OBJECTARR,
                NameAccessModifier.TRANSIENT,
                EventTypeBusModifier.NONBUS,
                false,
                EventTypeIdPair.Unassigned());
            var compositeEventType = BaseNestableEventUtil.MakeOATypeCompileTime(
                compositeTypeMetadata,
                measureTypeDef,
                null,
                null,
                null,
                null,
                services.BeanEventTypeFactoryPrivate,
                services.EventTypeCompileTimeResolver);
            services.EventTypeCompileTimeRegistry.NewType(compositeEventType);
            StreamTypeService compositeTypeServiceMeasure =
                new StreamTypeServiceImpl(compositeEventType, "MATCH_RECOGNIZE", true);

            // find MEASURE clause aggregations
            var measureReferencesMultivar = false;
            IList<ExprAggregateNode> measureAggregateExprNodes = new List<ExprAggregateNode>();
            foreach (var measureItem in matchRecognizeSpec.Measures) {
                ExprAggregateNodeUtil.GetAggregatesBottomUp(measureItem.Expr, measureAggregateExprNodes);
            }

            AggregationServiceForgeDesc[] aggregationServices = null;
            if (!measureAggregateExprNodes.IsEmpty()) {
                aggregationServices = PlanAggregations(
                    measureAggregateExprNodes,
                    compositeTypeServiceMeasure,
                    allStreamNames,
                    allTypes,
                    streamVariables,
                    variablesMultiple,
                    @base,
                    services);
                foreach (AggregationServiceForgeDesc svc in aggregationServices) {
                    if (svc != null) {
                        additionalForgeables.AddAll(svc.AdditionalForgeables);
                    }
                }
            }

            // validate each MEASURE clause expression
            IDictionary<string, object> rowTypeDef = new LinkedHashMap<string, object>();
            var streamRefVisitor = new ExprNodeStreamUseCollectVisitor();
            foreach (var measureItem in matchRecognizeSpec.Measures) {
                if (measureItem.Name == null) {
                    throw new ExprValidationException(
                        "The measures clause requires that each expression utilizes the AS keyword to assign a column name");
                }

                var validated = ValidateMeasureClause(
                    measureItem.Expr,
                    compositeTypeServiceMeasure,
                    variablesMultiple,
                    variablesSingle,
                    statementRawInfo,
                    services);
                measureItem.Expr = validated;
                rowTypeDef.Put(measureItem.Name, validated.Forge.EvaluationType);
                validated.Accept(streamRefVisitor);
            }

            // Determine if any of the multi-var streams are referenced in the measures (non-aggregated only)
            foreach (var @ref in streamRefVisitor.Referenced) {
                var rootPropName = @ref.RootPropertyNameIfAny;
                if (rootPropName != null) {
                    if (variablesMultiple.Contains(rootPropName)) {
                        measureReferencesMultivar = true;
                        break;
                    }
                }

                var streamRequired = @ref.StreamReferencedIfAny;
                if (streamRequired != null) {
                    var streamVariable = streamVariables.Get(streamRequired.Value);
                    if (streamVariable != null) {
                        var def = variableStreams.Get(streamVariable);
                        if (def != null && def.Second) {
                            measureReferencesMultivar = true;
                            break;
                        }
                    }
                }
            }

            var collectMultimatches = measureReferencesMultivar || defineAsksMultimatches;

            // create rowevent type
            var rowTypeName = services.EventTypeNameGeneratorStatement.AnonymousRowrecogRowName;
            var rowTypeMetadata = new EventTypeMetadata(
                rowTypeName,
                @base.ModuleName,
                EventTypeTypeClass.MATCHRECOGDERIVED,
                EventTypeApplicationType.MAP,
                NameAccessModifier.TRANSIENT,
                EventTypeBusModifier.NONBUS,
                false,
                EventTypeIdPair.Unassigned());
            var rowEventType = BaseNestableEventUtil.MakeMapTypeCompileTime(
                rowTypeMetadata,
                rowTypeDef,
                null,
                null,
                null,
                null,
                services.BeanEventTypeFactoryPrivate,
                services.EventTypeCompileTimeResolver);
            services.EventTypeCompileTimeRegistry.NewType(rowEventType);

            // validate partition-by expressions, if any
            ExprNode[] partitionBy;
            MultiKeyClassRef partitionMultiKey;
            if (!matchRecognizeSpec.PartitionByExpressions.IsEmpty()) {
                StreamTypeService typeServicePartition = new StreamTypeServiceImpl(
                    parentEventType,
                    "MATCH_RECOGNIZE_PARTITION",
                    true);
                IList<ExprNode> validated = new List<ExprNode>();
                var validationContext =
                    new ExprValidationContextBuilder(typeServicePartition, statementRawInfo, services)
                        .WithAllowBindingConsumption(true)
                        .Build();
                foreach (var partitionExpr in matchRecognizeSpec.PartitionByExpressions) {
                    validated.Add(
                        ExprNodeUtilityValidate.GetValidatedSubtree(
                            ExprNodeOrigin.MATCHRECOGPARTITION,
                            partitionExpr,
                            validationContext));
                }

                matchRecognizeSpec.PartitionByExpressions = validated;
                partitionBy = ExprNodeUtilityQuery.ToArray(validated);
                MultiKeyPlan multiKeyPlan = MultiKeyPlanner.PlanMultiKey(partitionBy, false, @base.StatementRawInfo, services.SerdeResolver);
                partitionMultiKey = multiKeyPlan.ClassRef;
                additionalForgeables.AddAll(multiKeyPlan.MultiKeyForgeables);
            }
            else {
                partitionBy = null;
                partitionMultiKey = null;
            }

            // validate interval if present
            if (matchRecognizeSpec.Interval != null) {
                var validationContext =
                    new ExprValidationContextBuilder(new StreamTypeServiceImpl(false), statementRawInfo, services)
                        .WithAllowBindingConsumption(true)
                        .Build();
                var validated = (ExprTimePeriod) ExprNodeUtilityValidate.GetValidatedSubtree(
                    ExprNodeOrigin.MATCHRECOGINTERVAL,
                    matchRecognizeSpec.Interval.TimePeriodExpr,
                    validationContext);
                matchRecognizeSpec.Interval.TimePeriodExpr = validated;
            }

            // compile variable definition expressions
            IDictionary<string, ExprNode> variableDefinitions = new Dictionary<string, ExprNode>();
            foreach (var defineItem in matchRecognizeSpec.Defines) {
                variableDefinitions.Put(defineItem.Identifier, defineItem.Expression);
            }

            // create evaluators
            var columnNames = new string[matchRecognizeSpec.Measures.Count];
            var columnForges = new ExprNode[matchRecognizeSpec.Measures.Count];
            var count = 0;
            foreach (var measureItem in matchRecognizeSpec.Measures) {
                columnNames[count] = measureItem.Name;
                columnForges[count] = measureItem.Expr;
                count++;
            }

            // build states
            var strand = RowRecogHelper.BuildStartStates(
                expandedPatternNode,
                variableDefinitions,
                variableStreams,
                isExprRequiresMultimatchState);
            var startStates = strand.StartStates.ToArray();
            RowRecogNFAStateForge[] allStates = strand.AllStates.ToArray();

            if (Log.IsInfoEnabled) {
                Log.Info("NFA tree:\n" + RowRecogNFAViewUtil.Print(startStates));
            }

            // determine names of multimatching variables
            string[] multimatchVariablesArray;
            int[] multimatchStreamNumToVariable;
            int[] multimatchVariableToStreamNum;
            if (variablesSingle.Count == variableStreams.Count) {
                multimatchVariablesArray = new string[0];
                multimatchStreamNumToVariable = new int[0];
                multimatchVariableToStreamNum = new int[0];
            }
            else {
                multimatchVariablesArray = new string[variableStreams.Count - variablesSingle.Count];
                multimatchVariableToStreamNum = new int[multimatchVariablesArray.Length];
                multimatchStreamNumToVariable = new int[variableStreams.Count];
                CompatExtensions.Fill(multimatchStreamNumToVariable, -1);
                count = 0;
                foreach (var entry in variableStreams) {
                    if (entry.Value.Second) {
                        var index = count;
                        multimatchVariablesArray[index] = entry.Key;
                        multimatchVariableToStreamNum[index] = entry.Value.First;
                        multimatchStreamNumToVariable[entry.Value.First] = index;
                        count++;
                    }
                }
            }

            var numEventsEventsPerStreamDefine =
                defineAsksMultimatches ? variableStreams.Count + 1 : variableStreams.Count;

            // determine interval-or-terminated
            var orTerminated = matchRecognizeSpec.Interval != null && matchRecognizeSpec.Interval.IsOrTerminated;
            TimePeriodComputeForge intervalCompute = null;
            if (matchRecognizeSpec.Interval != null) {
                intervalCompute = matchRecognizeSpec.Interval.TimePeriodExpr.TimePeriodComputeForge;
            }

            EventType multimatchEventType = null;
            if (defineAsksMultimatches) {
                multimatchEventType = GetDefineMultimatchEventType(variableStreams, parentEventType, @base, services);
            }

            // determine previous-access indexes and assign "prev" node indexes
            // Since an expression such as "prior(2, price), prior(8, price)" translates into {2, 8} the relative index is {0, 1}.
            // Map the expression-supplied index to a relative index
            int[] previousRandomAccessIndexes = null;
            if (!previousNodes.IsEmpty()) {
                previousRandomAccessIndexes = new int[previousNodes.Count];
                var countPrev = 0;
                foreach (var entry in previousNodes) {
                    previousRandomAccessIndexes[countPrev] = entry.Key;
                    foreach (var callback in entry.Value) {
                        callback.AssignedIndex = countPrev;
                    }

                    countPrev++;
                }
            }

            RowRecogDescForge forge = new RowRecogDescForge(
                parentEventType,
                rowEventType,
                compositeEventType,
                multimatchEventType,
                multimatchStreamNumToVariable,
                multimatchVariableToStreamNum,
                partitionBy,
                partitionMultiKey,
                variableStreams,
                matchRecognizeSpec.Interval != null,
                iterateOnly,
                unbound,
                orTerminated,
                collectMultimatches,
                defineAsksMultimatches,
                numEventsEventsPerStreamDefine,
                multimatchVariablesArray,
                startStates,
                allStates,
                matchRecognizeSpec.IsAllMatches,
                matchRecognizeSpec.Skip.Skip,
                columnForges,
                columnNames,
                intervalCompute,
                previousRandomAccessIndexes,
                aggregationServices);
            
            return new RowRecogPlan(forge, additionalForgeables);
        }

        private static AggregationServiceForgeDesc[] PlanAggregations(
            IList<ExprAggregateNode> measureAggregateExprNodes,
            StreamTypeService compositeTypeServiceMeasure,
            string[] allStreamNames,
            EventType[] allTypes,
            OrderedDictionary<int, string> streamVariables,
            ISet<string> variablesMultiple,
            StatementBaseInfo @base,
            StatementCompileTimeServices services)
        {
            IDictionary<int, IList<ExprAggregateNode>> measureExprAggNodesPerStream =
                new Dictionary<int, IList<ExprAggregateNode>>();

            foreach (var aggregateNode in measureAggregateExprNodes) {
                // validate node and params
                var count = 0;
                var visitor = new ExprNodeIdentifierVisitor(true);
                var isIStreamOnly = new bool[allStreamNames.Length];
                var typeServiceAggregateMeasure = new StreamTypeServiceImpl(
                    allTypes,
                    allStreamNames,
                    isIStreamOnly,
                    false,
                    true);

                var validationContext =
                    new ExprValidationContextBuilder(typeServiceAggregateMeasure, @base.StatementRawInfo, services)
                        .WithAllowBindingConsumption(true)
                        .Build();
                aggregateNode.ValidatePositionals(validationContext);

                if (aggregateNode.OptionalLocalGroupBy != null) {
                    throw new ExprValidationException(
                        "Match-recognize does not allow aggregation functions to specify a group-by");
                }

                foreach (var child in aggregateNode.ChildNodes) {
                    var validated = ExprNodeUtilityValidate.GetValidatedSubtree(
                        ExprNodeOrigin.MATCHRECOGMEASURE,
                        child,
                        validationContext);
                    validated.Accept(visitor);
                    aggregateNode.SetChildNode(count++, new ExprNodeValidated(validated));
                }

                // verify properties used within the aggregation
                ISet<int> aggregatedStreams = new HashSet<int>();
                foreach (var pair in visitor.ExprProperties) {
                    aggregatedStreams.Add(pair.First);
                }

                int? multipleVarStream = null;
                foreach (var streamNumAggregated in aggregatedStreams) {
                    var variable = streamVariables.Get(streamNumAggregated);
                    if (variablesMultiple.Contains(variable)) {
                        if (multipleVarStream == null) {
                            multipleVarStream = streamNumAggregated;
                            continue;
                        }

                        throw new ExprValidationException(
                            "Aggregation functions in the measure-clause must only refer to properties of exactly one group variable returning multiple events");
                    }
                }

                if (multipleVarStream == null) {
                    throw new ExprValidationException(
                        "Aggregation functions in the measure-clause must refer to one or more properties of exactly one group variable returning multiple events");
                }

                var aggNodesForStream = measureExprAggNodesPerStream.Get(multipleVarStream.Value);
                if (aggNodesForStream == null) {
                    aggNodesForStream = new List<ExprAggregateNode>();
                    measureExprAggNodesPerStream.Put(multipleVarStream.Value, aggNodesForStream);
                }

                aggNodesForStream.Add(aggregateNode);
            }

            // validate aggregation itself
            foreach (var entry in measureExprAggNodesPerStream) {
                foreach (var aggregateNode in entry.Value) {
                    var validationContext = new ExprValidationContextBuilder(
                            compositeTypeServiceMeasure,
                            @base.StatementRawInfo,
                            services)
                        .WithAllowBindingConsumption(true)
                        .WithMemberName(new ExprValidationMemberNameQualifiedRowRecogAgg(entry.Key))
                        .Build();
                    aggregateNode.Validate(validationContext);
                }
            }

            // get aggregation service per variable
            var aggServices = new AggregationServiceForgeDesc[allStreamNames.Length];
            var declareds = Arrays.AsList(@base.StatementSpec.DeclaredExpressions);
            foreach (var entry in measureExprAggNodesPerStream) {
                EventType[] typesPerStream = {allTypes[entry.Key]};
                AggregationServiceForgeDesc desc = AggregationServiceFactoryFactory.GetService(
                    entry.Value,
                    EmptyDictionary<ExprNode, string>.Instance, 
                    declareds,
                    new ExprNode[0],
                    null,
                    EmptyList<ExprAggregateNode>.Instance,
                    EmptyList<ExprAggregateNode>.Instance,
                    EmptyList<ExprAggregateNodeGroupKey>.Instance,
                    false,
                    @base.StatementRawInfo.Annotations,
                    services.VariableCompileTimeResolver,
                    true,
                    null,
                    null,
                    typesPerStream,
                    null,
                    @base.ContextName,
                    null,
                    services.TableCompileTimeResolver,
                    false,
                    true,
                    false,
                    services.ImportServiceCompileTime,
                    @base.StatementRawInfo,
                    services.SerdeResolver);

                aggServices[entry.Key] = desc;
            }

            return aggServices;
        }

        private static EventType GetDefineMultimatchEventType(
            LinkedHashMap<string, Pair<int, bool>> variableStreams,
            EventType parentEventType,
            StatementBaseInfo @base,
            StatementCompileTimeServices services)
        {
            IDictionary<string, object> multievent = new LinkedHashMap<string, object>();
            foreach (var entry in variableStreams) {
                if (entry.Value.Second) {
                    multievent.Put(entry.Key, new[] {parentEventType});
                }
            }

            var multimatchAllTypeName = services.EventTypeNameGeneratorStatement.AnonymousRowrecogMultimatchAllName;
            var multimatchAllTypeMetadata = new EventTypeMetadata(
                multimatchAllTypeName,
                @base.ModuleName,
                EventTypeTypeClass.MATCHRECOGDERIVED,
                EventTypeApplicationType.OBJECTARR,
                NameAccessModifier.TRANSIENT,
                EventTypeBusModifier.NONBUS,
                false,
                EventTypeIdPair.Unassigned());
            var multimatchAllEventType = BaseNestableEventUtil.MakeOATypeCompileTime(
                multimatchAllTypeMetadata,
                multievent,
                null,
                null,
                null,
                null,
                services.BeanEventTypeFactoryPrivate,
                services.EventTypeCompileTimeResolver);
            services.EventTypeCompileTimeRegistry.NewType(multimatchAllEventType);
            return multimatchAllEventType;
        }

        private static ExprNode ValidateMeasureClause(
            ExprNode measureNode,
            StreamTypeService typeServiceMeasure,
            ISet<string> variablesMultiple,
            ISet<string> variablesSingle,
            StatementRawInfo statementRawInfo,
            StatementCompileTimeServices services)
        {
            try {
                var validationContext = new ExprValidationContextBuilder(typeServiceMeasure, statementRawInfo, services)
                    .WithAllowBindingConsumption(true)
                    .WithDisablePropertyExpressionEventCollCache(true)
                    .WithAggregationFutureNameAlreadySet(true)
                    .Build();
                return ExprNodeUtilityValidate.GetValidatedSubtree(
                    ExprNodeOrigin.MATCHRECOGMEASURE,
                    measureNode,
                    validationContext);
            }
            catch (ExprValidationPropertyException e) {
                var grouped = CollectionUtil.ToString(variablesMultiple);
                var single = CollectionUtil.ToString(variablesSingle);
                var message = e.Message;
                if (!variablesMultiple.IsEmpty()) {
                    message += ", ensure that grouped variables (variables " +
                               grouped +
                               ") are accessed via index (i.e. variable[0].property) or appear within an aggregation";
                }

                if (!variablesSingle.IsEmpty()) {
                    message += ", ensure that singleton variables (variables " +
                               single +
                               ") are not accessed via index";
                }

                throw new ExprValidationPropertyException(message, e);
            }
        }

        private static ExprNode HandlePreviousFunctions(
            ExprNode defineItemExpression,
            OrderedDictionary<int, IList<ExprPreviousMatchRecognizeNode>> previousNodes)
        {
            var previousVisitor = new ExprNodePreviousVisitorWParent();
            defineItemExpression.Accept(previousVisitor);

            if (previousVisitor.Previous == null) {
                return defineItemExpression;
            }

            foreach (var previousNodePair in previousVisitor.Previous) {
                var previousNode = previousNodePair.Second;
                var matchRecogPrevNode = new ExprPreviousMatchRecognizeNode();

                if (previousNodePair.Second.ChildNodes.Length == 1) {
                    matchRecogPrevNode.AddChildNode(previousNode.ChildNodes[0]);
                    matchRecogPrevNode.AddChildNode(new ExprConstantNodeImpl(1));
                }
                else if (previousNodePair.Second.ChildNodes.Length == 2) {
                    var first = previousNode.ChildNodes[0];
                    var second = previousNode.ChildNodes[1];
                    if (first is ExprConstantNode && !(second is ExprConstantNode)) {
                        matchRecogPrevNode.AddChildNode(second);
                        matchRecogPrevNode.AddChildNode(first);
                    }
                    else if (!(first is ExprConstantNode) && second is ExprConstantNode) {
                        matchRecogPrevNode.AddChildNode(first);
                        matchRecogPrevNode.AddChildNode(second);
                    }
                    else {
                        throw new ExprValidationException("PREV operator requires a constant index");
                    }
                }

                if (previousNodePair.First == null) {
                    defineItemExpression = matchRecogPrevNode;
                }
                else {
                    ExprNodeUtilityModify.ReplaceChildNode(
                        previousNodePair.First,
                        previousNodePair.Second,
                        matchRecogPrevNode);
                }

                // store in a list per index such that we can consolidate this into a single buffer
                var index = matchRecogPrevNode.ConstantIndexNumber;
                var callbackList = previousNodes.Get(index);
                if (callbackList == null) {
                    callbackList = new List<ExprPreviousMatchRecognizeNode>();
                    previousNodes.Put(index, callbackList);
                }

                callbackList.Add(matchRecogPrevNode);
            }

            return defineItemExpression;
        }

        private static StreamTypeService BuildDefineStreamTypeServiceDefine(
            int defineNum,
            LinkedHashMap<string, Pair<int, bool>> variableStreams,
            MatchRecognizeDefineItem defineItem,
            IDictionary<string, ISet<string>> visibilityByIdentifier,
            EventType parentViewType,
            StatementRawInfo statementRawInfo,
            StatementCompileTimeServices services)
        {
            if (!variableStreams.ContainsKey(defineItem.Identifier)) {
                throw new ExprValidationException("Variable '" + defineItem.Identifier + "' does not occur in pattern");
            }

            var streamNamesDefine = new string[variableStreams.Count + 1];
            var typesDefine = new EventType[variableStreams.Count + 1];
            var isIStreamOnly = new bool[variableStreams.Count + 1];
            CompatExtensions.Fill(isIStreamOnly, true);

            var streamNumDefine = variableStreams.Get(defineItem.Identifier).First;
            streamNamesDefine[streamNumDefine] = defineItem.Identifier;
            typesDefine[streamNumDefine] = parentViewType;

            // add visible single-value
            var visibles = visibilityByIdentifier.Get(defineItem.Identifier);
            var hasVisibleMultimatch = false;
            if (visibles != null) {
                foreach (var visible in visibles) {
                    var def = variableStreams.Get(visible);
                    if (!def.Second) {
                        streamNamesDefine[def.First] = visible;
                        typesDefine[def.First] = parentViewType;
                    }
                    else {
                        hasVisibleMultimatch = true;
                    }
                }
            }

            // compile multi-matching event type (in last position), if any are used
            if (hasVisibleMultimatch) {
                IDictionary<string, object> multievent = new LinkedHashMap<string, object>();
                foreach (var entry in variableStreams) {
                    var identifier = entry.Key;
                    if (entry.Value.Second) {
                        if (visibles.Contains(identifier)) {
                            multievent.Put(identifier, new[] {parentViewType});
                        }
                        else {
                            multievent.Put("esper_matchrecog_internal", null);
                        }
                    }
                }

                var multimatchTypeName =
                    services.EventTypeNameGeneratorStatement.GetAnonymousRowrecogMultimatchDefineName(defineNum);
                var multimatchTypeMetadata = new EventTypeMetadata(
                    multimatchTypeName,
                    statementRawInfo.ModuleName,
                    EventTypeTypeClass.MATCHRECOGDERIVED,
                    EventTypeApplicationType.OBJECTARR,
                    NameAccessModifier.TRANSIENT,
                    EventTypeBusModifier.NONBUS,
                    false,
                    EventTypeIdPair.Unassigned());
                var multimatchEventType = BaseNestableEventUtil.MakeOATypeCompileTime(
                    multimatchTypeMetadata,
                    multievent,
                    null,
                    null,
                    null,
                    null,
                    services.BeanEventTypeFactoryPrivate,
                    services.EventTypeCompileTimeResolver);

                typesDefine[typesDefine.Length - 1] = multimatchEventType;
                streamNamesDefine[streamNamesDefine.Length - 1] = multimatchEventType.Name;
            }

            return new StreamTypeServiceImpl(typesDefine, streamNamesDefine, isIStreamOnly, false, true);
        }
    }
} // end of namespace