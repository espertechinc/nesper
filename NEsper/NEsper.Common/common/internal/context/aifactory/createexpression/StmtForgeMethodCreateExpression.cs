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
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.resultset.select.core;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.filterspec;
using com.espertech.esper.common.@internal.schedule;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.context.aifactory.createexpression
{
    public class StmtForgeMethodCreateExpression : StmtForgeMethodCreateSimpleBase
    {
        public StmtForgeMethodCreateExpression(StatementBaseInfo @base) : base(@base)
        {
        }

        protected override string Register(
            StatementCompileTimeServices services)
        {
            var spec = Base.StatementSpec.Raw.CreateExpressionDesc;

            string expressionName;
            if (spec.Expression != null) {
                // register expression
                expressionName = spec.Expression.Name;
                var visibility = services.ModuleVisibilityRules.GetAccessModifierExpression(Base, expressionName);
                CheckAlreadyDeclared(expressionName, services, -1);
                var item = spec.Expression;
                item.ModuleName = Base.ModuleName;
                item.Visibility = visibility;
                services.ExprDeclaredCompileTimeRegistry.NewExprDeclared(item);
            }
            else {
                // register script
                expressionName = spec.Script.Name;
                var numParameters = spec.Script.ParameterNames.Length;
                CheckAlreadyDeclared(expressionName, services, numParameters);
                var visibility =
                    services.ModuleVisibilityRules.GetAccessModifierScript(Base, expressionName, numParameters);
                var item = spec.Script;
                item.ModuleName = Base.ModuleName;
                item.Visibility = visibility;
                services.ScriptCompileTimeRegistry.NewScript(item);
            }
            
            return expressionName;
        }
        protected override StmtClassForgeable AiFactoryForgable(
            string className,
            CodegenNamespaceScope namespaceScope,
            EventType statementEventType,
            string objectName)
        {
            StatementAgentInstanceFactoryCreateExpressionForge forge = new StatementAgentInstanceFactoryCreateExpressionForge(statementEventType, objectName);
            return new StmtClassForgeableAIFactoryProviderCreateExpression(className, namespaceScope, forge);
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
                    "Script '" +
                    expressionName +
                    "' that takes the same number of parameters has already been declared");
            }
        }
    }
} // end of namespace