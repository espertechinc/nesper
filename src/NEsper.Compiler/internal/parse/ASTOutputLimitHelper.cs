///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using Antlr4.Runtime;
using Antlr4.Runtime.Tree;

using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.time.node;
using com.espertech.esper.compat;
using com.espertech.esper.grammar.@internal.generated;

namespace com.espertech.esper.compiler.@internal.parse
{
    /// <summary>
    ///     Builds an output limit spec from an output limit AST node.
    /// </summary>
    public class ASTOutputLimitHelper
    {
        public static OutputLimitSpec BuildOutputLimitSpec(
            CommonTokenStream tokenStream,
            EsperEPL2GrammarParser.OutputLimitContext ctx,
            IDictionary<ITree, ExprNode> astExprNodeMap)
        {
            var displayLimit = OutputLimitLimitType.DEFAULT;
            if (ctx.k != null)
            {
                switch (ctx.k.Type)
                {
                    case EsperEPL2GrammarParser.FIRST:
                        displayLimit = OutputLimitLimitType.FIRST;
                        break;

                    case EsperEPL2GrammarParser.LAST:
                        displayLimit = OutputLimitLimitType.LAST;
                        break;

                    case EsperEPL2GrammarParser.SNAPSHOT:
                        displayLimit = OutputLimitLimitType.SNAPSHOT;
                        break;

                    case EsperEPL2GrammarParser.ALL:
                        displayLimit = OutputLimitLimitType.ALL;
                        break;

                    default:
                        throw ASTWalkException.From("Encountered unrecognized token " + ctx.k.Text, tokenStream, ctx);
                }
            }

            // next is a variable, or time period, or number
            string variableName = null;
            double? rate = null;
            ExprNode whenExpression = null;
            IList<ExprNode> crontabScheduleSpec = null;
            IList<OnTriggerSetAssignment> thenExpressions = null;
            ExprTimePeriod timePeriodExpr = null;
            OutputLimitRateType rateType;
            ExprNode andAfterTerminateExpr = null;
            IList<OnTriggerSetAssignment> andAfterTerminateSetExpressions = null;

            if (ctx.t != null)
            {
                rateType = OutputLimitRateType.TERM;
                if (ctx.expression() != null)
                {
                    andAfterTerminateExpr = ASTExprHelper.ExprCollectSubNodes(ctx.expression(), 0, astExprNodeMap)[0];
                }

                if (ctx.onSetExpr() != null)
                {
                    andAfterTerminateSetExpressions = ASTExprHelper.GetOnTriggerSetAssignments(ctx.onSetExpr().onSetAssignmentList(), astExprNodeMap);
                }
            }
            else if (ctx.wh != null)
            {
                rateType = OutputLimitRateType.WHEN_EXPRESSION;
                whenExpression = ASTExprHelper.ExprCollectSubNodes(ctx.expression(), 0, astExprNodeMap)[0];
                if (ctx.onSetExpr() != null)
                {
                    thenExpressions = ASTExprHelper.GetOnTriggerSetAssignments(ctx.onSetExpr().onSetAssignmentList(), astExprNodeMap);
                }
            }
            else if (ctx.at != null)
            {
                rateType = OutputLimitRateType.CRONTAB;
                crontabScheduleSpec = ASTExprHelper.ExprCollectSubNodes(ctx.crontabLimitParameterSet(), 0, astExprNodeMap);
            }
            else
            {
                if (ctx.ev != null)
                {
                    rateType = ctx.e != null ? OutputLimitRateType.EVENTS : OutputLimitRateType.TIME_PERIOD;
                    if (ctx.i != null)
                    {
                        variableName = ctx.i.Text;
                    }
                    else if (ctx.timePeriod() != null)
                    {
                        timePeriodExpr = (ExprTimePeriod) ASTExprHelper.ExprCollectSubNodes(ctx.timePeriod(), 0, astExprNodeMap)[0];
                    }
                    else
                    {
                        ASTExprHelper.ExprCollectSubNodes(ctx.number(), 0, astExprNodeMap); // remove
                        rate = ctx.number().GetText().AsDouble();
                    }
                }
                else
                {
                    rateType = OutputLimitRateType.AFTER;
                }
            }

            // get the AFTER time period
            ExprTimePeriod afterTimePeriodExpr = null;
            int? afterNumberOfEvents = null;
            if (ctx.outputLimitAfter() != null)
            {
                if (ctx.outputLimitAfter().timePeriod() != null)
                {
                    ExprNode expression = ASTExprHelper.ExprCollectSubNodes(ctx.outputLimitAfter(), 0, astExprNodeMap)[0];
                    afterTimePeriodExpr = (ExprTimePeriod) expression;
                }
                else
                {
                    var constant = ASTConstantHelper.Parse(ctx.outputLimitAfter().number());
                    afterNumberOfEvents = constant.AsInt32();
                }
            }

            var andAfterTerminate = false;
            if (ctx.outputLimitAndTerm() != null)
            {
                andAfterTerminate = true;
                if (ctx.outputLimitAndTerm().expression() != null)
                {
                    andAfterTerminateExpr = ASTExprHelper.ExprCollectSubNodes(ctx.outputLimitAndTerm().expression(), 0, astExprNodeMap)[0];
                }

                if (ctx.outputLimitAndTerm().onSetExpr() != null)
                {
                    andAfterTerminateSetExpressions = ASTExprHelper.GetOnTriggerSetAssignments(
                        ctx.outputLimitAndTerm().onSetExpr().onSetAssignmentList(), astExprNodeMap);
                }
            }

            return new OutputLimitSpec(
                rate, variableName, rateType, displayLimit, whenExpression, thenExpressions, crontabScheduleSpec, timePeriodExpr, afterTimePeriodExpr,
                afterNumberOfEvents, andAfterTerminate, andAfterTerminateExpr, andAfterTerminateSetExpressions);
        }

        public static RowLimitSpec BuildRowLimitSpec(EsperEPL2GrammarParser.RowLimitContext ctx)
        {
            object numRows;
            object offset;
            if (ctx.o != null)
            { // format "rows offset offsetcount"
                numRows = ParseNumOrVariableIdent(ctx.n1, ctx.i1);
                offset = ParseNumOrVariableIdent(ctx.n2, ctx.i2);
            }
            else if (ctx.c != null)
            { // format "offsetcount, rows"
                offset = ParseNumOrVariableIdent(ctx.n1, ctx.i1);
                numRows = ParseNumOrVariableIdent(ctx.n2, ctx.i2);
            }
            else
            {
                numRows = ParseNumOrVariableIdent(ctx.n1, ctx.i1);
                offset = null;
            }

            int? numRowsInt = null;
            string numRowsVariable = null;
            if (numRows is string)
            {
                numRowsVariable = (string) numRows;
            }
            else
            {
                numRowsInt = numRows.AsInt32();
            }

            int? offsetInt = null;
            string offsetVariable = null;
            if (offset is string)
            {
                offsetVariable = (string) offset;
            }
            else
            {
                offsetInt = offset.AsBoxedInt32();
            }

            return new RowLimitSpec(numRowsInt, offsetInt, numRowsVariable, offsetVariable);
        }

        private static object ParseNumOrVariableIdent(
            EsperEPL2GrammarParser.NumberconstantContext num,
            IToken ident)
        {
            if (ident != null)
            {
                return ident.Text;
            }

            return ASTConstantHelper.Parse(num);
        }
    }
} // end of namespace