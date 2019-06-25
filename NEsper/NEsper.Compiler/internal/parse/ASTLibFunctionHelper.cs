///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;

using Antlr4.Runtime;
using Antlr4.Runtime.Tree;

using com.espertech.esper.common.client.configuration.compiler;
using com.espertech.esper.common.client.hook.aggmultifunc;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.compile.stage1.specmapper;
using com.espertech.esper.common.@internal.context.compile;
using com.espertech.esper.common.@internal.epl.enummethod.dot;
using com.espertech.esper.common.@internal.epl.expression.agg.@base;
using com.espertech.esper.common.@internal.epl.expression.agg.method;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.declared.compiletime;
using com.espertech.esper.common.@internal.epl.expression.dot.core;
using com.espertech.esper.common.@internal.epl.expression.funcs;
using com.espertech.esper.common.@internal.epl.table.compiletime;
using com.espertech.esper.common.@internal.settings;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compiler.@internal.generated;

namespace com.espertech.esper.compiler.@internal.parse
{
    public class ASTLibFunctionHelper
    {
        public static IList<ExprChainedSpec> GetLibFuncChain(
            IList<EsperEPL2GrammarParser.LibFunctionNoClassContext> ctxs,
            IDictionary<ITree, ExprNode> astExprNodeMap)
        {
            IList<ExprChainedSpec> chained = new List<ExprChainedSpec>(ctxs.Count);
            foreach (var ctx in ctxs)
            {
                var chainSpec = GetLibFunctionChainSpec(ctx, astExprNodeMap);
                chained.Add(chainSpec);
            }

            return chained;
        }

        public static ExprChainedSpec GetLibFunctionChainSpec(
            EsperEPL2GrammarParser.LibFunctionNoClassContext ctx,
            IDictionary<ITree, ExprNode> astExprNodeMap)
        {
            var methodName = StringValue.RemoveTicks(ctx.funcIdentChained().GetText());

            var parameters = GetExprNodesLibFunc(ctx.libFunctionArgs(), astExprNodeMap);
            var property = ctx.l == null;
            return new ExprChainedSpec(methodName, parameters, property);
        }

        public static IList<ExprNode> GetExprNodesLibFunc(
            EsperEPL2GrammarParser.LibFunctionArgsContext ctx,
            IDictionary<ITree, ExprNode> astExprNodeMap)
        {
            if (ctx == null)
            {
                return new EmptyList<ExprNode>();
            }

            IList<EsperEPL2GrammarParser.LibFunctionArgItemContext> args = ctx.libFunctionArgItem();
            if (args == null || args.IsEmpty())
            {
                return new EmptyList<ExprNode>();
            }

            IList<ExprNode> parameters = new List<ExprNode>(args.Count);
            foreach (var arg in args)
            {
                if (arg.expressionLambdaDecl() != null)
                {
                    var lambdaparams = GetLambdaGoesParams(arg.expressionLambdaDecl());
                    var goes = new ExprLambdaGoesNode(lambdaparams);
                    var lambdaExpr = ASTExprHelper.ExprCollectSubNodes(arg.expressionWithNamed(), 0, astExprNodeMap)[0];
                    goes.AddChildNode(lambdaExpr);
                    parameters.Add(goes);
                }
                else
                {
                    var parameter = ASTExprHelper.ExprCollectSubNodes(arg.expressionWithNamed(), 0, astExprNodeMap)[0];
                    parameters.Add(parameter);
                }
            }

            return parameters;
        }

        public static IList<string> GetLambdaGoesParams(EsperEPL2GrammarParser.ExpressionLambdaDeclContext ctx)
        {
            IList<string> parameters;
            if (ctx.i != null)
            {
                parameters = new List<string>(1);
                parameters.Add(ctx.i.Text);
            }
            else
            {
                parameters = ASTUtil.GetIdentList(ctx.columnList());
            }

            return parameters;
        }

        public static void HandleLibFunc(
            CommonTokenStream tokenStream,
            EsperEPL2GrammarParser.LibFunctionContext ctx,
            IDictionary<ITree, ExprNode> astExprNodeMap,
            LazyAllocatedMap<ConfigurationCompilerPlugInAggregationMultiFunction, AggregationMultiFunctionForge> plugInAggregations,
            ExpressionDeclDesc expressionDeclarations,
            IList<ExpressionScriptProvided> scriptExpressions,
            ContextCompileTimeDescriptor contextDescriptor,
            StatementSpecRaw statementSpec,
            StatementSpecMapEnv mapEnv)
        {
            var configurationInformation = mapEnv.Configuration;
            var importService = mapEnv.ImportService;

            var model = GetModel(ctx, tokenStream);
            var duckType = configurationInformation.Compiler.Expression.IsDuckTyping;
            var udfCache = configurationInformation.Compiler.Expression.IsUdfCache;

            // handle "some.xyz(...)" or "some.other.xyz(...)"
            if (model.ChainElements.Count == 1 &&
                model.OptionalClassIdent != null &&
                ASTTableExprHelper.CheckTableNameGetExprForProperty(mapEnv.TableCompileTimeResolver, model.OptionalClassIdent) == null)
            {
                var chainSpec = GetLibFunctionChainSpec(model.ChainElements[0], astExprNodeMap);

                var declaredExpr = ExprDeclaredHelper.GetExistsDeclaredExpr(
                    model.OptionalClassIdent, Collections.GetEmptyList<ExprNode>(), expressionDeclarations.Expressions, contextDescriptor, mapEnv);
                if (declaredExpr != null)
                {
                    AddMapContext(statementSpec, declaredExpr.Second);
                    ExprNode exprNode = new ExprDotNodeImpl(Collections.SingletonList(chainSpec), duckType, udfCache);
                    exprNode.AddChildNode(declaredExpr.First);
                    ASTExprHelper.ExprCollectAddSubNodesAddParentNode(exprNode, ctx, astExprNodeMap);
                    return;
                }

                IList<ExprChainedSpec> chainedSpecs = new List<ExprChainedSpec>(2);
                chainedSpecs.Add(new ExprChainedSpec(model.OptionalClassIdent, Collections.GetEmptyList<ExprNode>(), true));
                chainedSpecs.Add(chainSpec);
                ExprDotNode exprDotNode = new ExprDotNodeImpl(
                    chainedSpecs, configurationInformation.Compiler.Expression.IsDuckTyping, configurationInformation.Compiler.Expression.IsUdfCache);
                var variable = exprDotNode.IsVariableOpGetName(mapEnv.VariableCompileTimeResolver);
                if (variable != null)
                {
                    statementSpec.ReferencedVariables.Add(variable.VariableName);
                }

                ASTExprHelper.ExprCollectAddSubNodesAddParentNode(exprDotNode, ctx, astExprNodeMap);
                return;
            }

            // try additional built-in single-row function
            var singleRowExtNode = importService.ResolveSingleRowExtendedBuiltin(model.ChainElements[0].FuncName);
            if (singleRowExtNode != null)
            {
                if (model.ChainElements.Count == 1)
                {
                    ASTExprHelper.ExprCollectAddSubNodesAddParentNode(singleRowExtNode, ctx, astExprNodeMap);
                    return;
                }

                IList<ExprChainedSpec> spec = new List<ExprChainedSpec>();
                var firstArgs = model.ChainElements[0].Args;
                var childExpressions = GetExprNodesLibFunc(firstArgs, astExprNodeMap);
                singleRowExtNode.AddChildNodes(childExpressions);
                AddChainRemainderFromOffset(model.ChainElements, 1, spec, astExprNodeMap);
                ExprDotNode exprDotNode = new ExprDotNodeImpl(
                    spec, configurationInformation.Compiler.Expression.IsDuckTyping, configurationInformation.Compiler.Expression.IsUdfCache);
                exprDotNode.AddChildNode(singleRowExtNode);
                ASTExprHelper.ExprCollectAddSubNodesAddParentNode(exprDotNode, ctx, astExprNodeMap);
                return;
            }

            // try plug-in single-row function
            try
            {
                var firstFunctionName = model.ChainElements[0].FuncName;
                var firstFunctionIsProperty = !model.ChainElements[0].IsHasLeftParen;
                var classMethodPair = importService.ResolveSingleRow(firstFunctionName);
                IList<ExprChainedSpec> spec = new List<ExprChainedSpec>();
                var firstArgs = model.ChainElements[0].Args;
                var childExpressions = GetExprNodesLibFunc(firstArgs, astExprNodeMap);
                spec.Add(new ExprChainedSpec(classMethodPair.Second.MethodName, childExpressions, firstFunctionIsProperty));
                AddChainRemainderFromOffset(model.ChainElements, 1, spec, astExprNodeMap);
                var plugin = new ExprPlugInSingleRowNode(firstFunctionName, classMethodPair.First, spec, classMethodPair.Second);
                ASTExprHelper.ExprCollectAddSubNodesAddParentNode(plugin, ctx, astExprNodeMap);
                return;
            }
            catch (ImportUndefinedException e)
            {
                // Not an single-row function
            }

            // special case for min,max
            var firstFunction = model.ChainElements[0].FuncName;
            if (model.OptionalClassIdent == null &&
                string.Equals(firstFunction, "max", StringComparison.InvariantCultureIgnoreCase) ||
                string.Equals(firstFunction, "min", StringComparison.InvariantCultureIgnoreCase) ||
                string.Equals(firstFunction, "fmax", StringComparison.InvariantCultureIgnoreCase) ||
                string.Equals(firstFunction, "fmin", StringComparison.InvariantCultureIgnoreCase))
            {
                var firstArgs = model.ChainElements[0].Args;
                HandleMinMax(firstFunction, firstArgs, astExprNodeMap);
                if (model.ChainElements.Count <= 1)
                {
                    return;
                }

                IList<ExprChainedSpec> chainSpec = new List<ExprChainedSpec>();
                AddChainRemainderFromOffset(model.ChainElements, 1, chainSpec, astExprNodeMap);
                var exprNode = new ExprDotNodeImpl(chainSpec, duckType, udfCache);
                exprNode.AddChildNode(astExprNodeMap.Delete(firstArgs));
                astExprNodeMap.Put(ctx, exprNode);
                return;
            }

            // obtain chain with actual expressions
            IList<ExprChainedSpec> chain = new List<ExprChainedSpec>();
            AddChainRemainderFromOffset(model.ChainElements, 0, chain, astExprNodeMap);

            // add chain element for class info, if any
            var distinct = model.ChainElements[0].Args != null && model.ChainElements[0].Args.DISTINCT() != null;
            if (model.OptionalClassIdent != null)
            {
                chain.Insert(0, new ExprChainedSpec(model.OptionalClassIdent, Collections.GetEmptyList<ExprNode>(), true));
                distinct = false;
            }

            firstFunction = chain[0].Name;

            // try plug-in aggregation function
            var aggregationNode = ASTAggregationHelper.TryResolveAsAggregation(importService, distinct, firstFunction, plugInAggregations);
            if (aggregationNode != null)
            {
                ExprChainedSpec firstSpec = chain.DeleteAt(0);
                aggregationNode.AddChildNodes(firstSpec.Parameters);
                ExprNode exprNode;
                if (chain.IsEmpty())
                {
                    exprNode = aggregationNode;
                }
                else
                {
                    exprNode = new ExprDotNodeImpl(chain, duckType, udfCache);
                    exprNode.AddChildNode(aggregationNode);
                }

                ASTExprHelper.ExprCollectAddSubNodesAddParentNode(exprNode, ctx, astExprNodeMap);
                return;
            }

            // try declared or alias expression
            var declaredNode = ExprDeclaredHelper.GetExistsDeclaredExpr(
                firstFunction, chain[0].Parameters, expressionDeclarations.Expressions, contextDescriptor, mapEnv);
            if (declaredNode != null)
            {
                AddMapContext(statementSpec, declaredNode.Second);
                chain.RemoveAt(0);
                ExprNode exprNode;
                if (chain.IsEmpty())
                {
                    exprNode = declaredNode.First;
                }
                else
                {
                    exprNode = new ExprDotNodeImpl(chain, duckType, udfCache);
                    exprNode.AddChildNode(declaredNode.First);
                }

                ASTExprHelper.ExprCollectAddSubNodesAddParentNode(exprNode, ctx, astExprNodeMap);
                return;
            }

            // try script
            var scriptNode = ExprDeclaredHelper.GetExistsScript(
                configurationInformation.Compiler.Scripts.DefaultDialect, chain[0].Name, chain[0].Parameters, scriptExpressions, mapEnv);
            if (scriptNode != null)
            {
                chain.RemoveAt(0);
                ExprNode exprNode;
                if (chain.IsEmpty())
                {
                    exprNode = scriptNode;
                }
                else
                {
                    exprNode = new ExprDotNodeImpl(chain, duckType, udfCache);
                    exprNode.AddChildNode(scriptNode);
                }

                ASTExprHelper.ExprCollectAddSubNodesAddParentNode(exprNode, ctx, astExprNodeMap);
                return;
            }

            // try table
            var tableInfo = TableCompileTimeUtil.CheckTableNameGetLibFunc(
                mapEnv.TableCompileTimeResolver, importService, plugInAggregations, firstFunction, chain);
            if (tableInfo != null)
            {
                statementSpec.TableExpressions.Add(tableInfo.First);
                chain = tableInfo.Second;
                ExprNode exprNode;
                if (chain.IsEmpty())
                {
                    exprNode = tableInfo.First;
                }
                else
                {
                    exprNode = new ExprDotNodeImpl(chain, duckType, udfCache);
                    exprNode.AddChildNode(tableInfo.First);
                }

                ASTExprHelper.ExprCollectAddSubNodesAddParentNode(exprNode, ctx, astExprNodeMap);
                return;
            }

            // Could be a mapped property with an expression-parameter "mapped(expr)" or array property with an expression-parameter "array(expr)".
            ExprDotNode dotNode;
            if (chain.Count == 1)
            {
                dotNode = new ExprDotNodeImpl(chain, false, false);
            }
            else
            {
                dotNode = new ExprDotNodeImpl(chain, duckType, udfCache);
            }

            ASTExprHelper.ExprCollectAddSubNodesAddParentNode(dotNode, ctx, astExprNodeMap);
        }

        public static void AddMapContext(
            StatementSpecRaw statementSpec,
            StatementSpecMapContext mapContext)
        {
            statementSpec.TableExpressions.AddAll(mapContext.TableExpressions);
            statementSpec.ReferencedVariables.AddAll(mapContext.VariableNames);
        }

        private static void AddChainRemainderFromOffset(
            IList<ASTLibModelChainElement> chainElements,
            int offset,
            IList<ExprChainedSpec> specList,
            IDictionary<ITree, ExprNode> astExprNodeMap)
        {
            for (var i = offset; i < chainElements.Count; i++)
            {
                var spec = GetLibFunctionChainSpec(chainElements[i], astExprNodeMap);
                specList.Add(spec);
            }
        }

        private static ExprChainedSpec GetLibFunctionChainSpec(
            ASTLibModelChainElement element,
            IDictionary<ITree, ExprNode> astExprNodeMap)
        {
            var methodName = StringValue.RemoveTicks(element.FuncName);
            var parameters = GetExprNodesLibFunc(element.Args, astExprNodeMap);
            return new ExprChainedSpec(methodName, parameters, !element.IsHasLeftParen);
        }

        private static ASTLibModel GetModel(
            EsperEPL2GrammarParser.LibFunctionContext ctx,
            CommonTokenStream tokenStream)
        {
            var root = ctx.libFunctionWithClass();
            IList<EsperEPL2GrammarParser.LibFunctionNoClassContext> ctxElements = ctx.libFunctionNoClass();

            // there are no additional methods
            if (ctxElements == null || ctxElements.IsEmpty())
            {
                var classIdent = root.classIdentifier() == null ? null : ASTUtil.UnescapeClassIdent(root.classIdentifier());
                var ele = FromRoot(root);
                return new ASTLibModel(classIdent, Collections.SingletonList(ele));
            }

            // add root and chain to just a list of elements
            IList<ASTLibModelChainElement> chainElements = new List<ASTLibModelChainElement>(ctxElements.Count + 1);
            var rootElement = FromRoot(root);
            chainElements.Add(rootElement);
            foreach (var chainedCtx in ctxElements)
            {
                var chainedElement = new ASTLibModelChainElement(
                    chainedCtx.funcIdentChained().GetText(), 
                    chainedCtx.libFunctionArgs(),
                    chainedCtx.l != null);
                chainElements.Add(chainedElement);
            }

            // determine/remove the list of chain elements, from the start and uninterrupted, that don't have parameters (no parenthesis 'l')
            IList<ASTLibModelChainElement> chainElementsNoArgs = new List<ASTLibModelChainElement>(chainElements.Count);

            for (int ii = 0; ii < chainElements.Count; ii++) {
                var element = chainElements[ii];
                if (!element.IsHasLeftParen)
                { // has no parenthesis, therefore part of class identifier
                    chainElementsNoArgs.Add(element);
                    chainElements.RemoveAt(ii--);
                }
                else
                { // else stop here
                    break;
                }
            }

            // write the class identifier including the no-arg chain elements
            var classIdentBuf = new StringWriter();
            var delimiter = "";
            if (root.classIdentifier() != null)
            {
                classIdentBuf.Write(ASTUtil.UnescapeClassIdent(root.classIdentifier()));
                delimiter = ".";
            }

            foreach (var noarg in chainElementsNoArgs)
            {
                classIdentBuf.Write(delimiter);
                classIdentBuf.Write(noarg.FuncName);
                delimiter = ".";
            }

            if (chainElements.IsEmpty())
            {
                // would this be an event property, but then that is handled greedily by parser
                throw ASTWalkException.From("Encountered unrecognized lib function call", tokenStream, ctx);
            }

            // class ident can be null if empty
            var classIdentifierString = classIdentBuf.ToString();
            var classIdentifier = classIdentifierString.Length > 0 ? classIdentifierString : null;

            return new ASTLibModel(classIdentifier, chainElements);
        }

        public static ASTLibModelChainElement FromRoot(EsperEPL2GrammarParser.LibFunctionWithClassContext root)
        {
            if (root.funcIdentTop() != null)
            {
                return new ASTLibModelChainElement(root.funcIdentTop().GetText(), root.libFunctionArgs(), root.l != null);
            }

            return new ASTLibModelChainElement(root.funcIdentInner().GetText(), root.libFunctionArgs(), root.l != null);
        }

        // Min/Max nodes can be either an aggregate or a per-row function depending on the number or arguments
        private static void HandleMinMax(
            string ident,
            EsperEPL2GrammarParser.LibFunctionArgsContext ctxArgs,
            IDictionary<ITree, ExprNode> astExprNodeMap)
        {
            // Determine min or max
            var childNodeText = ident;
            MinMaxTypeEnum minMaxTypeEnum;
            var filtered = childNodeText.StartsWith("f");
            if (string.Equals(childNodeText, "min", StringComparison.InvariantCultureIgnoreCase) ||
                string.Equals(childNodeText, "fmin", StringComparison.InvariantCultureIgnoreCase))
            {
                minMaxTypeEnum = MinMaxTypeEnum.MIN;
            }
            else if (string.Equals(childNodeText, "max", StringComparison.InvariantCultureIgnoreCase) ||
                     string.Equals(childNodeText, "fmax", StringComparison.InvariantCultureIgnoreCase))
            {
                minMaxTypeEnum = MinMaxTypeEnum.MAX;
            }
            else
            {
                throw ASTWalkException.From("Encountered unrecognized min or max node '" + ident + "'");
            }

            IList<ExprNode> args = new EmptyList<ExprNode>();
            if (ctxArgs != null && ctxArgs.libFunctionArgItem() != null)
            {
                args = ASTExprHelper.ExprCollectSubNodes(ctxArgs, 0, astExprNodeMap);
            }

            var numArgsPositional = ExprAggregateNodeUtil.CountPositionalArgs(args);

            var isDistinct = ctxArgs != null && ctxArgs.DISTINCT() != null;
            if (numArgsPositional > 1 && isDistinct && !filtered)
            {
                throw ASTWalkException.From(
                    "The distinct keyword is not valid in per-row min and max " +
                    "functions with multiple sub-expressions");
            }

            ExprNode minMaxNode;
            if (!isDistinct && numArgsPositional > 1 && !filtered)
            {
                // use the row function
                minMaxNode = new ExprMinMaxRowNode(minMaxTypeEnum);
            }
            else
            {
                // use the aggregation function
                minMaxNode = new ExprMinMaxAggrNode(isDistinct, minMaxTypeEnum, filtered, false);
            }

            minMaxNode.AddChildNodes(args);
            astExprNodeMap.Put(ctxArgs, minMaxNode);
        }

        public class ASTLibModel
        {
            public ASTLibModel(
                string optionalClassIdent,
                IList<ASTLibModelChainElement> chainElements)
            {
                OptionalClassIdent = optionalClassIdent;
                ChainElements = chainElements;
            }

            public string OptionalClassIdent { get; }

            public IList<ASTLibModelChainElement> ChainElements { get; }
        }

        public class ASTLibModelChainElement
        {
            public ASTLibModelChainElement(
                string funcName,
                EsperEPL2GrammarParser.LibFunctionArgsContext args,
                bool hasLeftParen)
            {
                FuncName = funcName;
                Args = args;
                IsHasLeftParen = hasLeftParen;
            }

            public string FuncName { get; }

            public EsperEPL2GrammarParser.LibFunctionArgsContext Args { get; }

            public bool IsHasLeftParen { get; }
        }
    }
} // end of namespace