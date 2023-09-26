///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.soda;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.core;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.compile.util;
using com.espertech.esper.common.@internal.context.activator;
using com.espertech.esper.common.@internal.context.aifactory.ontrigger.onset;
using com.espertech.esper.common.@internal.context.aifactory.ontrigger.onsplit;
using com.espertech.esper.common.@internal.context.aifactory.ontrigger.ontrigger;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.subquery;
using com.espertech.esper.common.@internal.epl.join.querygraph;
using com.espertech.esper.common.@internal.epl.namedwindow.path;
using com.espertech.esper.common.@internal.epl.pattern.core;
using com.espertech.esper.common.@internal.epl.streamtype;
using com.espertech.esper.common.@internal.epl.subselect;
using com.espertech.esper.common.@internal.epl.table.compiletime;
using com.espertech.esper.common.@internal.epl.util;
using com.espertech.esper.common.@internal.@event.map;
using com.espertech.esper.common.@internal.fabric;
using com.espertech.esper.common.@internal.filterspec;
using com.espertech.esper.common.@internal.schedule;
using com.espertech.esper.compat.collections;


namespace com.espertech.esper.common.@internal.context.aifactory.ontrigger.core
{
    public class StmtForgeMethodOnTrigger : StmtForgeMethod
    {
        private readonly StatementBaseInfo @base;

        public StmtForgeMethodOnTrigger(StatementBaseInfo @base)
        {
            this.@base = @base;
        }

        public StmtForgeMethodResult Make(
            string @namespace,
            string classPostfix,
            StatementCompileTimeServices services)
        {
            // determine context
            var contextName = @base.StatementSpec.Raw.OptionalContextName;

            IList<FilterSpecTracked> filterSpecCompileds = new List<FilterSpecTracked>(2);
            IList<ScheduleHandleTracked> schedules = new List<ScheduleHandleTracked>(2);
            IList<NamedWindowConsumerStreamSpec> namedWindowConsumers = new List<NamedWindowConsumerStreamSpec>(2);
            IList<StmtClassForgeableFactory> additionalForgeables = new List<StmtClassForgeableFactory>(2);
            var fabricCharge = services.StateMgmtSettingsProvider.NewCharge();

            // create subselect information
            var subSelectActivationDesc = SubSelectHelperActivations.CreateSubSelectActivation(
                false,
                filterSpecCompileds,
                namedWindowConsumers,
                @base,
                services);
            var subselectActivation = subSelectActivationDesc.Subselects;
            additionalForgeables.AddAll(subSelectActivationDesc.AdditionalForgeables);
            fabricCharge.Add(subSelectActivationDesc.FabricCharge);

            // obtain activator
            var streamSpec = @base.StatementSpec.StreamSpecs[0];
            StreamSelector? optionalStreamSelector = null;
            OnTriggerActivatorDesc activatorResult;

            if (streamSpec is FilterStreamSpecCompiled filterStreamSpec) {
                activatorResult = ActivatorFilter(filterStreamSpec, services);
                filterSpecCompileds.Add(
                    new FilterSpecTracked(new CallbackAttributionStream(0), filterStreamSpec.FilterSpecCompiled));
            }
            else if (streamSpec is PatternStreamSpecCompiled patternStreamSpec) {
                var forges = patternStreamSpec.Root.CollectFactories();
                foreach (var forge in forges) {
                    forge.CollectSelfFilterAndSchedule(
                        factoryNodeId => new CallbackAttributionStreamPattern(0, factoryNodeId),
                        filterSpecCompileds,
                        schedules);
                }

                activatorResult = ActivatorPattern(patternStreamSpec, services);
                services.StateMgmtSettingsProvider.Pattern(
                    fabricCharge,
                    new PatternAttributionKeyStream(0),
                    patternStreamSpec,
                    @base.StatementRawInfo);
            }
            else if (streamSpec is NamedWindowConsumerStreamSpec namedSpec) {
                activatorResult = ActivatorNamedWindow(namedSpec, services);
                namedWindowConsumers.Add(namedSpec);
            }
            else if (streamSpec is TableQueryStreamSpec) {
                throw new ExprValidationException("Tables cannot be used in an on-action statement triggering stream");
            }
            else {
                throw new ExprValidationException("Unknown stream specification type: " + streamSpec);
            }

            var statementFieldsClassName =
                CodeGenerationIDGenerator.GenerateClassNameSimple(typeof(StatementFields), classPostfix);
            var namespaceScope = new CodegenNamespaceScope(
                @namespace,
                statementFieldsClassName,
                services.IsInstrumented,
                services.Configuration.Compiler.ByteCode);
            var aiFactoryProviderClassName = CodeGenerationIDGenerator.GenerateClassNameSimple(
                typeof(StatementAIFactoryProvider),
                classPostfix);

            // context-factory creation
            //
            // handle on-merge for table
            var onTriggerDesc = @base.StatementSpec.Raw.OnTriggerDesc;
            OnTriggerPlan onTriggerPlan;

            if (onTriggerDesc is OnTriggerWindowDesc windowDesc) {
                var namedWindow = services.NamedWindowCompileTimeResolver.Resolve(windowDesc.WindowName);
                TableMetaData table = null;
                if (namedWindow == null) {
                    table = services.TableCompileTimeResolver.Resolve(windowDesc.WindowName);
                    if (table == null) {
                        throw new ExprValidationException(
                            "A named window or table '" + windowDesc.WindowName + "' has not been declared");
                    }
                }

                var planDesc = new OnTriggerWindowPlan(
                    windowDesc,
                    contextName,
                    activatorResult,
                    optionalStreamSelector,
                    subselectActivation,
                    streamSpec);
                onTriggerPlan = OnTriggerWindowUtil.HandleContextFactoryOnTrigger(
                    aiFactoryProviderClassName,
                    namespaceScope,
                    classPostfix,
                    namedWindow,
                    table,
                    planDesc,
                    @base,
                    services);
            }
            else if (onTriggerDesc is OnTriggerSetDesc setDesc) {
                // variable assignments
                var plan = OnTriggerSetUtil.HandleSetVariable(
                    aiFactoryProviderClassName,
                    namespaceScope,
                    classPostfix,
                    activatorResult,
                    streamSpec.OptionalStreamName,
                    subselectActivation,
                    setDesc,
                    @base,
                    services);
                onTriggerPlan = new OnTriggerPlan(
                    plan.Forgeable,
                    plan.Forgeables,
                    plan.SelectSubscriberDescriptor,
                    plan.AdditionalForgeables,
                    plan.FabricCharge);
            }
            else {
                // split-stream use case
                var desc = (OnTriggerSplitStreamDesc)onTriggerDesc;
                onTriggerPlan = OnSplitStreamUtil.HandleSplitStream(
                    aiFactoryProviderClassName,
                    namespaceScope,
                    classPostfix,
                    desc,
                    streamSpec,
                    activatorResult,
                    subselectActivation,
                    @base,
                    services);
            }

            additionalForgeables.AddAll(onTriggerPlan.AdditionalForgeables);
            fabricCharge.Add(onTriggerPlan.FabricCharge);

            // build forge list
            IList<StmtClassForgeable> forgeables = new List<StmtClassForgeable>(2);
            foreach (var additional in additionalForgeables) {
                forgeables.Add(additional.Make(namespaceScope, classPostfix));
            }

            forgeables.AddAll(onTriggerPlan.Forgeables);
            forgeables.Add(onTriggerPlan.Factory);

            var statementProviderClassName =
                CodeGenerationIDGenerator.GenerateClassNameSimple(typeof(StatementProvider), classPostfix);
            var informationals = StatementInformationalsUtil.GetInformationals(
                @base,
                filterSpecCompileds,
                schedules,
                namedWindowConsumers,
                true,
                onTriggerPlan.SubscriberDescriptor,
                namespaceScope,
                services);
            forgeables.Add(
                new StmtClassForgeableStmtProvider(
                    aiFactoryProviderClassName,
                    statementProviderClassName,
                    informationals,
                    namespaceScope));
            forgeables.Add(new StmtClassForgeableStmtFields(statementFieldsClassName, namespaceScope));

            return new StmtForgeMethodResult(
                forgeables,
                filterSpecCompileds,
                schedules,
                namedWindowConsumers,
                FilterSpecCompiled.MakeExprNodeList(filterSpecCompileds, EmptyList<FilterSpecParamExprNodeForge>.Instance),
                namespaceScope,
                fabricCharge);
        }

        private OnTriggerActivatorDesc ActivatorNamedWindow(
            NamedWindowConsumerStreamSpec namedSpec,
            StatementCompileTimeServices services)
        {
            var namedWindow = namedSpec.NamedWindow;
            var triggerEventTypeName = namedSpec.NamedWindow.EventType.Name;

            var typesFilterValidation = new StreamTypeServiceImpl(
                namedWindow.EventType,
                namedSpec.OptionalStreamName,
                false);
            var filterSingle =
                ExprNodeUtilityMake.ConnectExpressionsByLogicalAndWhenNeeded(namedSpec.FilterExpressions);
            var filterQueryGraph = EPLValidationUtil.ValidateFilterGetQueryGraphSafe(
                filterSingle,
                typesFilterValidation,
                @base.StatementRawInfo,
                services);
            var activator = new ViewableActivatorNamedWindowForge(
                namedSpec,
                namedWindow,
                filterSingle,
                filterQueryGraph,
                false,
                namedSpec.OptPropertyEvaluator);

            var activatorResultEventType = namedWindow.EventType;
            if (namedSpec.OptPropertyEvaluator != null) {
                activatorResultEventType = namedSpec.OptPropertyEvaluator.FragmentEventType;
            }

            return new OnTriggerActivatorDesc(activator, triggerEventTypeName, activatorResultEventType);
        }

        private OnTriggerActivatorDesc ActivatorFilter(
            FilterStreamSpecCompiled filterStreamSpec,
            StatementCompileTimeServices services)
        {
            var triggerEventTypeName = filterStreamSpec.FilterSpecCompiled.FilterForEventTypeName;
            var activator = new ViewableActivatorFilterForge(filterStreamSpec.FilterSpecCompiled, false, 0, false, -1);
            var activatorResultEventType = filterStreamSpec.FilterSpecCompiled.ResultEventType;
            return new OnTriggerActivatorDesc(activator, triggerEventTypeName, activatorResultEventType);
        }

        private OnTriggerActivatorDesc ActivatorPattern(
            PatternStreamSpecCompiled patternStreamSpec,
            StatementCompileTimeServices services)
        {
            var triggerEventTypeName = patternStreamSpec.OptionalStreamName;
            var patternType = ViewableActivatorPatternForge.MakeRegisterPatternType(
                @base.ModuleName,
                0,
                null,
                patternStreamSpec,
                services);
            var patternContext = new PatternContext(0, patternStreamSpec.MatchedEventMapMeta, false, -1, false);
            var activator = new ViewableActivatorPatternForge(patternType, patternStreamSpec, patternContext, false);
            return new OnTriggerActivatorDesc(activator, triggerEventTypeName, patternType);
        }
    }
} // end of namespace