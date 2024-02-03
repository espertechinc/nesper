///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.pattern.core;
using com.espertech.esper.common.@internal.filterspec;
using com.espertech.esper.compat.logging;

namespace com.espertech.esper.common.@internal.epl.pattern.guard
{
    /// <summary>
    ///     Guard implementation that keeps a timer instance and quits when the timer expired,
    ///     and also keeps a count of the number of matches so far, checking both count and timer,
    ///     letting all <seealso cref="MatchedEventMap" /> instances pass until then.
    /// </summary>
    public class ExpressionGuard : Guard
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ExpressionGuard));

        private readonly MatchedEventConvertor _convertor;
        private readonly ExprEvaluator _expression;
        private readonly Quitable _quitable;

        public ExpressionGuard(
            MatchedEventConvertor convertor,
            ExprEvaluator expression,
            Quitable quitable)
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
            var eventsPerStream = _convertor.Invoke(matchEvent);

            try {
                var result = _expression.Evaluate(eventsPerStream, true, _quitable.Context.AgentInstanceContext);
                if (result == null) {
                    return false;
                }

                if (true.Equals(result)) {
                    return true;
                }

                _quitable.GuardQuit();
                return false;
            }
            catch (EPRuntimeException ex) {
                var message = "Failed to evaluate expression for pattern-guard for statement '" +
                              _quitable.Context.AgentInstanceContext.StatementName +
                              "'";
                if (ex.Message != null) {
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