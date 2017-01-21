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
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.expression.time;

namespace com.espertech.esper.pattern
{
    /// <summary>
    /// Utility for evaluating pattern expressions.
    /// </summary>
    public class PatternExpressionUtil
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    
        public static Object GetKeys(MatchedEventMap matchEvent, MatchedEventConvertor convertor, ExprEvaluator[] expressions, AgentInstanceContext agentInstanceContext)
        {
            var eventsPerStream = convertor.Convert(matchEvent);
            if (expressions.Length == 1) {
                return expressions[0].Evaluate(new EvaluateParams(eventsPerStream, true, agentInstanceContext));
            }
    
            var keys = new Object[expressions.Length];
            for (var i = 0; i < keys.Length; i++)
            {
                keys[i] = expressions[i].Evaluate(new EvaluateParams(eventsPerStream, true, agentInstanceContext));
            }
            return new MultiKeyUntyped(keys);
        }
    
        /// <summary>Ctor. </summary>
        /// <param name="objectName">is the pattern object name</param>
        /// <param name="beginState">the pattern begin state</param>
        /// <param name="parameters">object parameters</param>
        /// <param name="convertor">for converting to a event-per-stream view for use to evaluate expressions</param>
        /// <param name="exprEvaluatorContext">expression evaluation context</param>
        /// <returns>expression results</returns>
        /// <throws>EPException if the evaluate failed</throws>
        public static IList<Object> Evaluate(String objectName, MatchedEventMap beginState, IList<ExprNode> parameters, MatchedEventConvertor convertor, ExprEvaluatorContext exprEvaluatorContext)
        {
            IList<Object> results = new List<Object>();
            var count = 0;
            var eventsPerStream = convertor.Convert(beginState);
            foreach (var expr in parameters)
            {
                try
                {
                    var result = Evaluate(objectName, expr, eventsPerStream, exprEvaluatorContext);
                    results.Add(result);
                    count++;
                }
                catch (Exception ex)
                {
                    var message = objectName + " invalid parameter in expression " + count;
                    if (ex.Message != null)
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
        /// <param name="timePeriod">The time period.</param>
        /// <param name="convertor">to converting from pattern match to event-per-stream</param>
        /// <param name="exprEvaluatorContext">expression evaluation context</param>
        /// <returns>
        /// evaluation result
        /// </returns>
        /// <throws>EPException if the evaluation failed</throws>
        public static Object EvaluateTimePeriod(String objectName, MatchedEventMap beginState, ExprTimePeriod timePeriod, MatchedEventConvertor convertor, ExprEvaluatorContext exprEvaluatorContext)
        {
            EventBean[] eventsPerStream = convertor.Convert(beginState);
            try {
                return timePeriod.EvaluateGetTimePeriod(new EvaluateParams(eventsPerStream, true, exprEvaluatorContext));
            }
            catch (Exception ex)
            {
                throw HandleRuntimeException(ex, objectName);
            }
        }
        
        public static Object Evaluate(String objectName, MatchedEventMap beginState, ExprNode parameter, MatchedEventConvertor convertor, ExprEvaluatorContext exprEvaluatorContext)
        {
            var eventsPerStream = convertor.Convert(beginState);
            return Evaluate(objectName, parameter, eventsPerStream, exprEvaluatorContext);
        }
    
        private static Object Evaluate(String objectName, ExprNode expression, EventBean[] eventsPerStream, ExprEvaluatorContext exprEvaluatorContext)
        {
            try
            {
                return expression.ExprEvaluator.Evaluate(new EvaluateParams(eventsPerStream, true, exprEvaluatorContext));
            }
            catch (Exception ex)
            {
                throw HandleRuntimeException(ex, objectName);
            }
        }

        private static EPException HandleRuntimeException(Exception ex, string objectName)
        {
            var message = objectName + " failed to evaluate expression";
            if (ex.Message != null)
            {
                message += ": " + ex.Message;
            }
            Log.Error(message, ex);
            throw new EPException(message);
        }

        public static void ToPrecedenceFreeEPL(TextWriter writer, String delimiterText, IList<EvalFactoryNode> childNodes, PatternExpressionPrecedenceEnum precedence)
        {
            var delimiter = "";
            foreach (var child in childNodes)
            {
                writer.Write(delimiter);
                child.ToEPL(writer, precedence);
                delimiter = " " + delimiterText + " ";
            }
        }
    }
}
