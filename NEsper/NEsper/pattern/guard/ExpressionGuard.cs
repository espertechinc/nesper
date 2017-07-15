///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.pattern;

namespace com.espertech.esper.pattern.guard
{
    /// <summary>
    /// Guard implementation that keeps a timer instance and quits when the timer expired,
    /// and also keeps a count of the number of matches so far, checking both count and timer,
    /// letting all <seealso cref="com.espertech.esper.pattern.MatchedEventMap" /> instances pass until then.
    /// </summary>
    public class ExpressionGuard : Guard {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    
        private readonly Quitable quitable;
        private readonly MatchedEventConvertor convertor;
        private readonly ExprEvaluator expression;
    
        public ExpressionGuard(MatchedEventConvertor convertor, ExprEvaluator expression, Quitable quitable) {
            this.quitable = quitable;
            this.convertor = convertor;
            this.expression = expression;
        }
    
        public void StartGuard() {
        }
    
        public bool Inspect(MatchedEventMap matchEvent) {
            EventBean[] eventsPerStream = convertor.Convert(matchEvent);
    
            try {
                Object result = expression.Evaluate(eventsPerStream, true, quitable.Context.AgentInstanceContext);
                if (result == null) {
                    return false;
                }
    
                if (result.Equals(bool?.TRUE)) {
                    return true;
                }
    
                quitable.GuardQuit();
                return false;
            } catch (RuntimeException ex) {
                string message = "Failed to evaluate expression for pattern-guard for statement '" + quitable.Context.PatternContext.StatementName + "'";
                if (ex.Message != null) {
                    message += ": " + ex.Message;
                }
                Log.Error(message, ex);
                throw new EPException(message);
            }
        }
    
        public void StopGuard() {
        }
    
        public void Accept(EventGuardVisitor visitor) {
            visitor.VisitGuard(0);
        }
    }
} // end of namespace
