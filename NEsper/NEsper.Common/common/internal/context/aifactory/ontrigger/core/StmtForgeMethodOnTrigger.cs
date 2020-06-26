///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.soda;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.core;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.compile.stage3;
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

            IList<FilterSpecCompiled> filterSpecCompileds = new List<FilterSpecCompiled>();
            IList<ScheduleHandleCallbackProvider> schedules = new List<ScheduleHandleCallbackProvider>();
            IList<NamedWindowConsumerStreamSpec> namedWindowConsumers = new List<NamedWindowConsumerStreamSpec>();
            IList<StmtClassForgeableFactory> additionalForgeables = new List<StmtClassForgeableFactory>();

            // create subselect information
            var subSelectActivationDesc = SubSelectHelperActivations.CreateSubSelectActivation(
                filterSpecCompileds, namedWindowConsumers, @base, services);
            IDictionary<ExprSubselectNode, SubSelectActivationPlan> subselectActivation = subSelectActivationDesc.Subselects;
            additionalForgeables.AddAll(subSelectActivationDesc.AdditionalForgeables);

            // obtain activator
            var streamSpec = @base.StatementSpec.StreamSpecs[0];
            StreamSelector? optionalStreamSelector = null;
            OnTriggerActivatorDesc activatorResult;

            if (streamSpec is FilterStreamSpecCompiled) {
                var filterStreamSpec = (FilterStreamSpecCompiled) streamSpec;
                activatorResult = ActivatorFilter(filterStreamSpec, services);
                filterSpecCompileds.Add(filterStreamSpec.FilterSpecCompiled);
            }
            else if (streamSpec is PatternStreamSpecCompiled) {
                var patternStreamSpec = (PatternStreamSpecCompiled) streamSpec;
                var forges = patternStreamSpec.Root.CollectFactories();
                foreach (var forge in forges) {
                    forge.CollectSelfFilterAndSchedule(filterSpecCompileds, schedules);
                }

                activatorResult = ActivatorPattern(patternStreamSpec, services);
            }
            else if (streamSpec is NamedWindowConsumerStreamSpec) {
                var namedSpec = (NamedWindowConsumerStreamSpec) streamSpec;
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
                services.IsInstrumented);
            var aiFactoryProviderClassName = CodeGenerationIDGenerator.GenerateClassNameSimple(
                typeof(StatementAIFactoryProvider),
                classPostfix);

            // context-factory creation
            //
            // handle on-merge for table
            var onTriggerDesc = @base.StatementSpec.Raw.OnTriggerDesc;
            OnTriggerPlan onTriggerPlan;

            if (onTriggerDesc is OnTriggerWindowDesc) {
                var desc = (OnTriggerWindowDesc) onTriggerDesc;

                var namedWindow = services.NamedWindowCompileTimeResolver.Resolve(desc.WindowName);
                TableMetaData table = null;
                if (namedWindow == null) {
                    table = services.TableCompileTimeResolver.Resolve(desc.WindowName);
                    if (table == null) {
                        throw new ExprValidationException(
                            "A named window or table '" + desc.WindowName + "' has not been declared");
                    }
                }

                var planDesc = new OnTriggerWindowPlan(
                    desc,
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
            else if (onTriggerDesc is OnTriggerSetDesc) {
                // variable assignments
                var desc = (OnTriggerSetDesc) onTriggerDesc;
                var plan = OnTriggerSetUtil.HandleSetVariable(
                    aiFactoryProviderClassName,
                    namespaceScope,
                    classPostfix,
                    activatorResult,
                    streamSpec.OptionalStreamName,
                    subselectActivation,
                    desc,
                    @base,
                    services);
                onTriggerPlan = new OnTriggerPlan(
                    plan.Forgeable,
                    plan.Forgeables,
                    plan.SelectSubscriberDescriptor,
                    plan.AdditionalForgeables);
            }
            else {
                // split-stream use case
                var desc = (OnTriggerSplitStreamDesc) onTriggerDesc;
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

            // build forge list
            IList<StmtClassForgeable> forgeables = new List<StmtClassForgeable>();
            foreach (StmtClassForgeableFactory additional in additionalForgeables) {
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
            forgeables.Add(
                new StmtClassForgeableStmtFields(
                    statementFieldsClassName,
                    namespaceScope,
                    2));

            return new StmtForgeMethodResult(
                forgeables,
                filterSpecCompileds,
                schedules,
                namedWindowConsumers,
                FilterSpecCompiled.MakeExprNodeList(
                    filterSpecCompileds,
                    EmptyList<FilterSpecParamExprNodeForge>.Instance));
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
            var patternType =
                ViewableActivatorPatternForge.MakeRegisterPatternType(@base, 0, patternStreamSpec, services);
            var patternContext = new PatternContext(0, patternStreamSpec.MatchedEventMapMeta, false, -1, false);
            var activator = new ViewableActivatorPatternForge(patternType, patternStreamSpec, patternContext, false);
            return new OnTriggerActivatorDesc(activator, triggerEventTypeName, patternType);
        }
    }
} // end of namespace