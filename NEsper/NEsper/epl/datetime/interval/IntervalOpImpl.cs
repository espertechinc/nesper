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
using com.espertech.esper.compat;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.datetime.eval;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression.dot;
using com.espertech.esper.epl.expression.time;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.datetime.interval
{
    public class IntervalOpImpl : IntervalOp
    {
        private readonly ExprEvaluator _evaluatorTimestamp;

        private readonly int _parameterStreamNum;
        private readonly String _parameterPropertyStart;
        private readonly String _parameterPropertyEnd;

        private readonly IntervalOpEval _intervalOpEval;

        public IntervalOpImpl(
            DatetimeMethodEnum method,
            String methodNameUse,
            StreamTypeService streamTypeService,
            IList<ExprNode> expressions,
            TimeZoneInfo timeZone,
            TimeAbacus timeAbacus)
        {
            ExprEvaluator evaluatorEndTimestamp = null;
            Type timestampType;

            if (expressions[0] is ExprStreamUnderlyingNode)
            {
                var und = (ExprStreamUnderlyingNode)expressions[0];
                _parameterStreamNum = und.StreamId;
                EventType type = streamTypeService.EventTypes[_parameterStreamNum];
                _parameterPropertyStart = type.StartTimestampPropertyName;
                if (_parameterPropertyStart == null)
                {
                    throw new ExprValidationException("For date-time method '" + methodNameUse + "' the first parameter is event type '" + type.Name + "', however no timestamp property has been defined for this event type");
                }

                timestampType = type.GetPropertyType(_parameterPropertyStart);
                EventPropertyGetter getter = type.GetGetter(_parameterPropertyStart);
                _evaluatorTimestamp = new ExprEvaluatorStreamLongProp(_parameterStreamNum, getter);

                if (type.EndTimestampPropertyName != null)
                {
                    _parameterPropertyEnd = type.EndTimestampPropertyName;
                    EventPropertyGetter getterEndTimestamp = type.GetGetter(type.EndTimestampPropertyName);
                    evaluatorEndTimestamp = new ExprEvaluatorStreamLongProp(_parameterStreamNum, getterEndTimestamp);
                }
                else
                {
                    _parameterPropertyEnd = _parameterPropertyStart;
                }
            }
            else
            {
                _evaluatorTimestamp = expressions[0].ExprEvaluator;
                timestampType = _evaluatorTimestamp.ReturnType;

                String unresolvedPropertyName = null;
                if (expressions[0] is ExprIdentNode)
                {
                    var identNode = (ExprIdentNode)expressions[0];
                    _parameterStreamNum = identNode.StreamId;
                    _parameterPropertyStart = identNode.ResolvedPropertyName;
                    _parameterPropertyEnd = _parameterPropertyStart;
                    unresolvedPropertyName = identNode.UnresolvedPropertyName;
                }

                if (!_evaluatorTimestamp.ReturnType.IsDateTime())
                {
                    // ident node may represent a fragment
                    if (unresolvedPropertyName != null)
                    {
                        var propertyDesc = ExprIdentNodeUtil.GetTypeFromStream(
                            streamTypeService, unresolvedPropertyName, false, true);
                        if (propertyDesc.First.FragmentEventType != null)
                        {
                            EventType type = propertyDesc.First.FragmentEventType.FragmentType;
                            _parameterPropertyStart = type.StartTimestampPropertyName;
                            if (_parameterPropertyStart == null)
                            {
                                throw new ExprValidationException("For date-time method '" + methodNameUse + "' the first parameter is event type '" + type.Name + "', however no timestamp property has been defined for this event type");
                            }

                            timestampType = type.GetPropertyType(_parameterPropertyStart);
                            EventPropertyGetter getterFragment = streamTypeService.EventTypes[_parameterStreamNum].GetGetter(unresolvedPropertyName);
                            EventPropertyGetter getterStartTimestamp = type.GetGetter(_parameterPropertyStart);
                            _evaluatorTimestamp = new ExprEvaluatorStreamLongPropFragment(_parameterStreamNum, getterFragment, getterStartTimestamp);

                            if (type.EndTimestampPropertyName != null)
                            {
                                _parameterPropertyEnd = type.EndTimestampPropertyName;
                                EventPropertyGetter getterEndTimestamp = type.GetGetter(type.EndTimestampPropertyName);
                                evaluatorEndTimestamp = new ExprEvaluatorStreamLongPropFragment(_parameterStreamNum, getterFragment, getterEndTimestamp);
                            }
                            else
                            {
                                _parameterPropertyEnd = _parameterPropertyStart;
                            }
                        }
                    }
                    else
                    {
                        throw new ExprValidationException(
                            "For date-time method '" + methodNameUse +
                            "' the first parameter expression returns '" + _evaluatorTimestamp.ReturnType.GetCleanName() + 
                            "', however requires a DateTime or Long-type return value or event (with timestamp)");
                    }
                }
            }

            IntervalComputer intervalComputer = IntervalComputerFactory.Make(method, expressions, timeAbacus);

            // evaluation without end timestamp
            var timestampTypeBoxed = timestampType != null ? timestampType.GetBoxedType() : timestampType;
            if (evaluatorEndTimestamp == null)
            {
                if (timestampTypeBoxed == typeof(DateTime?) || timestampTypeBoxed == typeof(DateTimeOffset?))
                {
                    _intervalOpEval = new IntervalOpEvalCal(intervalComputer);
                }
                else if (timestampTypeBoxed == typeof(DateTimeEx))
                {
                    _intervalOpEval = new IntervalOpEvalDateTimeEx(intervalComputer);
                }
                else if (timestampTypeBoxed == typeof(long?))
                {
                    _intervalOpEval = new IntervalOpEvalLong(intervalComputer);
                }
                else
                {
                    throw new ArgumentException("Invalid interval first parameter type '" + timestampType + "'");
                }
            }
            else
            {
                if (timestampTypeBoxed == typeof(DateTime?) || timestampTypeBoxed == typeof(DateTimeOffset?))
                {
                    _intervalOpEval = new IntervalOpEvalCalWithEnd(intervalComputer, evaluatorEndTimestamp);
                }
                else if (timestampTypeBoxed == typeof(DateTimeEx))
                {
                    _intervalOpEval = new IntervalOpEvalDateTimeExWithEnd(intervalComputer, evaluatorEndTimestamp);
                }
                else if (timestampTypeBoxed == typeof(long?))
                {
                    _intervalOpEval = new IntervalOpEvalLongWithEnd(intervalComputer, evaluatorEndTimestamp);
                }
                else
                {
                    throw new ArgumentException("Invalid interval first parameter type '" + timestampType + "'");
                }
            }
        }

        /// <summary>
        /// Obtain information used by filter analyzer to handle this dot-method invocation as part of query planning/indexing.
        /// </summary>
        /// <param name="typesPerStream">The types per stream.</param>
        /// <param name="currentMethod">The current method.</param>
        /// <param name="currentParameters">The current parameters.</param>
        /// <param name="inputDesc">The input desc.</param>
        /// <returns></returns>
        public FilterExprAnalyzerDTIntervalAffector GetFilterDesc(
            EventType[] typesPerStream,
            DatetimeMethodEnum currentMethod,
            IList<ExprNode> currentParameters,
            ExprDotNodeFilterAnalyzerInput inputDesc)
        {
            // with intervals is not currently query planned
            if (currentParameters.Count > 1)
            {
                return null;
            }

            // Get input (target)
            int targetStreamNum;
            String targetPropertyStart;
            String targetPropertyEnd;
            if (inputDesc is ExprDotNodeFilterAnalyzerInputStream)
            {
                var targetStream = (ExprDotNodeFilterAnalyzerInputStream)inputDesc;
                targetStreamNum = targetStream.StreamNum;
                EventType targetType = typesPerStream[targetStreamNum];
                targetPropertyStart = targetType.StartTimestampPropertyName;
                targetPropertyEnd = targetType.EndTimestampPropertyName ?? targetPropertyStart;
            }
            else if (inputDesc is ExprDotNodeFilterAnalyzerInputProp)
            {
                var targetStream = (ExprDotNodeFilterAnalyzerInputProp)inputDesc;
                targetStreamNum = targetStream.StreamNum;
                targetPropertyStart = targetStream.PropertyName;
                targetPropertyEnd = targetStream.PropertyName;
            }
            else
            {
                return null;
            }

            // check parameter info
            if (_parameterPropertyStart == null)
            {
                return null;
            }

            return new FilterExprAnalyzerDTIntervalAffector(currentMethod, typesPerStream,
                    targetStreamNum, targetPropertyStart, targetPropertyEnd,
                    _parameterStreamNum, _parameterPropertyStart, _parameterPropertyEnd);
        }

        public object Evaluate(long startTs, long endTs, EvaluateParams evaluateParams)
        {
            var parameter = _evaluatorTimestamp.Evaluate(evaluateParams);
            if (parameter == null)
            {
                return parameter;
            }

            return _intervalOpEval.Evaluate(startTs, endTs, parameter, evaluateParams.EventsPerStream, evaluateParams.IsNewData, evaluateParams.ExprEvaluatorContext);
        }

        public interface IntervalOpEval
        {
            Object Evaluate(long startTs, long endTs, Object parameter, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context);
        }

        public abstract class IntervalOpEvalDateBase : IntervalOpEval
        {
            protected readonly IntervalComputer IntervalComputer;

            public abstract object Evaluate(long startTs,
                                            long endTs,
                                            object parameter,
                                            EventBean[] eventsPerStream,
                                            bool isNewData,
                                            ExprEvaluatorContext context);

            public IntervalOpEvalDateBase(IntervalComputer intervalComputer)
            {
                IntervalComputer = intervalComputer;
            }
        }

        public class IntervalOpEvalLong : IntervalOpEvalDateBase
        {
            public IntervalOpEvalLong(IntervalComputer intervalComputer)
                : base(intervalComputer)
            {
            }

            public override Object Evaluate(long startTs, long endTs, Object parameter, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context)
            {
                var time = ((long?)parameter).GetValueOrDefault();
                return IntervalComputer.Compute(startTs, endTs, time, time, eventsPerStream, isNewData, context);
            }
        }

        public class IntervalOpEvalCal : IntervalOpEvalDateBase
        {
            public IntervalOpEvalCal(IntervalComputer intervalComputer)
                : base(intervalComputer)
            {
            }

            public override Object Evaluate(long startTs, long endTs, Object parameter, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context)
            {
                long time = parameter.AsDateTimeOffset().TimeInMillis();
                return IntervalComputer.Compute(startTs, endTs, time, time, eventsPerStream, isNewData, context);
            }
        }

        public class IntervalOpEvalDateTimeEx : IntervalOpEvalDateBase
        {
            public IntervalOpEvalDateTimeEx(IntervalComputer intervalComputer)
                : base(intervalComputer)
            {
            }

            public override Object Evaluate(long startTs, long endTs, Object parameter, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context)
            {
                var time = ((DateTimeEx) parameter).TimeInMillis;
                return IntervalComputer.Compute(startTs, endTs, time, time, eventsPerStream, isNewData, context);
            }
        }

        public abstract class IntervalOpEvalDateWithEndBase : IntervalOpEval
        {
            protected readonly IntervalComputer IntervalComputer;
            private readonly ExprEvaluator _evaluatorEndTimestamp;

            protected IntervalOpEvalDateWithEndBase(IntervalComputer intervalComputer, ExprEvaluator evaluatorEndTimestamp)
            {
                IntervalComputer = intervalComputer;
                _evaluatorEndTimestamp = evaluatorEndTimestamp;
            }

            public abstract Object Evaluate(long startTs, long endTs, Object parameterStartTs, Object parameterEndTs, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context);

            public Object Evaluate(long startTs, long endTs, Object parameterStartTs, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context)
            {
                Object paramEndTs = _evaluatorEndTimestamp.Evaluate(new EvaluateParams(eventsPerStream, isNewData, context));
                if (paramEndTs == null)
                {
                    return null;
                }
                return Evaluate(startTs, endTs, parameterStartTs, paramEndTs, eventsPerStream, isNewData, context);
            }
        }

        public class IntervalOpEvalLongWithEnd : IntervalOpEvalDateWithEndBase
        {

            public IntervalOpEvalLongWithEnd(IntervalComputer intervalComputer, ExprEvaluator evaluatorEndTimestamp)
                : base(intervalComputer, evaluatorEndTimestamp)
            {
            }

            public override Object Evaluate(long startTs, long endTs, Object parameterStartTs, Object parameterEndTs, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context)
            {
                return IntervalComputer.Compute(startTs, endTs, parameterStartTs.AsLong(), parameterEndTs.AsLong(), eventsPerStream, isNewData, context);
            }
        }

        public class IntervalOpEvalCalWithEnd : IntervalOpEvalDateWithEndBase
        {
            public IntervalOpEvalCalWithEnd(IntervalComputer intervalComputer, ExprEvaluator evaluatorEndTimestamp)
                : base(intervalComputer, evaluatorEndTimestamp)
            {
            }

            public override Object Evaluate(long startTs, long endTs, Object parameterStartTs, Object parameterEndTs, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context)
            {
                return IntervalComputer.Compute(
                    startTs, endTs,
                    parameterStartTs.AsDateTimeOffset().TimeInMillis(),
                    parameterEndTs.AsDateTimeOffset().TimeInMillis(),
                    eventsPerStream, isNewData, context);
            }
        }

        public class IntervalOpEvalDateTimeExWithEnd : IntervalOpEvalDateWithEndBase
        {
            public IntervalOpEvalDateTimeExWithEnd(IntervalComputer intervalComputer, ExprEvaluator evaluatorEndTimestamp)
                : base(intervalComputer, evaluatorEndTimestamp)
            {
            }

            public override Object Evaluate(long startTs, long endTs, Object parameterStartTs, Object parameterEndTs, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context)
            {
                return IntervalComputer.Compute(
                    startTs, endTs,
                    ((DateTimeEx) parameterStartTs).TimeInMillis,
                    ((DateTimeEx) parameterEndTs).TimeInMillis,
                    eventsPerStream, isNewData, context);
            }
        }
    }
}
