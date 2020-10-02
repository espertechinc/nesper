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
using com.espertech.esper.common.@internal.serde.compiletime.resolve;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.context.aifactory.createvariable
{
    public class StmtForgeMethodCreateVariable : StmtForgeMethod
    {
        private readonly StatementBaseInfo _base;

        public StmtForgeMethodCreateVariable(StatementBaseInfo @base)
        {
            _base = @base;
        }

        public StmtForgeMethodResult Make(
            string @namespace,
            string classPostfix,
            StatementCompileTimeServices services)
        {
            var statementSpec = _base.StatementSpec;

            var createDesc = statementSpec.Raw.CreateVariableDesc;

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
                StreamTypeService typeService = new StreamTypeServiceImpl(
                    new EventType[0],
                    new string[0],
                    new bool[0],
                    false,
                    false);
                var validationContext =
                    new ExprValidationContextBuilder(typeService, _base.StatementRawInfo, services).Build();
                var validated = ExprNodeUtilityValidate.GetValidatedSubtree(
                    ExprNodeOrigin.VARIABLEASSIGN,
                    createDesc.Assignment,
                    validationContext);
                if (validated.Forge.ForgeConstantType == ExprForgeConstantType.COMPILETIMECONST) {
                    initialValue = validated.Forge.ExprEvaluator.Evaluate(null, true, null);
                }

                createDesc.Assignment = validated;
                initialValueExpr = validated.Forge;
            }

            var contextName = statementSpec.Raw.OptionalContextName;
            NameAccessModifier? contextVisibility = null;
            string contextModuleName = null;
            if (contextName != null) {
                var contextDetail = services.ContextCompileTimeResolver.GetContextInfo(contextName);
                if (contextDetail == null) {
                    throw new ExprValidationException("Failed to find context '" + contextName + "'");
                }

                contextVisibility = contextDetail.ContextVisibility;
                contextModuleName = contextDetail.ContextModuleName;
            }

            // get visibility
            var visibility = services.ModuleVisibilityRules.GetAccessModifierVariable(_base, createDesc.VariableName);

            // Compile metadata
            var compileTimeConstant = createDesc.IsConstant &&
                                      initialValueExpr != null &&
                                      initialValueExpr.ForgeConstantType.IsCompileTimeConstant;
            var metaData = VariableUtil.CompileVariable(
                createDesc.VariableName,
                _base.ModuleName,
                visibility,
                contextName,
                contextVisibility,
                contextModuleName,
                createDesc.VariableType,
                createDesc.IsConstant,
                compileTimeConstant,
                initialValue,
                services.ImportServiceCompileTime,
                services.ClassProvidedExtension,
                EventBeanTypedEventFactoryCompileTime.INSTANCE,
                services.EventTypeRepositoryPreconfigured,
                services.BeanEventTypeFactoryPrivate);

            // Register variable
            services.VariableCompileTimeRegistry.NewVariable(metaData);

            // Statement event type
            var eventTypePropertyTypes = Collections.SingletonDataMap(
                metaData.VariableName,
                metaData.Type);
            var eventTypeName = services.EventTypeNameGeneratorStatement.AnonymousTypeName;
            var eventTypeMetadata = new EventTypeMetadata(
                eventTypeName,
                _base.ModuleName,
                EventTypeTypeClass.STATEMENTOUT,
                EventTypeApplicationType.MAP,
                NameAccessModifier.TRANSIENT,
                EventTypeBusModifier.NONBUS,
                false,
                EventTypeIdPair.Unassigned());
            var outputEventType = BaseNestableEventUtil.MakeMapTypeCompileTime(
                eventTypeMetadata,
                eventTypePropertyTypes,
                null,
                null,
                null,
                null,
                services.BeanEventTypeFactoryPrivate,
                services.EventTypeCompileTimeResolver);
            services.EventTypeCompileTimeRegistry.NewType(outputEventType);

            // Handle output format
            var defaultSelectAllSpec = new StatementSpecCompiled();
            defaultSelectAllSpec.SelectClauseCompiled.WithSelectExprList(new SelectClauseElementWildcard());
            defaultSelectAllSpec.Raw.SelectStreamDirEnum = SelectClauseStreamSelectorEnum.RSTREAM_ISTREAM_BOTH;
            StreamTypeService streamTypeService = new StreamTypeServiceImpl(
                new EventType[] {outputEventType},
                new string[] {"trigger_stream"},
                new bool[] {true},
                false,
                false);
            var resultSetProcessor = ResultSetProcessorFactoryFactory.GetProcessorPrototype(
                new ResultSetSpec(defaultSelectAllSpec),
                streamTypeService,
                null,
                new bool[1],
                false,
                _base.ContextPropertyRegistry,
                false,
                false,
                _base.StatementRawInfo,
                services);

            // Code generation
            var statementFieldsClassName =
                CodeGenerationIDGenerator.GenerateClassNameSimple(typeof(StatementFields), classPostfix);
            var aiFactoryProviderClassName = CodeGenerationIDGenerator.GenerateClassNameSimple(
                typeof(StatementAIFactoryProvider),
                classPostfix);
            var classNameRSP = CodeGenerationIDGenerator.GenerateClassNameSimple(
                typeof(ResultSetProcessorFactoryProvider),
                classPostfix);
            var namespaceScope = new CodegenNamespaceScope(
                @namespace,
                statementFieldsClassName,
                services.IsInstrumented);
            
            // serde
            DataInputOutputSerdeForge serde = DataInputOutputSerdeForgeNotApplicable.INSTANCE;
            if (metaData.EventType == null) {
                serde = services.SerdeResolver.SerdeForVariable(
                    metaData.Type, metaData.VariableName, _base.StatementRawInfo);
            }

            var forge =
                new StatementAgentInstanceFactoryCreateVariableForge(
                    createDesc.VariableName,
                    initialValueExpr,
                    classNameRSP);
            var aiFactoryForgeable = new StmtClassForgeableAIFactoryProviderCreateVariable(
                aiFactoryProviderClassName,
                namespaceScope,
                forge,
                createDesc.VariableName,
                serde);

            var informationals = StatementInformationalsUtil.GetInformationals(
                _base,
                Collections.GetEmptyList<FilterSpecCompiled>(),
                Collections.GetEmptyList<ScheduleHandleCallbackProvider>(),
                Collections.GetEmptyList<NamedWindowConsumerStreamSpec>(),
                true,
                resultSetProcessor.SelectSubscriberDescriptor,
                namespaceScope,
                services);
            informationals.Properties.Put(StatementProperty.CREATEOBJECTNAME, createDesc.VariableName);

            var statementProviderClassName =
                CodeGenerationIDGenerator.GenerateClassNameSimple(typeof(StatementProvider), classPostfix);
            var stmtProvider = new StmtClassForgeableStmtProvider(
                aiFactoryProviderClassName,
                statementProviderClassName,
                informationals,
                namespaceScope);

            var forgeables = new List<StmtClassForgeable>();
            forgeables.Add(
                new StmtClassForgeableRSPFactoryProvider(
                    classNameRSP,
                    resultSetProcessor,
                    namespaceScope,
                    _base.StatementRawInfo));
            forgeables.Add(aiFactoryForgeable);
            forgeables.Add(stmtProvider);
            forgeables.Add(new StmtClassForgeableStmtFields(statementFieldsClassName, namespaceScope, 0));
            return new StmtForgeMethodResult(
                forgeables,
                Collections.GetEmptyList<FilterSpecCompiled>(),
                Collections.GetEmptyList<ScheduleHandleCallbackProvider>(),
                Collections.GetEmptyList<NamedWindowConsumerStreamSpec>(),
                Collections.GetEmptyList<FilterSpecParamExprNodeForge>());
        }
    }
} // end of namespace