///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;

using com.espertech.esper.client;
using com.espertech.esper.compat.logging;
using com.espertech.esper.epl.expression.core;

namespace com.espertech.esper.pattern.guard
{
    /// <summary>
    ///     Guard implementation that keeps a timer instance and quits when the timer expired,
    ///     and also keeps a count of the number of matches so far, checking both count and timer,
    ///     letting all <seealso cref="com.espertech.esper.pattern.MatchedEventMap" /> instances pass until then.
    /// </summary>
    public class ExpressionGuard : Guard
    {
        private static readonly ILog Log =
            LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly MatchedEventConvertor _convertor;
        private readonly ExprEvaluator _expression;
        private readonly Quitable _quitable;

        public ExpressionGuard(MatchedEventConvertor convertor, ExprEvaluator expression, Quitable quitable)
        {
            _quitable = quitable;
            _convertor = convertor;
            _expression = expression;
        }

        public void StartGuard()
        {
        }

        public bool Inspect(MatchedEventMap matchEvent)
        {
            EventBean[] eventsPerStream = _convertor.Convert(matchEvent);

            try
            {
                Object result =
                    _expression.Evaluate(
                        new EvaluateParams(eventsPerStream, true, _quitable.Context.AgentInstanceContext));
                if (result == null)
                {
                    return false;
                }

                if (true.Equals(result))
                {
                    return true;
                }

                _quitable.GuardQuit();
                return false;
            }
            catch (Exception ex)
            {
                string message = "Failed to evaluate expression for pattern-guard for statement '" +
                                 _quitable.Context.PatternContext.StatementName + "'";
                if (!string.IsNullOrWhiteSpace(ex.Message))
                {
                    message += ": " + ex.Message;
                }
                Log.Error(message, ex);
                throw new EPException(message);
            }
        }

        public void StopGuard()
        {
        }

        public void Accept(EventGuardVisitor visitor)
        {
            visitor.VisitGuard(0);
        }
    }
} // end of namespace