///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

using com.espertech.esper.client;
using com.espertech.esper.client.annotation;
using com.espertech.esper.collection;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression.time;
using com.espertech.esper.epl.expression.visitor;
using com.espertech.esper.epl.property;
using com.espertech.esper.events;
using com.espertech.esper.filter;
using com.espertech.esper.pattern;
using com.espertech.esper.pattern.guard;
using com.espertech.esper.pattern.observer;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.spec
{
    /// <summary>
    /// Pattern specification in unvalidated, unoptimized form.
    /// </summary>
    [Serializable]
    public class PatternStreamSpecRaw
        : StreamSpecBase
        , StreamSpecRaw
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly EvalFactoryNode _evalFactoryNode;
        private readonly bool _suppressSameEventMatches;
        private readonly bool _discardPartialsOnMatch;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="evalFactoryNode">pattern evaluation node representing pattern statement</param>
        /// <param name="viewSpecs">specifies what view to use to derive data</param>
        /// <param name="optionalStreamName">stream name, or null if none supplied</param>
        /// <param name="streamSpecOptions">additional options, such as unidirectional stream in a join</param>
        /// <param name="suppressSameEventMatches">if set to <c>true</c> [suppress same event matches].</param>
        /// <param name="discardPartialsOnMatch">if set to <c>true</c> [discard partials on match].</param>
        public PatternStreamSpecRaw(
            EvalFactoryNode evalFactoryNode,
            ViewSpec[] viewSpecs,
            string optionalStreamName,
            StreamSpecOptions streamSpecOptions,
            bool suppressSameEventMatches,
            bool discardPartialsOnMatch)
            : base(optionalStreamName, viewSpecs, streamSpecOptions)
        {
            _evalFactoryNode = evalFactoryNode;
            _suppressSameEventMatches = suppressSameEventMatches;
            _discardPartialsOnMatch = discardPartialsOnMatch;
        }

        /// <summary>
        /// Returns the pattern expression evaluation node for the top pattern operator.
        /// </summary>
        /// <value>parent pattern expression node</value>
        public EvalFactoryNode EvalFactoryNode
        {
            get { return _evalFactoryNode; }
        }

        public StreamSpecCompiled Compile(
            StatementContext context,
            ICollection<string> eventTypeReferences,
            bool isInsertInto,
            ICollection<int> assignedTypeNumberStack,
            bool isJoin,
            bool isContextDeclaration,
            bool isOnTrigger,
            string optionalStreamName)
        {
            return CompileInternal(
                context, eventTypeReferences, isInsertInto, assignedTypeNumberStack, null, null, isJoin,
                isContextDeclaration, isOnTrigger);
        }

        public PatternStreamSpecCompiled Compile(
            StatementContext context,
            ICollection<string> eventTypeReferences,
            bool isInsertInto,
            ICollection<int> assignedTypeNumberStack,
            MatchEventSpec priorTags,
            ISet<string> priorAllTags,
            bool isJoin,
            bool isContextDeclaration,
            bool isOnTrigger)
        {
            return CompileInternal(
                context, eventTypeReferences, isInsertInto, assignedTypeNumberStack, priorTags, priorAllTags, isJoin,
                isContextDeclaration, isOnTrigger);
        }

        private PatternStreamSpecCompiled CompileInternal(
            StatementContext context,
            ICollection<string> eventTypeReferences,
            bool isInsertInto,
            ICollection<int> assignedTypeNumberStack,
            MatchEventSpec tags,
            IEnumerable<string> priorAllTags,
            bool isJoin,
            bool isContextDeclaration,
            bool isOnTrigger)
        {
            // validate
            if ((_suppressSameEventMatches || _discardPartialsOnMatch) &&
                (isJoin || isContextDeclaration || isOnTrigger))
            {
                throw new ExprValidationException(
                    "Discard-partials and suppress-matches is not supported in a joins, context declaration and on-action");
            }

            if (tags == null)
            {
                tags = new MatchEventSpec();
            }
            var subexpressionIdStack = new ArrayDeque<int>(assignedTypeNumberStack);
            var evaluatorContextStmt = new ExprEvaluatorContextStatement(context, false);
            var nodeStack = new Stack<EvalFactoryNode>();

            // detemine ordered tags
            var filterFactoryNodes = EvalNodeUtil.RecursiveGetChildNodes(
                _evalFactoryNode, FilterForFilterFactoryNodes.INSTANCE);
            var allTagNamesOrdered = new LinkedHashSet<string>();
            if (priorAllTags != null)
            {
                allTagNamesOrdered.AddAll(priorAllTags);
            }
            foreach (var filterNode in filterFactoryNodes)
            {
                var factory = (EvalFilterFactoryNode)filterNode;
                int tagNumber;
                if (factory.EventAsName != null)
                {
                    if (!allTagNamesOrdered.Contains(factory.EventAsName))
                    {
                        allTagNamesOrdered.Add(factory.EventAsName);
                        tagNumber = allTagNamesOrdered.Count - 1;
                    }
                    else
                    {
                        tagNumber = FindTagNumber(factory.EventAsName, allTagNamesOrdered);
                    }
                    factory.EventAsTagNumber = tagNumber;
                }
            }

            RecursiveCompile(
                _evalFactoryNode, context, evaluatorContextStmt, eventTypeReferences, isInsertInto, tags,
                subexpressionIdStack, nodeStack, allTagNamesOrdered);

            var auditPattern = AuditEnum.PATTERN.GetAudit(context.Annotations);
            var auditPatternInstance = AuditEnum.PATTERNINSTANCES.GetAudit(context.Annotations);
            var compiledEvalFactoryNode = _evalFactoryNode;
            if (context.PatternNodeFactory.IsAuditSupported && (auditPattern != null || auditPatternInstance != null))
            {
                var instanceCount = new EvalAuditInstanceCount();
                compiledEvalFactoryNode = RecursiveAddAuditNode(
                    context.PatternNodeFactory, null, auditPattern != null, auditPatternInstance != null,
                    _evalFactoryNode, instanceCount);
            }

            return new PatternStreamSpecCompiled(
                compiledEvalFactoryNode, tags.TaggedEventTypes, tags.ArrayEventTypes, allTagNamesOrdered, ViewSpecs,
                OptionalStreamName, Options, _suppressSameEventMatches, _discardPartialsOnMatch);
        }

        private static void RecursiveCompile(
            EvalFactoryNode evalNode,
            StatementContext context,
            ExprEvaluatorContext evaluatorContext,
            ICollection<string> eventTypeReferences,
            bool isInsertInto,
            MatchEventSpec tags,
            Deque<int> subexpressionIdStack,
            Stack<EvalFactoryNode> parentNodeStack,
            ICollection<string> allTagNamesOrdered)
        {
            var counter = 0;
            parentNodeStack.Push(evalNode);
            foreach (var child in evalNode.ChildNodes)
            {
                subexpressionIdStack.AddLast(counter++);
                RecursiveCompile(
                    child, context, evaluatorContext, eventTypeReferences, isInsertInto, tags, subexpressionIdStack,
                    parentNodeStack, allTagNamesOrdered);
                subexpressionIdStack.RemoveLast();
            }
            parentNodeStack.Pop();

            LinkedHashMap<string, Pair<EventType, string>> newTaggedEventTypes = null;
            LinkedHashMap<string, Pair<EventType, string>> newArrayEventTypes = null;

            if (evalNode is EvalFilterFactoryNode)
            {
                var filterNode = (EvalFilterFactoryNode)evalNode;
                var eventName = filterNode.RawFilterSpec.EventTypeName;
                if (context.TableService.GetTableMetadata(eventName) != null)
                {
                    throw new ExprValidationException("Tables cannot be used in pattern filter atoms");
                }

                var resolvedEventType = FilterStreamSpecRaw.ResolveType(
                    context.EngineURI, eventName, context.EventAdapterService, context.PlugInTypeResolutionURIs);
                var finalEventType = resolvedEventType;
                var optionalTag = filterNode.EventAsName;
                var isPropertyEvaluation = false;
                var isParentMatchUntil = IsParentMatchUntil(evalNode, parentNodeStack);

                // obtain property event type, if final event type is properties
                if (filterNode.RawFilterSpec.OptionalPropertyEvalSpec != null)
                {
                    var optionalPropertyEvaluator =
                        PropertyEvaluatorFactory.MakeEvaluator(
                            context.Container,
                            filterNode.RawFilterSpec.OptionalPropertyEvalSpec,
                            resolvedEventType,
                            filterNode.EventAsName,
                            context.EventAdapterService,
                            context.EngineImportService,
                            context.SchedulingService,
                            context.VariableService,
                            context.ScriptingService,
                            context.TableService,
                            context.EngineURI,
                            context.StatementId,
                            context.StatementName,
                            context.Annotations,
                            subexpressionIdStack,
                            context.ConfigSnapshot,
                            context.NamedWindowMgmtService,
                            context.StatementExtensionServicesContext);
                    finalEventType = optionalPropertyEvaluator.FragmentEventType;
                    isPropertyEvaluation = true;
                }

                if (finalEventType is EventTypeSPI)
                {
                    eventTypeReferences.Add(((EventTypeSPI)finalEventType).Metadata.PrimaryName);
                }

                // If a tag was supplied for the type, the tags must stay with this type, i.e. a=BeanA -> b=BeanA -> a=BeanB is a no
                if (optionalTag != null)
                {
                    var pair = tags.TaggedEventTypes.Get(optionalTag);
                    EventType existingType = null;
                    if (pair != null)
                    {
                        existingType = pair.First;
                    }
                    if (existingType == null)
                    {
                        pair = tags.ArrayEventTypes.Get(optionalTag);
                        if (pair != null)
                        {
                            throw new ExprValidationException(
                                "Tag '" + optionalTag + "' for event '" + eventName +
                                "' used in the repeat-until operator cannot also appear in other filter expressions");
                        }
                    }
                    if ((existingType != null) && (existingType != finalEventType))
                    {
                        throw new ExprValidationException(
                            "Tag '" + optionalTag + "' for event '" + eventName +
                            "' has already been declared for events of type " + existingType.UnderlyingType.FullName);
                    }
                    pair = new Pair<EventType, string>(finalEventType, eventName);

                    // add tagged type
                    if (isPropertyEvaluation || isParentMatchUntil)
                    {
                        newArrayEventTypes = new LinkedHashMap<string, Pair<EventType, string>>();
                        newArrayEventTypes.Put(optionalTag, pair);
                    }
                    else
                    {
                        newTaggedEventTypes = new LinkedHashMap<string, Pair<EventType, string>>();
                        newTaggedEventTypes.Put(optionalTag, pair);
                    }
                }

                // For this filter, filter types are all known tags at this time,
                // and additionally stream 0 (self) is our event type.
                // Stream type service allows resolution by property name event if that name appears in other tags.
                // by defaulting to stream zero.
                // Stream zero is always the current event type, all others follow the order of the map (stream 1 to N).
                var selfStreamName = optionalTag;
                if (selfStreamName == null)
                {
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
                LinkedHashMap<string, Pair<EventType, string>> arrayCompositeEventTypes = null;
                if (tags.ArrayEventTypes != null && !tags.ArrayEventTypes.IsEmpty())
                {
                    arrayCompositeEventTypes = new LinkedHashMap<string, Pair<EventType, string>>();
                    var patternSubexEventType = GetPatternSubexEventType(
                        context.StatementId, "pattern", subexpressionIdStack);

                    foreach (var entry in tags.ArrayEventTypes)
                    {
                        var specificArrayType = new LinkedHashMap<string, Pair<EventType, string>>();
                        specificArrayType.Put(entry.Key, entry.Value);
                        var arrayTagCompositeEventType =
                            context.EventAdapterService.CreateSemiAnonymousMapType(
                                patternSubexEventType, Collections.GetEmptyMap<string, Pair<EventType, string>>(),
                                specificArrayType, isInsertInto);
                        context.StatementSemiAnonymousTypeRegistry.Register(arrayTagCompositeEventType);

                        var tag = entry.Key;
                        if (!filterTypes.ContainsKey(tag))
                        {
                            var pair = new Pair<EventType, string>(arrayTagCompositeEventType, tag);
                            filterTypes.Put(tag, pair);
                            arrayCompositeEventTypes.Put(tag, pair);
                        }
                    }
                }

                StreamTypeService streamTypeService = new StreamTypeServiceImpl(
                    filterTypes, context.EngineURI, true, false);
                var exprNodes = filterNode.RawFilterSpec.FilterExpressions;

                var spec = FilterSpecCompiler.MakeFilterSpec(
                    resolvedEventType, eventName, exprNodes,
                    filterNode.RawFilterSpec.OptionalPropertyEvalSpec,
                    filterTaggedEventTypes,
                    arrayCompositeEventTypes,
                    streamTypeService,
                    null, context, subexpressionIdStack);
                filterNode.FilterSpec = spec;
            }
            else if (evalNode is EvalObserverFactoryNode)
            {
                var observerNode = (EvalObserverFactoryNode)evalNode;
                try
                {
                    var observerFactory = context.PatternResolutionService.Create(observerNode.PatternObserverSpec);

                    var streamTypeService = GetStreamTypeService(
                        context.EngineURI, context.StatementId, context.EventAdapterService, tags.TaggedEventTypes,
                        tags.ArrayEventTypes, subexpressionIdStack, "observer", context);
                    var validationContext = new ExprValidationContext(
                        context.Container,
                        streamTypeService,
                        context.EngineImportService,
                        context.StatementExtensionServicesContext, null,
                        context.SchedulingService,
                        context.VariableService,
                        context.TableService, evaluatorContext,
                        context.EventAdapterService,
                        context.StatementName,
                        context.StatementId,
                        context.Annotations,
                        context.ContextDescriptor,
                        context.ScriptingService,
                        false, false, false, false, null, false);
                    var validated = ValidateExpressions(
                        ExprNodeOrigin.PATTERNOBSERVER, observerNode.PatternObserverSpec.ObjectParameters,
                        validationContext);

                    MatchedEventConvertor convertor = new MatchedEventConvertorImpl(
                        tags.TaggedEventTypes, tags.ArrayEventTypes, allTagNamesOrdered, context.EventAdapterService);

                    observerNode.ObserverFactory = observerFactory;
                    observerFactory.SetObserverParameters(validated, convertor, validationContext);
                }
                catch (ObserverParameterException e)
                {
                    throw new ExprValidationException(
                        "Invalid parameter for pattern observer '" + observerNode.ToPrecedenceFreeEPL() + "': " +
                        e.Message, e);
                }
                catch (PatternObjectException e)
                {
                    throw new ExprValidationException(
                        "Failed to resolve pattern observer '" + observerNode.ToPrecedenceFreeEPL() + "': " + e.Message,
                        e);
                }
            }
            else if (evalNode is EvalGuardFactoryNode)
            {
                var guardNode = (EvalGuardFactoryNode)evalNode;
                try
                {
                    var guardFactory = context.PatternResolutionService.Create(guardNode.PatternGuardSpec);

                    var streamTypeService = GetStreamTypeService(
                        context.EngineURI, context.StatementId, context.EventAdapterService, tags.TaggedEventTypes,
                        tags.ArrayEventTypes, subexpressionIdStack, "guard", context);
                    var validationContext = new ExprValidationContext(
                        context.Container,
                        streamTypeService,
                        context.EngineImportService,
                        context.StatementExtensionServicesContext, null,
                        context.SchedulingService,
                        context.VariableService,
                        context.TableService, evaluatorContext,
                        context.EventAdapterService,
                        context.StatementName,
                        context.StatementId,
                        context.Annotations,
                        context.ContextDescriptor,
                        context.ScriptingService,
                        false, false, false, false, null, false);
                    var validated = ValidateExpressions(
                        ExprNodeOrigin.PATTERNGUARD, guardNode.PatternGuardSpec.ObjectParameters, validationContext);

                    MatchedEventConvertor convertor = new MatchedEventConvertorImpl(
                        tags.TaggedEventTypes, tags.ArrayEventTypes, allTagNamesOrdered, context.EventAdapterService);

                    guardNode.GuardFactory = guardFactory;
                    guardFactory.SetGuardParameters(validated, convertor);
                }
                catch (GuardParameterException e)
                {
                    throw new ExprValidationException(
                        "Invalid parameter for pattern guard '" + guardNode.ToPrecedenceFreeEPL() + "': " + e.Message, e);
                }
                catch (PatternObjectException e)
                {
                    throw new ExprValidationException(
                        "Failed to resolve pattern guard '" + guardNode.ToPrecedenceFreeEPL() + "': " + e.Message, e);
                }
            }
            else if (evalNode is EvalEveryDistinctFactoryNode)
            {
                var distinctNode = (EvalEveryDistinctFactoryNode)evalNode;
                var matchEventFromChildNodes = AnalyzeMatchEvent(distinctNode);
                var streamTypeService = GetStreamTypeService(
                    context.EngineURI, context.StatementId, context.EventAdapterService,
                    matchEventFromChildNodes.TaggedEventTypes, matchEventFromChildNodes.ArrayEventTypes,
                    subexpressionIdStack, "every-distinct", context);
                var validationContext = new ExprValidationContext(
                    context.Container,
                    streamTypeService,
                    context.EngineImportService,
                    context.StatementExtensionServicesContext, null,
                    context.SchedulingService,
                    context.VariableService,
                    context.TableService, evaluatorContext,
                    context.EventAdapterService,
                    context.StatementName,
                    context.StatementId,
                    context.Annotations,
                    context.ContextDescriptor,
                    context.ScriptingService,
                    false, false, false, false, null, false);
                IList<ExprNode> validated;
                try
                {
                    validated = ValidateExpressions(
                        ExprNodeOrigin.PATTERNEVERYDISTINCT, distinctNode.Expressions, validationContext);
                }
                catch (ExprValidationPropertyException ex)
                {
                    throw new ExprValidationPropertyException(
                        ex.Message +
                        ", every-distinct requires that all properties resolve from sub-expressions to the every-distinct",
                        ex.InnerException);
                }

                MatchedEventConvertor convertor =
                    new MatchedEventConvertorImpl(
                        matchEventFromChildNodes.TaggedEventTypes, matchEventFromChildNodes.ArrayEventTypes,
                        allTagNamesOrdered, context.EventAdapterService);

                distinctNode.Convertor = convertor;

                // Determine whether some expressions are constants or time period
                IList<ExprNode> distinctExpressions = new List<ExprNode>();
                ExprTimePeriodEvalDeltaConst timeDeltaComputation = null;
                ExprNode expiryTimeExp = null;
                var count = -1;
                var last = validated.Count - 1;
                foreach (var expr in validated)
                {
                    count++;
                    if (count == last && expr is ExprTimePeriod)
                    {
                        expiryTimeExp = expr;
                        var timePeriodExpr = (ExprTimePeriod)expiryTimeExp;
                        timeDeltaComputation =
                            timePeriodExpr.ConstEvaluator(new ExprEvaluatorContextStatement(context, false));
                    }
                    else if (expr.IsConstantResult)
                    {
                        if (count == last)
                        {
                            var evaluateParams = new EvaluateParams(null, true, evaluatorContext);
                            var value = expr.ExprEvaluator.Evaluate(evaluateParams);
                            if (!(value.IsNumber()))
                            {
                                throw new ExprValidationException(
                                    "Invalid parameter for every-distinct, expected number of seconds constant (constant not considered for distinct)");
                            }

                            var secondsExpire = expr.ExprEvaluator.Evaluate(evaluateParams);

                            long? timeExpire;
                            if (secondsExpire == null)
                            {
                                timeExpire = null;
                            }
                            else
                            {
                                timeExpire = context.TimeAbacus.DeltaForSecondsNumber(secondsExpire);
                            }

                            if (timeExpire != null && timeExpire > 0)
                            {
                                timeDeltaComputation = new ExprTimePeriodEvalDeltaConstGivenDelta(timeExpire.Value);
                                expiryTimeExp = expr;
                            }
                            else
                            {
                                Log.Warn("Invalid seconds-expire " + timeExpire + " for " + ExprNodeUtility.ToExpressionStringMinPrecedenceSafe(expr));
                            }
                        }
                        else
                        {
                            Log.Warn(
                                "Every-distinct node utilizes an expression returning a constant value, please check expression '{0}', not adding expression to distinct-value expression list",
                                expr.ToExpressionStringMinPrecedenceSafe());
                        }
                    }
                    else
                    {
                        distinctExpressions.Add(expr);
                    }
                }
                if (distinctExpressions.IsEmpty())
                {
                    throw new ExprValidationException(
                        "Every-distinct node requires one or more distinct-value expressions that each return non-constant result values");
                }
                distinctNode.SetDistinctExpressions(distinctExpressions, timeDeltaComputation, expiryTimeExp);
            }
            else if (evalNode is EvalMatchUntilFactoryNode)
            {
                var matchUntilNode = (EvalMatchUntilFactoryNode)evalNode;

                // compile bounds expressions, if any
                var untilMatchEventSpec = new MatchEventSpec(tags.TaggedEventTypes, tags.ArrayEventTypes);
                var streamTypeService = GetStreamTypeService(
                    context.EngineURI, context.StatementId, context.EventAdapterService,
                    untilMatchEventSpec.TaggedEventTypes, untilMatchEventSpec.ArrayEventTypes, subexpressionIdStack,
                    "until", context);
                var validationContext = new ExprValidationContext(
                    context.Container,
                    streamTypeService,
                    context.EngineImportService,
                    context.StatementExtensionServicesContext, null,
                    context.SchedulingService,
                    context.VariableService,
                    context.TableService, evaluatorContext,
                    context.EventAdapterService,
                    context.StatementName,
                    context.StatementId,
                    context.Annotations,
                    context.ContextDescriptor,
                    context.ScriptingService,
                    false, false, false, false, null, false);

                var lower = ValidateBounds(matchUntilNode.LowerBounds, validationContext);
                matchUntilNode.LowerBounds = lower;

                var upper = ValidateBounds(matchUntilNode.UpperBounds, validationContext);
                matchUntilNode.UpperBounds = upper;

                var single = ValidateBounds(matchUntilNode.SingleBound, validationContext);
                matchUntilNode.SingleBound = single;

                var convertor = new MatchedEventConvertorImpl(
                    untilMatchEventSpec.TaggedEventTypes, untilMatchEventSpec.ArrayEventTypes, allTagNamesOrdered,
                    context.EventAdapterService);
                matchUntilNode.Convertor = convertor;

                // compile new tag lists
                ISet<string> arrayTags = null;
                var matchUntilAnalysisResult = EvalNodeUtil.RecursiveAnalyzeChildNodes(matchUntilNode.ChildNodes[0]);
                foreach (var filterNode in matchUntilAnalysisResult.FilterNodes)
                {
                    var optionalTag = filterNode.EventAsName;
                    if (optionalTag != null)
                    {
                        if (arrayTags == null)
                        {
                            arrayTags = new HashSet<string>();
                        }
                        arrayTags.Add(optionalTag);
                    }
                }

                if (arrayTags != null)
                {
                    foreach (var arrayTag in arrayTags)
                    {
                        if (!tags.ArrayEventTypes.ContainsKey(arrayTag))
                        {
                            tags.ArrayEventTypes.Put(arrayTag, tags.TaggedEventTypes.Get(arrayTag));
                            tags.TaggedEventTypes.Remove(arrayTag);
                        }
                    }
                }
                matchUntilNode.TagsArrayed = GetIndexesForTags(allTagNamesOrdered, arrayTags);
            }
            else if (evalNode is EvalFollowedByFactoryNode)
            {
                var followedByNode = (EvalFollowedByFactoryNode) evalNode;
                StreamTypeService streamTypeService = new StreamTypeServiceImpl(context.EngineURI, false);
                var validationContext = new ExprValidationContext(
                    context.Container,
                    streamTypeService,
                    context.EngineImportService,
                    context.StatementExtensionServicesContext, null,
                    context.SchedulingService,
                    context.VariableService,
                    context.TableService,
                    evaluatorContext,
                    context.EventAdapterService,
                    context.StatementName,
                    context.StatementId,
                    context.Annotations,
                    context.ContextDescriptor,
                    context.ScriptingService,
                    false, false, false, false, null, false);

                if (followedByNode.OptionalMaxExpressions != null)
                {
                    IList<ExprNode> validated = new List<ExprNode>();
                    foreach (var maxExpr in followedByNode.OptionalMaxExpressions)
                    {
                        if (maxExpr == null)
                        {
                            validated.Add(null);
                        }
                        else
                        {
                            var visitor = new ExprNodeSummaryVisitor();
                            maxExpr.Accept(visitor);
                            if (!visitor.IsPlain)
                            {
                                var errorMessage = "Invalid maximum expression in followed-by, " + visitor.GetMessage() +
                                                   " are not allowed within the expression";
                                Log.Error(errorMessage);
                                throw new ExprValidationException(errorMessage);
                            }

                            var validatedExpr = ExprNodeUtility.GetValidatedSubtree(
                                ExprNodeOrigin.FOLLOWEDBYMAX, maxExpr, validationContext);
                            validated.Add(validatedExpr);
                            if ((validatedExpr.ExprEvaluator.ReturnType == null) ||
                                (!validatedExpr.ExprEvaluator.ReturnType.IsNumeric()))
                            {
                                var message = "Invalid maximum expression in followed-by, the expression must return an integer value";
                                throw new ExprValidationException(message);
                            }
                        }
                    }
                    followedByNode.OptionalMaxExpressions = validated;
                }
            }

            if (newTaggedEventTypes != null)
            {
                tags.TaggedEventTypes.PutAll(newTaggedEventTypes);
            }
            if (newArrayEventTypes != null)
            {
                tags.ArrayEventTypes.PutAll(newArrayEventTypes);
            }
        }

        private static ExprNode ValidateBounds(ExprNode bounds, ExprValidationContext validationContext)
        {
            var message = "Match-until bounds value expressions must return a numeric value";
            if (bounds != null)
            {
                var validated = ExprNodeUtility.GetValidatedSubtree(
                    ExprNodeOrigin.PATTERNMATCHUNTILBOUNDS, bounds, validationContext);
                if ((validated.ExprEvaluator.ReturnType == null) ||
                    (!validated.ExprEvaluator.ReturnType.IsNumeric()))
                {
                    throw new ExprValidationException(message);
                }
                return validated;
            }
            return null;
        }

        private static int[] GetIndexesForTags(ICollection<string> allTagNamesOrdered, ICollection<string> arrayTags)
        {
            if (arrayTags == null || arrayTags.IsEmpty())
            {
                return new int[0];
            }
            var indexes = new int[arrayTags.Count];
            var count = 0;
            foreach (var arrayTag in arrayTags)
            {
                var found = FindTagNumber(arrayTag, allTagNamesOrdered);
                indexes[count] = found;
                count++;
            }
            return indexes;
        }

        private static int FindTagNumber(string findTag, IEnumerable<string> allTagNamesOrdered)
        {
            var index = 0;
            foreach (var tag in allTagNamesOrdered)
            {
                if (findTag.Equals(tag))
                {
                    return index;
                }
                index++;
            }
            throw new EPException("Failed to find tag '" + findTag + "' among known tags");
        }

        private static bool IsParentMatchUntil(EvalFactoryNode currentNode, Stack<EvalFactoryNode> parentNodeStack)
        {
            if (parentNodeStack.Count == 0)
            {
                return false;
            }

            foreach (var deepParent in parentNodeStack.OfType<EvalMatchUntilFactoryNode>())
            {
                var matchUntilFactoryNode = deepParent;
                if (matchUntilFactoryNode.ChildNodes[0] == currentNode)
                {
                    return true;
                }
            }
            return false;
        }

        private static IList<ExprNode> ValidateExpressions(
            ExprNodeOrigin exprNodeOrigin,
            IList<ExprNode> objectParameters,
            ExprValidationContext validationContext)
        {
            if (objectParameters == null)
            {
                return objectParameters;
            }
            IList<ExprNode> validated = new List<ExprNode>();
            foreach (var node in objectParameters)
            {
                validated.Add(ExprNodeUtility.GetValidatedSubtree(exprNodeOrigin, node, validationContext));
            }
            return validated;
        }

        private static StreamTypeService GetStreamTypeService(
            string engineURI,
            int statementId,
            EventAdapterService eventAdapterService,
            IDictionary<string, Pair<EventType, string>> taggedEventTypes,
            IDictionary<string, Pair<EventType, string>> arrayEventTypes,
            IEnumerable<int> subexpressionIdStack,
            string objectType,
            StatementContext statementContext)
        {
            var filterTypes = new LinkedHashMap<string, Pair<EventType, string>>();
            filterTypes.PutAll(taggedEventTypes);

            // handle array tags (match-until clause)
            if (arrayEventTypes != null)
            {
                var patternSubexEventType = GetPatternSubexEventType(statementId, objectType, subexpressionIdStack);
                var arrayTagCompositeEventType = eventAdapterService.CreateSemiAnonymousMapType(
                    patternSubexEventType, new Dictionary<string, Pair<EventType, string>>(), arrayEventTypes, false);
                statementContext.StatementSemiAnonymousTypeRegistry.Register(arrayTagCompositeEventType);
                foreach (var entry in arrayEventTypes)
                {
                    var tag = entry.Key;
                    if (!filterTypes.ContainsKey(tag))
                    {
                        var pair = new Pair<EventType, string>(arrayTagCompositeEventType, tag);
                        filterTypes.Put(tag, pair);
                    }
                }
            }

            return new StreamTypeServiceImpl(filterTypes, engineURI, true, false);
        }

        private static string GetPatternSubexEventType(
            int statementId,
            string objectType,
            IEnumerable<int> subexpressionIdStack)
        {
            var writer = new StringWriter();
            writer.Write(statementId);
            writer.Write("_");
            writer.Write(objectType);
            foreach (var num in subexpressionIdStack)
            {
                writer.Write("_");
                writer.Write(num);
            }
            return writer.ToString();
        }

        private static EvalFactoryNode RecursiveAddAuditNode(
            PatternNodeFactory patternNodeFactory,
            EvalFactoryNode parentNode,
            bool auditPattern,
            bool auditPatternInstance,
            EvalFactoryNode evalNode,
            EvalAuditInstanceCount instanceCount)
        {
            var writer = new StringWriter();
            evalNode.ToEPL(writer, PatternExpressionPrecedenceEnum.MINIMUM);
            var expressionText = writer.ToString();
            var filterChildNonQuitting = parentNode != null && parentNode.IsFilterChildNonQuitting;
            EvalFactoryNode audit = patternNodeFactory.MakeAuditNode(
                auditPattern, auditPatternInstance, expressionText, instanceCount, filterChildNonQuitting);
            audit.AddChildNode(evalNode);

            IList<EvalFactoryNode> newChildNodes = new List<EvalFactoryNode>();
            foreach (var child in evalNode.ChildNodes)
            {
                newChildNodes.Add(
                    RecursiveAddAuditNode(
                        patternNodeFactory, evalNode, auditPattern, auditPatternInstance, child, instanceCount));
            }

            evalNode.ChildNodes.Clear();
            evalNode.AddChildNodes(newChildNodes);

            return audit;
        }

        private static MatchEventSpec AnalyzeMatchEvent(EvalFactoryNode relativeNode)
        {
            var taggedEventTypes = new LinkedHashMap<string, Pair<EventType, string>>();
            var arrayEventTypes = new LinkedHashMap<string, Pair<EventType, string>>();

            // Determine all the filter nodes used in the pattern
            var evalNodeAnalysisResult = EvalNodeUtil.RecursiveAnalyzeChildNodes(relativeNode);

            // collect all filters underneath
            foreach (var filterNode in evalNodeAnalysisResult.FilterNodes)
            {
                var optionalTag = filterNode.EventAsName;
                if (optionalTag != null)
                {
                    taggedEventTypes.Put(
                        optionalTag,
                        new Pair<EventType, string>(
                            filterNode.FilterSpec.FilterForEventType, filterNode.FilterSpec.FilterForEventTypeName));
                }
            }

            // collect those filters under a repeat since they are arrays
            var arrayTags = new HashSet<string>();
            foreach (var matchUntilNode in evalNodeAnalysisResult.RepeatNodes)
            {
                var matchUntilAnalysisResult = EvalNodeUtil.RecursiveAnalyzeChildNodes(matchUntilNode.ChildNodes[0]);
                foreach (var filterNode in matchUntilAnalysisResult.FilterNodes)
                {
                    var optionalTag = filterNode.EventAsName;
                    if (optionalTag != null)
                    {
                        arrayTags.Add(optionalTag);
                    }
                }
            }

            // for each array tag change collection
            foreach (var arrayTag in arrayTags)
            {
                if (taggedEventTypes.Get(arrayTag) != null)
                {
                    arrayEventTypes.Put(arrayTag, taggedEventTypes.Get(arrayTag));
                    taggedEventTypes.Remove(arrayTag);
                }
            }

            return new MatchEventSpec(taggedEventTypes, arrayEventTypes);
        }

        public bool IsSuppressSameEventMatches
        {
            get { return _suppressSameEventMatches; }
        }

        public bool IsDiscardPartialsOnMatch
        {
            get { return _discardPartialsOnMatch; }
        }

        public class FilterForFilterFactoryNodes : EvalNodeUtilFactoryFilter
        {
            public static readonly FilterForFilterFactoryNodes INSTANCE = new FilterForFilterFactoryNodes();

            public bool Consider(EvalFactoryNode node)
            {
                return node is EvalFilterFactoryNode;
            }
        }
    }
} // end of namespace
