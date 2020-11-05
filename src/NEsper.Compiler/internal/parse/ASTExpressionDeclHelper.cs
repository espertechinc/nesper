///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;

using Antlr4.Runtime;
using Antlr4.Runtime.Tree;

using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.compile.stage1.specmapper;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.compat.collections;
using com.espertech.esper.grammar.@internal.generated;

namespace com.espertech.esper.compiler.@internal.parse
{
    using MyPair = Pair<ExpressionDeclItem, ExpressionScriptProvided>;

    public class ASTExpressionDeclHelper
    {
        public static MyPair WalkExpressionDecl(
            EsperEPL2GrammarParser.ExpressionDeclContext ctx,
            IList<string> scriptBodies,
            IDictionary<ITree, ExprNode> astExprNodeMap,
            CommonTokenStream tokenStream)
        {
            var name = ctx.name.Text;

            if (ctx.alias != null)
            {
                if (!ctx.alias.Text.ToLowerInvariant().Trim().Equals("alias"))
                {
                    throw ASTWalkException.From(
                        "For expression alias '" + name + "' expecting 'alias' keyword but received '" + ctx.alias.Text + "'");
                }

                if (ctx.columnList() != null)
                {
                    throw ASTWalkException.From(
                        "For expression alias '" + name + "' expecting no parameters but received '" + tokenStream.GetText(ctx.columnList()) + "'");
                }

                if (ctx.expressionDef() != null && ctx.expressionDef().expressionLambdaDecl() != null)
                {
                    throw ASTWalkException.From(
                        "For expression alias '" + name + "' expecting an expression without parameters but received '" +
                        tokenStream.GetText(ctx.expressionDef().expressionLambdaDecl()) + "'");
                }

                if (ctx.expressionDef().stringconstant() != null)
                {
                    throw ASTWalkException.From("For expression alias '" + name + "' expecting an expression but received a script");
                }

                var node = ASTExprHelper.ExprCollectSubNodes(ctx, 0, astExprNodeMap)[0];
                var alias = ctx.name.Text;
                var expressionUnmap = StatementSpecMapper.Unmap(node);
                var decl = new ExpressionDeclItem(alias, new string[0], true);
                decl.OptionalSoda = expressionUnmap;
                return new MyPair(decl, null);
            }

            if (ctx.expressionDef().stringconstant() != null)
            {
                var expressionText = scriptBodies.DeleteAt(0);
                var parameters = ASTUtil.GetIdentList(ctx.columnList());
                var optionalReturnType = ctx.classIdentifier() == null ? null : ASTUtil.UnescapeClassIdent(ctx.classIdentifier());
                var optionalReturnTypeArray = ctx.array != null;
                var optionalDialect = ctx.expressionDialect() == null ? null : ctx.expressionDialect().d.Text;
                var optionalEventTypeName = ASTTypeExpressionAnnoHelper.ExpectMayTypeAnno(ctx.typeExpressionAnnotation(), tokenStream);
                var script = new ExpressionScriptProvided(
                    name, expressionText, parameters.ToArray(),
                    optionalReturnType, optionalReturnTypeArray, optionalEventTypeName, optionalDialect);
                return new MyPair(null, script);
            }

            var ctxexpr = ctx.expressionDef();
            var inner = ASTExprHelper.ExprCollectSubNodes(ctxexpr.expression(), 0, astExprNodeMap)[0];

            IList<string> parametersNames = new EmptyList<string>();
            var lambdactx = ctxexpr.expressionLambdaDecl();
            if (ctxexpr.expressionLambdaDecl() != null)
            {
                parametersNames = ASTLambdaHelper.GetLambdaGoesParams(lambdactx);
            }

            var expression = StatementSpecMapper.Unmap(inner);
            var expr = new ExpressionDeclItem(name, parametersNames.ToArray(), false);
            expr.OptionalSoda = expression;
            return new MyPair(expr, null);
        }

        public static string WalkClassDecl(IList<string> classBodies)
        {
            return classBodies.DeleteAt(0);
        }
    }
} // end of namespace