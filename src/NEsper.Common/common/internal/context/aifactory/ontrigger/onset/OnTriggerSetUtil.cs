///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.meta;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.core;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.context.aifactory.ontrigger.core;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.subquery;
using com.espertech.esper.common.@internal.epl.resultset.core;
using com.espertech.esper.common.@internal.epl.streamtype;
using com.espertech.esper.common.@internal.epl.subselect;
using com.espertech.esper.common.@internal.epl.table.strategy;
using com.espertech.esper.common.@internal.epl.variable.core;
using com.espertech.esper.common.@internal.@event.core;

namespace com.espertech.esper.common.@internal.context.aifactory.ontrigger.onset
{
    public class OnTriggerSetUtil
    {
        public static OnTriggerSetPlan HandleSetVariable(
            string className,
            CodegenNamespaceScope namespaceScope,
            string classPostfix,
            OnTriggerActivatorDesc activatorResult,
            string optionalStreamName,
            IDictionary<ExprSubselectNode, SubSelectActivationPlan> subselectActivation,
            OnTriggerSetDesc desc,
            StatementBaseInfo @base,
            StatementCompileTimeServices services)
        {
            StreamTypeService typeService = new StreamTypeServiceImpl(
                new[] {activatorResult.ActivatorResultEventType},
                new[] {optionalStreamName},
                new[] {true},
                false,
                false);
            var validationContext = new ExprValidationContextBuilder(typeService, @base.StatementRawInfo, services)
                .WithAllowBindingConsumption(true)
                .Build();

            // handle subselects
            SubSelectHelperForgePlan subSelectForgePlan = SubSelectHelperForgePlanner.PlanSubSelect(
                @base,
                subselectActivation,
                new string[] {optionalStreamName},
                new EventType[] {activatorResult.ActivatorResultEventType},
                new string[] {activatorResult.TriggerEventTypeName},
                services);
            IDictionary<ExprSubselectNode, SubSelectFactoryForge> subselectForges = subSelectForgePlan.Subselects;

            // validate assignments
            foreach (var assignment in desc.Assignments) {
                ExprNodeUtilityValidate.ValidateAssignment(true, ExprNodeOrigin.UPDATEASSIGN, assignment, validationContext);
            }

            // create read-write logic
            VariableReadWritePackageForge variableReadWritePackageForge = new VariableReadWritePackageForge(
                desc.Assignments, @base.StatementName, services);

            // plan table access
            var tableAccessForges = ExprTableEvalHelperPlan.PlanTableAccess(@base.StatementSpec.TableAccessNodes);

            // create output event type
            var eventTypeName = services.EventTypeNameGeneratorStatement.AnonymousTypeName;
            var eventTypeMetadata = new EventTypeMetadata(
                eventTypeName,
                @base.ModuleName,
                EventTypeTypeClass.STATEMENTOUT,
                EventTypeApplicationType.MAP,
                NameAccessModifier.TRANSIENT,
                EventTypeBusModifier.NONBUS,
                false,
                EventTypeIdPair.Unassigned());
            var eventType = BaseNestableEventUtil.MakeMapTypeCompileTime(
                eventTypeMetadata,
                variableReadWritePackageForge.VariableTypes,
                null,
                null,
                null,
                null,
                services.BeanEventTypeFactoryPrivate,
                services.EventTypeCompileTimeResolver);
            services.EventTypeCompileTimeRegistry.NewType(eventType);

            // Handle output format
            var defaultSelectAllSpec = new StatementSpecCompiled();
            defaultSelectAllSpec.SelectClauseCompiled.WithSelectExprList(new SelectClauseElementWildcard());
            defaultSelectAllSpec.Raw.SelectStreamDirEnum = SelectClauseStreamSelectorEnum.RSTREAM_ISTREAM_BOTH;
            StreamTypeService streamTypeService = new StreamTypeServiceImpl(
                new EventType[] {eventType},
                new[] {"trigger_stream"},
                new[] {true},
                false,
                false);
            var resultSetProcessor = ResultSetProcessorFactoryFactory.GetProcessorPrototype(
                new ResultSetSpec(defaultSelectAllSpec),
                streamTypeService,
                null,
                new bool[1],
                false,
                @base.ContextPropertyRegistry,
                false,
                false,
                @base.StatementRawInfo,
                services);
            var classNameRSP = CodeGenerationIDGenerator.GenerateClassNameSimple(
                typeof(ResultSetProcessorFactoryProvider),
                classPostfix);

            var forge = new StatementAgentInstanceFactoryOnTriggerSetForge(
                activatorResult.Activator,
                eventType,
                subselectForges,
                tableAccessForges,
                variableReadWritePackageForge,
                classNameRSP);
            var forgeables = new List<StmtClassForgeable>();
            forgeables.Add(new StmtClassForgeableRSPFactoryProvider(classNameRSP, resultSetProcessor, namespaceScope, @base.StatementRawInfo));

            var onTrigger = new StmtClassForgeableAIFactoryProviderOnTrigger(className, namespaceScope, forge);
            return new OnTriggerSetPlan(
                onTrigger,
                forgeables,
                resultSetProcessor.SelectSubscriberDescriptor,
                subSelectForgePlan.AdditionalForgeables);
        }
    }
} // end of namespace