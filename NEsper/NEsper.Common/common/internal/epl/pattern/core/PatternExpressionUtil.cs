///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.filterspec;
using com.espertech.esper.compat.logging;

namespace com.espertech.esper.common.@internal.epl.pattern.core
{
    /// <summary>
    ///     Utility for evaluating pattern expressions.
    /// </summary>
    public class PatternExpressionUtil
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(PatternExpressionUtil));

        public static void ToPrecedenceFreeEPL(
            StringWriter writer, string delimiterText, IList<EvalForgeNode> childNodes,
            PatternExpressionPrecedenceEnum precedence)
        {
            var delimiter = "";
            foreach (var child in childNodes) {
                writer.Write(delimiter);
                child.ToEPL(writer, precedence);
                delimiter = " " + delimiterText + " ";
            }
        }

        public static object GetKeys(
            MatchedEventMap matchEvent, MatchedEventConvertor convertor, ExprEvaluator expression,
            AgentInstanceContext agentInstanceContext)
        {
            var eventsPerStream = convertor.Convert(matchEvent);
            return expression.Evaluate(eventsPerStream, true, agentInstanceContext);
        }

        public static object EvaluateChecked(
            string objectName, ExprEvaluator evaluator, EventBean[] eventsPerStream,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            try {
                return evaluator.Evaluate(eventsPerStream, true, exprEvaluatorContext);
            }
            catch (EPException) {
                throw;
            }
            catch (Exception ex) {
                throw HandleRuntimeEx(ex, objectName);
            }
        }

        public static EPException HandleRuntimeEx(Exception ex, string objectName)
        {
            var message = objectName + " failed to evaluate expression";
            if (ex.Message != null) {
                message += ": " + ex.Message;
            }

            log.Error(message, ex);
            throw new EPException(message);
        }
    }
} // end of namespace