///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression.time;
using com.espertech.esper.util;

namespace com.espertech.esper.pattern.observer
{
    /// <summary>Factory for making observer instances.</summary>
    [Serializable]
    public class TimerIntervalObserverFactory
        : ObserverFactory,
          MetaDefItem
    {
        private const string NAME = "Timer-interval observer";

        /// <summary>Parameters.</summary>
        private ExprNode _parameter;

        /// <summary>Convertor to events-per-stream.</summary>
        [NonSerialized] private MatchedEventConvertor _convertor;

        public void SetObserverParameters(
            IList<ExprNode> parameters,
            MatchedEventConvertor convertor,
            ExprValidationContext validationContext)
        {
            ObserverParameterUtil.ValidateNoNamedParameters(NAME, parameters);
            const string errorMessage = NAME + " requires a single numeric or time period parameter";
            if (parameters.Count != 1)
            {
                throw new ObserverParameterException(errorMessage);
            }
            if (!(parameters[0] is ExprTimePeriod))
            {
                var returnType = parameters[0].ExprEvaluator.ReturnType;
                if (!returnType.IsNumeric())
                {
                    throw new ObserverParameterException(errorMessage);
                }
            }

            _parameter = parameters[0];
            _convertor = convertor;
        }

        public long ComputeDelta(MatchedEventMap beginState, PatternAgentInstanceContext context)
        {
            if (_parameter is ExprTimePeriod)
            {
                var timePeriod = (ExprTimePeriod) _parameter;
                return timePeriod.NonconstEvaluator()
                    .DeltaUseEngineTime(_convertor.Convert(beginState), context.AgentInstanceContext);
            }
            else
            {
                var result = _parameter.ExprEvaluator.Evaluate(
                    new EvaluateParams(_convertor.Convert(beginState), true, context.AgentInstanceContext));
                if (result == null)
                {
                    throw new EPException("Null value returned for guard expression");
                }
                return context.StatementContext.TimeAbacus.DeltaForSecondsNumber(result);
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
            return new TimerIntervalObserver(ComputeDelta(beginState, context), beginState, observerEventEvaluator);
        }

        public bool IsNonRestarting
        {
            get { return false; }
        }
    }
} // end of namespace
