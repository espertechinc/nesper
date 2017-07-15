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

using com.espertech.esper.compat;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.expression.time;
using com.espertech.esper.epl.generated;
using com.espertech.esper.epl.spec;
using com.espertech.esper.epl.variable;
using com.espertech.esper.schedule;

namespace com.espertech.esper.epl.parse
{
    /// <summary>
    /// Builds an output limit spec from an output limit AST node.
    /// </summary>
    public class ASTOutputLimitHelper
    {
        /// <summary>
        /// Build an output limit spec from the AST node supplied.
        /// </summary>
        /// <param name="astExprNodeMap">is the map of current AST tree nodes to their respective expression root node</param>
        /// <param name="engineURI">the engine uri</param>
        /// <param name="timeProvider">provides time</param>
        /// <param name="variableService">provides variable resolution</param>
        /// <param name="exprEvaluatorContext">context for expression evaluatiom</param>
        /// <returns>output limit spec</returns>
        public static OutputLimitSpec BuildOutputLimitSpec(
            CommonTokenStream tokenStream,
            EsperEPL2GrammarParser.OutputLimitContext ctx,
            IDictionary<ITree, ExprNode> astExprNodeMap,
            VariableService variableService,
            String engineURI,
            TimeProvider timeProvider,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            OutputLimitLimitType displayLimit = OutputLimitLimitType.DEFAULT;
            if (ctx.k != null)
            {
                switch (ctx.k.Type)
                {
                    case EsperEPL2GrammarParser.FIRST: displayLimit = OutputLimitLimitType.FIRST; break;
                    case EsperEPL2GrammarParser.LAST: displayLimit = OutputLimitLimitType.LAST; break;
                    case EsperEPL2GrammarParser.SNAPSHOT: displayLimit = OutputLimitLimitType.SNAPSHOT; break;
                    case EsperEPL2GrammarParser.ALL: displayLimit = OutputLimitLimitType.ALL; break;
                    default: throw ASTWalkException.From("Encountered unrecognized token " + ctx.k.Text, tokenStream, ctx);
                }
            }

            // next is a variable, or time period, or number
            String variableName = null;
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
                        timePeriodExpr = (ExprTimePeriod)ASTExprHelper.ExprCollectSubNodes(ctx.timePeriod(), 0, astExprNodeMap)[0];
                    }
                    else
                    {
                        ASTExprHelper.ExprCollectSubNodes(ctx.number(), 0, astExprNodeMap);  // remove
                        rate = Double.Parse(ctx.number().GetText());
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
                    afterTimePeriodExpr = (ExprTimePeriod)expression;
                }
                else
                {
                    Object constant = ASTConstantHelper.Parse(ctx.outputLimitAfter().number());
                    afterNumberOfEvents = constant.AsInt();
                }
            }

            bool andAfterTerminate = false;
            if (ctx.outputLimitAndTerm() != null)
            {
                andAfterTerminate = true;
                if (ctx.outputLimitAndTerm().expression() != null)
                {
                    andAfterTerminateExpr = ASTExprHelper.ExprCollectSubNodes(ctx.outputLimitAndTerm().expression(), 0, astExprNodeMap)[0];
                }
                if (ctx.outputLimitAndTerm().onSetExpr() != null)
                {
                    andAfterTerminateSetExpressions = ASTExprHelper.GetOnTriggerSetAssignments(ctx.outputLimitAndTerm().onSetExpr().onSetAssignmentList(), astExprNodeMap);
                }
            }

            return new OutputLimitSpec(rate, variableName, rateType, displayLimit, whenExpression, thenExpressions, crontabScheduleSpec, timePeriodExpr, afterTimePeriodExpr, afterNumberOfEvents, andAfterTerminate, andAfterTerminateExpr, andAfterTerminateSetExpressions);
        }

        /// <summary>
        /// Builds a row limit specification.
        /// </summary>
        /// <returns>row limit spec</returns>
        public static RowLimitSpec BuildRowLimitSpec(EsperEPL2GrammarParser.RowLimitContext ctx)
        {
            Object numRows;
            Object offset;
            if (ctx.o != null)
            {    // format "rows offset offsetcount"
                numRows = ParseNumOrVariableIdent(ctx.n1, ctx.i1);
                offset = ParseNumOrVariableIdent(ctx.n2, ctx.i2);
            }
            else if (ctx.c != null)
            {   // format "offsetcount, rows"
                offset = ParseNumOrVariableIdent(ctx.n1, ctx.i1);
                numRows = ParseNumOrVariableIdent(ctx.n2, ctx.i2);
            }
            else
            {
                numRows = ParseNumOrVariableIdent(ctx.n1, ctx.i1);
                offset = null;
            }

            int? numRowsInt = null;
            String numRowsVariable = null;
            if (numRows is String)
            {
                numRowsVariable = (String)numRows;
            }
            else
            {
                numRowsInt = (int?)numRows;
            }

            int? offsetInt = null;
            String offsetVariable = null;
            if (offset is String)
            {
                offsetVariable = (String)offset;
            }
            else
            {
                offsetInt = (int?)offset;
            }

            return new RowLimitSpec(numRowsInt, offsetInt, numRowsVariable, offsetVariable);
        }

        private static Object ParseNumOrVariableIdent(EsperEPL2GrammarParser.NumberconstantContext num, IToken ident)
        {
            if (ident != null)
            {
                return ident.Text;
            }
            else
            {
                return ASTConstantHelper.Parse(num);
            }
        }
    }
}
