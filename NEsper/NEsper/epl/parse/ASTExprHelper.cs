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

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.threading;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression.ops;
using com.espertech.esper.epl.expression.time;
using com.espertech.esper.epl.generated;
using com.espertech.esper.epl.spec;
using com.espertech.esper.epl.variable;
using com.espertech.esper.pattern;
using com.espertech.esper.rowregex;
using com.espertech.esper.type;

namespace com.espertech.esper.epl.parse
{
    public static class ASTExprHelper
    {
        public static ExprNode ResolvePropertyOrVariableIdentifier(String identifier, VariableService variableService, StatementSpecRaw spec)
        {
            var metaData = variableService.GetVariableMetaData(identifier);
            if (metaData != null)
            {
                var exprNode = new ExprVariableNodeImpl(metaData, null);
                spec.HasVariables = true;
                AddVariableReference(spec, metaData.VariableName);
                var message = VariableServiceUtil.CheckVariableContextName(spec.OptionalContextName, metaData);
                if (message != null)
                {
                    throw ASTWalkException.From(message);
                }
                return exprNode;
            }
            else
            {
                return new ExprIdentNodeImpl(identifier);
            }
        }

        public static void AddVariableReference(StatementSpecRaw statementSpec, String variableName)
        {
            if (statementSpec.ReferencedVariables == null)
            {
                statementSpec.ReferencedVariables = new HashSet<String>();
            }
            statementSpec.ReferencedVariables.Add(variableName);
        }

        public static ExprTimePeriod TimePeriodGetExprAllParams(
            EsperEPL2GrammarParser.TimePeriodContext ctx,
            IDictionary<ITree, ExprNode> astExprNodeMap,
            VariableService variableService,
            StatementSpecRaw spec,
            ConfigurationInformation config,
            TimeAbacus timeAbacus,
            ILockManager lockManager)
        {
            var nodes = new ExprNode[9];
            for (var i = 0; i < ctx.ChildCount; i++)
            {
                var unitRoot = ctx.GetChild(i);

                ExprNode valueExpr;
                if (ASTUtil.IsTerminatedOfType(unitRoot.GetChild(0), EsperEPL2GrammarLexer.IDENT))
                {
                    var ident = unitRoot.GetChild(0).GetText();
                    valueExpr = ASTExprHelper.ResolvePropertyOrVariableIdentifier(ident, variableService, spec);
                }
                else
                {
                    var @ref = new Atomic<ExprNode>();
                    ExprAction action = (exprNode, astExprNodeMapX, nodeX) =>
                    {
                        astExprNodeMapX.Remove(nodeX);
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
                config.EngineDefaults.Expression.TimeZone,
                nodes[0] != null, 
                nodes[1] != null, 
                nodes[2] != null, 
                nodes[3] != null, 
                nodes[4] != null, 
                nodes[5] != null, 
                nodes[6] != null, 
                nodes[7] != null, 
                nodes[8] != null, 
                timeAbacus,
                lockManager);

            foreach (ExprNode node in nodes) {
                if (node != null) timeNode.AddChildNode(node);
            }

            return timeNode;
        }

        public static ExprTimePeriod TimePeriodGetExprJustSeconds(
            EsperEPL2GrammarParser.ExpressionContext expression,
            IDictionary<ITree, ExprNode> astExprNodeMap,
            ConfigurationInformation config, 
            TimeAbacus timeAbacus,
            ILockManager lockManager) 
        {
            var node = ExprCollectSubNodes(expression, 0, astExprNodeMap)[0];
            var timeNode = new ExprTimePeriodImpl(
                config.EngineDefaults.Expression.TimeZone,
                false, false, false, 
                false, false, false, 
                true, false, false, 
                timeAbacus,
                lockManager);
            timeNode.AddChildNode(node);
            return timeNode;
        }

        /// <summary>
        /// Returns the list of set-variable assignments under the given node.
        /// </summary>
        /// <param name="ctx">The CTX.</param>
        /// <param name="astExprNodeMap">map of AST to expression</param>
        /// <returns>
        /// list of assignments
        /// </returns>
        internal static IList<OnTriggerSetAssignment> GetOnTriggerSetAssignments(
            EsperEPL2GrammarParser.OnSetAssignmentListContext ctx,
            IDictionary<ITree, ExprNode> astExprNodeMap)
        {
            if (ctx == null || ctx.onSetAssignment().IsEmpty())
            {
                return Collections.GetEmptyList<OnTriggerSetAssignment>();
            }
            IList<EsperEPL2GrammarParser.OnSetAssignmentContext> ctxs = ctx.onSetAssignment();
            IList<OnTriggerSetAssignment> assignments = new List<OnTriggerSetAssignment>(ctx.onSetAssignment().Length);
            foreach (var assign in ctxs)
            {
                ExprNode childEvalNode;
                if (assign.eventProperty() != null)
                {
                    var prop = ASTExprHelper.ExprCollectSubNodes(assign.eventProperty(), 0, astExprNodeMap)[0];
                    var value = ASTExprHelper.ExprCollectSubNodes(assign.expression(), 0, astExprNodeMap)[0];
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

        public static void PatternCollectAddSubnodesAddParentNode(EvalFactoryNode evalNode, ITree node, IDictionary<ITree, EvalFactoryNode> astPatternNodeMap)
        {
            if (evalNode == null)
            {
                throw ASTWalkException.From("Invalid null expression node for '" + ASTUtil.PrintNode(node) + "'");
            }
            for (var i = 0; i < node.ChildCount; i++)
            {
                var childNode = node.GetChild(i);
                var childEvalNode = PatternGetRemoveTopNode(childNode, astPatternNodeMap);
                if (childEvalNode != null)
                {
                    evalNode.AddChildNode(childEvalNode);
                }
            }
            astPatternNodeMap.Put(node, evalNode);
        }

        public static EvalFactoryNode PatternGetRemoveTopNode(ITree node, IDictionary<ITree, EvalFactoryNode> astPatternNodeMap)
        {
            var pattern = astPatternNodeMap.Get(node);
            if (pattern != null)
            {
                astPatternNodeMap.Remove(node);
                return pattern;
            }
            for (var i = 0; i < node.ChildCount; i++)
            {
                pattern = PatternGetRemoveTopNode(node.GetChild(i), astPatternNodeMap);
                if (pattern != null)
                {
                    return pattern;
                }
            }
            return null;
        }

        public static void RegExCollectAddSubNodesAddParentNode(RowRegexExprNode exprNode, ITree node, IDictionary<ITree, RowRegexExprNode> astRegExNodeMap)
        {
            RegExCollectAddSubNodes(exprNode, node, astRegExNodeMap);
            astRegExNodeMap.Put(node, exprNode);
        }

        public static void RegExCollectAddSubNodes(RowRegexExprNode regexNode, ITree node, IDictionary<ITree, RowRegexExprNode> astRegExNodeMap)
        {
            if (regexNode == null)
            {
                throw ASTWalkException.From("Invalid null expression node for '" + ASTUtil.PrintNode(node) + "'");
            }
            RegExAction action = (exprNode, astRegExNodeMapX, nodeX) =>
            {
                astRegExNodeMapX.Remove(nodeX);
                regexNode.AddChildNode(exprNode);
            };
            for (var i = 0; i < node.ChildCount; i++)
            {
                var childNode = node.GetChild(i);
                RegExApplyActionRecursive(childNode, astRegExNodeMap, action);
            }
        }

        public static void RegExApplyActionRecursive(ITree node, IDictionary<ITree, RowRegexExprNode> astRegExNodeMap, RegExAction action)
        {
            var expr = astRegExNodeMap.Get(node);
            if (expr != null)
            {
                action.Invoke(expr, astRegExNodeMap, node);
                return;
            }
            for (var i = 0; i < node.ChildCount; i++)
            {
                RegExApplyActionRecursive(node.GetChild(i), astRegExNodeMap, action);
            }
        }

        public static void ExprCollectAddSubNodesAddParentNode(ExprNode exprNode, ITree node, IDictionary<ITree, ExprNode> astExprNodeMap)
        {
            ExprCollectAddSubNodes(exprNode, node, astExprNodeMap);
            astExprNodeMap.Put(node, exprNode);
        }

        public static void ExprCollectAddSubNodes(ExprNode parentNode, ITree node, IDictionary<ITree, ExprNode> astExprNodeMap)
        {
            if (parentNode == null)
            {
                throw ASTWalkException.From("Invalid null expression node for '" + ASTUtil.PrintNode(node) + "'");
            }
            if (node == null)
            {
                return;
            }
            ExprAction action = (exprNode, astExprNodeMapX, nodeX) =>
            {
                astExprNodeMapX.Remove(nodeX);
                parentNode.AddChildNode(exprNode);
            };
            for (var i = 0; i < node.ChildCount; i++)
            {
                var childNode = node.GetChild(i);
                RecursiveFindRemoveChildExprNode(childNode, astExprNodeMap, action);
            }
        }

        public static void ExprCollectAddSingle(ExprNode parentNode, ITree node, IDictionary<ITree, ExprNode> astExprNodeMap)
        {
            if (parentNode == null)
            {
                throw ASTWalkException.From("Invalid null expression node for '" + ASTUtil.PrintNode(node) + "'");
            }
            if (node == null)
            {
                return;
            }
            ExprAction action = (exprNodeX, astExprNodeMapX, nodeX) =>
            {
                astExprNodeMapX.Remove(nodeX);
                parentNode.AddChildNode(exprNodeX);
            };
            RecursiveFindRemoveChildExprNode(node, astExprNodeMap, action);
        }

        public static void ExprCollectAddSubNodesExpressionCtx(ExprNode parentNode, IList<EsperEPL2GrammarParser.ExpressionContext> expressionContexts, IDictionary<ITree, ExprNode> astExprNodeMap)
        {
            ExprAction action = (exprNode, astExprNodeMapX, node) =>
            {
                astExprNodeMapX.Remove(node);
                parentNode.AddChildNode(exprNode);
            };
            foreach (var ctx in expressionContexts)
            {
                RecursiveFindRemoveChildExprNode(ctx, astExprNodeMap, action);
            }
        }

        public static IList<ExprNode> ExprCollectSubNodes(ITree parentNode, int startIndex, IDictionary<ITree, ExprNode> astExprNodeMap)
        {
            var selfNode = astExprNodeMap.Delete(parentNode);
            if (selfNode != null)
            {
                return Collections.SingletonList(selfNode);
            }
            IList<ExprNode> exprNodes = new List<ExprNode>();
            ExprAction action = (exprNode, astExprNodeMapX, node) =>
            {
                astExprNodeMapX.Remove(node);
                exprNodes.Add(exprNode);
            };
            for (var i = startIndex; i < parentNode.ChildCount; i++)
            {
                var currentNode = parentNode.GetChild(i);
                RecursiveFindRemoveChildExprNode(currentNode, astExprNodeMap, action);
            }
            return exprNodes;
        }

        private static void RecursiveFindRemoveChildExprNode(ITree node, IDictionary<ITree, ExprNode> astExprNodeMap, ExprAction action)
        {
            var expr = astExprNodeMap.Get(node);
            if (expr != null)
            {
                action.Invoke(expr, astExprNodeMap, node);
                return;
            }
            for (var i = 0; i < node.ChildCount; i++)
            {
                RecursiveFindRemoveChildExprNode(node.GetChild(i), astExprNodeMap, action);
            }
        }

        public static RowRegexExprNode RegExGetRemoveTopNode(ITree node, IDictionary<ITree, RowRegexExprNode> astRowRegexNodeMap)
        {
            var regex = astRowRegexNodeMap.Get(node);
            if (regex != null)
            {
                astRowRegexNodeMap.Remove(node);
                return regex;
            }
            for (var i = 0; i < node.ChildCount; i++)
            {
                regex = RegExGetRemoveTopNode(node.GetChild(i), astRowRegexNodeMap);
                if (regex != null)
                {
                    return regex;
                }
            }
            return null;
        }

        public static ExprNode MathGetExpr(IParseTree ctx, IDictionary<ITree, ExprNode> astExprNodeMap, ConfigurationInformation configurationInformation)
        {

            var count = 1;
            var @base = ASTExprHelper.ExprCollectSubNodes(ctx.GetChild(0), 0, astExprNodeMap)[0];

            while (true)
            {
                int token = ASTUtil.GetAssertTerminatedTokenType(ctx.GetChild(count));
                var mathArithTypeEnum = TokenToMathEnum(token);

                var right = ASTExprHelper.ExprCollectSubNodes(ctx.GetChild(count + 1), 0, astExprNodeMap)[0];

                var math = new ExprMathNode(mathArithTypeEnum,
                        configurationInformation.EngineDefaults.Expression.IsIntegerDivision,
                        configurationInformation.EngineDefaults.Expression.IsDivisionByZeroReturnsNull);
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

        public static void AddOptionalNumber(ExprNode exprNode, EsperEPL2GrammarParser.NumberContext number)
        {
            if (number == null)
            {
                return;
            }
            ExprConstantNode constantNode = new ExprConstantNodeImpl(ASTConstantHelper.Parse(number));
            exprNode.AddChildNode(constantNode);
        }

        public static void AddOptionalSimpleProperty(ExprNode exprNode, IToken token, VariableService variableService, StatementSpecRaw spec)
        {
            if (token == null)
            {
                return;
            }
            var node = ASTExprHelper.ResolvePropertyOrVariableIdentifier(token.Text, variableService, spec);
            exprNode.AddChildNode(node);
        }

        public static ExprNode[] ExprCollectSubNodesPerNode(IList<EsperEPL2GrammarParser.ExpressionContext> expression, IDictionary<ITree, ExprNode> astExprNodeMap)
        {
            var nodes = new ExprNode[expression.Count];
            for (var i = 0; i < expression.Count; i++)
            {
                nodes[i] = ExprCollectSubNodes(expression[i], 0, astExprNodeMap)[0];
            }
            return nodes;
        }

        public delegate void ExprAction(ExprNode exprNode, IDictionary<ITree, ExprNode> astExprNodeMap, ITree node);
        public delegate void RegExAction(RowRegexExprNode exprNode, IDictionary<ITree, RowRegexExprNode> astRegExNodeMap, ITree node);
    }
}
