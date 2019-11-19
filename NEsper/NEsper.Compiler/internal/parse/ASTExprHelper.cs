///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using Antlr4.Runtime;
using Antlr4.Runtime.Sharpen;
using Antlr4.Runtime.Tree;

using com.espertech.esper.common.client.configuration;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.ops;
using com.espertech.esper.common.@internal.epl.expression.time.abacus;
using com.espertech.esper.common.@internal.epl.expression.time.node;
using com.espertech.esper.common.@internal.epl.expression.variable;
using com.espertech.esper.common.@internal.epl.pattern.core;
using com.espertech.esper.common.@internal.epl.rowrecog.expr;
using com.espertech.esper.common.@internal.epl.variable.compiletime;
using com.espertech.esper.common.@internal.epl.variable.core;
using com.espertech.esper.common.@internal.type;
using com.espertech.esper.compat.collections;
using com.espertech.esper.grammar.@internal.generated;

namespace com.espertech.esper.compiler.@internal.parse
{
    public class ASTExprHelper
    {
        public static ExprNode ResolvePropertyOrVariableIdentifier(
            string identifier,
            VariableCompileTimeResolver variableCompileTimeResolver,
            StatementSpecRaw spec)
        {
            VariableMetaData metaData = variableCompileTimeResolver.Resolve(identifier);
            if (metaData != null)
            {
                ExprVariableNodeImpl exprNode = new ExprVariableNodeImpl(metaData, null);
                spec.ReferencedVariables.Add(metaData.VariableName);
                string message = VariableUtil.CheckVariableContextName(spec.OptionalContextName, metaData);
                if (message != null)
                {
                    throw ASTWalkException.From(message);
                }

                return exprNode;
            }

            return new ExprIdentNodeImpl(identifier);
        }

        public static ExprTimePeriod TimePeriodGetExprAllParams(
            EsperEPL2GrammarParser.TimePeriodContext ctx,
            IDictionary<ITree, ExprNode> astExprNodeMap,
            VariableCompileTimeResolver variableCompileTimeResolver,
            StatementSpecRaw spec,
            Configuration config,
            TimeAbacus timeAbacus)
        {
            ExprNode[] nodes = new ExprNode[9];
            for (int i = 0; i < ctx.ChildCount; i++)
            {
                IParseTree unitRoot = ctx.GetChild(i);

                ExprNode valueExpr;
                if (ASTUtil.IsTerminatedOfType(unitRoot.GetChild(0), EsperEPL2GrammarLexer.IDENT))
                {
                    string ident = unitRoot.GetChild(0).GetText();
                    valueExpr = ASTExprHelper.ResolvePropertyOrVariableIdentifier(ident, variableCompileTimeResolver, spec);
                }
                else
                {
                    AtomicReference<ExprNode> @ref = new AtomicReference<ExprNode>();
                    ExprAction action = (exprNode, astExprNodeMapX, node) => {
                        astExprNodeMapX.Remove(node);
                        @ref.Set(exprNode);
                    };
                    ASTExprHelper.RecursiveFindRemoveChildExprNode(unitRoot.GetChild(0), astExprNodeMap, action);
                    valueExpr = @ref.Get();
                }

                if (ASTUtil.GetRuleIndexIfProvided(unitRoot) == EsperEPL2GrammarParser.RULE_microsecondPart)
                {
                    nodes[8] = valueExpr;
                }

                if (ASTUtil.GetRuleIndexIfProvided(unitRoot) == EsperEPL2GrammarParser.RULE_millisecondPart)
                {
                    nodes[7] = valueExpr;
                }

                if (ASTUtil.GetRuleIndexIfProvided(unitRoot) == EsperEPL2GrammarParser.RULE_secondPart)
                {
                    nodes[6] = valueExpr;
                }

                if (ASTUtil.GetRuleIndexIfProvided(unitRoot) == EsperEPL2GrammarParser.RULE_minutePart)
                {
                    nodes[5] = valueExpr;
                }

                if (ASTUtil.GetRuleIndexIfProvided(unitRoot) == EsperEPL2GrammarParser.RULE_hourPart)
                {
                    nodes[4] = valueExpr;
                }

                if (ASTUtil.GetRuleIndexIfProvided(unitRoot) == EsperEPL2GrammarParser.RULE_dayPart)
                {
                    nodes[3] = valueExpr;
                }

                if (ASTUtil.GetRuleIndexIfProvided(unitRoot) == EsperEPL2GrammarParser.RULE_weekPart)
                {
                    nodes[2] = valueExpr;
                }

                if (ASTUtil.GetRuleIndexIfProvided(unitRoot) == EsperEPL2GrammarParser.RULE_monthPart)
                {
                    nodes[1] = valueExpr;
                }

                if (ASTUtil.GetRuleIndexIfProvided(unitRoot) == EsperEPL2GrammarParser.RULE_yearPart)
                {
                    nodes[0] = valueExpr;
                }
            }

            ExprTimePeriod timeNode = new ExprTimePeriodImpl(
                nodes[0] != null, nodes[1] != null,
                nodes[2] != null, nodes[3] != null,
                nodes[4] != null, nodes[5] != null,
                nodes[6] != null, nodes[7] != null,
                nodes[8] != null, timeAbacus);

            foreach (ExprNode node in nodes)
            {
                if (node != null)
                {
                    timeNode.AddChildNode(node);
                }
            }

            return timeNode;
        }

        public static ExprTimePeriod TimePeriodGetExprJustSeconds(
            EsperEPL2GrammarParser.ExpressionContext expression,
            IDictionary<ITree, ExprNode> astExprNodeMap,
            TimeAbacus timeAbacus)
        {
            ExprNode node = ExprCollectSubNodes(expression, 0, astExprNodeMap)[0];
            ExprTimePeriod timeNode = new ExprTimePeriodImpl(false, false, false, false, false, false, true, false, false, timeAbacus);
            timeNode.AddChildNode(node);
            return timeNode;
        }

        public static IList<OnTriggerSetAssignment> GetOnTriggerSetAssignments(
            EsperEPL2GrammarParser.OnSetAssignmentListContext ctx,
            IDictionary<ITree, ExprNode> astExprNodeMap)
        {
            if (ctx == null || ctx.onSetAssignment().IsEmpty())
            {
                return new EmptyList<OnTriggerSetAssignment>();
            }

            IList<EsperEPL2GrammarParser.OnSetAssignmentContext> ctxs = ctx.onSetAssignment();
            IList<OnTriggerSetAssignment> assignments = new List<OnTriggerSetAssignment>(ctx.onSetAssignment().Length);
            foreach (EsperEPL2GrammarParser.OnSetAssignmentContext assign in ctxs)
            {
                ExprNode childEvalNode;
                if (assign.eventProperty() != null)
                {
                    ExprNode prop = ASTExprHelper.ExprCollectSubNodes(assign.eventProperty(), 0, astExprNodeMap)[0];
                    ExprNode value = ASTExprHelper.ExprCollectSubNodes(assign.expression(), 0, astExprNodeMap)[0];
                    ExprEqualsNode equals = new ExprEqualsNodeImpl(false, false);
                    equals.AddChildNode(prop);
                    equals.AddChildNode(value);
                    childEvalNode = equals;
                }
                else
                {
                    childEvalNode = ASTExprHelper.ExprCollectSubNodes(assign, 0, astExprNodeMap)[0];
                }

                assignments.Add(new OnTriggerSetAssignment(childEvalNode));
            }

            return assignments;
        }

        public static void PatternCollectAddSubnodesAddParentNode(
            EvalForgeNode evalNode,
            ITree node,
            IDictionary<ITree, EvalForgeNode> astPatternNodeMap)
        {
            if (evalNode == null)
            {
                throw ASTWalkException.From("Invalid null expression node for '" + ASTUtil.PrintNode(node) + "'");
            }

            for (int i = 0; i < node.ChildCount; i++)
            {
                var childNode = node.GetChild(i);
                EvalForgeNode childEvalNode = PatternGetRemoveTopNode(childNode, astPatternNodeMap);
                if (childEvalNode != null)
                {
                    evalNode.AddChildNode(childEvalNode);
                }
            }

            astPatternNodeMap.Put(node, evalNode);
        }

        public static EvalForgeNode PatternGetRemoveTopNode(
            ITree node,
            IDictionary<ITree, EvalForgeNode> astPatternNodeMap)
        {
            EvalForgeNode pattern = astPatternNodeMap.Get(node);
            if (pattern != null)
            {
                astPatternNodeMap.Remove(node);
                return pattern;
            }

            for (int i = 0; i < node.ChildCount; i++)
            {
                pattern = PatternGetRemoveTopNode(node.GetChild(i), astPatternNodeMap);
                if (pattern != null)
                {
                    return pattern;
                }
            }

            return null;
        }

        public static void RegExCollectAddSubNodesAddParentNode(
            RowRecogExprNode exprNode,
            ITree node,
            IDictionary<ITree, RowRecogExprNode> astRegExNodeMap)
        {
            RegExCollectAddSubNodes(exprNode, node, astRegExNodeMap);
            astRegExNodeMap.Put(node, exprNode);
        }

        public static void RegExCollectAddSubNodes(
            RowRecogExprNode regexNode,
            ITree node,
            IDictionary<ITree, RowRecogExprNode> astRegExNodeMap)
        {
            if (regexNode == null)
            {
                throw ASTWalkException.From("Invalid null expression node for '" + ASTUtil.PrintNode(node) + "'");
            }

            RegExAction action = (exprNode, astExprNodeMapX, nodeX) => {
                astExprNodeMapX.Remove(nodeX);
                regexNode.AddChildNode(exprNode);
            };
            for (int i = 0; i < node.ChildCount; i++)
            {
                ITree childNode = node.GetChild(i);
                RegExApplyActionRecursive(childNode, astRegExNodeMap, action);
            }
        }

        public static void RegExApplyActionRecursive(
            ITree node,
            IDictionary<ITree, RowRecogExprNode> astRegExNodeMap,
            RegExAction action)
        {
            RowRecogExprNode expr = astRegExNodeMap.Get(node);
            if (expr != null)
            {
                action.Invoke(expr, astRegExNodeMap, node);
                return;
            }

            for (int i = 0; i < node.ChildCount; i++)
            {
                RegExApplyActionRecursive(node.GetChild(i), astRegExNodeMap, action);
            }
        }

        public static void ExprCollectAddSubNodesAddParentNode(
            ExprNode exprNode,
            ITree node,
            IDictionary<ITree, ExprNode> astExprNodeMap)
        {
            ExprCollectAddSubNodes(exprNode, node, astExprNodeMap);
            astExprNodeMap.Put(node, exprNode);
        }

        public static void ExprCollectAddSubNodes(
            ExprNode parentNode,
            ITree node,
            IDictionary<ITree, ExprNode> astExprNodeMap)
        {
            if (parentNode == null)
            {
                throw ASTWalkException.From("Invalid null expression node for '" + ASTUtil.PrintNode(node) + "'");
            }

            if (node == null)
            {
                return;
            }

            ExprAction action = (exprNode, astExprNodeMapX, nodeX) => {
                astExprNodeMapX.Remove(nodeX);
                parentNode.AddChildNode(exprNode);
            };
            for (int i = 0; i < node.ChildCount; i++)
            {
                ITree childNode = node.GetChild(i);
                RecursiveFindRemoveChildExprNode(childNode, astExprNodeMap, action);
            }
        }

        public static void ExprCollectAddSingle(
            ExprNode parentNode,
            ITree node,
            IDictionary<ITree, ExprNode> astExprNodeMap)
        {
            if (parentNode == null)
            {
                throw ASTWalkException.From("Invalid null expression node for '" + ASTUtil.PrintNode(node) + "'");
            }

            if (node == null)
            {
                return;
            }

            ExprAction action = (exprNode, astExprNodeMapX, nodeX) => {
                astExprNodeMapX.Remove(nodeX);
                parentNode.AddChildNode(exprNode);
            };
            RecursiveFindRemoveChildExprNode(node, astExprNodeMap, action);
        }

        public static void ExprCollectAddSubNodesExpressionCtx(
            ExprNode parentNode,
            IList<EsperEPL2GrammarParser.ExpressionContext> expressionContexts,
            IDictionary<ITree, ExprNode> astExprNodeMap)
        {
            ExprAction action = (exprNode, astExprNodeMapX, node) => {
                astExprNodeMapX.Remove(node);
                parentNode.AddChildNode(exprNode);
            };
            foreach (EsperEPL2GrammarParser.ExpressionContext ctx in expressionContexts)
            {
                RecursiveFindRemoveChildExprNode(ctx, astExprNodeMap, action);
            }
        }

        public static IList<ExprNode> ExprCollectSubNodes(
            ITree parentNode,
            int startIndex,
            IDictionary<ITree, ExprNode> astExprNodeMap)
        {
            ExprNode selfNode = astExprNodeMap.Delete(parentNode);
            if (selfNode != null)
            {
                return Collections.SingletonList(selfNode);
            }

            IList<ExprNode> exprNodes = new List<ExprNode>();
            ExprAction action = (exprNode, astExprNodeMapX, node) => {
                astExprNodeMapX.Remove(node);
                exprNodes.Add(exprNode);
            };

            for (int i = startIndex; i < parentNode.ChildCount; i++)
            {
                ITree currentNode = parentNode.GetChild(i);
                RecursiveFindRemoveChildExprNode(currentNode, astExprNodeMap, action);
            }

            return exprNodes;
        }

        private static void RecursiveFindRemoveChildExprNode(
            ITree node,
            IDictionary<ITree, ExprNode> astExprNodeMap,
            ExprAction action)
        {
            ExprNode expr = astExprNodeMap.Get(node);
            if (expr != null)
            {
                action.Invoke(expr, astExprNodeMap, node);
                return;
            }

            for (int i = 0; i < node.ChildCount; i++)
            {
                RecursiveFindRemoveChildExprNode(node.GetChild(i), astExprNodeMap, action);
            }
        }

        public static RowRecogExprNode RegExGetRemoveTopNode(
            ITree node,
            IDictionary<ITree, RowRecogExprNode> astRowRegexNodeMap)
        {
            RowRecogExprNode regex = astRowRegexNodeMap.Get(node);
            if (regex != null)
            {
                astRowRegexNodeMap.Remove(node);
                return regex;
            }

            for (int i = 0; i < node.ChildCount; i++)
            {
                regex = RegExGetRemoveTopNode(node.GetChild(i), astRowRegexNodeMap);
                if (regex != null)
                {
                    return regex;
                }
            }

            return null;
        }

        public static ExprNode MathGetExpr(
            IParseTree ctx,
            IDictionary<ITree, ExprNode> astExprNodeMap,
            Configuration configurationInformation)
        {
            int count = 1;
            ExprNode @base = ASTExprHelper.ExprCollectSubNodes(ctx.GetChild(0), 0, astExprNodeMap)[0];

            while (true)
            {
                int token = ASTUtil.GetAssertTerminatedTokenType(ctx.GetChild(count));
                MathArithTypeEnum mathArithTypeEnum = TokenToMathEnum(token);

                ExprNode right = ASTExprHelper.ExprCollectSubNodes(ctx.GetChild(count + 1), 0, astExprNodeMap)[0];

                ExprMathNode math = new ExprMathNode(
                    mathArithTypeEnum,
                    configurationInformation.Compiler.Expression.IsIntegerDivision,
                    configurationInformation.Compiler.Expression.IsDivisionByZeroReturnsNull);
                math.AddChildNode(@base);
                math.AddChildNode(right);
                @base = math;

                count += 2;
                if (count >= ctx.ChildCount)
                {
                    break;
                }
            }

            return @base;
        }

        private static MathArithTypeEnum TokenToMathEnum(int token)
        {
            switch (token)
            {
                case EsperEPL2GrammarLexer.DIV:
                    return MathArithTypeEnum.DIVIDE;

                case EsperEPL2GrammarLexer.STAR:
                    return MathArithTypeEnum.MULTIPLY;

                case EsperEPL2GrammarLexer.PLUS:
                    return MathArithTypeEnum.ADD;

                case EsperEPL2GrammarLexer.MINUS:
                    return MathArithTypeEnum.SUBTRACT;

                case EsperEPL2GrammarLexer.MOD:
                    return MathArithTypeEnum.MODULO;

                default:
                    throw ASTWalkException.From("Encountered unrecognized math token type " + token);
            }
        }

        public static void AddOptionalNumber(
            ExprNode exprNode,
            EsperEPL2GrammarParser.NumberContext number)
        {
            if (number == null)
            {
                return;
            }

            ExprConstantNode constantNode = new ExprConstantNodeImpl(ASTConstantHelper.Parse(number));
            exprNode.AddChildNode(constantNode);
        }

        public static void AddOptionalSimpleProperty(
            ExprNode exprNode,
            IToken token,
            VariableCompileTimeResolver variableCompileTimeResolver,
            StatementSpecRaw spec)
        {
            if (token == null)
            {
                return;
            }

            ExprNode node = ASTExprHelper.ResolvePropertyOrVariableIdentifier(token.Text, variableCompileTimeResolver, spec);
            exprNode.AddChildNode(node);
        }

        public static ExprNode[] ExprCollectSubNodesPerNode(
            IList<EsperEPL2GrammarParser.ExpressionContext> expression,
            IDictionary<ITree, ExprNode> astExprNodeMap)
        {
            ExprNode[] nodes = new ExprNode[expression.Count];
            for (int i = 0; i < expression.Count; i++)
            {
                nodes[i] = ExprCollectSubNodes(expression[i], 0, astExprNodeMap)[0];
            }

            return nodes;
        }

        public delegate void ExprAction(
            ExprNode exprNode,
            IDictionary<ITree, ExprNode> astExprNodeMap,
            ITree node);

        public delegate void RegExAction(
            RowRecogExprNode exprNode,
            IDictionary<ITree, RowRecogExprNode> astRegExNodeMap,
            ITree node);
    }
} // end of namespace