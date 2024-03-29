///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.pattern.core;
using com.espertech.esper.common.@internal.filterspec;
using com.espertech.esper.common.@internal.schedule;
using com.espertech.esper.compat.logging;

namespace com.espertech.esper.common.@internal.epl.pattern.observer
{
    /// <summary>
    ///     Factory for 'crontab' observers that indicate truth when a time point was reached.
    /// </summary>
    public class TimerAtObserverFactory : ObserverFactory
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(TimerAtObserverFactory));

        private MatchedEventConvertor _optionalConvertor;
        private ExprEvaluator[] _parameters;
        private int _scheduleCallbackId = -1;
        private ScheduleSpec _spec;

        public ExprEvaluator[] Parameters {
            set => _parameters = value;
        }

        public MatchedEventConvertor OptionalConvertor {
            set => _optionalConvertor = value;
        }

        public ScheduleSpec Spec {
            set => _spec = value;
        }

        public int ScheduleCallbackId {
            set => _scheduleCallbackId = value;
        }

        public EventObserver MakeObserver(
            PatternAgentInstanceContext context,
            MatchedEventMap beginState,
            ObserverEventEvaluator observerEventEvaluator,
            object observerState,
            bool isFilterChildNonQuitting)
        {
            return new TimerAtObserver(ComputeSpec(beginState, context), beginState, observerEventEvaluator);
        }

        public bool IsNonRestarting => false;

        public ScheduleSpec ComputeSpec(
            MatchedEventMap beginState,
            PatternAgentInstanceContext context)
        {
            if (_spec != null) {
                return _spec;
            }

            var observerParameters = EvaluateRuntime(
                beginState,
                _parameters,
                _optionalConvertor,
                context.AgentInstanceContext);
            try {
                return ScheduleSpecUtil.ComputeValues(observerParameters);
            }
            catch (ScheduleParameterException e) {
                throw new EPException("Error computing crontab schedule specification: " + e.Message, e);
            }
        }

        private static object[] EvaluateRuntime(
            MatchedEventMap beginState,
            ExprEvaluator[] parameters,
            MatchedEventConvertor optionalConvertor,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            var results = new object[parameters.Length];
            var count = 0;
            var eventsPerStream = optionalConvertor?.Invoke(beginState);
            foreach (var expr in parameters) {
                try {
                    var result = expr.Evaluate(eventsPerStream, true, exprEvaluatorContext);
                    results[count] = result;
                    count++;
                }
                catch (EPException) {
                    throw;
                }
                catch (Exception ex) {
                    var message = "Timer-at observer invalid parameter in expression " + count;
                    if (ex.Message != null) {
                        message += ": " + ex.Message;
                    }

                    Log.Error(message, ex);
                    throw new EPException(message);
                }
            }

            return results;
        }
    }
} // end of namespace