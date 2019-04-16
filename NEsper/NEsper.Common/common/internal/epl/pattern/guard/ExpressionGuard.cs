///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
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
        private static readonly ILog log = LogManager.GetLogger(typeof(ExpressionGuard));

        private readonly MatchedEventConvertor convertor;
        private readonly ExprEvaluator expression;
        private readonly Quitable quitable;

        public ExpressionGuard(
            MatchedEventConvertor convertor,
            ExprEvaluator expression,
            Quitable quitable)
        {
            this.quitable = quitable;
            this.convertor = convertor;
            this.expression = expression;
        }

        public void StartGuard()
        {
        }

        public bool Inspect(MatchedEventMap matchEvent)
        {
            var eventsPerStream = convertor.Convert(matchEvent);

            try {
                var result = expression.Evaluate(eventsPerStream, true, quitable.Context.AgentInstanceContext);
                if (result == null) {
                    return false;
                }

                if (true.Equals(result)) {
                    return true;
                }

                quitable.GuardQuit();
                return false;
            }
            catch (EPRuntimeException ex) {
                var message = "Failed to evaluate expression for pattern-guard for statement '" +
                              quitable.Context.AgentInstanceContext.StatementName + "'";
                if (ex.Message != null) {
                    message += ": " + ex.Message;
                }

                log.Error(message, ex);
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