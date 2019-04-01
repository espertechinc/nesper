///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
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
        private static readonly ILog log = LogManager.GetLogger(typeof(TimerAtObserverFactory));

        private MatchedEventConvertor optionalConvertor;
        private ExprEvaluator[] parameters;
        private int scheduleCallbackId = -1;
        private ScheduleSpec spec;

        public ExprEvaluator[] Parameters {
            set => parameters = value;
        }

        public MatchedEventConvertor OptionalConvertor {
            set => optionalConvertor = value;
        }

        public ScheduleSpec Spec {
            set => spec = value;
        }

        public int ScheduleCallbackId {
            set => scheduleCallbackId = value;
        }

        public EventObserver MakeObserver(
            PatternAgentInstanceContext context, MatchedEventMap beginState,
            ObserverEventEvaluator observerEventEvaluator, object observerState, bool isFilterChildNonQuitting)
        {
            return new TimerAtObserver(ComputeSpec(beginState, context), beginState, observerEventEvaluator);
        }

        public bool IsNonRestarting => false;

        public ScheduleSpec ComputeSpec(MatchedEventMap beginState, PatternAgentInstanceContext context)
        {
            if (spec != null) {
                return spec;
            }

            var observerParameters = EvaluateRuntime(
                beginState, parameters, optionalConvertor, context.AgentInstanceContext);
            try {
                return ScheduleSpecUtil.ComputeValues(observerParameters);
            }
            catch (ScheduleParameterException e) {
                throw new EPException("Error computing crontab schedule specification: " + e.Message, e);
            }
        }

        private static object[] EvaluateRuntime(
            MatchedEventMap beginState, ExprEvaluator[] parameters, MatchedEventConvertor optionalConvertor,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            var results = new object[parameters.Length];
            var count = 0;
            var eventsPerStream = optionalConvertor == null ? null : optionalConvertor.Convert(beginState);
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

                    log.Error(message, ex);
                    throw new EPException(message);
                }
            }

            return results;
        }
    }
} // end of namespace