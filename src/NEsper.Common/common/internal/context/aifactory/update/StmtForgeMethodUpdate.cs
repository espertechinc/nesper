///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.core;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.subquery;
using com.espertech.esper.common.@internal.epl.resultset.select.core;
using com.espertech.esper.common.@internal.epl.streamtype;
using com.espertech.esper.common.@internal.epl.subselect;
using com.espertech.esper.common.@internal.filterspec;
using com.espertech.esper.common.@internal.schedule;
using com.espertech.esper.common.@internal.statement.helper;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.context.aifactory.update
{
    public class StmtForgeMethodUpdate : StmtForgeMethod
    {
        private readonly StatementBaseInfo _base;

        public StmtForgeMethodUpdate(StatementBaseInfo @base)
        {
            this._base = @base;
        }

        public StmtForgeMethodResult Make(
            string @namespace,
            string classPostfix,
            StatementCompileTimeServices services)
        {
            var statementSpec = _base.StatementSpec;

            // determine context
            var contextName = _base.StatementSpec.Raw.OptionalContextName;
            if (contextName != null) {
                throw new ExprValidationException("Update IStream is not supported in conjunction with a context");
            }

            var streamSpec = statementSpec.StreamSpecs[0];
            var updateSpec = statementSpec.Raw.UpdateDesc;
            string triggereventTypeName;
            EventType streamEventType;
            
            List<StmtClassForgeableFactory> additionalForgeables = new List<StmtClassForgeableFactory>();

            if (streamSpec is FilterStreamSpecCompiled) {
                var filterStreamSpec = (FilterStreamSpecCompiled) streamSpec;
                triggereventTypeName = filterStreamSpec.FilterSpecCompiled.FilterForEventTypeName;
                streamEventType = filterStreamSpec.FilterSpecCompiled.FilterForEventType;
            }
            else if (streamSpec is NamedWindowConsumerStreamSpec) {
                var namedSpec = (NamedWindowConsumerStreamSpec) streamSpec;
                streamEventType = namedSpec.NamedWindow.EventType;
                triggereventTypeName = streamEventType.Name;
            }
            else if (streamSpec is TableQueryStreamSpec) {
                throw new ExprValidationException("Tables cannot be used in an update-istream statement");
            }
            else {
                throw new ExprValidationException("Unknown stream specification streamEventType: " + streamSpec);
            }

            // determine a stream name
            var streamName = triggereventTypeName;
            if (updateSpec.OptionalStreamName != null) {
                streamName = updateSpec.OptionalStreamName;
            }

            StreamTypeService typeService = new StreamTypeServiceImpl(
                new[] {streamEventType},
                new[] {streamName},
                new[] {true},
                false,
                false);

            // create subselect information
            IList<FilterSpecCompiled> filterSpecCompileds = new List<FilterSpecCompiled>();
            IList<NamedWindowConsumerStreamSpec> namedWindowConsumers = new List<NamedWindowConsumerStreamSpec>();
            SubSelectActivationDesc subSelectActivationDesc = SubSelectHelperActivations.CreateSubSelectActivation(
                filterSpecCompileds, namedWindowConsumers, _base, services);
            additionalForgeables.AddAll(subSelectActivationDesc.AdditionalForgeables);
            IDictionary<ExprSubselectNode, SubSelectActivationPlan> subselectActivation = subSelectActivationDesc.Subselects;

            // handle subselects
            SubSelectHelperForgePlan subSelectForgePlan = SubSelectHelperForgePlanner.PlanSubSelect(
                _base, subselectActivation, typeService.StreamNames, typeService.EventTypes, new String[]{triggereventTypeName}, services);
            IDictionary<ExprSubselectNode, SubSelectFactoryForge> subselectForges = subSelectForgePlan.Subselects;
            additionalForgeables.AddAll(subSelectForgePlan.AdditionalForgeables);

            var validationContext =
                new ExprValidationContextBuilder(typeService, _base.StatementRawInfo, services).Build();

            foreach (var assignment in updateSpec.Assignments) {
                ExprNodeUtilityValidate.ValidateAssignment(
                    false, ExprNodeOrigin.UPDATEASSIGN, assignment, validationContext);
            }

            if (updateSpec.OptionalWhereClause != null) {
                var validated = ExprNodeUtilityValidate.GetValidatedSubtree(
                    ExprNodeOrigin.WHERE,
                    updateSpec.OptionalWhereClause,
                    validationContext);
                updateSpec.OptionalWhereClause = validated;
                EPStatementStartMethodHelperValidate.ValidateNoAggregations(
                    validated,
                    "Aggregation functions may not be used within an update-clause");
            }

            // build route information
            var routerDesc = InternalEventRouterDescFactory.GetValidatePreprocessing(
                streamEventType,
                updateSpec,
                _base.StatementRawInfo.Annotations);

            var statementFieldsClassName =
                CodeGenerationIDGenerator.GenerateClassNameSimple(typeof(StatementFields), classPostfix);
            var namespaceScope = new CodegenNamespaceScope(
                @namespace,
                statementFieldsClassName,
                services.IsInstrumented);

            var aiFactoryProviderClassName = CodeGenerationIDGenerator.GenerateClassNameSimple(
                typeof(StatementAIFactoryProvider),
                classPostfix);
            var forge = new StatementAgentInstanceFactoryUpdateForge(routerDesc, subselectForges);
            var aiFactoryForgeable = new StmtClassForgeableAIFactoryProviderUpdate(
                aiFactoryProviderClassName,
                namespaceScope,
                forge);

            var selectSubscriberDescriptor = new SelectSubscriberDescriptor(
                new[] {streamEventType.UnderlyingType},
                new[] {"*"},
                false,
                null,
                null);
            
            var informationals = StatementInformationalsUtil.GetInformationals(
                _base,
                filterSpecCompileds,
                Collections.GetEmptyList<ScheduleHandleCallbackProvider>(),
                Collections.GetEmptyList<NamedWindowConsumerStreamSpec>(),
                false,
                selectSubscriberDescriptor,
                namespaceScope,
                services);
            var statementProviderClassName =
                CodeGenerationIDGenerator.GenerateClassNameSimple(typeof(StatementProvider), classPostfix);
            var stmtProvider = new StmtClassForgeableStmtProvider(
                aiFactoryProviderClassName,
                statementProviderClassName,
                informationals,
                namespaceScope);

            IList<StmtClassForgeable> forgeables = new List<StmtClassForgeable>();
            additionalForgeables.ForEach(_ => forgeables.Add(_.Make(namespaceScope, classPostfix)));
            forgeables.Add(aiFactoryForgeable);
            forgeables.Add(stmtProvider);
            forgeables.Add(new StmtClassForgeableStmtFields(statementFieldsClassName, namespaceScope, 0));
            return new StmtForgeMethodResult(
                forgeables,
                filterSpecCompileds,
                Collections.GetEmptyList<ScheduleHandleCallbackProvider>(),
                namedWindowConsumers,
                Collections.GetEmptyList<FilterSpecParamExprNodeForge>());
        }
    }
} // end of namespace