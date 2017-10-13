///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using Antlr4.Runtime;
using Antlr4.Runtime.Tree;

using com.espertech.esper.collection;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.generated;
using com.espertech.esper.epl.spec;

namespace com.espertech.esper.epl.parse
{
    public class ASTExpressionDeclHelper
    {
        public static Pair<ExpressionDeclItem, ExpressionScriptProvided> WalkExpressionDecl(
            EsperEPL2GrammarParser.ExpressionDeclContext ctx,
            IList<String> scriptBodies,
            IDictionary<ITree, ExprNode> astExprNodeMap,
            CommonTokenStream tokenStream)
        {
            var name = ctx.name.Text;

            if (ctx.alias != null)
            {
                if (ctx.alias.Text.ToLower().Trim() != "alias")
                {
                    throw ASTWalkException.From("For expression alias '" + name + "' expecting 'alias' keyword but received '" + ctx.alias.Text + "'");
                }
                if (ctx.columnList() != null)
                {
                    throw ASTWalkException.From("For expression alias '" + name + "' expecting no parameters but received '" + tokenStream.GetText(ctx.columnList()) + "'");
                }
                if (ctx.expressionDef() != null && ctx.expressionDef().expressionLambdaDecl() != null)
                {
                    throw ASTWalkException.From("For expression alias '" + name + "' expecting an expression without parameters but received '" + tokenStream.GetText(ctx.expressionDef().expressionLambdaDecl()) + "'");
                }
                if (ctx.expressionDef().stringconstant() != null)
                {
                    throw ASTWalkException.From("For expression alias '" + name + "' expecting an expression but received a script");
                }
                var exprNode = ASTExprHelper.ExprCollectSubNodes(ctx, 0, astExprNodeMap)[0];
                var alias = ctx.name.Text;
                var decl = new ExpressionDeclItem(alias, Collections.GetEmptyList<String>(), exprNode, true);
                return new Pair<ExpressionDeclItem, ExpressionScriptProvided>(decl, null);
            }

            if (ctx.expressionDef().stringconstant() != null)
            {
                var expressionText = scriptBodies[0];
                scriptBodies.RemoveAt(0);
                var parameters = ASTUtil.GetIdentList(ctx.columnList());
                var optionalReturnType = ctx.classIdentifier() == null
                    ? null
                    : ASTUtil.UnescapeClassIdent(ctx.classIdentifier());
                var optionalReturnTypeArray = ctx.array != null;
                var optionalDialect = ctx.expressionDialect() == null ? null : ctx.expressionDialect().d.Text;
                var optionalEventTypeName = ASTTypeExpressionAnnoHelper.ExpectMayTypeAnno(ctx.typeExpressionAnnotation(), tokenStream); 
                var script = new ExpressionScriptProvided(
                    name, expressionText, parameters,
                    optionalReturnType, 
                    optionalReturnTypeArray, 
                    optionalEventTypeName, 
                    optionalDialect);
                return new Pair<ExpressionDeclItem, ExpressionScriptProvided>(null, script);
            }

            var ctxexpr = ctx.expressionDef();
            var inner = ASTExprHelper.ExprCollectSubNodes(ctxexpr.expression(), 0, astExprNodeMap)[0];

            var parametersNames = Collections.GetEmptyList<string>();
            var lambdactx = ctxexpr.expressionLambdaDecl();
            if (ctxexpr.expressionLambdaDecl() != null)
            {
                parametersNames = ASTLibFunctionHelper.GetLambdaGoesParams(lambdactx);
            }

            var expr = new ExpressionDeclItem(name, parametersNames, inner, false);
            return new Pair<ExpressionDeclItem, ExpressionScriptProvided>(expr, null);
        }
    }
}
