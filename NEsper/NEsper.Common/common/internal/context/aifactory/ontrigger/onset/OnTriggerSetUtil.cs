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
            CodegenPackageScope packageScope,
            string classPostfix,
            OnTriggerActivatorDesc activatorResult,
            string optionalStreamName,
            IDictionary<ExprSubselectNode, SubSelectActivationPlan> subselectActivation,
            OnTriggerSetDesc desc,
            StatementBaseInfo @base,
            StatementCompileTimeServices services)
        {
            StreamTypeService typeService = new StreamTypeServiceImpl(
                new[] {activatorResult.ActivatorResultEventType}, new[] {optionalStreamName}, new[] {true}, false,
                false);
            var validationContext = new ExprValidationContextBuilder(typeService, @base.StatementRawInfo, services)
                .WithAllowBindingConsumption(true).Build();

            // handle subselects
            var subselectForges = SubSelectHelperForgePlanner.PlanSubSelect(
                @base, subselectActivation, new[] {optionalStreamName},
                new[] {activatorResult.ActivatorResultEventType}, new[] {activatorResult.TriggerEventTypeName},
                services);

            // validate assignments
            foreach (var assignment in desc.Assignments) {
                var validated = ExprNodeUtilityValidate.GetValidatedAssignment(assignment, validationContext);
                assignment.Expression = validated;
            }

            // create read-write logic
            var variableReadWritePackageForge = new VariableReadWritePackageForge(desc.Assignments, services);

            // plan table access
            var tableAccessForges = ExprTableEvalHelperPlan.PlanTableAccess(@base.StatementSpec.TableAccessNodes);

            // create output event type
            var eventTypeName = services.EventTypeNameGeneratorStatement.AnonymousTypeName;
            var eventTypeMetadata = new EventTypeMetadata(
                eventTypeName, @base.ModuleName, EventTypeTypeClass.STATEMENTOUT, EventTypeApplicationType.MAP,
                NameAccessModifier.TRANSIENT, EventTypeBusModifier.NONBUS, false, EventTypeIdPair.Unassigned());
            var eventType = BaseNestableEventUtil.MakeMapTypeCompileTime(
                eventTypeMetadata,
                variableReadWritePackageForge.VariableTypes, null, null, null, null,
                services.BeanEventTypeFactoryPrivate, services.EventTypeCompileTimeResolver);
            services.EventTypeCompileTimeRegistry.NewType(eventType);

            // Handle output format
            var defaultSelectAllSpec = new StatementSpecCompiled();
            defaultSelectAllSpec.SelectClauseCompiled.WithSelectExprList(new SelectClauseElementWildcard());
            defaultSelectAllSpec.Raw.SelectStreamDirEnum = SelectClauseStreamSelectorEnum.RSTREAM_ISTREAM_BOTH;
            StreamTypeService streamTypeService = new StreamTypeServiceImpl(
                new EventType[] {eventType}, new[] {"trigger_stream"}, new[] {true}, false, false);
            var resultSetProcessor = ResultSetProcessorFactoryFactory.GetProcessorPrototype(
                new ResultSetSpec(defaultSelectAllSpec),
                streamTypeService, null, new bool[1], false, @base.ContextPropertyRegistry, false, false,
                @base.StatementRawInfo, services);
            var classNameRSP = CodeGenerationIDGenerator.GenerateClassNameSimple(
                typeof(ResultSetProcessorFactoryProvider), classPostfix);

            var forge = new StatementAgentInstanceFactoryOnTriggerSetForge(
                activatorResult.Activator, eventType, subselectForges, tableAccessForges, variableReadWritePackageForge,
                classNameRSP);
            IList<StmtClassForgable> forgables = new List<StmtClassForgable>();
            forgables.Add(
                new StmtClassForgableRSPFactoryProvider(
                    classNameRSP, resultSetProcessor, packageScope, @base.StatementRawInfo));

            var onTrigger = new StmtClassForgableAIFactoryProviderOnTrigger(className, packageScope, forge);
            return new OnTriggerSetPlan(onTrigger, forgables, resultSetProcessor.SelectSubscriberDescriptor);
        }
    }
} // end of namespace