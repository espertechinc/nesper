///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
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
        private readonly StatementBaseInfo @base;

        public StmtForgeMethodCreateVariable(StatementBaseInfo @base)
        {
            this.@base = @base;
        }

        public StmtForgeMethodResult Make(
            string @namespace,
            string classPostfix,
            StatementCompileTimeServices services)
        {
            var statementSpec = @base.StatementSpec;
            IList<StmtClassForgeableFactory> additionalForgeables = new List<StmtClassForgeableFactory>(2);

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
                    Array.Empty<EventType>(),
                    Array.Empty<string>(),
                    Array.Empty<bool>(),
                    false,
                    false);
                var validationContext =
                    new ExprValidationContextBuilder(typeService, @base.StatementRawInfo, services).Build();
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
            var visibility = services.ModuleVisibilityRules.GetAccessModifierVariable(@base, createDesc.VariableName);

            // Compile metadata
            var compileTimeConstant = createDesc.IsConstant &&
                                      initialValueExpr != null &&
                                      initialValueExpr.ForgeConstantType.IsCompileTimeConstant;
            VariableMetadataWithForgables metaWithForgables = VariableUtil.CompileVariable(
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
                @base.StatementRawInfo,
                services);
            var metaData = metaWithForgables.VariableMetaData;
            additionalForgeables.AddAll(metaWithForgables.Forgables);

            // Register variable
            services.VariableCompileTimeRegistry.NewVariable(metaData);

            // Statement event type
            var eventTypePropertyTypes = Collections.SingletonDataMap(metaData.VariableName, metaData.Type);
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
            defaultSelectAllSpec.SelectClauseCompiled.SelectExprList = new[] { new SelectClauseElementWildcard() };
            defaultSelectAllSpec.Raw.SelectStreamDirEnum = SelectClauseStreamSelectorEnum.RSTREAM_ISTREAM_BOTH;
            StreamTypeService streamTypeService = new StreamTypeServiceImpl(
                new EventType[] { outputEventType },
                new string[] { "trigger_stream" },
                new bool[] { true },
                false,
                false);
            var resultSetProcessor = ResultSetProcessorFactoryFactory.GetProcessorPrototype(
                ResultSetProcessorAttributionKeyStatement.INSTANCE,
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
                services.IsInstrumented,
                services.Configuration.Compiler.ByteCode);

            // serde
            DataInputOutputSerdeForge serde = DataInputOutputSerdeForgeSkip.INSTANCE;
            if (metaData.EventType == null) {
                serde = services.SerdeResolver.SerdeForVariable(
                    metaData.Type,
                    metaData.VariableName,
                    @base.StatementRawInfo);
            }

            var forge = new StatementAgentInstanceFactoryCreateVariableForge(
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
                @base,
                EmptyList<FilterSpecTracked>.Instance,
                EmptyList<ScheduleHandleTracked>.Instance, 
                EmptyList<NamedWindowConsumerStreamSpec>.Instance, 
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

            IList<StmtClassForgeable> forgeables = new List<StmtClassForgeable>();
            foreach (var additional in additionalForgeables) {
                forgeables.Add(additional.Make(namespaceScope, classPostfix));
            }

            forgeables.Add(
                new StmtClassForgeableRSPFactoryProvider(
                    classNameRSP,
                    resultSetProcessor,
                    namespaceScope,
                    @base.StatementRawInfo,
                    services.SerdeResolver.IsTargetHA));
            forgeables.Add(aiFactoryForgeable);
            forgeables.Add(stmtProvider);
            forgeables.Add(new StmtClassForgeableStmtFields(statementFieldsClassName, namespaceScope));
            return new StmtForgeMethodResult(
                forgeables,
                EmptyList<FilterSpecTracked>.Instance, 
                EmptyList<ScheduleHandleTracked>.Instance, 
                EmptyList<NamedWindowConsumerStreamSpec>.Instance, 
                EmptyList<FilterSpecParamExprNodeForge>.Instance, 
                namespaceScope,
                services.StateMgmtSettingsProvider.NewCharge());
        }
    }
} // end of namespace