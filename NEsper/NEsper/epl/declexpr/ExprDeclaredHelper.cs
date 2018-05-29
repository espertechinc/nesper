///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.container;
using com.espertech.esper.core.context.util;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.script;
using com.espertech.esper.epl.spec;

namespace com.espertech.esper.epl.declexpr
{
    public class ExprDeclaredHelper
    {
        public static ExprDeclaredNodeImpl GetExistsDeclaredExpr(
            IContainer container,
            String name,
            IList<ExprNode> parameters,
            ICollection<ExpressionDeclItem> expressionDeclarations,
            ExprDeclaredService exprDeclaredService, 
            ContextDescriptor contextDescriptor)
        {
            // Find among local expressions
            if (expressionDeclarations.IsNotEmpty())
            {
                foreach (ExpressionDeclItem declNode in expressionDeclarations)
                {
                    if (declNode.Name.Equals(name))
                    {
                        return new ExprDeclaredNodeImpl(
                            container, declNode, parameters, contextDescriptor);
                    }
                }
            }

            // find among global expressions
            ExpressionDeclItem found = exprDeclaredService.GetExpression(name);
            if (found != null)
            {
                return new ExprDeclaredNodeImpl(
                    container, found, parameters, contextDescriptor);
            }
            return null;
        }

        public static ExprNodeScript GetExistsScript(
            String defaultDialect,
            String expressionName, 
            IList<ExprNode> parameters,
            ICollection<ExpressionScriptProvided> scriptExpressions,
            ExprDeclaredService exprDeclaredService)
        {
            if (scriptExpressions.IsNotEmpty())
            {
                var scriptProvided = FindScript(expressionName, parameters.Count, scriptExpressions);
                if (scriptProvided != null)
                {
                    return new ExprNodeScript(defaultDialect, scriptProvided, parameters);
                }
            }

            var globalScripts = exprDeclaredService.GetScriptsByName(expressionName);
            var script = FindScript(expressionName, parameters.Count, globalScripts);
            if (script != null)
            {
                return new ExprNodeScript(defaultDialect, script, parameters);
            }
            return null;
        }

        private static ExpressionScriptProvided FindScript(
            String name, int parameterCount, 
            ICollection<ExpressionScriptProvided> scriptsByName)
        {
            if (scriptsByName == null || scriptsByName.IsEmpty())
            {
                return null;
            }
            ExpressionScriptProvided nameMatchedScript = null;
            foreach (ExpressionScriptProvided script in scriptsByName)
            {
                if (script.Name.Equals(name) && script.ParameterNames.Count == parameterCount)
                {
                    return script;
                }
                if (script.Name.Equals(name))
                {
                    nameMatchedScript = script;
                }
            }
            return nameMatchedScript;
        }
    }
}
