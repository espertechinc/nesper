///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.expression.core;

namespace com.espertech.esper.common.@internal.context.aifactory.createexpression
{
    public class StmtForgeMethodCreateExpression : StmtForgeMethodCreateSimpleBase
    {
        public StmtForgeMethodCreateExpression(StatementBaseInfo @base) : base(@base)
        {
        }

        protected override StmtForgeMethodRegisterResult Register(StatementCompileTimeServices services)
        {
            var spec = _base.StatementSpec.Raw.CreateExpressionDesc;

            string expressionName;
            if (spec.Expression != null) {
                // register expression
                expressionName = spec.Expression.Name;
                var visibility = services.ModuleVisibilityRules.GetAccessModifierExpression(_base, expressionName);
                CheckAlreadyDeclared(expressionName, services, -1);
                var item = spec.Expression;
                item.ModuleName = _base.ModuleName;
                item.Visibility = visibility;
                services.ExprDeclaredCompileTimeRegistry.NewExprDeclared(item);
            }
            else {
                // register script
                expressionName = spec.Script.Name;
                var numParameters = spec.Script.ParameterNames.Length;
                CheckAlreadyDeclared(expressionName, services, numParameters);
                var visibility =
                    services.ModuleVisibilityRules.GetAccessModifierScript(_base, expressionName, numParameters);
                var item = spec.Script;
                item.ModuleName = _base.ModuleName;
                item.Visibility = visibility;
                services.ScriptCompileTimeRegistry.NewScript(item);
            }

            return new StmtForgeMethodRegisterResult(expressionName, services.StateMgmtSettingsProvider.NewCharge());
        }

        protected override StmtClassForgeable AiFactoryForgable(
            string className,
            CodegenNamespaceScope namespaceScope,
            EventType statementEventType,
            string objectName)
        {
            var forge = new StatementAgentInstanceFactoryCreateExpressionForge(statementEventType, objectName);
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