///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.meta;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.core;
using com.espertech.esper.common.@internal.compile.multikey;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.compile.util;
using com.espertech.esper.common.@internal.context.activator;
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
using com.espertech.esper.common.@internal.fabric;
using com.espertech.esper.common.@internal.filterspec;
using com.espertech.esper.common.@internal.schedule;
using com.espertech.esper.common.@internal.serde.compiletime.eventtype;
using com.espertech.esper.common.@internal.serde.compiletime.resolve;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.context.util.ContextPropertyEventType;

namespace com.espertech.esper.common.@internal.context.aifactory.createcontext
{
    public partial class StmtForgeMethodCreateContext : StmtForgeMethod
    {
        private readonly StatementBaseInfo _base;

        public StmtForgeMethodCreateContext(StatementBaseInfo @base)
        {
            this._base = @base;
        }

        public StmtForgeMethodResult Make(
            string @namespace,
            string classPostfix,
            StatementCompileTimeServices services)
        {
            var statementSpec = _base.StatementSpec;
            if (statementSpec.Raw.OptionalContextName != null) {
                throw new ExprValidationException(
                    "A create-context statement cannot itself be associated to a context, please declare a nested context instead");
            }

            IList<FilterSpecTracked> filterSpecCompileds = new List<FilterSpecTracked>();
            IList<ScheduleHandleTracked> scheduleHandleCallbackProviders = new List<ScheduleHandleTracked>();
            IList<FilterSpecParamExprNodeForge> filterBooleanExpressions = new List<FilterSpecParamExprNodeForge>();
            var fabricCharge = services.StateMgmtSettingsProvider.NewCharge();

            var context = statementSpec.Raw.CreateContextDesc;
            if (services.ContextCompileTimeResolver.GetContextInfo(context.ContextName) != null) {
                throw new ExprValidationException("Context by name '" + context.ContextName + "' already exists");
            }

            // compile filter specs, if any
            var contextVisibility =
                services.ModuleVisibilityRules.GetAccessModifierContext(_base, context.ContextName);
            var validationEnv = new CreateContextValidationEnv(
                context.ContextName,
                contextVisibility,
                _base.StatementRawInfo,
                services,
                filterSpecCompileds,
                scheduleHandleCallbackProviders,
                filterBooleanExpressions);
            var validationResult = ValidateContextDetail(
                context.ContextDetail,
                0,
                validationEnv);
            var additionalForgeables = validationResult.AdditionalForgables;
            fabricCharge.Add(validationResult.FabricCharge);

            // get controller factory forges
            var controllerFactoryForges = GetForges(
                context.ContextName,
                context.ContextDetail);

            // build context properties type information
            var contextProps = MakeContextProperies(
                controllerFactoryForges,
                _base.StatementRawInfo,
                services);

            // allocate type for context properties
            var contextEventTypeName =
                services.EventTypeNameGeneratorStatement.GetContextPropertyTypeName(context.ContextName);
            var metadata = new EventTypeMetadata(
                contextEventTypeName,
                _base.ModuleName,
                EventTypeTypeClass.CONTEXTPROPDERIVED,
                EventTypeApplicationType.MAP,
                NameAccessModifier.TRANSIENT,
                EventTypeBusModifier.NONBUS,
                false,
                EventTypeIdPair.Unassigned());
            var contextPropertiesType = BaseNestableEventUtil.MakeMapTypeCompileTime(
                metadata,
                contextProps,
                null,
                null,
                null,
                null,
                services.BeanEventTypeFactoryPrivate,
                services.EventTypeCompileTimeResolver);
            services.EventTypeCompileTimeRegistry.NewType(contextPropertiesType);

            // register context
            var validationInfo =
                new ContextControllerPortableInfo[controllerFactoryForges.Length];
            for (var i = 0; i < validationInfo.Length; i++) {
                validationInfo[i] = controllerFactoryForges[i].ValidationInfo;
            }

            var detail = new ContextMetaData(
                context.ContextName,
                _base.ModuleName,
                contextVisibility,
                contextPropertiesType,
                validationInfo);
            services.ContextCompileTimeRegistry.NewContext(detail);

            // build context properties type information
            fabricCharge.Add(PlanStateSettings(detail, controllerFactoryForges, _base.StatementRawInfo, services));

            // define output event type
            var statementEventTypeName =
                services.EventTypeNameGeneratorStatement.GetContextStatementTypeName(context.ContextName);
            var statementTypeMetadata = new EventTypeMetadata(
                statementEventTypeName,
                _base.ModuleName,
                EventTypeTypeClass.STATEMENTOUT,
                EventTypeApplicationType.MAP,
                NameAccessModifier.TRANSIENT,
                EventTypeBusModifier.NONBUS,
                false,
                EventTypeIdPair.Unassigned());
            EventType statementEventType = BaseNestableEventUtil.MakeMapTypeCompileTime(
                statementTypeMetadata,
                EmptyDictionary<string, object>.Instance, 
                null,
                null,
                null,
                null,
                services.BeanEventTypeFactoryPrivate,
                services.EventTypeCompileTimeResolver);
            services.EventTypeCompileTimeRegistry.NewType(statementEventType);

            var statementFieldsClassName =
                CodeGenerationIDGenerator.GenerateClassNameSimple(typeof(StatementFields), classPostfix);
            var namespaceScope = new CodegenNamespaceScope(
                @namespace,
                statementFieldsClassName,
                services.IsInstrumented,
                services.Configuration.Compiler.ByteCode);

            IList<StmtClassForgeable> forgeables = new List<StmtClassForgeable>();
            foreach (var additional in additionalForgeables) {
                forgeables.Add(additional.Make(namespaceScope, classPostfix));
            }

            var statementProviderClassName =
                CodeGenerationIDGenerator.GenerateClassNameSimple(typeof(StatementProvider), classPostfix);
            var statementAIFactoryProviderClassName =
                CodeGenerationIDGenerator.GenerateClassNameSimple(typeof(StatementAIFactoryProvider), classPostfix);

            var forge =
                new StatementAgentInstanceFactoryCreateContextForge(context.ContextName, statementEventType);
            var partitionIdSvcStateMgmtSettings = services.StateMgmtSettingsProvider.Context
                .ContextPartitionId(fabricCharge, _base.StatementRawInfo, detail);
            forgeables.Add(
                new StmtClassForgeableAIFactoryProviderCreateContext(
                    statementAIFactoryProviderClassName,
                    namespaceScope,
                    context.ContextName,
                    controllerFactoryForges,
                    contextPropertiesType,
                    forge,
                    partitionIdSvcStateMgmtSettings));

            var selectSubscriberDescriptor = new SelectSubscriberDescriptor();
            var informationals = StatementInformationalsUtil.GetInformationals(
                _base,
                filterSpecCompileds,
                scheduleHandleCallbackProviders,
                EmptyList<NamedWindowConsumerStreamSpec>.Instance, 
                false,
                selectSubscriberDescriptor,
                namespaceScope,
                services);
            informationals.Properties.Put(StatementProperty.CREATEOBJECTNAME, context.ContextName);
            forgeables.Add(
                new StmtClassForgeableStmtProvider(
                    statementAIFactoryProviderClassName,
                    statementProviderClassName,
                    informationals,
                    namespaceScope));
            forgeables.Add(new StmtClassForgeableStmtFields(statementFieldsClassName, namespaceScope));

            services.StateMgmtSettingsProvider.Context.Context(fabricCharge, detail, controllerFactoryForges);

            return new StmtForgeMethodResult(
                forgeables,
                filterSpecCompileds,
                scheduleHandleCallbackProviders,
                EmptyList<NamedWindowConsumerStreamSpec>.Instance, 
                FilterSpecCompiled.MakeExprNodeList(filterSpecCompileds, filterBooleanExpressions),
                namespaceScope,
                fabricCharge);
        }

        private IDictionary<string, object> MakeContextProperies(
            ContextControllerFactoryForge[] controllers,
            StatementRawInfo statementRawInfo,
            StatementCompileTimeServices services)
        {
            var props = new LinkedHashMap<string, object>();
            props.Put(PROP_CTX_NAME, typeof(string));
            props.Put(PROP_CTX_ID, typeof(int?));

            if (controllers.Length == 1) {
                controllers[0]
                    .ValidateGetContextProps(
                        props,
                        controllers[0].FactoryEnv.OutermostContextName,
                        0,
                        statementRawInfo,
                        services);
                return props;
            }

            for (var level = 0; level < controllers.Length; level++) {
                var nestedContextName = controllers[level].FactoryEnv.ContextName;
                var propsPerLevel = new LinkedHashMap<string, object>();
                propsPerLevel.Put(PROP_CTX_NAME, typeof(string));
                if (level == controllers.Length - 1) {
                    propsPerLevel.Put(PROP_CTX_ID, typeof(int?));
                }

                controllers[level]
                    .ValidateGetContextProps(propsPerLevel, nestedContextName, level, statementRawInfo, services);
                props.Put(nestedContextName, propsPerLevel);
            }

            return props;
        }

        private FabricCharge PlanStateSettings(
            ContextMetaData detail,
            ContextControllerFactoryForge[] controllers,
            StatementRawInfo statementRawInfo,
            StatementCompileTimeServices services)
        {
            var fabricCharge = services.StateMgmtSettingsProvider.NewCharge();
            for (var level = 0; level < controllers.Length; level++) {
                var nestedContextName = controllers[level].FactoryEnv.ContextName;
                controllers[level]
                    .PlanStateSettings(detail, fabricCharge, level, nestedContextName, statementRawInfo, services);
            }

            return fabricCharge;
        }

        private ValidateContextDetailResult ValidateContextDetail(
            ContextSpec contextSpec,
            int nestingLevel,
            CreateContextValidationEnv validationEnv)
        {
            IList<StmtClassForgeableFactory> additionalForgeables = new List<StmtClassForgeableFactory>(2);
            var fabricCharge = validationEnv.Services.StateMgmtSettingsProvider.NewCharge();

            if (contextSpec is ContextSpecKeyed segmented) {
                var asNames = new Dictionary<string, EventType>();
                var partitionHasNameAssignment = false;
                Type[] getterTypes = null;
                foreach (var partition in segmented.Items) {
                    var pair = CompilePartitonedFilterSpec(
                        partition.FilterSpecRaw,
                        nestingLevel,
                        validationEnv);
                    var filterSpecCompiled = pair.First;
                    additionalForgeables.AddAll(pair.Second);
                    partition.FilterSpecCompiled = filterSpecCompiled;

                    var getters = new EventPropertyGetterSPI[partition.PropertyNames.Count];
                    var serdes = new DataInputOutputSerdeForge[partition.PropertyNames.Count];
                    var eventType = (EventTypeSPI)filterSpecCompiled.FilterForEventType;
                    getterTypes = new Type[partition.PropertyNames.Count];
                    for (var i = 0; i < partition.PropertyNames.Count; i++) {
                        var propertyName = partition.PropertyNames[i];
                        var getter = eventType.GetGetterSPI(propertyName);
                        if (getter == null) {
                            throw new ExprValidationException(
                                "For context '" +
                                validationEnv.ContextName +
                                "' property name '" +
                                propertyName +
                                "' not found on type " +
                                eventType.Name);
                        }

                        getters[i] = getter;
                        getterTypes[i] = eventType.GetPropertyType(propertyName);
                        serdes[i] = validationEnv.Services.SerdeResolver.SerdeForFilter(
                            getterTypes[i],
                            validationEnv.StatementRawInfo);
                    }

                    partition.Getters = getters;
                    partition.LookupableSerdes = serdes;

                    if (partition.AliasName != null) {
                        partitionHasNameAssignment = true;
                        ValidateAsName(asNames, partition.AliasName, filterSpecCompiled.FilterForEventType);
                    }
                }

                validationEnv.Services.StateMgmtSettingsProvider.Context
                    .FilterContextKeyed(nestingLevel, fabricCharge, segmented.Items);

                // plan multi-key, make sure we use the same multikey for all items
                var multiKeyPlan = MultiKeyPlanner.PlanMultiKey(
                    getterTypes,
                    false,
                    _base.StatementRawInfo,
                    validationEnv.Services.SerdeResolver);
                additionalForgeables.AddAll(multiKeyPlan.MultiKeyForgeables);
                foreach (var partition in segmented.Items) {
                    partition.KeyMultiKey = multiKeyPlan.ClassRef;
                }

                segmented.MultiKeyClassRef = multiKeyPlan.ClassRef;

                if (segmented.OptionalInit != null) {
                    asNames.Clear();
                    foreach (var initCondition in segmented.OptionalInit) {
                        var pair = ValidateRewriteContextCondition(
                            true,
                            true,
                            nestingLevel,
                            initCondition,
                            new MatchEventSpec(),
                            EmptySet<string>.Instance,
                            validationEnv);
                        additionalForgeables.AddAll(pair.AdditionalForgeables);
                        fabricCharge.Add(pair.FabricCharge);

                        var filterForType = initCondition.FilterSpecCompiled.FilterForEventType;
                        var found = false;
                        foreach (var partition in segmented.Items) {
                            if (partition.FilterSpecCompiled.FilterForEventType == filterForType) {
                                found = true;
                                break;
                            }
                        }

                        if (!found) {
                            throw new ExprValidationException(
                                "Segmented context '" +
                                validationEnv.ContextName +
                                "' requires that all of the event types that are listed in the initialized-by also appear in the partition-by, Type '" +
                                filterForType.Name +
                                "' is not one of the types listed in partition-by");
                        }

                        if (initCondition.OptionalFilterAsName != null) {
                            if (partitionHasNameAssignment) {
                                throw new ExprValidationException(
                                    "Segmented context '" +
                                    validationEnv.ContextName +
                                    "' requires that either partition-by or initialized-by assign stream names, but not both");
                            }

                            ValidateAsName(asNames, initCondition.OptionalFilterAsName, filterForType);
                        }
                    }
                }

                if (segmented.OptionalTermination != null) {
                    var matchEventSpec = new MatchEventSpec();
                    var allTags = new LinkedHashSet<string>();
                    foreach (var partition in segmented.Items) {
                        if (partition.AliasName != null) {
                            allTags.Add(partition.AliasName);
                            var eventType = partition.FilterSpecCompiled.FilterForEventType;
                            matchEventSpec.TaggedEventTypes.Put(
                                partition.AliasName,
                                new Pair<EventType, string>(eventType, partition.FilterSpecRaw.EventTypeName));
                            var serdeForgeables = SerdeEventTypeUtility.Plan(
                                eventType,
                                validationEnv.StatementRawInfo,
                                validationEnv.Services.SerdeEventTypeRegistry,
                                validationEnv.Services.SerdeResolver,
                                validationEnv.Services.StateMgmtSettingsProvider);
                            additionalForgeables.AddAll(serdeForgeables);
                        }
                    }

                    if (segmented.OptionalInit != null) {
                        foreach (var initCondition in segmented.OptionalInit) {
                            if (initCondition.OptionalFilterAsName != null) {
                                allTags.Add(initCondition.OptionalFilterAsName);
                                matchEventSpec.TaggedEventTypes.Put(
                                    initCondition.OptionalFilterAsName,
                                    new Pair<EventType, string>(
                                        initCondition.FilterSpecCompiled.FilterForEventType,
                                        initCondition.FilterSpecRaw.EventTypeName));
                            }
                        }
                    }

                    var endCondition = ValidateRewriteContextCondition(
                        false,
                        true,
                        nestingLevel,
                        segmented.OptionalTermination,
                        matchEventSpec,
                        allTags,
                        validationEnv);
                    fabricCharge.Add(endCondition.FabricCharge);
                    additionalForgeables.AddAll(endCondition.AdditionalForgeables);
                    segmented.OptionalTermination = endCondition.Condition;
                }
            }
            else if (contextSpec is ContextSpecCategory category) {
                // compile filter
                ValidateNotTable(category.FilterSpecRaw.EventTypeName, validationEnv.Services);
                var raw = new FilterStreamSpecRaw(
                    category.FilterSpecRaw,
                    ViewSpec.EMPTY_VIEWSPEC_ARRAY,
                    null,
                    StreamSpecOptions.DEFAULT);
                var compiledDesc = StreamSpecCompiler.CompileFilter(
                    raw,
                    null,
                    validationEnv.StatementRawInfo,
                    validationEnv.Services);
                var result = (FilterStreamSpecCompiled)compiledDesc.StreamSpecCompiled;
                additionalForgeables.AddAll(compiledDesc.AdditionalForgeables);
                category.FilterSpecCompiled = result.FilterSpecCompiled;
                validationEnv.FilterSpecCompileds.Add(
                    new FilterSpecTracked(
                        new CallbackAttributionContextController(nestingLevel),
                        result.FilterSpecCompiled));

                // compile expressions
                foreach (var item in category.Items) {
                    ValidateNotTable(category.FilterSpecRaw.EventTypeName, validationEnv.Services);
                    var filterSpecRaw = new FilterSpecRaw(
                        category.FilterSpecRaw.EventTypeName,
                        Collections.SingletonList(item.Expression),
                        null);
                    var rawExpr = new FilterStreamSpecRaw(
                        filterSpecRaw,
                        ViewSpec.EMPTY_VIEWSPEC_ARRAY,
                        null,
                        StreamSpecOptions.DEFAULT);
                    var compiledDescItems = StreamSpecCompiler.CompileFilter(
                        rawExpr,
                        null,
                        validationEnv.StatementRawInfo,
                        validationEnv.Services);
                    var compiled = (FilterStreamSpecCompiled)compiledDescItems.StreamSpecCompiled;
                    additionalForgeables.AddAll(compiledDescItems.AdditionalForgeables);
                    compiled.FilterSpecCompiled.TraverseFilterBooleanExpr(
                        validationEnv.FilterBooleanExpressions.Add);
                    item.FilterPlan = compiled.FilterSpecCompiled.Parameters;
                    validationEnv.FilterSpecCompileds.Add(
                        new FilterSpecTracked(
                            new CallbackAttributionContextController(nestingLevel),
                            compiled.FilterSpecCompiled));
                }
            }
            else if (contextSpec is ContextSpecHash hashed) {
                foreach (var hashItem in hashed.Items) {
                    var raw = new FilterStreamSpecRaw(
                        hashItem.FilterSpecRaw,
                        ViewSpec.EMPTY_VIEWSPEC_ARRAY,
                        null,
                        StreamSpecOptions.DEFAULT);
                    ValidateNotTable(hashItem.FilterSpecRaw.EventTypeName, validationEnv.Services);
                    var compiledDesc = StreamSpecCompiler.Compile(
                        raw,
                        false,
                        true,
                        false,
                        null,
                        0,
                        validationEnv.StatementRawInfo,
                        validationEnv.Services);
                    additionalForgeables.AddAll(compiledDesc.AdditionalForgeables);
                    var result = (FilterStreamSpecCompiled)compiledDesc.StreamSpecCompiled;
                    validationEnv.FilterSpecCompileds.Add(
                        new FilterSpecTracked(
                            new CallbackAttributionContextController(nestingLevel),
                            result.FilterSpecCompiled));
                    hashItem.FilterSpecCompiled = result.FilterSpecCompiled;

                    // validate parameters
                    var streamTypes = new StreamTypeServiceImpl(
                        result.FilterSpecCompiled.FilterForEventType,
                        null,
                        true);
                    var validationContext =
                        new ExprValidationContextBuilder(
                                streamTypes,
                                validationEnv.StatementRawInfo,
                                validationEnv.Services).WithIsFilterExpression(true)
                            .Build();
                    ExprNodeUtilityValidate.Validate(
                        ExprNodeOrigin.CONTEXT,
                        Collections.SingletonList(hashItem.Function),
                        validationContext);
                }

                ContextControllerHashUtil.ValidateContextDesc(
                    validationEnv.ContextName,
                    hashed,
                    validationEnv.StatementRawInfo,
                    validationEnv.Services);
                validationEnv.Services.StateMgmtSettingsProvider.Context
                    .FilterContextHash(nestingLevel, fabricCharge, hashed.Items);
            }
            else if (contextSpec is ContextSpecInitiatedTerminated def) {
                var startCondition = ValidateRewriteContextCondition(
                    true,
                    false,
                    nestingLevel,
                    def.StartCondition,
                    new MatchEventSpec(),
                    new LinkedHashSet<string>(),
                    validationEnv);
                additionalForgeables.AddAll(startCondition.AdditionalForgeables);
                fabricCharge.Add(startCondition.FabricCharge);

                var endCondition = ValidateRewriteContextCondition(
                    false,
                    false,
                    nestingLevel,
                    def.EndCondition,
                    startCondition.Matches,
                    startCondition.AllTags,
                    validationEnv);
                additionalForgeables.AddAll(endCondition.AdditionalForgeables);
                fabricCharge.Add(endCondition.FabricCharge);
                def.StartCondition = startCondition.Condition;
                def.EndCondition = endCondition.Condition;

                if (def.DistinctExpressions != null) {
                    if (!(startCondition.Condition is ContextSpecConditionFilter)) {
                        throw new ExprValidationException(
                            "Distinct-expressions require a stream as the initiated-by condition");
                    }

                    var distinctExpressions = def.DistinctExpressions;
                    if (distinctExpressions.Length == 0) {
                        throw new ExprValidationException("Distinct-expressions have not been provided");
                    }

                    var filter = (ContextSpecConditionFilter)startCondition.Condition;
                    if (filter.OptionalFilterAsName == null) {
                        throw new ExprValidationException(
                            "Distinct-expressions require that a stream name is assigned to the stream using 'as'");
                    }

                    var types = new StreamTypeServiceImpl(
                        filter.FilterSpecCompiled.FilterForEventType,
                        filter.OptionalFilterAsName,
                        true);
                    var validationContext =
                        new ExprValidationContextBuilder(types, validationEnv.StatementRawInfo, validationEnv.Services)
                            .WithAllowBindingConsumption(true)
                            .Build();
                    for (var i = 0; i < distinctExpressions.Length; i++) {
                        ExprNodeUtilityValidate.ValidatePlainExpression(
                            ExprNodeOrigin.CONTEXTDISTINCT,
                            distinctExpressions[i]);
                        distinctExpressions[i] = ExprNodeUtilityValidate.GetValidatedSubtree(
                            ExprNodeOrigin.CONTEXTDISTINCT,
                            distinctExpressions[i],
                            validationContext);
                    }

                    var multiKeyPlan = MultiKeyPlanner.PlanMultiKey(
                        distinctExpressions,
                        false,
                        _base.StatementRawInfo,
                        validationEnv.Services.SerdeResolver);
                    def.DistinctMultiKey = multiKeyPlan.ClassRef;
                    additionalForgeables.AddAll(multiKeyPlan.MultiKeyForgeables);
                }
            }
            else if (contextSpec is ContextNested nested) {
                var level = 0;
                ISet<string> namesUsed = new HashSet<string>();
                namesUsed.Add(validationEnv.ContextName);
                foreach (var nestedContext in nested.Contexts) {
                    if (namesUsed.Contains(nestedContext.ContextName)) {
                        throw new ExprValidationException(
                            "Context by name '" +
                            nestedContext.ContextName +
                            "' has already been declared within nested context '" +
                            validationEnv.ContextName +
                            "'");
                    }

                    namesUsed.Add(nestedContext.ContextName);

                    var result = ValidateContextDetail(
                        nestedContext.ContextDetail,
                        level,
                        validationEnv);
                    additionalForgeables.AddAll(result.AdditionalForgables);
                    fabricCharge.Add(result.FabricCharge);

                    level++;
                }
            }
            else {
                throw new IllegalStateException("Unrecognized context detail " + contextSpec);
            }

            return new ValidateContextDetailResult(additionalForgeables, fabricCharge);
        }

        private Pair<FilterSpecCompiled, IList<StmtClassForgeableFactory>> CompilePartitonedFilterSpec(
            FilterSpecRaw filterSpecRaw,
            int nestingLevel,
            CreateContextValidationEnv validationEnv)
        {
            ValidateNotTable(filterSpecRaw.EventTypeName, validationEnv.Services);
            var raw = new FilterStreamSpecRaw(
                filterSpecRaw,
                ViewSpec.EMPTY_VIEWSPEC_ARRAY,
                null,
                StreamSpecOptions.DEFAULT);
            var compiledDesc = StreamSpecCompiler.Compile(
                raw,
                false,
                true,
                false,
                null,
                0,
                validationEnv.StatementRawInfo,
                validationEnv.Services);
            if (!(compiledDesc.StreamSpecCompiled is FilterStreamSpecCompiled filters)) {
                throw new ExprValidationException("Partition criteria may not include named windows");
            }

            var spec = filters.FilterSpecCompiled;
            validationEnv.FilterSpecCompileds.Add(
                new FilterSpecTracked(new CallbackAttributionContextController(nestingLevel), spec));
            return new Pair<FilterSpecCompiled, IList<StmtClassForgeableFactory>>(spec, compiledDesc.AdditionalForgeables);
        }

        private void ValidateNotTable(
            string eventTypeName,
            StatementCompileTimeServices services)
        {
            if (services.TableCompileTimeResolver.Resolve(eventTypeName) != null) {
                throw new ExprValidationException("Tables cannot be used in a context declaration");
            }
        }

        private void ValidateAsName(
            IDictionary<string, EventType> asNames,
            string asName,
            EventType filterForType)
        {
            var existing = asNames.Get(asName);
            if (existing != null && !EventTypeUtility.IsTypeOrSubTypeOf(filterForType, existing)) {
                throw new ExprValidationException(
                    "Name '" + asName + "' already used for type '" + existing.Name + "'");
            }

            if (existing == null) {
                asNames.Put(asName, filterForType);
            }
        }

        private ContextControllerFactoryForge[] GetForges(
            string contextName,
            ContextSpec contextDetail)
        {
            if (!(contextDetail is ContextNested nested)) {
                var factoryEnv = new ContextControllerFactoryEnv(
                    contextName,
                    contextName,
                    1,
                    1);
                return new ContextControllerFactoryForge[] { Make(factoryEnv, contextDetail) };
            }

            var forges = new ContextControllerFactoryForge[nested.Contexts.Count];
            var nestingLevel = 1;
            foreach (var desc in nested.Contexts) {
                var factoryEnv = new ContextControllerFactoryEnv(
                    contextName,
                    desc.ContextName,
                    nestingLevel,
                    nested.Contexts.Count);
                forges[nestingLevel - 1] = Make(factoryEnv, desc.ContextDetail);
                nestingLevel++;
            }

            return forges;
        }

        private ContextControllerFactoryForge Make(
            ContextControllerFactoryEnv factoryContext,
            ContextSpec detail)
        {
            ContextControllerFactoryForge forge;
            if (detail is ContextSpecInitiatedTerminated terminated) {
                forge = new ContextControllerInitTermFactoryForge(
                    factoryContext,
                    terminated);
            }
            else if (detail is ContextSpecKeyed keyed) {
                forge = new ContextControllerKeyedFactoryForge(factoryContext, keyed);
            }
            else if (detail is ContextSpecCategory category) {
                forge = new ContextControllerCategoryFactoryForge(factoryContext, category);
            }
            else if (detail is ContextSpecHash hash) {
                forge = new ContextControllerHashFactoryForge(factoryContext, hash);
            }
            else {
                throw new UnsupportedOperationException(
                    "Context detail " + detail + " is not yet supported in a nested context");
            }

            return forge;
        }

        private ContextDetailMatchPair ValidateRewriteContextCondition(
            bool startCondition,
            bool keyed,
            int nestingLevel,
            ContextSpecCondition endpoint,
            MatchEventSpec priorMatches,
            ISet<string> priorAllTags,
            CreateContextValidationEnv validationEnv)
        {
            var fabricCharge = validationEnv.Services.StateMgmtSettingsProvider.NewCharge();
            if (endpoint is ContextSpecConditionCrontab crontab) {
                var forgesPerCrontab = new ExprForge[crontab.Crontabs.Count][];
                for (var i = 0; i < crontab.Crontabs.Count; i++) {
                    var item = crontab.Crontabs[i];
                    var forges = ScheduleExpressionUtil.CrontabScheduleValidate(
                        ExprNodeOrigin.CONTEXTCONDITION,
                        item,
                        false,
                        validationEnv.StatementRawInfo,
                        validationEnv.Services);
                    forgesPerCrontab[i] = forges;
                }

                crontab.ForgesPerCrontab = forgesPerCrontab;
                var tracked = new ScheduleHandleTracked(
                    new CallbackAttributionContextCondition(nestingLevel, startCondition),
                    crontab);
                validationEnv.ScheduleHandleCallbackProviders.Add(tracked);
                return new ContextDetailMatchPair(
                    crontab,
                    new MatchEventSpec(),
                    new LinkedHashSet<string>(),
                    EmptyList<StmtClassForgeableFactory>.Instance, 
                    fabricCharge);
            }

            if (endpoint is ContextSpecConditionTimePeriod timePeriod) {
                var validationContext = new ExprValidationContextBuilder(
                    new StreamTypeServiceImpl(false),
                    validationEnv.StatementRawInfo,
                    validationEnv.Services).Build();
                ExprNodeUtilityValidate.GetValidatedSubtree(
                    ExprNodeOrigin.CONTEXTCONDITION,
                    timePeriod.TimePeriod,
                    validationContext);
                if (timePeriod.TimePeriod.IsConstantResult) {
                    if (timePeriod.TimePeriod.EvaluateAsSeconds(null, true, null) < 0) {
                        throw new ExprValidationException(
                            "Invalid negative time period expression '" +
                            ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceSafe(timePeriod.TimePeriod) +
                            "'");
                    }
                }

                var tracked = new ScheduleHandleTracked(
                    new CallbackAttributionContextCondition(nestingLevel, startCondition),
                    timePeriod);
                validationEnv.ScheduleHandleCallbackProviders.Add(tracked);
                return new ContextDetailMatchPair(
                    timePeriod,
                    new MatchEventSpec(),
                    new LinkedHashSet<string>(),
                    EmptyList<StmtClassForgeableFactory>.Instance, 
                    fabricCharge);
            }

            if (endpoint is ContextSpecConditionPattern condition) {
                var validatedDesc = ValidatePatternContextConditionPattern(
                    startCondition,
                    keyed,
                    nestingLevel,
                    condition,
                    priorMatches,
                    priorAllTags,
                    validationEnv);
                return new ContextDetailMatchPair(
                    condition,
                    validatedDesc.MatchEventSpec,
                    validatedDesc.AllTags,
                    validatedDesc.AdditionalForgeables,
                    validatedDesc.FabricCharge);
            }

            if (endpoint is ContextSpecConditionFilter filter) {
                ValidateNotTable(filter.FilterSpecRaw.EventTypeName, validationEnv.Services);

                // compile as filter if there are no prior match to consider
                if (priorMatches == null ||
                    (priorMatches.ArrayEventTypes.IsEmpty() && priorMatches.TaggedEventTypes.IsEmpty())) {
                    var rawExpr = new FilterStreamSpecRaw(
                        filter.FilterSpecRaw,
                        ViewSpec.EMPTY_VIEWSPEC_ARRAY,
                        null,
                        StreamSpecOptions.DEFAULT);
                    var compiledDesc = StreamSpecCompiler.Compile(
                        rawExpr,
                        false,
                        true,
                        false,
                        filter.OptionalFilterAsName,
                        0,
                        validationEnv.StatementRawInfo,
                        validationEnv.Services);
                    var compiled = (FilterStreamSpecCompiled)compiledDesc.StreamSpecCompiled;
                    filter.FilterSpecCompiled = compiled.FilterSpecCompiled;
                    var matchEventSpec = new MatchEventSpec();
                    var filterForType = compiled.FilterSpecCompiled.FilterForEventType;
                    var allTags = new LinkedHashSet<string>();
                    if (filter.OptionalFilterAsName != null) {
                        matchEventSpec.TaggedEventTypes.Put(
                            filter.OptionalFilterAsName,
                            new Pair<EventType, string>(filterForType, rawExpr.RawFilterSpec.EventTypeName));
                        allTags.Add(filter.OptionalFilterAsName);
                    }

                    validationEnv.FilterSpecCompileds.Add(
                        new FilterSpecTracked(
                            new CallbackAttributionContextCondition(nestingLevel, startCondition),
                            compiled.FilterSpecCompiled));
                    var serdeForgeables = SerdeEventTypeUtility.Plan(
                        filter.FilterSpecCompiled.FilterForEventType,
                        validationEnv.StatementRawInfo,
                        validationEnv.Services.SerdeEventTypeRegistry,
                        validationEnv.Services.SerdeResolver,
                        validationEnv.Services.StateMgmtSettingsProvider);
                    IList<StmtClassForgeableFactory> allForgeables = compiledDesc.AdditionalForgeables
                        .Concat(serdeForgeables)
                        .ToList();
                    return new ContextDetailMatchPair(filter, matchEventSpec, allTags, allForgeables, fabricCharge);
                }

                // compile as pattern if there are prior matches to consider, since this is a type of followed-by relationship
                EvalForgeNode forgeNode = new EvalFilterForgeNode(
                    validationEnv.Services.IsAttachPatternText,
                    filter.FilterSpecRaw,
                    filter.OptionalFilterAsName,
                    0);
                var pattern = new ContextSpecConditionPattern(forgeNode, true, false, null);
                var validated = ValidatePatternContextConditionPattern(
                    startCondition,
                    keyed,
                    nestingLevel,
                    pattern,
                    priorMatches,
                    priorAllTags,
                    validationEnv);
                return new ContextDetailMatchPair(
                    pattern,
                    validated.MatchEventSpec,
                    validated.AllTags,
                    validated.AdditionalForgeables,
                    validated.FabricCharge);
            }
            else if (endpoint is ContextSpecConditionImmediate || endpoint is ContextSpecConditionNever) {
                return new ContextDetailMatchPair(
                    endpoint,
                    new MatchEventSpec(),
                    new LinkedHashSet<string>(),
                    EmptyList<StmtClassForgeableFactory>.Instance, 
                    fabricCharge);
            }
            else {
                throw new IllegalStateException("Unrecognized endpoint type " + endpoint);
            }
        }

        private PatternValidatedDesc ValidatePatternContextConditionPattern(
            bool startCondition,
            bool keyed,
            int nestingLevel,
            ContextSpecConditionPattern pattern,
            MatchEventSpec priorMatches,
            ISet<string> priorAllTags,
            CreateContextValidationEnv validationEnv)
        {
            var raw = new PatternStreamSpecRaw(
                pattern.PatternRaw,
                ViewSpec.EMPTY_VIEWSPEC_ARRAY,
                null,
                StreamSpecOptions.DEFAULT,
                false,
                false);
            var streamNumber = GetStreamNumberForNestingLevel(nestingLevel, startCondition);
            IList<StmtClassForgeableFactory> additionalForgeables = new List<StmtClassForgeableFactory>(2);
            var compiledDesc = StreamSpecCompiler.CompilePatternWTags(
                raw,
                priorMatches,
                priorAllTags,
                false,
                true,
                false,
                streamNumber,
                validationEnv.StatementRawInfo,
                validationEnv.Services);
            additionalForgeables.AddAll(compiledDesc.AdditionalForgeables);
            var compiled = (PatternStreamSpecCompiled)compiledDesc.StreamSpecCompiled;
            pattern.PatternCompiled = compiled;

            pattern.PatternContext = new PatternContext(
                streamNumber,
                compiled.MatchedEventMapMeta,
                true,
                nestingLevel,
                startCondition);

            var forges = compiled.Root.CollectFactories();
            foreach (var forge in forges) {
                forge.CollectSelfFilterAndSchedule(
                    factoryNodeId => new CallbackAttributionContextConditionPattern(
                        nestingLevel,
                        startCondition,
                        factoryNodeId),
                    validationEnv.FilterSpecCompileds,
                    validationEnv.ScheduleHandleCallbackProviders);
            }

            MatchEventSpec matchEventSpec;
            ISet<string> allTags;
            if (pattern.AsName == null) {
                matchEventSpec = new MatchEventSpec(compiled.TaggedEventTypes, compiled.ArrayEventTypes);
                allTags = compiled.AllTags;
            }
            else {
                var patternTags = FilterSpecCompilerTagUtil.GetTagNumbers(pattern.PatternRaw);
                pattern.PatternTags = patternTags.ToArray();

                var streamNameForNaming = GetStreamNumberForNestingLevel(nestingLevel, startCondition);
                var patternType = ViewableActivatorPatternForge.MakeRegisterPatternType(
                    validationEnv.StatementRawInfo.ModuleName,
                    streamNameForNaming,
                    patternTags,
                    pattern.PatternCompiled,
                    validationEnv.Services);
                var serdeForgeables = SerdeEventTypeUtility.Plan(
                    patternType,
                    validationEnv.StatementRawInfo,
                    validationEnv.Services.SerdeEventTypeRegistry,
                    validationEnv.Services.SerdeResolver,
                    validationEnv.Services.StateMgmtSettingsProvider);
                additionalForgeables.AddAll(serdeForgeables);

                pattern.AsNameEventType = patternType;

                matchEventSpec = new MatchEventSpec();
                matchEventSpec.TaggedEventTypes.Put(
                    pattern.AsName,
                    new Pair<EventType, string>(patternType, patternType.Name));
                allTags = new LinkedHashSet<string>();
                allTags.Add(pattern.AsName);
            }

            var fabricCharge = validationEnv.Services.StateMgmtSettingsProvider.NewCharge();
            var attributionKey = new PatternAttributionKeyContextCondition(
                validationEnv.ContextName,
                validationEnv.ContextVisibility,
                validationEnv.StatementRawInfo.ModuleName,
                nestingLevel,
                startCondition,
                keyed);
            validationEnv.Services.StateMgmtSettingsProvider.Pattern(
                fabricCharge,
                attributionKey,
                compiled,
                validationEnv.StatementRawInfo);
            return new PatternValidatedDesc(matchEventSpec, allTags, additionalForgeables, fabricCharge);
        }
    }
} // end of namespace