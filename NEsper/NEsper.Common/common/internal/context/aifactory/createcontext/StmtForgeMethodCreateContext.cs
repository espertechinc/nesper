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
using com.espertech.esper.common.client.meta;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.core;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.context.compile;
using com.espertech.esper.common.@internal.context.controller.category;
using com.espertech.esper.common.@internal.context.controller.core;
using com.espertech.esper.common.@internal.context.controller.hash;
using com.espertech.esper.common.@internal.context.controller.initterm;
using com.espertech.esper.common.@internal.context.controller.keyed;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.pattern.core;
using com.espertech.esper.common.@internal.epl.pattern.filter;
using com.espertech.esper.common.@internal.epl.resultset.select.core;
using com.espertech.esper.common.@internal.epl.streamtype;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.@event.map;
using com.espertech.esper.common.@internal.filterspec;
using com.espertech.esper.common.@internal.schedule;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.context.aifactory.createcontext
{
    public class StmtForgeMethodCreateContext : StmtForgeMethod
    {
        private readonly StatementBaseInfo @base;

        public StmtForgeMethodCreateContext(StatementBaseInfo @base)
        {
            this.@base = @base;
        }

        public StmtForgeMethodResult Make(
            string packageName, string classPostfix, StatementCompileTimeServices services)
        {
            StatementSpecCompiled statementSpec = @base.StatementSpec;
            if (statementSpec.Raw.OptionalContextName != null) {
                throw new ExprValidationException(
                    "A create-context statement cannot itself be associated to a context, please declare a nested context instead");
            }

            IList<FilterSpecCompiled> filterSpecCompileds = new List<FilterSpecCompiled>();
            IList<ScheduleHandleCallbackProvider> scheduleHandleCallbackProviders =
                new List<ScheduleHandleCallbackProvider>();
            IList<FilterSpecParamExprNodeForge> filterBooleanExpressions = new List<FilterSpecParamExprNodeForge>();

            CreateContextDesc context = statementSpec.Raw.CreateContextDesc;
            if (services.ContextCompileTimeResolver.GetContextInfo(context.ContextName) != null) {
                throw new ExprValidationException("Context by name '" + context.ContextName + "' already exists");
            }

            // compile filter specs, if any
            CreateContextValidationEnv validationEnv = new CreateContextValidationEnv(
                context.ContextName, @base.StatementRawInfo, services, filterSpecCompileds,
                scheduleHandleCallbackProviders, filterBooleanExpressions);
            ValidateContextDetail(context.ContextDetail, 0, validationEnv);

            // get controller factory forges
            ContextControllerFactoryForge[] controllerFactoryForges = GetForges(
                context.ContextName, context.ContextDetail);

            // build context properties type information
            IDictionary<string, object> contextProps = MakeContextProperies(
                controllerFactoryForges, @base.StatementRawInfo, services);

            // allocate type for context properties
            string contextEventTypeName =
                services.EventTypeNameGeneratorStatement.GetContextPropertyTypeName(context.ContextName);
            EventTypeMetadata metadata = new EventTypeMetadata(
                contextEventTypeName, @base.ModuleName, EventTypeTypeClass.CONTEXTPROPDERIVED,
                EventTypeApplicationType.MAP, NameAccessModifier.TRANSIENT, EventTypeBusModifier.NONBUS, false,
                EventTypeIdPair.Unassigned());
            MapEventType contextPropertiesType = BaseNestableEventUtil.MakeMapTypeCompileTime(
                metadata, contextProps, null, null, null, null, services.BeanEventTypeFactoryPrivate,
                services.EventTypeCompileTimeResolver);
            services.EventTypeCompileTimeRegistry.NewType(contextPropertiesType);

            // register context
            NameAccessModifier visibilityContext =
                services.ModuleVisibilityRules.GetAccessModifierContext(@base, context.ContextName);
            ContextControllerPortableInfo[] validationInfo =
                new ContextControllerPortableInfo[controllerFactoryForges.Length];
            for (int i = 0; i < validationInfo.Length; i++) {
                validationInfo[i] = controllerFactoryForges[i].ValidationInfo;
            }

            ContextMetaData detail = new ContextMetaData(
                context.ContextName, @base.ModuleName, visibilityContext, contextPropertiesType, validationInfo);
            services.ContextCompileTimeRegistry.NewContext(detail);

            // define output event type
            string statementEventTypeName =
                services.EventTypeNameGeneratorStatement.GetContextStatementTypeName(context.ContextName);
            EventTypeMetadata statementTypeMetadata = new EventTypeMetadata(
                statementEventTypeName, @base.ModuleName, EventTypeTypeClass.STATEMENTOUT, EventTypeApplicationType.MAP,
                NameAccessModifier.TRANSIENT, EventTypeBusModifier.NONBUS, false, EventTypeIdPair.Unassigned());
            EventType statementEventType = BaseNestableEventUtil.MakeMapTypeCompileTime(
                statementTypeMetadata, Collections.EmptyMap(), null, null, null, null,
                services.BeanEventTypeFactoryPrivate, services.EventTypeCompileTimeResolver);
            services.EventTypeCompileTimeRegistry.NewType(statementEventType);

            IList<StmtClassForgable> forgables = new List<StmtClassForgable>();

            string statementProviderClassName =
                CodeGenerationIDGenerator.GenerateClassNameSimple(typeof(StatementProvider), classPostfix);
            string statementAIFactoryProviderClassName =
                CodeGenerationIDGenerator.GenerateClassNameSimple(typeof(StatementAIFactoryProvider), classPostfix);
            string statementFieldsClassName =
                CodeGenerationIDGenerator.GenerateClassNameSimple(typeof(StatementFields), classPostfix);
            CodegenPackageScope packageScope = new CodegenPackageScope(
                packageName, statementFieldsClassName, services.IsInstrumented);

            StatementAgentInstanceFactoryCreateContextForge forge =
                new StatementAgentInstanceFactoryCreateContextForge(context.ContextName, statementEventType);
            forgables.Add(
                new StmtClassForgableAIFactoryProviderCreateContext(
                    statementAIFactoryProviderClassName, packageScope, context.ContextName, controllerFactoryForges,
                    contextPropertiesType, forge));

            SelectSubscriberDescriptor selectSubscriberDescriptor = new SelectSubscriberDescriptor();
            StatementInformationalsCompileTime informationals = StatementInformationalsUtil.GetInformationals(
                @base, filterSpecCompileds, scheduleHandleCallbackProviders, Collections.GetEmptyList(), false,
                selectSubscriberDescriptor, packageScope, services);
            forgables.Add(
                new StmtClassForgableStmtProvider(
                    statementAIFactoryProviderClassName, statementProviderClassName, informationals, packageScope));
            forgables.Add(new StmtClassForgableStmtFields(statementFieldsClassName, packageScope, 0));

            return new StmtForgeMethodResult(
                forgables, filterSpecCompileds, scheduleHandleCallbackProviders, Collections.GetEmptyList(),
                FilterSpecCompiled.MakeExprNodeList(filterSpecCompileds, filterBooleanExpressions));
        }

        private IDictionary<string, object> MakeContextProperies(
            ContextControllerFactoryForge[] controllers, StatementRawInfo statementRawInfo,
            StatementCompileTimeServices services)
        {

            LinkedHashMap<string, object> props = new LinkedHashMap<string, object>();
            props.Put(ContextPropertyEventType.PROP_CTX_NAME, typeof(string));
            props.Put(ContextPropertyEventType.PROP_CTX_ID, typeof(int));

            if (controllers.Length == 1) {
                controllers[0].ValidateGetContextProps(
                    props, controllers[0].FactoryEnv.OutermostContextName, statementRawInfo, services);
                return props;
            }

            for (int level = 0; level < controllers.Length; level++) {
                string nestedContextName = controllers[level].FactoryEnv.ContextName;
                LinkedHashMap<string, object> propsPerLevel = new LinkedHashMap<string, object>();
                propsPerLevel.Put(ContextPropertyEventType.PROP_CTX_NAME, typeof(string));
                if (level == controllers.Length - 1) {
                    propsPerLevel.Put(ContextPropertyEventType.PROP_CTX_ID, typeof(int));
                }

                controllers[level].ValidateGetContextProps(
                    propsPerLevel, nestedContextName, statementRawInfo, services);
                props.Put(nestedContextName, propsPerLevel);
            }

            return props;
        }

        private void ValidateContextDetail(
            ContextSpec contextSpec, int nestingLevel, CreateContextValidationEnv validationEnv)
        {
            ISet<string> eventTypesReferenced = new HashSet<string>();
            if (contextSpec is ContextSpecKeyed) {
                ContextSpecKeyed segmented = (ContextSpecKeyed) contextSpec;
                IDictionary<string, EventType> asNames = new Dictionary<string, EventType>();
                bool partitionHasNameAssignment = false;
                foreach (ContextSpecKeyedItem partition in segmented.Items) {
                    FilterSpecCompiled filterSpecCompiled = CompilePartitonedFilterSpec(
                        partition.FilterSpecRaw, eventTypesReferenced, validationEnv);
                    partition.FilterSpecCompiled = filterSpecCompiled;

                    EventPropertyGetterSPI[] getters = new EventPropertyGetterSPI[partition.PropertyNames.Count];
                    EventTypeSPI eventType = (EventTypeSPI) filterSpecCompiled.FilterForEventType;
                    for (int i = 0; i < partition.PropertyNames.Count; i++) {
                        string propertyName = partition.PropertyNames.Get(i);
                        EventPropertyGetterSPI getter = eventType.GetGetterSPI(propertyName);
                        getters[i] = getter;
                    }

                    partition.Getters = getters;

                    if (partition.AliasName != null) {
                        partitionHasNameAssignment = true;
                        ValidateAsName(asNames, partition.AliasName, filterSpecCompiled.FilterForEventType);
                    }
                }

                if (segmented.OptionalInit != null) {
                    asNames.Clear();
                    foreach (ContextSpecConditionFilter initCondition in segmented.OptionalInit) {
                        ValidateRewriteContextCondition(
                            true, nestingLevel, initCondition, eventTypesReferenced, new MatchEventSpec(),
                            Collections.EmptySet(), validationEnv);

                        EventType filterForType = initCondition.FilterSpecCompiled.FilterForEventType;
                        bool found = false;
                        foreach (ContextSpecKeyedItem partition in segmented.Items) {
                            if (partition.FilterSpecCompiled.FilterForEventType == filterForType) {
                                found = true;
                                break;
                            }
                        }

                        if (!found) {
                            throw new ExprValidationException(
                                "Segmented context '" + validationEnv.ContextName +
                                "' requires that all of the event types that are listed in the initialized-by also appear in the partition-by, type '" +
                                filterForType.Name + "' is not one of the types listed in partition-by");
                        }

                        if (initCondition.OptionalFilterAsName != null) {
                            if (partitionHasNameAssignment) {
                                throw new ExprValidationException(
                                    "Segmented context '" + validationEnv.ContextName +
                                    "' requires that either partition-by or initialized-by assign stream names, but not both");
                            }

                            ValidateAsName(asNames, initCondition.OptionalFilterAsName, filterForType);
                        }
                    }
                }

                if (segmented.OptionalTermination != null) {
                    MatchEventSpec matchEventSpec = new MatchEventSpec();
                    LinkedHashSet<string> allTags = new LinkedHashSet<>();
                    foreach (ContextSpecKeyedItem partition in segmented.Items) {
                        if (partition.AliasName != null) {
                            allTags.Add(partition.AliasName);
                            matchEventSpec.TaggedEventTypes.Put(
                                partition.AliasName,
                                new Pair<>(
                                    partition.FilterSpecCompiled.FilterForEventType,
                                    partition.FilterSpecRaw.EventTypeName));
                        }
                    }

                    if (segmented.OptionalInit != null) {
                        foreach (ContextSpecConditionFilter initCondition in segmented.OptionalInit) {
                            if (initCondition.OptionalFilterAsName != null) {
                                allTags.Add(initCondition.OptionalFilterAsName);
                                matchEventSpec.TaggedEventTypes.Put(
                                    initCondition.OptionalFilterAsName,
                                    new Pair<>(
                                        initCondition.FilterSpecCompiled.FilterForEventType,
                                        initCondition.FilterSpecRaw.EventTypeName));
                            }
                        }
                    }

                    ContextDetailMatchPair endCondition = ValidateRewriteContextCondition(
                        false, nestingLevel, segmented.OptionalTermination, eventTypesReferenced, matchEventSpec,
                        allTags, validationEnv);
                    segmented.OptionalTermination = endCondition.Condition;
                }
            }
            else if (contextSpec is ContextSpecCategory) {

                // compile filter
                ContextSpecCategory category = (ContextSpecCategory) contextSpec;
                ValidateNotTable(category.FilterSpecRaw.EventTypeName, validationEnv.Services);
                FilterStreamSpecRaw raw = new FilterStreamSpecRaw(
                    category.FilterSpecRaw, ViewSpec.EMPTY_VIEWSPEC_ARRAY, null, StreamSpecOptions.DEFAULT);
                FilterStreamSpecCompiled result = (FilterStreamSpecCompiled) StreamSpecCompiler.CompileFilter(
                    raw, false, false, true, false, null, validationEnv.StatementRawInfo, validationEnv.Services);
                category.FilterSpecCompiled = result.FilterSpecCompiled;
                validationEnv.FilterSpecCompileds.Add(result.FilterSpecCompiled);

                // compile expressions
                foreach (ContextSpecCategoryItem item in category.Items) {
                    ValidateNotTable(category.FilterSpecRaw.EventTypeName, validationEnv.Services);
                    FilterSpecRaw filterSpecRaw = new FilterSpecRaw(
                        category.FilterSpecRaw.EventTypeName, Collections.SingletonList(item.Expression), null);
                    FilterStreamSpecRaw rawExpr = new FilterStreamSpecRaw(
                        filterSpecRaw, ViewSpec.EMPTY_VIEWSPEC_ARRAY, null, StreamSpecOptions.DEFAULT);
                    FilterStreamSpecCompiled compiled = (FilterStreamSpecCompiled) StreamSpecCompiler.CompileFilter(
                        rawExpr, false, false, true, false, null, validationEnv.StatementRawInfo,
                        validationEnv.Services);
                    compiled.FilterSpecCompiled.TraverseFilterBooleanExpr(
                        validationEnv.FilterBooleanExpressions.Add);
                    item.CompiledFilterParam = compiled.FilterSpecCompiled.Parameters;
                }
            }
            else if (contextSpec is ContextSpecHash) {
                ContextSpecHash hashed = (ContextSpecHash) contextSpec;
                foreach (ContextSpecHashItem hashItem in hashed.Items) {
                    FilterStreamSpecRaw raw = new FilterStreamSpecRaw(
                        hashItem.FilterSpecRaw, ViewSpec.EMPTY_VIEWSPEC_ARRAY, null, StreamSpecOptions.DEFAULT);
                    ValidateNotTable(hashItem.FilterSpecRaw.EventTypeName, validationEnv.Services);
                    FilterStreamSpecCompiled result = (FilterStreamSpecCompiled) StreamSpecCompiler.Compile(
                        raw, eventTypesReferenced, false, false, true, false, null, 0, validationEnv.StatementRawInfo,
                        validationEnv.Services);
                    validationEnv.FilterSpecCompileds.Add(result.FilterSpecCompiled);
                    hashItem.FilterSpecCompiled = result.FilterSpecCompiled;

                    // validate parameters
                    StreamTypeServiceImpl streamTypes = new StreamTypeServiceImpl(
                        result.FilterSpecCompiled.FilterForEventType, null, true);
                    ExprValidationContext validationContext =
                        new ExprValidationContextBuilder(
                                streamTypes, validationEnv.StatementRawInfo, validationEnv.Services)
                            .WithIsFilterExpression(true).Build();
                    ExprNodeUtilityValidate.Validate(
                        ExprNodeOrigin.CONTEXT, Collections.SingletonList(hashItem.Function), validationContext);
                }
            }
            else if (contextSpec is ContextSpecInitiatedTerminated) {
                ContextSpecInitiatedTerminated def = (ContextSpecInitiatedTerminated) contextSpec;
                ContextDetailMatchPair startCondition = ValidateRewriteContextCondition(
                    true, nestingLevel, def.StartCondition, eventTypesReferenced, new MatchEventSpec(),
                    new LinkedHashSet<string>(), validationEnv);
                ContextDetailMatchPair endCondition = ValidateRewriteContextCondition(
                    false, nestingLevel, def.EndCondition, eventTypesReferenced, startCondition.Matches,
                    startCondition.AllTags, validationEnv);
                def.StartCondition = startCondition.Condition;
                def.EndCondition = endCondition.Condition;

                if (def.DistinctExpressions != null) {
                    if (!(startCondition.Condition is ContextSpecConditionFilter)) {
                        throw new ExprValidationException(
                            "Distinct-expressions require a stream as the initiated-by condition");
                    }

                    ExprNode[] distinctExpressions = def.DistinctExpressions;
                    if (distinctExpressions.Length == 0) {
                        throw new ExprValidationException("Distinct-expressions have not been provided");
                    }

                    ContextSpecConditionFilter filter = (ContextSpecConditionFilter) startCondition.Condition;
                    if (filter.OptionalFilterAsName == null) {
                        throw new ExprValidationException(
                            "Distinct-expressions require that a stream name is assigned to the stream using 'as'");
                    }

                    StreamTypeServiceImpl types = new StreamTypeServiceImpl(
                        filter.FilterSpecCompiled.FilterForEventType, filter.OptionalFilterAsName, true);
                    ExprValidationContext validationContext =
                        new ExprValidationContextBuilder(types, validationEnv.StatementRawInfo, validationEnv.Services)
                            .WithAllowBindingConsumption(true).Build();
                    for (int i = 0; i < distinctExpressions.Length; i++) {
                        ExprNodeUtilityValidate.ValidatePlainExpression(
                            ExprNodeOrigin.CONTEXTDISTINCT, distinctExpressions[i]);
                        distinctExpressions[i] = ExprNodeUtilityValidate.GetValidatedSubtree(
                            ExprNodeOrigin.CONTEXTDISTINCT, distinctExpressions[i], validationContext);
                    }
                }
            }
            else if (contextSpec is ContextNested) {
                ContextNested nested = (ContextNested) contextSpec;
                int level = 0;
                ISet<string> namesUsed = new HashSet<string>();
                namesUsed.Add(validationEnv.ContextName);
                foreach (CreateContextDesc nestedContext in nested.Contexts) {
                    if (namesUsed.Contains(nestedContext.ContextName)) {
                        throw new ExprValidationException(
                            "Context by name '" + nestedContext.ContextName +
                            "' has already been declared within nested context '" + validationEnv.ContextName + "'");
                    }

                    namesUsed.Add(nestedContext.ContextName);

                    ValidateContextDetail(nestedContext.ContextDetail, level, validationEnv);
                    level++;
                }
            }
            else {
                throw new IllegalStateException("Unrecognized context detail " + contextSpec);
            }
        }

        private FilterSpecCompiled CompilePartitonedFilterSpec(
            FilterSpecRaw filterSpecRaw, ISet<string> eventTypesReferenced, CreateContextValidationEnv validationEnv)
        {
            ValidateNotTable(filterSpecRaw.EventTypeName, validationEnv.Services);
            FilterStreamSpecRaw raw = new FilterStreamSpecRaw(
                filterSpecRaw, ViewSpec.EMPTY_VIEWSPEC_ARRAY, null, StreamSpecOptions.DEFAULT);
            StreamSpecCompiled compiled = StreamSpecCompiler.Compile(
                raw, eventTypesReferenced, false, false, true, false, null, 0, validationEnv.StatementRawInfo,
                validationEnv.Services);
            if (!(compiled is FilterStreamSpecCompiled)) {
                throw new ExprValidationException("Partition criteria may not include named windows");
            }

            FilterStreamSpecCompiled filters = (FilterStreamSpecCompiled) compiled;
            FilterSpecCompiled spec = filters.FilterSpecCompiled;
            validationEnv.FilterSpecCompileds.Add(spec);
            return spec;
        }

        private void ValidateNotTable(string eventTypeName, StatementCompileTimeServices services)
        {
            if (services.TableCompileTimeResolver.Resolve(eventTypeName) != null) {
                throw new ExprValidationException("Tables cannot be used in a context declaration");
            }
        }

        private void ValidateAsName(IDictionary<string, EventType> asNames, string asName, EventType filterForType)
        {
            EventType existing = asNames.Get(asName);
            if (existing != null && !EventTypeUtility.IsTypeOrSubTypeOf(filterForType, existing)) {
                throw new ExprValidationException(
                    "Name '" + asName + "' already used for type '" + existing.Name + "'");
            }

            if (existing == null) {
                asNames.Put(asName, filterForType);
            }
        }

        private ContextControllerFactoryForge[] GetForges(string contextName, ContextSpec contextDetail)
        {
            if (!(contextDetail is ContextNested)) {
                ContextControllerFactoryEnv factoryEnv = new ContextControllerFactoryEnv(
                    contextName, contextName, 1, 1);
                return new ContextControllerFactoryForge[] {Make(factoryEnv, contextDetail)};
            }

            ContextNested nested = (ContextNested) contextDetail;
            ContextControllerFactoryForge[] forges = new ContextControllerFactoryForge[nested.Contexts.Count];
            int nestingLevel = 1;
            foreach (CreateContextDesc desc in nested.Contexts) {
                ContextControllerFactoryEnv factoryEnv = new ContextControllerFactoryEnv(
                    contextName, desc.ContextName, nestingLevel, nested.Contexts.Count);
                forges[nestingLevel - 1] = Make(factoryEnv, desc.ContextDetail);
                nestingLevel++;
            }

            return forges;
        }

        private ContextControllerFactoryForge Make(ContextControllerFactoryEnv factoryContext, ContextSpec detail)
        {
            ContextControllerFactoryForge forge;
            if (detail is ContextSpecInitiatedTerminated) {
                forge = new ContextControllerInitTermFactoryForge(
                    factoryContext, (ContextSpecInitiatedTerminated) detail);
            }
            else if (detail is ContextSpecKeyed) {
                forge = new ContextControllerKeyedFactoryForge(factoryContext, (ContextSpecKeyed) detail);
            }
            else if (detail is ContextSpecCategory) {
                forge = new ContextControllerCategoryFactoryForge(factoryContext, (ContextSpecCategory) detail);
            }
            else if (detail is ContextSpecHash) {
                forge = new ContextControllerHashFactoryForge(factoryContext, (ContextSpecHash) detail);
            }
            else {
                throw new UnsupportedOperationException(
                    "Context detail " + detail + " is not yet supported in a nested context");
            }

            return forge;
        }

        private ContextDetailMatchPair ValidateRewriteContextCondition(
            bool isStartCondition, int nestingLevel, ContextSpecCondition endpoint, ISet<string> eventTypesReferenced,
            MatchEventSpec priorMatches, ISet<string> priorAllTags, CreateContextValidationEnv validationEnv)
        {
            if (endpoint is ContextSpecConditionCrontab) {
                ContextSpecConditionCrontab crontab = (ContextSpecConditionCrontab) endpoint;
                ExprForge[] forges = ScheduleExpressionUtil.CrontabScheduleValidate(
                    ExprNodeOrigin.CONTEXTCONDITION, crontab.Crontab, false, validationEnv.StatementRawInfo,
                    validationEnv.Services);
                crontab.Forges = forges;
                validationEnv.ScheduleHandleCallbackProviders.Add(crontab);
                return new ContextDetailMatchPair(crontab, new MatchEventSpec(), new LinkedHashSet<string>());
            }

            if (endpoint is ContextSpecConditionTimePeriod) {
                ContextSpecConditionTimePeriod timePeriod = (ContextSpecConditionTimePeriod) endpoint;
                ExprValidationContext validationContext = new ExprValidationContextBuilder(
                    new StreamTypeServiceImpl(false), validationEnv.StatementRawInfo, validationEnv.Services).Build();
                ExprNodeUtilityValidate.GetValidatedSubtree(
                    ExprNodeOrigin.CONTEXTCONDITION, timePeriod.TimePeriod, validationContext);
                if (timePeriod.TimePeriod.IsConstantResult) {
                    if (timePeriod.TimePeriod.EvaluateAsSeconds(null, true, null) < 0) {
                        throw new ExprValidationException(
                            "Invalid negative time period expression '" +
                            ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceSafe(timePeriod.TimePeriod) + "'");
                    }
                }

                validationEnv.ScheduleHandleCallbackProviders.Add(timePeriod);
                return new ContextDetailMatchPair(timePeriod, new MatchEventSpec(), new LinkedHashSet<>());
            }

            if (endpoint is ContextSpecConditionPattern) {
                ContextSpecConditionPattern pattern = (ContextSpecConditionPattern) endpoint;
                Pair<MatchEventSpec, ISet<string>> matches = ValidatePatternContextConditionPattern(
                    isStartCondition, nestingLevel, pattern, eventTypesReferenced, priorMatches, priorAllTags,
                    validationEnv);
                return new ContextDetailMatchPair(pattern, matches.First, matches.Second);
            }

            if (endpoint is ContextSpecConditionFilter) {
                ContextSpecConditionFilter filter = (ContextSpecConditionFilter) endpoint;
                ValidateNotTable(filter.FilterSpecRaw.EventTypeName, validationEnv.Services);

                // compile as filter if there are no prior match to consider
                if (priorMatches == null ||
                    (priorMatches.ArrayEventTypes.IsEmpty() && priorMatches.TaggedEventTypes.IsEmpty())) {
                    FilterStreamSpecRaw rawExpr = new FilterStreamSpecRaw(
                        filter.FilterSpecRaw, ViewSpec.EMPTY_VIEWSPEC_ARRAY, null, StreamSpecOptions.DEFAULT);
                    FilterStreamSpecCompiled compiled = (FilterStreamSpecCompiled) StreamSpecCompiler.Compile(
                        rawExpr, eventTypesReferenced, false, false, true, false, filter.OptionalFilterAsName, 0,
                        validationEnv.StatementRawInfo, validationEnv.Services);
                    filter.FilterSpecCompiled = compiled.FilterSpecCompiled;
                    MatchEventSpec matchEventSpec = new MatchEventSpec();
                    EventType filterForType = compiled.FilterSpecCompiled.FilterForEventType;
                    LinkedHashSet<string> allTags = new LinkedHashSet<string>();
                    if (filter.OptionalFilterAsName != null) {
                        matchEventSpec.TaggedEventTypes.Put(
                            filter.OptionalFilterAsName,
                            new Pair<EventType, string>(filterForType, rawExpr.RawFilterSpec.EventTypeName));
                        allTags.Add(filter.OptionalFilterAsName);
                    }

                    validationEnv.FilterSpecCompileds.Add(compiled.FilterSpecCompiled);
                    return new ContextDetailMatchPair(filter, matchEventSpec, allTags);
                }

                // compile as pattern if there are prior matches to consider, since this is a type of followed-by relationship
                EvalForgeNode forgeNode = new EvalFilterForgeNode(filter.FilterSpecRaw, filter.OptionalFilterAsName, 0);
                ContextSpecConditionPattern pattern = new ContextSpecConditionPattern(forgeNode, true, false);
                Pair<MatchEventSpec, ISet<string>> matches = ValidatePatternContextConditionPattern(
                    isStartCondition, nestingLevel, pattern, eventTypesReferenced, priorMatches, priorAllTags,
                    validationEnv);
                return new ContextDetailMatchPair(pattern, matches.First, matches.Second);
            }
            else if (endpoint is ContextSpecConditionImmediate || endpoint is ContextSpecConditionNever) {
                return new ContextDetailMatchPair(endpoint, new MatchEventSpec(), new LinkedHashSet<string>());
            }
            else {
                throw new IllegalStateException("Unrecognized endpoint type " + endpoint);
            }
        }

        private Pair<MatchEventSpec, ISet<string>> ValidatePatternContextConditionPattern(
            bool isStartCondition, int nestingLevel, ContextSpecConditionPattern pattern,
            ISet<string> eventTypesReferenced, MatchEventSpec priorMatches, ISet<string> priorAllTags,
            CreateContextValidationEnv validationEnv)
        {
            PatternStreamSpecRaw raw = new PatternStreamSpecRaw(
                pattern.PatternRaw, ViewSpec.EMPTY_VIEWSPEC_ARRAY, null, StreamSpecOptions.DEFAULT, false, false);
            PatternStreamSpecCompiled compiled = StreamSpecCompiler.CompilePatternWTags(
                raw, eventTypesReferenced, false, priorMatches, priorAllTags, false, true, false, 0,
                validationEnv.StatementRawInfo, validationEnv.Services);
            pattern.PatternCompiled = compiled;

            pattern.PatternContext = new PatternContext(
                0, compiled.MatchedEventMapMeta, true, nestingLevel, isStartCondition);

            IList<EvalForgeNode> forges = compiled.Root.CollectFactories();
            foreach (EvalForgeNode forge in forges) {
                forge.CollectSelfFilterAndSchedule(
                    validationEnv.FilterSpecCompileds, validationEnv.ScheduleHandleCallbackProviders);
            }

            return new Pair<>(
                new MatchEventSpec(compiled.TaggedEventTypes, compiled.ArrayEventTypes), compiled.AllTags);
        }

        private class ContextDetailMatchPair
        {
            private readonly ContextSpecCondition condition;
            private readonly MatchEventSpec matches;
            private readonly ISet<string> allTags;

            private ContextDetailMatchPair(ContextSpecCondition condition, MatchEventSpec matches, ISet<string> allTags)
            {
                this.condition = condition;
                this.matches = matches;
                this.allTags = allTags;
            }

            public ContextSpecCondition Condition {
                get { return condition; }
            }

            public MatchEventSpec Matches {
                get { return matches; }
            }

            public ISet<string> AllTags {
                get { return allTags; }
            }
        }
    }
} // end of namespace