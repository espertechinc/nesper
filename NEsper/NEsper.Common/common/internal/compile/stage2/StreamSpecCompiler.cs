///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.annotation;
using com.espertech.esper.common.client.meta;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.compile.multikey;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.epl.contained;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.time.eval;
using com.espertech.esper.common.@internal.epl.expression.time.node;
using com.espertech.esper.common.@internal.epl.expression.visitor;
using com.espertech.esper.common.@internal.epl.pattern.core;
using com.espertech.esper.common.@internal.epl.pattern.everydistinct;
using com.espertech.esper.common.@internal.epl.pattern.filter;
using com.espertech.esper.common.@internal.epl.pattern.followedby;
using com.espertech.esper.common.@internal.epl.pattern.guard;
using com.espertech.esper.common.@internal.epl.pattern.matchuntil;
using com.espertech.esper.common.@internal.epl.pattern.observer;
using com.espertech.esper.common.@internal.epl.streamtype;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.serde.compiletime.eventtype;
using com.espertech.esper.common.@internal.settings;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;

namespace com.espertech.esper.common.@internal.compile.stage2
{
    public partial class StreamSpecCompiler
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(StreamSpecCompiler));

        public static StreamSpecCompiledDesc Compile(
            StreamSpecRaw spec,
            ISet<string> eventTypeReferences,
            bool isInsertInto,
            bool isJoin,
            bool isContextDeclaration,
            bool isOnTrigger,
            string optionalStreamName,
            int streamNum,
            StatementRawInfo statementRawInfo,
            StatementCompileTimeServices services)
        {
            if (spec is DBStatementStreamSpec dbStatementStreamSpec) {
                return new StreamSpecCompiledDesc(dbStatementStreamSpec, EmptyList<StmtClassForgeableFactory>.Instance);
            }

            if (spec is FilterStreamSpecRaw filterStreamSpecRaw) {
                return CompileFilter(
                    filterStreamSpecRaw,
                    isInsertInto,
                    isJoin,
                    isContextDeclaration,
                    isOnTrigger,
                    optionalStreamName,
                    statementRawInfo,
                    services);
            }

            if (spec is PatternStreamSpecRaw patternStreamSpecRaw) {
                return CompilePattern(
                    patternStreamSpecRaw,
                    eventTypeReferences,
                    isInsertInto,
                    isJoin,
                    isContextDeclaration,
                    isOnTrigger,
                    optionalStreamName,
                    streamNum,
                    statementRawInfo,
                    services);
            }

            if (spec is MethodStreamSpec methodStreamSpec) {
                return new StreamSpecCompiledDesc(
                    CompileMethod(methodStreamSpec),
                    EmptyList<StmtClassForgeableFactory>.Instance);
            }

            throw new IllegalStateException("Unrecognized stream spec " + spec);
        }

        public static StreamSpecCompiledDesc CompileFilter(
            FilterStreamSpecRaw streamSpec,
            bool isInsertInto,
            bool isJoin,
            bool isContextDeclaration,
            bool isOnTrigger,
            string optionalStreamName,
            StatementRawInfo statementRawInfo,
            StatementCompileTimeServices services)
        {
            // Determine the event type
            var rawFilterSpec = streamSpec.RawFilterSpec;
            var eventTypeName = rawFilterSpec.EventTypeName;

            var table = services.TableCompileTimeResolver.Resolve(eventTypeName);
            if (table != null) {
                if (streamSpec.ViewSpecs != null && streamSpec.ViewSpecs.Length > 0) {
                    throw new ExprValidationException("Views are not supported with tables");
                }

                if (streamSpec.RawFilterSpec.OptionalPropertyEvalSpec != null) {
                    throw new ExprValidationException("Contained-event expressions are not supported with tables");
                }

                StreamTypeService streamTypeService = new StreamTypeServiceImpl(
                    new[] {table.InternalEventType},
                    new[] {optionalStreamName},
                    new[] {true},
                    false,
                    false);
                FilterSpecValidatedDesc desc = FilterSpecCompiler.ValidateAllowSubquery(
                    ExprNodeOrigin.FILTER,
                    rawFilterSpec.FilterExpressions,
                    streamTypeService,
                    null,
                    null,
                    statementRawInfo,
                    services);
                TableQueryStreamSpec tableStreamSpec = new TableQueryStreamSpec(
                    streamSpec.OptionalStreamName,
                    streamSpec.ViewSpecs,
                    streamSpec.Options,
                    table,
                    desc.Expressions);
                return new StreamSpecCompiledDesc(
                    tableStreamSpec,
                    desc.AdditionalForgeables);
            }

            // Could be a named window
            var namedWindowInfo = services.NamedWindowCompileTimeResolver.Resolve(eventTypeName);
            if (namedWindowInfo != null) {
                StreamTypeService streamTypeService = new StreamTypeServiceImpl(
                    new[] {namedWindowInfo.EventType},
                    new[] {optionalStreamName},
                    new[] {true},
                    false,
                    false);

                FilterSpecValidatedDesc validated = FilterSpecCompiler.ValidateAllowSubquery(
                    ExprNodeOrigin.FILTER,
                    rawFilterSpec.FilterExpressions,
                    streamTypeService,
                    null,
                    null,
                    statementRawInfo,
                    services);

                PropertyEvaluatorForge optionalPropertyEvaluator = null;
                if (rawFilterSpec.OptionalPropertyEvalSpec != null) {
                    optionalPropertyEvaluator = PropertyEvaluatorForgeFactory.MakeEvaluator(
                        rawFilterSpec.OptionalPropertyEvalSpec,
                        namedWindowInfo.EventType,
                        streamSpec.OptionalStreamName,
                        statementRawInfo,
                        services);
                }

                NamedWindowConsumerStreamSpec consumer = new NamedWindowConsumerStreamSpec(
                    namedWindowInfo,
                    streamSpec.OptionalStreamName,
                    streamSpec.ViewSpecs,
                    validated.Expressions,
                    streamSpec.Options,
                    optionalPropertyEvaluator);

                return new StreamSpecCompiledDesc(consumer, validated.AdditionalForgeables);
            }

            var eventType = ResolveTypeName(eventTypeName, services.EventTypeCompileTimeResolver);

            // Validate all nodes, make sure each returns a boolean and types are good;
            // Also decompose all AND super nodes into individual expressions
            StreamTypeService streamTypeServiceX = new StreamTypeServiceImpl(
                new[] {eventType},
                new[] {streamSpec.OptionalStreamName},
                new[] {true},
                false,
                false);

            FilterSpecCompiledDesc desc = FilterSpecCompiler.MakeFilterSpec(
                eventType, 
                eventTypeName,
                rawFilterSpec.FilterExpressions,
                rawFilterSpec.OptionalPropertyEvalSpec,
                null, null, null, // no tags
                streamTypeServiceX,
                streamSpec.OptionalStreamName,
                statementRawInfo,
                services);
            
            FilterStreamSpecCompiled compiled = new FilterStreamSpecCompiled(
                desc.FilterSpecCompiled,
                streamSpec.ViewSpecs,
                streamSpec.OptionalStreamName,
                streamSpec.Options);

            return new StreamSpecCompiledDesc(compiled, desc.AdditionalForgeables);
        }

        public static StreamSpecCompiledDesc CompilePattern(
            PatternStreamSpecRaw streamSpecRaw,
            ISet<string> eventTypeReferences,
            bool isInsertInto,
            bool isJoin,
            bool isContextDeclaration,
            bool isOnTrigger,
            string optionalStreamName,
            int streamNum,
            StatementRawInfo statementRawInfo,
            StatementCompileTimeServices services)
        {
            return CompilePatternWTags(
                streamSpecRaw,
                eventTypeReferences,
                isInsertInto,
                null,
                null,
                isJoin,
                isContextDeclaration,
                isOnTrigger,
                streamNum,
                statementRawInfo,
                services);
        }

        public static StreamSpecCompiledDesc CompilePatternWTags(
            PatternStreamSpecRaw streamSpecRaw,
            ISet<string> eventTypeReferences,
            bool isInsertInto,
            MatchEventSpec tags,
            ISet<string> priorAllTags,
            bool isJoin,
            bool isContextDeclaration,
            bool isOnTrigger,
            int streamNum,
            StatementRawInfo statementRawInfo,
            StatementCompileTimeServices services)
        {
            // validate
            if ((streamSpecRaw.IsSuppressSameEventMatches || streamSpecRaw.IsDiscardPartialsOnMatch) &&
                (isJoin || isContextDeclaration || isOnTrigger)) {
                throw new ExprValidationException(
                    "Discard-partials and suppress-matches is not supported in a joins, context declaration and on-action");
            }

            if (tags == null) {
                tags = new MatchEventSpec();
            }

            var nodeStack = new Stack<EvalForgeNode>();

            // determine ordered tags
            ISet<string> allTagNamesOrdered = FilterSpecCompilerTagUtil.GetAllTagNamesOrdered(priorAllTags, streamSpecRaw.EvalForgeNode);

            // construct root : assigns factory node ids
            var top = streamSpecRaw.EvalForgeNode;
            var root = new EvalRootForgeNode(top, statementRawInfo.Annotations);
            var additionalForgeables = new List<StmtClassForgeableFactory>();
            
            RecursiveCompile(
                top,
                tags,
                nodeStack,
                allTagNamesOrdered,
                streamNum,
                additionalForgeables,
                statementRawInfo,
                services);

            var hook = (PatternCompileHook) ImportUtil.GetAnnotationHook(
                statementRawInfo.Annotations,
                HookType.INTERNAL_PATTERNCOMPILE,
                typeof(PatternCompileHook),
                services.ImportServiceCompileTime);
            if (hook != null) {
                hook.Pattern(root);
            }

            PatternStreamSpecCompiled compiled = new PatternStreamSpecCompiled(
                root,
                tags.TaggedEventTypes,
                tags.ArrayEventTypes,
                allTagNamesOrdered,
                streamSpecRaw.ViewSpecs,
                streamSpecRaw.OptionalStreamName,
                streamSpecRaw.Options,
                streamSpecRaw.IsSuppressSameEventMatches,
                streamSpecRaw.IsDiscardPartialsOnMatch);

            return new StreamSpecCompiledDesc(compiled, additionalForgeables);
        }

        private static void RecursiveCompile(
            EvalForgeNode evalNode,
            MatchEventSpec tags,
            Stack<EvalForgeNode> parentNodeStack,
            ISet<string> allTagNamesOrdered,
            int streamNum,
            IList<StmtClassForgeableFactory> additionalForgeables,
            StatementRawInfo statementRawInfo,
            StatementCompileTimeServices services)
        {
            parentNodeStack.Push(evalNode);
            foreach (var child in evalNode.ChildNodes) {
                RecursiveCompile(
                    child,
                    tags,
                    parentNodeStack,
                    allTagNamesOrdered,
                    streamNum,
                    additionalForgeables,
                    statementRawInfo,
                    services);
            }

            parentNodeStack.Pop();

            IDictionary<string, Pair<EventType, string>> newTaggedEventTypes = null;
            IDictionary<string, Pair<EventType, string>> newArrayEventTypes = null;

            if (evalNode is EvalFilterForgeNode) {
                var filterNode = (EvalFilterForgeNode) evalNode;
                var eventName = filterNode.RawFilterSpec.EventTypeName;
                if (services.TableCompileTimeResolver.Resolve(eventName) != null) {
                    throw new ExprValidationException("Tables cannot be used in pattern filter atoms");
                }

                var resolvedEventType = ResolveTypeName(eventName, services.EventTypeCompileTimeResolver);
                var finalEventType = resolvedEventType;
                var optionalTag = filterNode.EventAsName;
                var isPropertyEvaluation = false;
                var isParentMatchUntil = IsParentMatchUntil(evalNode, parentNodeStack);

                // obtain property event type, if final event type is properties
                if (filterNode.RawFilterSpec.OptionalPropertyEvalSpec != null) {
                    var optionalPropertyEvaluator = PropertyEvaluatorForgeFactory.MakeEvaluator(
                        filterNode.RawFilterSpec.OptionalPropertyEvalSpec,
                        resolvedEventType,
                        filterNode.EventAsName,
                        statementRawInfo,
                        services);
                    finalEventType = optionalPropertyEvaluator.FragmentEventType;
                    isPropertyEvaluation = true;
                }

                // If a tag was supplied for the type, the tags must stay with this type, i.e. a=BeanA -> b=BeanA -> a=BeanB is a no
                if (optionalTag != null) {
                    var pair = tags.TaggedEventTypes.Get(optionalTag);
                    EventType existingType = null;
                    if (pair != null) {
                        existingType = pair.First;
                    }

                    if (existingType == null) {
                        pair = tags.ArrayEventTypes.Get(optionalTag);
                        if (pair != null) {
                            throw new ExprValidationException(
                                "Tag '" +
                                optionalTag +
                                "' for event '" +
                                eventName +
                                "' used in the repeat-until operator cannot also appear in other filter expressions");
                        }
                    }

                    if (existingType != null && existingType != finalEventType) {
                        throw new ExprValidationException(
                            "Tag '" +
                            optionalTag +
                            "' for event '" +
                            eventName +
                            "' has already been declared for events of type " +
                            existingType.UnderlyingType.Name);
                    }

                    pair = new Pair<EventType, string>(finalEventType, eventName);

                    // add tagged type
                    if (isPropertyEvaluation || isParentMatchUntil) {
                        newArrayEventTypes = new LinkedHashMap<string, Pair<EventType, string>>();
                        newArrayEventTypes.Put(optionalTag, pair);
                    }
                    else {
                        newTaggedEventTypes = new LinkedHashMap<string, Pair<EventType, string>>();
                        newTaggedEventTypes.Put(optionalTag, pair);
                    }

                    var forgeables = SerdeEventTypeUtility.Plan(
                        pair.First,
                        statementRawInfo,
                        services.SerdeEventTypeRegistry,
                        services.SerdeResolver);
                    additionalForgeables.AddAll(forgeables);
                }

                // For this filter, filter types are all known tags at this time,
                // and additionally stream 0 (self) is our event type.
                // Stream type service allows resolution by property name event if that name appears in other tags.
                // by defaulting to stream zero.
                // Stream zero is always the current event type, all others follow the order of the map (stream 1 to N).
                var selfStreamName = optionalTag;
                if (selfStreamName == null) {
                    selfStreamName = "s_" + UuidGenerator.Generate();
                }

                var filterTypes = new LinkedHashMap<string, Pair<EventType, string>>();
                var typePair = new Pair<EventType, string>(finalEventType, eventName);
                filterTypes.Put(selfStreamName, typePair);
                filterTypes.PutAll(tags.TaggedEventTypes);

                // for the filter, specify all tags used
                var filterTaggedEventTypes = new LinkedHashMap<string, Pair<EventType, string>>(tags.TaggedEventTypes);
                filterTaggedEventTypes.Remove(optionalTag);

                // handle array tags (match-until clause)
                IDictionary<string, Pair<EventType, string>> arrayCompositeEventTypes = null;
                if (tags.ArrayEventTypes != null && !tags.ArrayEventTypes.IsEmpty()) {
                    arrayCompositeEventTypes = new LinkedHashMap<string, Pair<EventType, string>>();

                    foreach (var entry in tags.ArrayEventTypes) {
                        var specificArrayType = new LinkedHashMap<string, Pair<EventType, string>>();
                        specificArrayType.Put(entry.Key, entry.Value);

                        var eventTypeName = services.EventTypeNameGeneratorStatement.GetAnonymousPatternNameWTag(
                            streamNum,
                            evalNode.FactoryNodeId,
                            entry.Key);
                        var mapProps = GetMapProperties(
                            Collections.GetEmptyMap<string, Pair<EventType, string>>(),
                            specificArrayType);
                        var metadata = new EventTypeMetadata(
                            eventTypeName,
                            statementRawInfo.ModuleName,
                            EventTypeTypeClass.PATTERNDERIVED,
                            EventTypeApplicationType.MAP,
                            NameAccessModifier.TRANSIENT,
                            EventTypeBusModifier.NONBUS,
                            false,
                            EventTypeIdPair.Unassigned());
                        var mapEventType = BaseNestableEventUtil.MakeMapTypeCompileTime(
                            metadata,
                            mapProps,
                            null,
                            null,
                            null,
                            null,
                            services.BeanEventTypeFactoryPrivate,
                            services.EventTypeCompileTimeResolver);
                        services.EventTypeCompileTimeRegistry.NewType(mapEventType);

                        var tag = entry.Key;
                        if (!filterTypes.ContainsKey(tag)) {
                            var pair = new Pair<EventType, string>(mapEventType, tag);
                            filterTypes.Put(tag, pair);
                            arrayCompositeEventTypes.Put(tag, pair);
                        }

                        var forgeables = SerdeEventTypeUtility.Plan(
                            mapEventType,
                            statementRawInfo,
                            services.SerdeEventTypeRegistry,
                            services.SerdeResolver);
                        additionalForgeables.AddAll(forgeables);
                    }
                }

                StreamTypeService streamTypeService = new StreamTypeServiceImpl(filterTypes, true, false);
                var exprNodes = filterNode.RawFilterSpec.FilterExpressions;

                FilterSpecCompiledDesc compiled = FilterSpecCompiler.MakeFilterSpec(
                    resolvedEventType,
                    eventName,
                    exprNodes,
                    filterNode.RawFilterSpec.OptionalPropertyEvalSpec,
                    filterTaggedEventTypes,
                    arrayCompositeEventTypes,
                    allTagNamesOrdered,
                    streamTypeService,
                    null,
                    statementRawInfo,
                    services);
                
                filterNode.FilterSpec = compiled.FilterSpecCompiled;
                additionalForgeables.AddAll(compiled.AdditionalForgeables);
            }
            else if (evalNode is EvalObserverForgeNode) {
                var observerNode = (EvalObserverForgeNode) evalNode;
                try {
                    var observerForge =
                        services.PatternResolutionService.Create(observerNode.PatternObserverSpec);

                    var streamTypeService = GetStreamTypeService(
                        tags.TaggedEventTypes,
                        tags.ArrayEventTypes,
                        observerNode,
                        streamNum,
                        statementRawInfo,
                        services);
                    var validationContext = new ExprValidationContextBuilder(
                        streamTypeService,
                        statementRawInfo,
                        services).Build();
                    var validated = ValidateExpressions(
                        ExprNodeOrigin.PATTERNOBSERVER,
                        observerNode.PatternObserverSpec.ObjectParameters,
                        validationContext);

                    var convertor = new MatchedEventConvertorForge(
                        tags.TaggedEventTypes,
                        tags.ArrayEventTypes,
                        allTagNamesOrdered,
                        null,
                        false);

                    observerNode.ObserverFactory = observerForge;
                    observerForge.SetObserverParameters(validated, convertor, validationContext);
                }
                catch (ObserverParameterException e) {
                    throw new ExprValidationException(
                        "Invalid parameter for pattern observer '" +
                        observerNode.ToPrecedenceFreeEPL() +
                        "': " +
                        e.Message,
                        e);
                }
                catch (PatternObjectException e) {
                    throw new ExprValidationException(
                        "Failed to resolve pattern observer '" + observerNode.ToPrecedenceFreeEPL() + "': " + e.Message,
                        e);
                }
            }
            else if (evalNode is EvalGuardForgeNode) {
                var guardNode = (EvalGuardForgeNode) evalNode;
                try {
                    var guardForge = services.PatternResolutionService.Create(guardNode.PatternGuardSpec);

                    var streamTypeService = GetStreamTypeService(
                        tags.TaggedEventTypes,
                        tags.ArrayEventTypes,
                        guardNode,
                        streamNum,
                        statementRawInfo,
                        services);
                    var validationContext = new ExprValidationContextBuilder(
                        streamTypeService,
                        statementRawInfo,
                        services).Build();
                    var validated = ValidateExpressions(
                        ExprNodeOrigin.PATTERNGUARD,
                        guardNode.PatternGuardSpec.ObjectParameters,
                        validationContext);

                    var convertor = new MatchedEventConvertorForge(
                        tags.TaggedEventTypes,
                        tags.ArrayEventTypes,
                        allTagNamesOrdered,
                        null,
                        false);

                    guardNode.GuardForge = guardForge;
                    guardForge.SetGuardParameters(validated, convertor, services);
                }
                catch (GuardParameterException e) {
                    throw new ExprValidationException(
                        "Invalid parameter for pattern guard '" + guardNode.ToPrecedenceFreeEPL() + "': " + e.Message,
                        e);
                }
                catch (PatternObjectException e) {
                    throw new ExprValidationException(
                        "Failed to resolve pattern guard '" + guardNode.ToPrecedenceFreeEPL() + "': " + e.Message,
                        e);
                }
            }
            else if (evalNode is EvalEveryDistinctForgeNode) {
                var distinctNode = (EvalEveryDistinctForgeNode) evalNode;
                var matchEventFromChildNodes = AnalyzeMatchEvent(distinctNode);
                var streamTypeService = GetStreamTypeService(
                    matchEventFromChildNodes.TaggedEventTypes,
                    matchEventFromChildNodes.ArrayEventTypes,
                    distinctNode,
                    streamNum,
                    statementRawInfo,
                    services);
                var validationContext =
                    new ExprValidationContextBuilder(streamTypeService, statementRawInfo, services).Build();
                IList<ExprNode> validated;
                try {
                    validated = ValidateExpressions(
                        ExprNodeOrigin.PATTERNEVERYDISTINCT,
                        distinctNode.Expressions,
                        validationContext);
                }
                catch (ExprValidationPropertyException ex) {
                    throw new ExprValidationPropertyException(
                        ex.Message +
                        ", every-distinct requires that all properties resolve from sub-expressions to the every-distinct",
                        ex.InnerException);
                }

                var convertor = new MatchedEventConvertorForge(
                    matchEventFromChildNodes.TaggedEventTypes,
                    matchEventFromChildNodes.ArrayEventTypes,
                    allTagNamesOrdered,
                    null,
                    false);

                distinctNode.Convertor = convertor;

                // Determine whether some expressions are constants or time period
                IList<ExprNode> distinctExpressions = new List<ExprNode>();
                TimePeriodComputeForge timePeriodComputeForge = null;
                ExprNode expiryTimeExp = null;
                var count = -1;
                var last = validated.Count - 1;
                foreach (var expr in validated) {
                    count++;
                    if (count == last && expr is ExprTimePeriod) {
                        expiryTimeExp = expr;
                        var timePeriodExpr = (ExprTimePeriod) expiryTimeExp;
                        timePeriodComputeForge = timePeriodExpr.TimePeriodComputeForge;
                    }
                    else if (expr.Forge.ForgeConstantType.IsCompileTimeConstant) {
                        if (count == last) {
                            var value = expr.Forge.ExprEvaluator.Evaluate(null, true, null);
                            if (!value.IsNumber()) {
                                throw new ExprValidationException(
                                    "Invalid parameter for every-distinct, expected number of seconds constant (constant not considered for distinct)");
                            }

                            var secondsExpire = expr.Forge.ExprEvaluator.Evaluate(null, true, null);
                            var timeExpire = secondsExpire == null
                                ? (long?) null
                                : (long?) services.ImportServiceCompileTime.TimeAbacus.DeltaForSecondsNumber(
                                    secondsExpire);
                            if (timeExpire != null && timeExpire > 0) {
                                timePeriodComputeForge = new TimePeriodComputeConstGivenDeltaForge(timeExpire.Value);
                                expiryTimeExp = expr;
                            }
                            else {
                                Log.Warn(
                                    "Invalid seconds-expire " +
                                    timeExpire +
                                    " for " +
                                    ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceSafe(expr));
                            }
                        }
                        else {
                            Log.Warn(
                                "Every-distinct node utilizes an expression returning a constant value, please check expression '" +
                                ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceSafe(expr) +
                                "', not adding expression to distinct-value expression list");
                        }
                    }
                    else {
                        distinctExpressions.Add(expr);
                    }
                }

                if (distinctExpressions.IsEmpty()) {
                    throw new ExprValidationException(
                        "Every-distinct node requires one or more distinct-value expressions that each return non-constant result values");
                }

                var multiKeyPlan = MultiKeyPlanner.PlanMultiKey(distinctExpressions.ToArray(), false, statementRawInfo, services.SerdeResolver);
                distinctNode.SetDistinctExpressions(distinctExpressions, multiKeyPlan.ClassRef, timePeriodComputeForge, expiryTimeExp);
                additionalForgeables.AddAll(multiKeyPlan.MultiKeyForgeables);
            }
            else if (evalNode is EvalMatchUntilForgeNode) {
                var matchUntilNode = (EvalMatchUntilForgeNode) evalNode;

                // compile bounds expressions, if any
                var untilMatchEventSpec = new MatchEventSpec(tags.TaggedEventTypes, tags.ArrayEventTypes);
                var streamTypeService = GetStreamTypeService(
                    untilMatchEventSpec.TaggedEventTypes,
                    untilMatchEventSpec.ArrayEventTypes,
                    matchUntilNode,
                    streamNum,
                    statementRawInfo,
                    services);
                var validationContext =
                    new ExprValidationContextBuilder(streamTypeService, statementRawInfo, services).Build();

                var lower = ValidateBounds(matchUntilNode.LowerBounds, validationContext);
                matchUntilNode.LowerBounds = lower;

                var upper = ValidateBounds(matchUntilNode.UpperBounds, validationContext);
                matchUntilNode.UpperBounds = upper;

                var single = ValidateBounds(matchUntilNode.SingleBound, validationContext);
                matchUntilNode.SingleBound = single;

                bool tightlyBound;
                if (matchUntilNode.SingleBound != null) {
                    ValidateMatchUntil(matchUntilNode.SingleBound, matchUntilNode.SingleBound, false);
                    tightlyBound = true;
                }
                else {
                    var allowZeroLowerBounds = matchUntilNode.LowerBounds != null && matchUntilNode.UpperBounds != null;
                    tightlyBound = ValidateMatchUntil(
                        matchUntilNode.LowerBounds,
                        matchUntilNode.UpperBounds,
                        allowZeroLowerBounds);
                }

                if (matchUntilNode.SingleBound == null && !tightlyBound && matchUntilNode.ChildNodes.Count < 2) {
                    throw new ExprValidationException("Variable bounds repeat operator requires an until-expression");
                }

                var convertor = new MatchedEventConvertorForge(
                    untilMatchEventSpec.TaggedEventTypes,
                    untilMatchEventSpec.ArrayEventTypes,
                    allTagNamesOrdered,
                    null,
                    false);

                matchUntilNode.Convertor = convertor;

                // compile new tag lists
                ISet<string> arrayTags = null;
                var matchUntilAnalysisResult = EvalNodeUtil.RecursiveAnalyzeChildNodes(matchUntilNode.ChildNodes[0]);
                foreach (var filterNode in matchUntilAnalysisResult.FilterNodes) {
                    var optionalTag = filterNode.EventAsName;
                    if (optionalTag != null) {
                        if (arrayTags == null) {
                            arrayTags = new HashSet<string>();
                        }

                        arrayTags.Add(optionalTag);
                    }
                }

                if (arrayTags != null) {
                    foreach (var arrayTag in arrayTags) {
                        if (!tags.ArrayEventTypes.ContainsKey(arrayTag)) {
                            tags.ArrayEventTypes.Put(arrayTag, tags.TaggedEventTypes.Get(arrayTag));
                            tags.TaggedEventTypes.Remove(arrayTag);
                        }
                    }
                }

                matchUntilNode.TagsArrayedSet = GetIndexesForTags(allTagNamesOrdered, arrayTags);
            }
            else if (evalNode is EvalFollowedByForgeNode) {
                var followedByNode = (EvalFollowedByForgeNode) evalNode;
                StreamTypeService streamTypeService = new StreamTypeServiceImpl(false);
                var validationContext =
                    new ExprValidationContextBuilder(streamTypeService, statementRawInfo, services).Build();

                if (followedByNode.OptionalMaxExpressions != null) {
                    IList<ExprNode> validated = new List<ExprNode>();
                    foreach (var maxExpr in followedByNode.OptionalMaxExpressions) {
                        if (maxExpr == null) {
                            validated.Add(null);
                        }
                        else {
                            var visitor = new ExprNodeSummaryVisitor();
                            maxExpr.Accept(visitor);
                            if (!visitor.IsPlain) {
                                var errorMessage = "Invalid maximum expression in followed-by, " +
                                                   visitor.Message +
                                                   " are not allowed within the expression";
                                Log.Error(errorMessage);
                                throw new ExprValidationException(errorMessage);
                            }

                            var validatedExpr = ExprNodeUtilityValidate.GetValidatedSubtree(
                                ExprNodeOrigin.FOLLOWEDBYMAX,
                                maxExpr,
                                validationContext);
                            validated.Add(validatedExpr);
                            var returnType = validatedExpr.Forge.EvaluationType;
                            if (returnType == null || !returnType.IsNumeric()) {
                                var message =
                                    "Invalid maximum expression in followed-by, the expression must return an integer value";
                                throw new ExprValidationException(message);
                            }
                        }
                    }

                    followedByNode.OptionalMaxExpressions = validated;
                }
            }

            if (newTaggedEventTypes != null) {
                tags.TaggedEventTypes.PutAll(newTaggedEventTypes);
            }

            if (newArrayEventTypes != null) {
                tags.ArrayEventTypes.PutAll(newArrayEventTypes);
            }
        }

        private static ExprNode ValidateBounds(
            ExprNode bounds,
            ExprValidationContext validationContext)
        {
            var message = "Match-until bounds value expressions must return a numeric value";
            if (bounds != null) {
                var validated = ExprNodeUtilityValidate.GetValidatedSubtree(
                    ExprNodeOrigin.PATTERNMATCHUNTILBOUNDS,
                    bounds,
                    validationContext);
                var returnType = validated.Forge.EvaluationType;
                if (returnType == null || !returnType.IsNumeric()) {
                    throw new ExprValidationException(message);
                }

                return validated;
            }

            return null;
        }

        private static int[] GetIndexesForTags(
            ISet<string> allTagNamesOrdered,
            ISet<string> arrayTags)
        {
            if (arrayTags == null || arrayTags.IsEmpty()) {
                return new int[0];
            }

            var indexes = new int[arrayTags.Count];
            var count = 0;
            foreach (var arrayTag in arrayTags) {
                var found = FilterSpecCompilerTagUtil.FindTagNumber(arrayTag, allTagNamesOrdered);
                indexes[count] = found;
                count++;
            }

            return indexes;
        }

        private static bool IsParentMatchUntil(
            EvalForgeNode currentNode,
            Stack<EvalForgeNode> parentNodeStack)
        {
            if (parentNodeStack.IsEmptyCollection()) {
                return false;
            }

            foreach (var deepParent in parentNodeStack) {
                if (deepParent is EvalMatchUntilForgeNode) {
                    var matchUntilFactoryNode = (EvalMatchUntilForgeNode) deepParent;
                    if (matchUntilFactoryNode.ChildNodes[0] == currentNode) {
                        return true;
                    }
                }
            }

            return false;
        }

        private static MatchEventSpec AnalyzeMatchEvent(EvalForgeNode relativeNode)
        {
            var taggedEventTypes = new LinkedHashMap<string, Pair<EventType, string>>();
            var arrayEventTypes = new LinkedHashMap<string, Pair<EventType, string>>();

            // Determine all the filter nodes used in the pattern
            var evalNodeAnalysisResult = EvalNodeUtil.RecursiveAnalyzeChildNodes(relativeNode);

            // collect all filters underneath
            foreach (var filterNode in evalNodeAnalysisResult.FilterNodes) {
                var optionalTag = filterNode.EventAsName;
                if (optionalTag != null) {
                    taggedEventTypes.Put(
                        optionalTag,
                        new Pair<EventType, string>(
                            filterNode.FilterSpecCompiled.FilterForEventType,
                            filterNode.FilterSpecCompiled.FilterForEventTypeName));
                }
            }

            // collect those filters under a repeat since they are arrays
            ISet<string> arrayTags = new HashSet<string>();
            foreach (var matchUntilNode in evalNodeAnalysisResult.RepeatNodes) {
                var matchUntilAnalysisResult = EvalNodeUtil.RecursiveAnalyzeChildNodes(matchUntilNode.ChildNodes[0]);
                foreach (var filterNode in matchUntilAnalysisResult.FilterNodes) {
                    var optionalTag = filterNode.EventAsName;
                    if (optionalTag != null) {
                        arrayTags.Add(optionalTag);
                    }
                }
            }

            // for each array tag change collection
            foreach (var arrayTag in arrayTags) {
                if (taggedEventTypes.Get(arrayTag) != null) {
                    arrayEventTypes.Put(arrayTag, taggedEventTypes.Get(arrayTag));
                    taggedEventTypes.Remove(arrayTag);
                }
            }

            return new MatchEventSpec(taggedEventTypes, arrayEventTypes);
        }

        public static StreamSpecCompiled CompileMethod(MethodStreamSpec methodStreamSpec)
        {
            if (!methodStreamSpec.Ident.Equals("method")) {
                throw new ExprValidationException("Expecting keyword 'method', found '" + methodStreamSpec.Ident + "'");
            }

            if (methodStreamSpec.MethodName == null) {
                throw new ExprValidationException("No method name specified for method-based join");
            }

            return methodStreamSpec;
        }

        private static StreamTypeService GetStreamTypeService(
            IDictionary<string, Pair<EventType, string>> taggedEventTypes,
            IDictionary<string, Pair<EventType, string>> arrayEventTypes,
            EvalForgeNode forge,
            int streamNum,
            StatementRawInfo statementRawInfo,
            StatementCompileTimeServices services)
        {
            var filterTypes = new LinkedHashMap<string, Pair<EventType, string>>();
            filterTypes.PutAll(taggedEventTypes);

            // handle array tags (match-until clause)
            if (arrayEventTypes != null) {
                var eventTypeName =
                    services.EventTypeNameGeneratorStatement.GetAnonymousPatternName(streamNum, forge.FactoryNodeId);
                var metadata = new EventTypeMetadata(
                    eventTypeName,
                    statementRawInfo.ModuleName,
                    EventTypeTypeClass.PATTERNDERIVED,
                    EventTypeApplicationType.MAP,
                    NameAccessModifier.TRANSIENT,
                    EventTypeBusModifier.NONBUS,
                    false,
                    EventTypeIdPair.Unassigned());
                var mapProperties = GetMapProperties(
                    new Dictionary<string, Pair<EventType, string>>(),
                    arrayEventTypes);
                var mapEventType = BaseNestableEventUtil.MakeMapTypeCompileTime(
                    metadata,
                    mapProperties,
                    null,
                    null,
                    null,
                    null,
                    services.BeanEventTypeFactoryPrivate,
                    services.EventTypeCompileTimeResolver);
                services.EventTypeCompileTimeRegistry.NewType(mapEventType);

                EventType arrayTagCompositeEventType = mapEventType;
                foreach (var entry in arrayEventTypes) {
                    var tag = entry.Key;
                    if (!filterTypes.ContainsKey(tag)) {
                        var pair = new Pair<EventType, string>(arrayTagCompositeEventType, tag);
                        filterTypes.Put(tag, pair);
                    }
                }
            }

            return new StreamTypeServiceImpl(filterTypes, true, false);
        }

        private static IList<ExprNode> ValidateExpressions(
            ExprNodeOrigin exprNodeOrigin,
            IList<ExprNode> objectParameters,
            ExprValidationContext validationContext)
        {
            if (objectParameters == null) {
                return objectParameters;
            }

            IList<ExprNode> validated = new List<ExprNode>();
            foreach (var node in objectParameters) {
                validated.Add(ExprNodeUtilityValidate.GetValidatedSubtree(exprNodeOrigin, node, validationContext));
            }

            return validated;
        }

        public static EventType ResolveTypeName(
            string eventTypeName,
            EventTypeCompileTimeResolver eventTypeCompileTimeResolver)
        {
            var eventType = eventTypeCompileTimeResolver.GetTypeByName(eventTypeName);
            if (eventType == null) {
                throw new ExprValidationException(
                    "Failed to resolve event type, named window or table by name '" + eventTypeName + "'");
            }

            return eventType;
        }

        private static IDictionary<string, object> GetMapProperties(
            IDictionary<string, Pair<EventType, string>> taggedEventTypes,
            IDictionary<string, Pair<EventType, string>> arrayEventTypes)
        {
            IDictionary<string, object> mapProperties = new LinkedHashMap<string, object>();
            foreach (var entry in taggedEventTypes) {
                mapProperties.Put(entry.Key, entry.Value.First);
            }

            foreach (var entry in arrayEventTypes) {
                mapProperties.Put(entry.Key, new[] {entry.Value.First});
            }

            return mapProperties;
        }

        /// <summary>
        ///     Validate.
        /// </summary>
        /// <param name="lowerBounds">is the lower bounds, or null if none supplied</param>
        /// <param name="upperBounds">is the upper bounds, or null if none supplied</param>
        /// <param name="isAllowLowerZero">true to allow zero value for lower range</param>
        /// <returns>true if closed range of constants and the constants are the same value</returns>
        /// <throws>ExprValidationException validation ex</throws>
        public static bool ValidateMatchUntil(
            ExprNode lowerBounds,
            ExprNode upperBounds,
            bool isAllowLowerZero)
        {
            var isConstants = true;
            object constantLower = null;
            var numericMessage = "Match-until bounds expect a numeric or expression value";
            if (lowerBounds != null && lowerBounds.Forge.ForgeConstantType == ExprForgeConstantType.COMPILETIMECONST) {
                constantLower = lowerBounds.Forge.ExprEvaluator.Evaluate(null, true, null);
                if (constantLower == null || !constantLower.IsNumber()) {
                    throw new ExprValidationException(numericMessage);
                }
            }
            else {
                isConstants = lowerBounds == null;
            }

            object constantUpper = null;
            if (upperBounds != null && upperBounds.Forge.ForgeConstantType == ExprForgeConstantType.COMPILETIMECONST) {
                constantUpper = upperBounds.Forge.ExprEvaluator.Evaluate(null, true, null);
                if (constantUpper == null || !constantUpper.IsNumber()) {
                    throw new ExprValidationException(numericMessage);
                }
            }
            else {
                isConstants = isConstants && upperBounds == null;
            }

            if (!isConstants) {
                return true;
            }

            if (constantLower != null && constantUpper != null) {
                var lower = constantLower.AsInt32();
                var upper = constantUpper.AsInt32();
                if (lower > upper) {
                    throw new ExprValidationException(
                        "Incorrect range specification, lower bounds value '" +
                        lower +
                        "' is higher then higher bounds '" +
                        upper +
                        "'");
                }
            }

            VerifyMatchUntilConstant(constantLower, isAllowLowerZero);
            VerifyMatchUntilConstant(constantUpper, false);

            return constantLower != null && constantUpper != null && constantLower.Equals(constantUpper);
        }

        private static void VerifyMatchUntilConstant(
            object value,
            bool isAllowZero)
        {
            if (value != null) {
                var bound = value.AsInt32();
                if (isAllowZero) {
                    if (bound < 0) {
                        throw new ExprValidationException(
                            "Incorrect range specification, a bounds value of negative value is not allowed");
                    }
                }
                else {
                    if (bound <= 0) {
                        throw new ExprValidationException(
                            "Incorrect range specification, a bounds value of zero or negative value is not allowed");
                    }
                }
            }
        }
    }
} // end of namespace