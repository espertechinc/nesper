///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.client;
using com.espertech.esper.compat.logging;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.schedule;
using com.espertech.esper.util;

namespace com.espertech.esper.pattern.observer
{
    /// <summary>
    /// Factory for 'crontab' observers that indicate truth when a time point was reached.
    /// </summary>
    [Serializable]
    public class TimerAtObserverFactory
        : ObserverFactory
        , MetaDefItem
    {
        private static readonly ILog Log =
            LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>Parameters.</summary>
        private IList<ExprNode> _parameters;

        /// <summary>Convertor.</summary>
        [NonSerialized] private MatchedEventConvertor _convertor;

        /// <summary>The schedule specification for the timer-at.</summary>
        private ScheduleSpec _spec = null;

        public void SetObserverParameters(
            IList<ExprNode> parameters,
            MatchedEventConvertor convertor,
            ExprValidationContext validationContext)
        {
            ObserverParameterUtil.ValidateNoNamedParameters("timer:at", parameters);
            if (Log.IsDebugEnabled)
            {
                Log.Debug(".setObserverParameters " + parameters);
            }

            if ((parameters.Count < 5) || (parameters.Count > 7))
            {
                throw new ObserverParameterException("Invalid number of parameters for timer:at");
            }

            _parameters = parameters;
            _convertor = convertor;

            // if all parameters are constants, lets try to evaluate and build a schedule for early validation
            bool allConstantResult = true;
            foreach (ExprNode param in parameters)
            {
                if (!param.IsConstantResult)
                {
                    allConstantResult = false;
                }
            }

            if (allConstantResult)
            {
                try
                {
                    var observerParameters = PatternExpressionUtil.Evaluate(
                        "Timer-at observer", new MatchedEventMapImpl(convertor.MatchedEventMapMeta), parameters,
                        convertor, null);
                    _spec = ScheduleSpecUtil.ComputeValues(observerParameters.ToArray());
                }
                catch (ScheduleParameterException e)
                {
                    throw new ObserverParameterException(
                        "Error computing crontab schedule specification: " + e.Message, e);
                }
            }
        }

        public ScheduleSpec ComputeSpec(MatchedEventMap beginState, PatternAgentInstanceContext context)
        {
            if (_spec != null)
            {
                return _spec;
            }
            var observerParameters = PatternExpressionUtil.Evaluate(
                "Timer-at observer", beginState, _parameters, _convertor, context.AgentInstanceContext);
            try
            {
                return ScheduleSpecUtil.ComputeValues(observerParameters.ToArray());
            }
            catch (ScheduleParameterException e)
            {
                throw new EPException("Error computing crontab schedule specification: " + e.Message, e);
            }
        }

        public EventObserver MakeObserver(
            PatternAgentInstanceContext context,
            MatchedEventMap beginState,
            ObserverEventEvaluator observerEventEvaluator,
            EvalStateNodeNumber stateNodeId,
            Object observerState,
            bool isFilterChildNonQuitting)
        {
            return new TimerAtObserver(ComputeSpec(beginState, context), beginState, observerEventEvaluator);
        }

        public bool IsNonRestarting
        {
            get { return false; }
        }
    }
} // end of namespace
