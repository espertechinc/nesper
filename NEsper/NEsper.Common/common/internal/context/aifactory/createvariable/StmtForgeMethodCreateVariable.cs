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
using com.espertech.esper.common.@internal.context.compile;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.resultset.core;
using com.espertech.esper.common.@internal.epl.streamtype;
using com.espertech.esper.common.@internal.epl.util;
using com.espertech.esper.common.@internal.epl.variable.compiletime;
using com.espertech.esper.common.@internal.epl.variable.core;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.@event.map;
using com.espertech.esper.common.@internal.filterspec;
using com.espertech.esper.common.@internal.schedule;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.context.aifactory.createvariable
{
    public class StmtForgeMethodCreateVariable : StmtForgeMethod
    {
        private readonly StatementBaseInfo @base;

        public StmtForgeMethodCreateVariable(StatementBaseInfo @base)
        {
            this.@base = @base;
        }

        public StmtForgeMethodResult Make(
            string packageName,
            string classPostfix,
            StatementCompileTimeServices services)
        {
            StatementSpecCompiled statementSpec = @base.StatementSpec;

            CreateVariableDesc createDesc = statementSpec.Raw.CreateVariableDesc;

            // Check if the variable is already declared
            EPLValidationUtil.ValidateAlreadyExistsTableOrVariable(
                createDesc.VariableName,
                services.VariableCompileTimeResolver,
                services.TableCompileTimeResolver,
                services.EventTypeCompileTimeResolver);

            // Get assignment value when compile-time-constant
            object initialValue = null;
            ExprForge initialValueExpr = null;
            if (createDesc.Assignment != null) {
                // Evaluate assignment expression
                StreamTypeService typeService = new StreamTypeServiceImpl(new EventType[0], new string[0], new bool[0], false, false);
                ExprValidationContext validationContext = new ExprValidationContextBuilder(typeService, @base.StatementRawInfo, services).Build();
                ExprNode validated = ExprNodeUtilityValidate.GetValidatedSubtree(
                    ExprNodeOrigin.VARIABLEASSIGN, createDesc.Assignment, validationContext);
                if (validated.Forge.ForgeConstantType == ExprForgeConstantType.COMPILETIMECONST) {
                    initialValue = validated.Forge.ExprEvaluator.Evaluate(null, true, null);
                }

                createDesc.Assignment = validated;
                initialValueExpr = validated.Forge;
            }

            string contextName = statementSpec.Raw.OptionalContextName;
            NameAccessModifier contextVisibility = null;
            string contextModuleName = null;
            if (contextName != null) {
                ContextMetaData contextDetail = services.ContextCompileTimeResolver.GetContextInfo(contextName);
                if (contextDetail == null) {
                    throw new ExprValidationException("Failed to find context '" + contextName + "'");
                }

                contextVisibility = contextDetail.ContextVisibility;
                contextModuleName = contextDetail.ContextModuleName;
            }

            // get visibility
            NameAccessModifier visibility = services.ModuleVisibilityRules.GetAccessModifierVariable(@base, createDesc.VariableName);

            // Compile metadata
            bool compileTimeConstant = createDesc.IsConstant
                                       && initialValueExpr != null
                                       && initialValueExpr.ForgeConstantType.IsCompileTimeConstant;
            VariableMetaData metaData = VariableUtil.CompileVariable(
                createDesc.VariableName,
                @base.ModuleName,
                visibility,
                contextName,
                contextVisibility,
                contextModuleName,
                createDesc.VariableType,
                createDesc.IsConstant,
                compileTimeConstant,
                initialValue,
                services.ImportServiceCompileTime,
                EventBeanTypedEventFactoryCompileTime.INSTANCE,
                services.EventTypeRepositoryPreconfigured,
                services.BeanEventTypeFactoryPrivate);

            // Register variable
            services.VariableCompileTimeRegistry.NewVariable(metaData);

            // Statement event type
            IDictionary<string, object> eventTypePropertyTypes = Collections.SingletonDataMap(
                metaData.VariableName, metaData.Type);
            string eventTypeName = services.EventTypeNameGeneratorStatement.AnonymousTypeName;
            EventTypeMetadata eventTypeMetadata = new EventTypeMetadata(
                eventTypeName, @base.ModuleName, EventTypeTypeClass.STATEMENTOUT, EventTypeApplicationType.MAP, NameAccessModifier.TRANSIENT,
                EventTypeBusModifier.NONBUS, false, EventTypeIdPair.Unassigned());
            MapEventType outputEventType = BaseNestableEventUtil.MakeMapTypeCompileTime(
                eventTypeMetadata, eventTypePropertyTypes, null, null, null, null, services.BeanEventTypeFactoryPrivate,
                services.EventTypeCompileTimeResolver);
            services.EventTypeCompileTimeRegistry.NewType(outputEventType);

            // Handle output format
            StatementSpecCompiled defaultSelectAllSpec = new StatementSpecCompiled();
            defaultSelectAllSpec.SelectClauseCompiled.WithSelectExprList(new SelectClauseElementWildcard());
            defaultSelectAllSpec.Raw.SelectStreamDirEnum = SelectClauseStreamSelectorEnum.RSTREAM_ISTREAM_BOTH;
            StreamTypeService streamTypeService = new StreamTypeServiceImpl(
                new EventType[] {outputEventType}, new string[] {"trigger_stream"}, new bool[] {true}, false, false);
            ResultSetProcessorDesc resultSetProcessor = ResultSetProcessorFactoryFactory.GetProcessorPrototype(
                new ResultSetSpec(defaultSelectAllSpec),
                streamTypeService, null, new bool[1], false, @base.ContextPropertyRegistry, false, false, @base.StatementRawInfo, services);

            // Code generation
            string statementFieldsClassName = CodeGenerationIDGenerator.GenerateClassNameSimple(typeof(StatementFields), classPostfix);
            string aiFactoryProviderClassName = CodeGenerationIDGenerator.GenerateClassNameSimple(typeof(StatementAIFactoryProvider), classPostfix);
            string classNameRSP = CodeGenerationIDGenerator.GenerateClassNameSimple(typeof(ResultSetProcessorFactoryProvider), classPostfix);
            CodegenPackageScope packageScope = new CodegenPackageScope(
                packageName, statementFieldsClassName, services.IsInstrumented);

            StatementAgentInstanceFactoryCreateVariableForge forge =
                new StatementAgentInstanceFactoryCreateVariableForge(createDesc.VariableName, initialValueExpr, classNameRSP);
            StmtClassForgableAIFactoryProviderCreateVariable aiFactoryForgable = new StmtClassForgableAIFactoryProviderCreateVariable(
                aiFactoryProviderClassName, packageScope, forge, createDesc.VariableName);

            StatementInformationalsCompileTime informationals = StatementInformationalsUtil.GetInformationals(
                @base,
                Collections.GetEmptyList<FilterSpecCompiled>(),
                Collections.GetEmptyList<ScheduleHandleCallbackProvider>(),
                Collections.GetEmptyList<NamedWindowConsumerStreamSpec>(), true,
                resultSetProcessor.SelectSubscriberDescriptor,
                packageScope, services);
            string statementProviderClassName = CodeGenerationIDGenerator.GenerateClassNameSimple(typeof(StatementProvider), classPostfix);
            StmtClassForgableStmtProvider stmtProvider = new StmtClassForgableStmtProvider(
                aiFactoryProviderClassName, statementProviderClassName, informationals, packageScope);

            IList<StmtClassForgable> forgables = new List<StmtClassForgable>();
            forgables.Add(new StmtClassForgableRSPFactoryProvider(classNameRSP, resultSetProcessor, packageScope, @base.StatementRawInfo));
            forgables.Add(aiFactoryForgable);
            forgables.Add(stmtProvider);
            forgables.Add(new StmtClassForgableStmtFields(statementFieldsClassName, packageScope, 0));
            return new StmtForgeMethodResult(
                forgables,
                Collections.GetEmptyList<FilterSpecCompiled>(),
                Collections.GetEmptyList<ScheduleHandleCallbackProvider>(),
                Collections.GetEmptyList<NamedWindowConsumerStreamSpec>(),
                Collections.GetEmptyList<FilterSpecParamExprNodeForge>());
        }
    }
} // end of namespace