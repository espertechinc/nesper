///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.IO;

using Antlr4.Runtime;
using Antlr4.Runtime.Tree;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.container;
using com.espertech.esper.core.context.util;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.declexpr;
using com.espertech.esper.epl.enummethod.dot;
using com.espertech.esper.epl.expression.baseagg;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression.dot;
using com.espertech.esper.epl.expression.funcs;
using com.espertech.esper.epl.expression.methodagg;
using com.espertech.esper.epl.generated;
using com.espertech.esper.epl.spec;
using com.espertech.esper.epl.table.mgmt;
using com.espertech.esper.epl.variable;
using com.espertech.esper.plugin;
using com.espertech.esper.type;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.parse
{
    public class ASTLibFunctionHelper
    {
        public static IList<ExprChainedSpec> GetLibFuncChain(
            ICollection<EsperEPL2GrammarParser.LibFunctionNoClassContext> ctxs,
            IDictionary<ITree, ExprNode> astExprNodeMap)
        {
            var chained = new List<ExprChainedSpec>(ctxs.Count);
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
            var methodName = ASTConstantHelper.RemoveTicks(ctx.funcIdentChained().GetText());

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
                return Collections.GetEmptyList<ExprNode>();
            }
            IList<EsperEPL2GrammarParser.LibFunctionArgItemContext> args = ctx.libFunctionArgItem();
            if (args == null || args.IsEmpty())
            {
                return Collections.GetEmptyList<ExprNode>();
            }
            var parameters = new List<ExprNode>(args.Count);
            foreach (var arg in args)
            {
                if (arg.expressionLambdaDecl() != null)
                {
                    var lambdaparams = GetLambdaGoesParams(arg.expressionLambdaDecl());
                    var goes = new ExprLambdaGoesNode(lambdaparams);
                    var lambdaExpr =
                        ASTExprHelper.ExprCollectSubNodes(arg.expressionWithNamed(), 0, astExprNodeMap)[0];
                    goes.AddChildNode(lambdaExpr);
                    parameters.Add(goes);
                }
                else
                {
                    var parameter =
                        ASTExprHelper.ExprCollectSubNodes(arg.expressionWithNamed(), 0, astExprNodeMap)[0];
                    parameters.Add(parameter);
                }
            }
            return parameters;
        }

        internal static IList<string> GetLambdaGoesParams(EsperEPL2GrammarParser.ExpressionLambdaDeclContext ctx)
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
            IContainer container,
            CommonTokenStream tokenStream,
            EsperEPL2GrammarParser.LibFunctionContext ctx,
            ConfigurationInformation configurationInformation,
            EngineImportService engineImportService,
            IDictionary<ITree, ExprNode> astExprNodeMap,
            LazyAllocatedMap<ConfigurationPlugInAggregationMultiFunction, PlugInAggregationMultiFunctionFactory> plugInAggregations,
            string engineURI,
            ExpressionDeclDesc expressionDeclarations,
            ExprDeclaredService exprDeclaredService,
            IList<ExpressionScriptProvided> scriptExpressions,
            ContextDescriptor contextDescriptor,
            TableService tableService,
            StatementSpecRaw statementSpec,
            VariableService variableService)
        {
            var model = GetModel(ctx, tokenStream);
            var duckType = configurationInformation.EngineDefaults.Expression.IsDuckTyping;
            var udfCache = configurationInformation.EngineDefaults.Expression.IsUdfCache;

            // handle "some.Xyz(...)" or "some.other.Xyz(...)"
            if (model.ChainElements.Count == 1 &&
                model.OptionalClassIdent != null &&
                ASTTableExprHelper.CheckTableNameGetExprForProperty(tableService, model.OptionalClassIdent) == null)
            {

                var chainSpec = GetLibFunctionChainSpec(model.ChainElements[0], astExprNodeMap);

                var declaredNodeX = ExprDeclaredHelper.GetExistsDeclaredExpr(
                    container,
                    model.OptionalClassIdent, 
                    Collections.GetEmptyList<ExprNode>(), 
                    expressionDeclarations.Expressions,
                    exprDeclaredService, 
                    contextDescriptor);
                if (declaredNodeX != null)
                {
                    var exprNode = new ExprDotNodeImpl(Collections.SingletonList(chainSpec), duckType, udfCache);
                    exprNode.AddChildNode(declaredNodeX);
                    ASTExprHelper.ExprCollectAddSubNodesAddParentNode(exprNode, ctx, astExprNodeMap);
                    return;
                }

                var chainX = new List<ExprChainedSpec>(2);
                chainX.Add(new ExprChainedSpec(model.OptionalClassIdent, Collections.GetEmptyList<ExprNode>(), true));
                chainX.Add(chainSpec);
                var dotNodeX = new ExprDotNodeImpl(
                    chainX, configurationInformation.EngineDefaults.Expression.IsDuckTyping,
                    configurationInformation.EngineDefaults.Expression.IsUdfCache);
                if (dotNodeX.IsVariableOpGetName(variableService) != null)
                {
                    statementSpec.HasVariables = true;
                }
                ASTExprHelper.ExprCollectAddSubNodesAddParentNode(dotNodeX, ctx, astExprNodeMap);
                return;
            }

            // try additional built-in single-row function
            var singleRowExtNode =
                engineImportService.ResolveSingleRowExtendedBuiltin(model.ChainElements[0].FuncName);
            if (singleRowExtNode != null)
            {
                if (model.ChainElements.Count == 1)
                {
                    ASTExprHelper.ExprCollectAddSubNodesAddParentNode(singleRowExtNode, ctx, astExprNodeMap);
                    return;
                }
                var spec = new List<ExprChainedSpec>();
                var firstArgs = model.ChainElements[0].Args;
                var childExpressions = GetExprNodesLibFunc(firstArgs, astExprNodeMap);
                singleRowExtNode.AddChildNodes(childExpressions);
                AddChainRemainderFromOffset(model.ChainElements, 1, spec, astExprNodeMap);
                var dotNodeX = new ExprDotNodeImpl(
                    spec, configurationInformation.EngineDefaults.Expression.IsDuckTyping,
                    configurationInformation.EngineDefaults.Expression.IsUdfCache);
                dotNodeX.AddChildNode(singleRowExtNode);
                ASTExprHelper.ExprCollectAddSubNodesAddParentNode(dotNodeX, ctx, astExprNodeMap);
                return;
            }

            // try plug-in single-row function
            try
            {
                var firstFunctionX = model.ChainElements[0].FuncName;
                var firstFunctionIsProperty = !model.ChainElements[0].HasLeftParen;
                var classMethodPair =
                    engineImportService.ResolveSingleRow(firstFunctionX);
                var spec = new List<ExprChainedSpec>();
                var firstArgs = model.ChainElements[0].Args;
                var childExpressions = GetExprNodesLibFunc(firstArgs, astExprNodeMap);
                spec.Add(
                    new ExprChainedSpec(classMethodPair.Second.MethodName, childExpressions, firstFunctionIsProperty));
                AddChainRemainderFromOffset(model.ChainElements, 1, spec, astExprNodeMap);
                var plugin = new ExprPlugInSingleRowNode(
                    firstFunctionX, classMethodPair.First, spec, classMethodPair.Second);
                ASTExprHelper.ExprCollectAddSubNodesAddParentNode(plugin, ctx, astExprNodeMap);
                return;
            }
            catch (EngineImportUndefinedException)
            {
                // Not an single-row function
            }
            catch (EngineImportException e)
            {
                throw new IllegalStateException("Error resolving single-row function: " + e.Message, e);
            }

            // special case for min,max
            var firstFunction = model.ChainElements[0].FuncName;
            if ((firstFunction.ToLowerInvariant().Equals("max")) || (firstFunction.ToLowerInvariant().Equals("min")) ||
                (firstFunction.ToLowerInvariant().Equals("fmax")) || (firstFunction.ToLowerInvariant().Equals("fmin")))
            {
                var firstArgs = model.ChainElements[0].Args;
                HandleMinMax(firstFunction, firstArgs, astExprNodeMap);
                return;
            }

            // obtain chain with actual expressions
            IList<ExprChainedSpec> chain = new List<ExprChainedSpec>();
            AddChainRemainderFromOffset(model.ChainElements, 0, chain, astExprNodeMap);

            // add chain element for class INFO, if any
            var distinct = model.ChainElements[0].Args != null && model.ChainElements[0].Args.DISTINCT() != null;
            if (model.OptionalClassIdent != null)
            {
                chain.Insert(
                    0, new ExprChainedSpec(model.OptionalClassIdent, Collections.GetEmptyList<ExprNode>(), true));
                distinct = false;
            }
            firstFunction = chain[0].Name;

            // try plug-in aggregation function
            var aggregationNode = ASTAggregationHelper.TryResolveAsAggregation(
                engineImportService, distinct, firstFunction, plugInAggregations, engineURI);
            if (aggregationNode != null)
            {
                var firstSpec = chain.DeleteAt(0);
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
                container,
                firstFunction, 
                chain[0].Parameters, 
                expressionDeclarations.Expressions, 
                exprDeclaredService,
                contextDescriptor);
            if (declaredNode != null)
            {
                chain.RemoveAt(0);
                ExprNode exprNode;
                if (chain.IsEmpty())
                {
                    exprNode = declaredNode;
                }
                else
                {
                    exprNode = new ExprDotNodeImpl(chain, duckType, udfCache);
                    exprNode.AddChildNode(declaredNode);
                }
                ASTExprHelper.ExprCollectAddSubNodesAddParentNode(exprNode, ctx, astExprNodeMap);
                return;
            }

            // try script
            var scriptNode =
                ExprDeclaredHelper.GetExistsScript(
                    configurationInformation.EngineDefaults.Scripts.DefaultDialect, chain[0].Name, chain[0].Parameters,
                    scriptExpressions, exprDeclaredService);
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
            var tableInfo = ASTTableExprHelper.CheckTableNameGetLibFunc(
                    tableService, engineImportService, plugInAggregations, engineURI, firstFunction, chain);
            if (tableInfo != null)
            {
                ASTTableExprHelper.AddTableExpressionReference(statementSpec, tableInfo.First);
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

            // Could be a mapped property with an expression-parameter "Mapped(expr)" or array property with an expression-parameter "Array(expr)".
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
            var methodName = ASTConstantHelper.RemoveTicks(element.FuncName);
            var parameters = GetExprNodesLibFunc(element.Args, astExprNodeMap);
            return new ExprChainedSpec(methodName, parameters, !element.HasLeftParen);
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
                var classIdent = root.classIdentifier() == null
                    ? null
                    : ASTUtil.UnescapeClassIdent(root.classIdentifier());
                var ele = FromRoot(root);
                return new ASTLibModel(classIdent, Collections.SingletonList(ele));
            }

            // add root and chain to just a list of elements
            var chainElements = new List<ASTLibModelChainElement>(ctxElements.Count + 1);
            var rootElement = FromRoot(root);
            chainElements.Add(rootElement);
            foreach (var chainedCtx in ctxElements)
            {
                var chainedElement = new ASTLibModelChainElement(
                    chainedCtx.funcIdentChained().GetText(), chainedCtx.libFunctionArgs(), chainedCtx.l != null);
                chainElements.Add(chainedElement);
            }

            // determine/remove the list of chain elements, from the start and uninterrupted, that don't have parameters (no parenthesis 'l')
            var chainElementsNoArgs = new List<ASTLibModelChainElement>(chainElements.Count);
            for (var ii = 0; ii < chainElements.Count; ii++) 
            {
                var element = chainElements[ii];
                if (!element.HasLeftParen)
                {
                    // has no parenthesis, therefore part of class identifier
                    chainElementsNoArgs.Add(element);
                    chainElements.RemoveAt(ii);
                    ii--;
                }
                else
                {
                    // else stop here
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
                return new ASTLibModelChainElement(
                    root.funcIdentTop().GetText(), root.libFunctionArgs(), root.l != null);
            }
            else
            {
                return new ASTLibModelChainElement(
                    root.funcIdentInner().GetText(), root.libFunctionArgs(), root.l != null);
            }
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
            if (childNodeText.ToLowerInvariant().Equals("min") || childNodeText.ToLowerInvariant().Equals("fmin"))
            {
                minMaxTypeEnum = MinMaxTypeEnum.MIN;
            }
            else if (childNodeText.ToLowerInvariant().Equals("max") || childNodeText.ToLowerInvariant().Equals("fmax"))
            {
                minMaxTypeEnum = MinMaxTypeEnum.MAX;
            }
            else
            {
                throw ASTWalkException.From("Uncountered unrecognized min or max node '" + ident + "'");
            }

            var args = Collections.GetEmptyList<ExprNode>();
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
            public ASTLibModel(string optionalClassIdent, IList<ASTLibModelChainElement> chainElements)
            {
                OptionalClassIdent = optionalClassIdent;
                ChainElements = chainElements;
            }

            public string OptionalClassIdent { get; private set; }

            public IList<ASTLibModelChainElement> ChainElements { get; private set; }
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
                HasLeftParen = hasLeftParen;
            }

            public string FuncName { get; private set; }

            public EsperEPL2GrammarParser.LibFunctionArgsContext Args { get; private set; }

            public bool HasLeftParen { get; private set; }
        }
    }
} // end of namespace
