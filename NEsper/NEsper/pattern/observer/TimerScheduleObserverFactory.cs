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
using com.espertech.esper.client.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression.time;
using com.espertech.esper.schedule;
using com.espertech.esper.util;

namespace com.espertech.esper.pattern.observer
{
    /// <summary>
    ///     Factory for ISO8601 repeating interval observers that indicate truth when a time point was reached.
    /// </summary>
    [Serializable]
    public class TimerScheduleObserverFactory
        : ObserverFactory
        , MetaDefItem
    {
        private const string NAME_OBSERVER = "Timer-schedule observer";

        private const string ISO_NAME = "iso";
        private const string REPETITIONS_NAME = "repetitions";
        private const string DATE_NAME = "date";
        private const string PERIOD_NAME = "period";

        private readonly string[] NAMED_PARAMETERS =
        {
            ISO_NAME,
            REPETITIONS_NAME,
            DATE_NAME,
            PERIOD_NAME
        };

        [NonSerialized] internal MatchedEventConvertor Convertor;

        /// <summary>Convertor.</summary>
        [NonSerialized] internal ITimerScheduleSpecCompute ScheduleComputer;

        internal TimerScheduleSpec Spec;

        public void SetObserverParameters(IList<ExprNode> parameters, MatchedEventConvertor convertor, ExprValidationContext validationContext)
        {
            Convertor = convertor;

            // obtains name parameters
            IDictionary<string, ExprNamedParameterNode> namedExpressions;
            try
            {
                namedExpressions = ExprNodeUtility.GetNamedExpressionsHandleDups(parameters);
                ExprNodeUtility.ValidateNamed(namedExpressions, NAMED_PARAMETERS);
            }
            catch (ExprValidationException e)
            {
                throw new ObserverParameterException(e.Message, e);
            }

            bool allConstantResult;
            ExprNamedParameterNode isoStringExpr = namedExpressions.Get(ISO_NAME);
            if (namedExpressions.Count == 1 && isoStringExpr != null)
            {
                try
                {
                    allConstantResult = ExprNodeUtility.ValidateNamedExpectType(
                        isoStringExpr, new Type[]
                        {
                            typeof (string)
                        });
                }
                catch (ExprValidationException ex)
                {
                    throw new ObserverParameterException(ex.Message, ex);
                }
                ScheduleComputer = new TimerScheduleSpecComputeISOString(isoStringExpr.ChildNodes[0]);
            }
            else if (isoStringExpr != null)
            {
                throw new ObserverParameterException(
                    "The '" + ISO_NAME + "' parameter is exclusive of other parameters");
            }
            else if (namedExpressions.Count == 0)
            {
                throw new ObserverParameterException("No parameters provided");
            }
            else
            {
                allConstantResult = true;
                ExprNamedParameterNode dateNamedNode = namedExpressions.Get(DATE_NAME);
                ExprNamedParameterNode repetitionsNamedNode = namedExpressions.Get(REPETITIONS_NAME);
                ExprNamedParameterNode periodNamedNode = namedExpressions.Get(PERIOD_NAME);
                if (dateNamedNode == null && periodNamedNode == null)
                {
                    throw new ObserverParameterException("Either the date or period parameter is required");
                }
                try
                {
                    if (dateNamedNode != null)
                    {
                        allConstantResult = ExprNodeUtility.ValidateNamedExpectType(
                            dateNamedNode, new Type[]
                            {
                                typeof (string),
                                typeof (DateTime),
                                typeof (DateTimeOffset),
                                typeof (long?)
                            });
                    }
                    if (repetitionsNamedNode != null)
                    {
                        allConstantResult &= ExprNodeUtility.ValidateNamedExpectType(
                            repetitionsNamedNode, new Type[]
                            {
                                typeof (int?),
                                typeof (long?)
                            });
                    }
                    if (periodNamedNode != null)
                    {
                        allConstantResult &= ExprNodeUtility.ValidateNamedExpectType(
                            periodNamedNode, new Type[]
                            {
                                typeof (TimePeriod)
                            });
                    }
                }
                catch (ExprValidationException ex)
                {
                    throw new ObserverParameterException(ex.Message, ex);
                }
                ExprNode dateNode = dateNamedNode == null ? null : dateNamedNode.ChildNodes[0];
                ExprNode repetitionsNode = repetitionsNamedNode == null ? null : repetitionsNamedNode.ChildNodes[0];
                ExprTimePeriod periodNode = periodNamedNode == null
                    ? null
                    : (ExprTimePeriod) periodNamedNode.ChildNodes[0];
                ScheduleComputer = new TimerScheduleSpecComputeFromExpr(dateNode, repetitionsNode, periodNode);
            }

            if (allConstantResult)
            {
                try
                {
                    Spec = ScheduleComputer.Compute(
                        convertor, new MatchedEventMapImpl(convertor.MatchedEventMapMeta), null, validationContext.EngineImportService.TimeZone);
                }
                catch (ScheduleParameterException ex)
                {
                    throw new ObserverParameterException(ex.Message, ex);
                }
            }
        }

        public EventObserver MakeObserver(
            PatternAgentInstanceContext context,
            MatchedEventMap beginState,
            ObserverEventEvaluator observerEventEvaluator,
            EvalStateNodeNumber stateNodeId,
            object observerState,
            bool isFilterChildNonQuitting)
        {
            return new TimerScheduleObserver(
                ComputeSpecDynamic(beginState, context), beginState, observerEventEvaluator, isFilterChildNonQuitting);
        }

        public bool IsNonRestarting()
        {
            return true;
        }

        protected TimerScheduleSpec ComputeSpecDynamic(MatchedEventMap beginState, PatternAgentInstanceContext context)
        {
            if (Spec != null)
            {
                return Spec;
            }
            try
            {
                return ScheduleComputer.Compute(Convertor, beginState, context.AgentInstanceContext, context.StatementContext.EngineImportService.TimeZone);
            }
            catch (ScheduleParameterException e)
            {
                throw new EPException("Error computing iso8601 schedule specification: " + e.Message, e);
            }
        }

        internal interface ITimerScheduleSpecCompute
        {
            TimerScheduleSpec Compute(MatchedEventConvertor convertor, MatchedEventMap beginState, ExprEvaluatorContext exprEvaluatorContext, TimeZoneInfo timeZone);
        }

        internal class TimerScheduleSpecComputeFromExpr : ITimerScheduleSpecCompute
        {
            private readonly ExprNode _dateNode;
            private readonly ExprTimePeriod _periodNode;
            private readonly ExprNode _repetitionsNode;

            internal TimerScheduleSpecComputeFromExpr(
                ExprNode dateNode,
                ExprNode repetitionsNode,
                ExprTimePeriod periodNode)
            {
                _dateNode = dateNode;
                _repetitionsNode = repetitionsNode;
                _periodNode = periodNode;
            }

            public TimerScheduleSpec Compute(MatchedEventConvertor convertor, MatchedEventMap beginState, ExprEvaluatorContext exprEvaluatorContext, TimeZoneInfo timeZone)
            {
                DateTimeEx optionalDate = null;
                if (_dateNode != null)
                {
                    object param = PatternExpressionUtil.Evaluate(
                        NAME_OBSERVER, beginState, _dateNode, convertor, exprEvaluatorContext);
                    if (param is string)
                    {
                        optionalDate = TimerScheduleISO8601Parser.ParseDate((string) param);
                    }
                    else if (param.IsLong())
                    {
                        optionalDate = new DateTimeEx(param.AsDateTimeOffset(timeZone), timeZone);
                    }
                    else if (param.IsInt())
                    {
                        optionalDate = new DateTimeEx(param.AsDateTimeOffset(timeZone), timeZone);
                    }
                    else if (param is DateTimeOffset || param is DateTime)
                    {
                        optionalDate = new DateTimeEx(param.AsDateTimeOffset(timeZone), timeZone);
                    }
                }

                TimePeriod optionalTimePeriod = null;
                if (_periodNode != null)
                {
                    object param = PatternExpressionUtil.EvaluateTimePeriod(
                        NAME_OBSERVER, beginState, _periodNode, convertor, exprEvaluatorContext);
                    optionalTimePeriod = (TimePeriod) param;
                }

                long? optionalRepeatCount = null;
                if (_repetitionsNode != null)
                {
                    object param = PatternExpressionUtil.Evaluate(
                        NAME_OBSERVER, beginState, _repetitionsNode, convertor, exprEvaluatorContext);
                    if (param != null)
                    {
                        optionalRepeatCount = param.AsLong();
                    }
                }

                if (optionalDate == null && optionalTimePeriod == null)
                {
                    throw new EPException("Required date or time period are both null for " + NAME_OBSERVER);
                }

                return new TimerScheduleSpec(optionalDate, optionalRepeatCount, optionalTimePeriod);
            }
        }

        internal class TimerScheduleSpecComputeISOString : ITimerScheduleSpecCompute
        {
            private readonly ExprNode _parameter;

            internal TimerScheduleSpecComputeISOString(ExprNode parameter)
            {
                _parameter = parameter;
            }

            public TimerScheduleSpec Compute(MatchedEventConvertor convertor, MatchedEventMap beginState, ExprEvaluatorContext exprEvaluatorContext, TimeZoneInfo timeZone)
            {
                object param = PatternExpressionUtil.Evaluate(
                    NAME_OBSERVER, beginState, _parameter, convertor, exprEvaluatorContext);
                var iso = (string) param;
                if (iso == null)
                {
                    throw new ScheduleParameterException("Received null parameter value");
                }
                return TimerScheduleISO8601Parser.Parse(iso);
            }
        }
    }
} // end of namespace