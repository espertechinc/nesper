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
using com.espertech.esper.common.@internal.fabric;
using com.espertech.esper.common.@internal.filterspec;
using com.espertech.esper.common.@internal.schedule;
using com.espertech.esper.common.@internal.statement.helper;
using com.espertech.esper.compat.collections;


namespace com.espertech.esper.common.@internal.context.aifactory.update
{
    public class StmtForgeMethodUpdate : StmtForgeMethod
    {
        private readonly StatementBaseInfo @base;

        public StmtForgeMethodUpdate(StatementBaseInfo @base)
        {
            this.@base = @base;
        }

        public StmtForgeMethodResult Make(
            string @namespace,
            string classPostfix,
            StatementCompileTimeServices services)
        {
            var statementSpec = @base.StatementSpec;

            // determine context
            var contextName = @base.StatementSpec.Raw.OptionalContextName;
            if (contextName != null) {
                throw new ExprValidationException("Update IStream is not supported in conjunction with a context");
            }

            var streamSpec = statementSpec.StreamSpecs[0];
            var updateSpec = statementSpec.Raw.UpdateDesc;
            IList<StmtClassForgeableFactory> additionalForgeables = new List<StmtClassForgeableFactory>(2);
            var fabricCharge = services.StateMgmtSettingsProvider.NewCharge();
            string triggereventTypeName;
            EventType streamEventType;

            if (streamSpec is FilterStreamSpecCompiled filterStreamSpec) {
                triggereventTypeName = filterStreamSpec.FilterSpecCompiled.FilterForEventTypeName;
                streamEventType = filterStreamSpec.FilterSpecCompiled.FilterForEventType;
            }
            else if (streamSpec is NamedWindowConsumerStreamSpec namedSpec) {
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
                new EventType[] { streamEventType },
                new string[] { streamName },
                new bool[] { true },
                false,
                false);

            // create subselect information
            IList<FilterSpecTracked> filterSpecCompileds = new List<FilterSpecTracked>();
            IList<NamedWindowConsumerStreamSpec> namedWindowConsumers = new List<NamedWindowConsumerStreamSpec>();
            var subSelectActivationDesc = SubSelectHelperActivations.CreateSubSelectActivation(
                false,
                filterSpecCompileds,
                namedWindowConsumers,
                @base,
                services);
            additionalForgeables.AddAll(subSelectActivationDesc.AdditionalForgeables);
            fabricCharge.Add(subSelectActivationDesc.FabricCharge);
            var subselectActivation = subSelectActivationDesc.Subselects;

            // handle subselects
            var subSelectForgePlan = SubSelectHelperForgePlanner.PlanSubSelect(
                @base,
                subselectActivation,
                typeService.StreamNames,
                typeService.EventTypes,
                new string[] { triggereventTypeName },
                services);
            var subselectForges = subSelectForgePlan.Subselects;
            additionalForgeables.AddAll(subSelectForgePlan.AdditionalForgeables);
            fabricCharge.Add(subSelectForgePlan.FabricCharge);

            var validationContext =
                new ExprValidationContextBuilder(typeService, @base.StatementRawInfo, services).Build();

            foreach (var assignment in updateSpec.Assignments) {
                ExprNodeUtilityValidate.ValidateAssignment(
                    false,
                    ExprNodeOrigin.UPDATEASSIGN,
                    assignment,
                    validationContext);
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
                @base.StatementRawInfo.Annotations);

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
            var forge = new StatementAgentInstanceFactoryUpdateForge(routerDesc, subselectForges);
            var aiFactoryForgeable = new StmtClassForgeableAIFactoryProviderUpdate(
                aiFactoryProviderClassName,
                namespaceScope,
                forge);

            var selectSubscriberDescriptor = new SelectSubscriberDescriptor(
                new Type[] { streamEventType.UnderlyingType },
                new string[] { "*" },
                false,
                null,
                null);
            var informationals = StatementInformationalsUtil.GetInformationals(
                @base,
                filterSpecCompileds,
                EmptyList<ScheduleHandleTracked>.Instance, 
                EmptyList<NamedWindowConsumerStreamSpec>.Instance, 
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
            foreach (var additional in additionalForgeables) {
                forgeables.Add(additional.Make(namespaceScope, classPostfix));
            }

            forgeables.Add(aiFactoryForgeable);
            forgeables.Add(stmtProvider);
            forgeables.Add(new StmtClassForgeableStmtFields(statementFieldsClassName, namespaceScope));
            return new StmtForgeMethodResult(
                forgeables,
                filterSpecCompileds,
                EmptyList<ScheduleHandleTracked>.Instance, 
                namedWindowConsumers,
                EmptyList<FilterSpecParamExprNodeForge>.Instance, 
                namespaceScope,
                fabricCharge);
        }
    }
} // end of namespace