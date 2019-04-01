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
using com.espertech.esper.common.client.meta;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.core;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.resultset.select.core;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.filterspec;
using com.espertech.esper.common.@internal.schedule;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.context.aifactory.createexpression
{
    public class StmtForgeMethodCreateExpression : StmtForgeMethod
    {
        private readonly StatementBaseInfo @base;

        public StmtForgeMethodCreateExpression(StatementBaseInfo @base)
        {
            this.@base = @base;
        }

        public StmtForgeMethodResult Make(
            string packageName,
            string classPostfix,
            StatementCompileTimeServices services)
        {
            CreateExpressionDesc spec = @base.StatementSpec.Raw.CreateExpressionDesc;

            string expressionName;
            if (spec.Expression != null) {
                // register expression
                expressionName = spec.Expression.Name;
                NameAccessModifier visibility = services.ModuleVisibilityRules.GetAccessModifierExpression(@base, expressionName);
                CheckAlreadyDeclared(expressionName, services, -1);
                ExpressionDeclItem item = spec.Expression;
                item.ModuleName = base.ModuleName;
                item.Visibility = visibility;
                services.ExprDeclaredCompileTimeRegistry.NewExprDeclared(item);
            }
            else {
                // register script
                expressionName = spec.Script.Name;
                int numParameters = spec.Script.ParameterNames.Length;
                CheckAlreadyDeclared(expressionName, services, numParameters);
                NameAccessModifier visibility = services.ModuleVisibilityRules.GetAccessModifierScript(@base, expressionName, numParameters);
                ExpressionScriptProvided item = spec.Script;
                item.ModuleName = base.ModuleName;
                item.Visibility = visibility;
                services.ScriptCompileTimeRegistry.NewScript(item);
            }

            // define output event type
            string statementEventTypeName = services.EventTypeNameGeneratorStatement.AnonymousTypeName;
            EventTypeMetadata statementTypeMetadata = new EventTypeMetadata(
                statementEventTypeName, @base.ModuleName, EventTypeTypeClass.STATEMENTOUT, EventTypeApplicationType.MAP, NameAccessModifier.TRANSIENT,
                EventTypeBusModifier.NONBUS, false, EventTypeIdPair.Unassigned());
            EventType statementEventType = BaseNestableEventUtil.MakeMapTypeCompileTime(
                statementTypeMetadata, new EmptyDictionary<string, object>(), null, null, null, null, services.BeanEventTypeFactoryPrivate,
                services.EventTypeCompileTimeResolver);
            services.EventTypeCompileTimeRegistry.NewType(statementEventType);

            CodegenPackageScope packageScope = new CodegenPackageScope(packageName, null, services.IsInstrumented);

            string aiFactoryProviderClassName = CodeGenerationIDGenerator.GenerateClassNameSimple(typeof(StatementAIFactoryProvider), classPostfix);
            StatementAgentInstanceFactoryCreateExpressionForge forge =
                new StatementAgentInstanceFactoryCreateExpressionForge(statementEventType, expressionName);
            StmtClassForgableAIFactoryProviderCreateExpression aiFactoryForgable =
                new StmtClassForgableAIFactoryProviderCreateExpression(aiFactoryProviderClassName, packageScope, forge);

            SelectSubscriberDescriptor selectSubscriberDescriptor = new SelectSubscriberDescriptor();
            StatementInformationalsCompileTime informationals = StatementInformationalsUtil.GetInformationals(
                @base, 
                new EmptyList<FilterSpecCompiled>(), 
                new EmptyList<ScheduleHandleCallbackProvider>(), 
                new EmptyList<NamedWindowConsumerStreamSpec>(), 
                false, selectSubscriberDescriptor, packageScope, services);
            string statementProviderClassName = CodeGenerationIDGenerator.GenerateClassNameSimple(typeof(StatementProvider), classPostfix);
            StmtClassForgableStmtProvider stmtProvider = new StmtClassForgableStmtProvider(
                aiFactoryProviderClassName, statementProviderClassName, informationals, packageScope);

            IList<StmtClassForgable> forgables = new List<StmtClassForgable>();
            forgables.Add(aiFactoryForgable);
            forgables.Add(stmtProvider);
            return new StmtForgeMethodResult(
                forgables,
                new EmptyList<FilterSpecCompiled>(),
                new EmptyList<ScheduleHandleCallbackProvider>(),
                new EmptyList<NamedWindowConsumerStreamSpec>(),
                new EmptyList<FilterSpecParamExprNodeForge>());
        }

        private void CheckAlreadyDeclared(
            string expressionName,
            StatementCompileTimeServices services,
            int numParameters)
        {
            if (services.ExprDeclaredCompileTimeResolver.Resolve(expressionName) != null) {
                throw new ExprValidationException("Expression '" + expressionName + "' has already been declared");
            }

            if (services.ScriptCompileTimeResolver.Resolve(expressionName, numParameters) != null) {
                throw new ExprValidationException(
                    "Script '" + expressionName + "' that takes the same number of parameters has already been declared");
            }
        }
    }
} // end of namespace