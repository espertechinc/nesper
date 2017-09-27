///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;

using com.espertech.esper.client;
using com.espertech.esper.collection;
using com.espertech.esper.compat.logging;
using com.espertech.esper.core.context.util;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression.time;

namespace com.espertech.esper.pattern
{
    /// <summary>Utility for evaluating pattern expressions.</summary>
    public class PatternExpressionUtil
    {
        private static readonly ILog Log =
            LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static Object GetKeys(
            MatchedEventMap matchEvent,
            MatchedEventConvertor convertor,
            ExprEvaluator[] expressions,
            AgentInstanceContext agentInstanceContext)
        {
            var eventsPerStream = convertor.Convert(matchEvent);
            var evaluateParams = new EvaluateParams(eventsPerStream, true, agentInstanceContext);
            if (expressions.Length == 1)
            {
                return expressions[0].Evaluate(evaluateParams);
            }

            var keys = new Object[expressions.Length];
            for (int i = 0; i < keys.Length; i++)
            {
                keys[i] = expressions[i].Evaluate(evaluateParams);
            }
            return new MultiKeyUntyped(keys);
        }

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="objectName">is the pattern object name</param>
        /// <param name="beginState">the pattern begin state</param>
        /// <param name="parameters">object parameters</param>
        /// <param name="convertor">for converting to a event-per-stream view for use to evaluate expressions</param>
        /// <param name="exprEvaluatorContext">expression evaluation context</param>
        /// <exception cref="EPException">if the evaluate failed</exception>
        /// <returns>expression results</returns>
        public static IList<Object> Evaluate(
            string objectName,
            MatchedEventMap beginState,
            IList<ExprNode> parameters,
            MatchedEventConvertor convertor,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            var results = new List<Object>();
            int count = 0;
            EventBean[] eventsPerStream = convertor.Convert(beginState);
            foreach (ExprNode expr in parameters)
            {
                try
                {
                    Object result = Evaluate(objectName, expr, eventsPerStream, exprEvaluatorContext);
                    results.Add(result);
                    count++;
                }
                catch (Exception ex)
                {
                    string message = objectName + " invalid parameter in expression " + count;
                    if (!string.IsNullOrEmpty(ex.Message))
                    {
                        message += ": " + ex.Message;
                    }
                    Log.Error(message, ex);
                    throw new EPException(message);
                }
            }
            return results;
        }

        /// <summary>
        /// Evaluate the pattern expression.
        /// </summary>
        /// <param name="objectName">pattern object name</param>
        /// <param name="beginState">pattern state</param>
        /// <param name="convertor">to converting from pattern match to event-per-stream</param>
        /// <param name="exprEvaluatorContext">expression evaluation context</param>
        /// <param name="timePeriod">time period</param>
        /// <exception cref="EPException">if the evaluation failed</exception>
        /// <returns>evaluation result</returns>
        public static Object EvaluateTimePeriod(
            string objectName,
            MatchedEventMap beginState,
            ExprTimePeriod timePeriod,
            MatchedEventConvertor convertor,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            EventBean[] eventsPerStream = convertor.Convert(beginState);
            try
            {
                return timePeriod.EvaluateGetTimePeriod(new EvaluateParams(eventsPerStream, true, exprEvaluatorContext));
            }
            catch (Exception ex)
            {
                throw HandleRuntimeEx(ex, objectName);
            }
        }

        public static Object Evaluate(
            string objectName,
            MatchedEventMap beginState,
            ExprNode parameter,
            MatchedEventConvertor convertor,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            EventBean[] eventsPerStream = convertor.Convert(beginState);
            return Evaluate(objectName, parameter, eventsPerStream, exprEvaluatorContext);
        }

        private static Object Evaluate(
            string objectName,
            ExprNode expression,
            EventBean[] eventsPerStream,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            try
            {
                return expression.ExprEvaluator.Evaluate(
                    new EvaluateParams(eventsPerStream, true, exprEvaluatorContext));
            }
            catch (Exception ex)
            {
                throw HandleRuntimeEx(ex, objectName);
            }
        }

        private static EPException HandleRuntimeEx(Exception ex, string objectName)
        {
            string message = objectName + " failed to evaluate expression";
            if (!string.IsNullOrEmpty(ex.Message))
            {
                message += ": " + ex.Message;
            }
            Log.Error(message, ex);
            throw new EPException(message);
        }

        public static void ToPrecedenceFreeEPL(
            TextWriter writer,
            string delimiterText,
            IList<EvalFactoryNode> childNodes,
            PatternExpressionPrecedenceEnum precedence)
        {
            string delimiter = "";
            foreach (EvalFactoryNode child in childNodes)
            {
                writer.Write(delimiter);
                child.ToEPL(writer, precedence);
                delimiter = " " + delimiterText + " ";
            }
        }
    }
} // end of namespace
