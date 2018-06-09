///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;
using System.Linq;
using System.Reflection;

using com.espertech.esper.client;
using com.espertech.esper.client.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.compat.threading;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.expression.time
{
    /// <summary>
    /// Expression representing a time period.
    /// <para/>
    /// Child nodes to this expression carry the actual parts and must return a numeric value.
    /// </summary>
    [Serializable]
    public class ExprTimePeriodImpl
        : ExprNodeBase
        , ExprTimePeriod
        , ExprEvaluator
    {
        private readonly TimeZoneInfo _timeZone;
        private readonly bool _hasYear;
        private readonly bool _hasMonth;
        private readonly bool _hasWeek;
        private readonly bool _hasDay;
        private readonly bool _hasHour;
        private readonly bool _hasMinute;
        private readonly bool _hasSecond;
        private readonly bool _hasMillisecond;
        private readonly bool _hasMicrosecond;
        private readonly TimeAbacus _timeAbacus;
        private bool _hasVariable;

        [NonSerialized] private ExprEvaluator[] _evaluators;
        [NonSerialized] private TimePeriodAdder[] _adders;
        [NonSerialized] private ILockManager _lockManager;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="timeZone">The time zone.</param>
        /// <param name="hasYear">if set to <c>true</c> [has year].</param>
        /// <param name="hasMonth">if set to <c>true</c> [has month].</param>
        /// <param name="hasWeek">if set to <c>true</c> [has week].</param>
        /// <param name="hasDay">true if the expression has that part, false if not</param>
        /// <param name="hasHour">true if the expression has that part, false if not</param>
        /// <param name="hasMinute">true if the expression has that part, false if not</param>
        /// <param name="hasSecond">true if the expression has that part, false if not</param>
        /// <param name="hasMillisecond">true if the expression has that part, false if not</param>
        /// <param name="hasMicrosecond">if set to <c>true</c> [has microsecond].</param>
        /// <param name="timeAbacus">The time abacus.</param>
        /// <param name="lockManager">The lock manager.</param>
        public ExprTimePeriodImpl(
            TimeZoneInfo timeZone,
            bool hasYear,
            bool hasMonth,
            bool hasWeek,
            bool hasDay,
            bool hasHour,
            bool hasMinute,
            bool hasSecond,
            bool hasMillisecond,
            bool hasMicrosecond,
            TimeAbacus timeAbacus,
            ILockManager lockManager)
        {
            _lockManager = lockManager;
            _timeZone = timeZone;
            _hasYear = hasYear;
            _hasMonth = hasMonth;
            _hasWeek = hasWeek;
            _hasDay = hasDay;
            _hasHour = hasHour;
            _hasMinute = hasMinute;
            _hasSecond = hasSecond;
            _hasMillisecond = hasMillisecond;
            _hasMicrosecond = hasMicrosecond;
            _timeAbacus = timeAbacus;
        }

        public ExprTimePeriodEvalDeltaConst ConstEvaluator(ExprEvaluatorContext context)
        {
            if (!_hasMonth && !_hasYear)
            {
                double seconds = EvaluateAsSeconds(null, true, context);
                long msec = _timeAbacus.DeltaForSecondsDouble(seconds);
                return new ExprTimePeriodEvalDeltaConstGivenDelta(msec);
            }
            else
            {
                var evaluateParams = new EvaluateParams(null, true, context);
                var values = new int[_adders.Length];
                for (int i = 0; i < values.Length; i++)
                {
                    values[i] = _evaluators[i].Evaluate(evaluateParams).AsInt();
                }
                return new ExprTimePeriodEvalDeltaConstGivenDtxAdd(
                    _lockManager, _adders, values, _timeZone, _timeAbacus);
            }
        }

        public ExprTimePeriodEvalDeltaNonConst NonconstEvaluator()
        {
            if (!_hasMonth && !_hasYear)
            {
                return new ExprTimePeriodEvalDeltaNonConstMsec(this);
            }
            else
            {
                return new ExprTimePeriodEvalDeltaNonConstDtxAdd(_timeZone, this);
            }
        }

        public TimeAbacus TimeAbacus
        {
            get { return _timeAbacus; }
        }

        public override ExprEvaluator ExprEvaluator
        {
            get { return this; }
        }

        public object Evaluate(EvaluateParams evaluateParams)
        {
            throw new IllegalStateException("Time-Period expression must be evaluated via any of " + typeof(ExprTimePeriod).Name + " interface methods");
        }

        public TimePeriodAdder[] Adders
        {
            get { return _adders; }
        }

        public ExprEvaluator[] Evaluators
        {
            get { return _evaluators; }
        }

        /// <summary>Indicator whether the time period has a day part child expression. </summary>
        /// <value>true for part present, false for not present</value>
        public bool HasDay
        {
            get { return _hasDay; }
        }

        /// <summary>Indicator whether the time period has a hour part child expression. </summary>
        /// <value>true for part present, false for not present</value>
        public bool HasHour
        {
            get { return _hasHour; }
        }

        /// <summary>Indicator whether the time period has a minute part child expression. </summary>
        /// <value>true for part present, false for not present</value>
        public bool HasMinute
        {
            get { return _hasMinute; }
        }

        /// <summary>Indicator whether the time period has a second part child expression. </summary>
        /// <value>true for part present, false for not present</value>
        public bool HasSecond
        {
            get { return _hasSecond; }
        }

        /// <summary>Indicator whether the time period has a millisecond part child expression. </summary>
        /// <value>true for part present, false for not present</value>
        public bool HasMillisecond
        {
            get { return _hasMillisecond; }
        }

        /// <summary>Indicator whether the time period has a microsecond part child expression. </summary>
        /// <value>true for part present, false for not present</value>
        public bool HasMicrosecond
        {
            get { return _hasMicrosecond; }
        }

        /// <summary>Indicator whether the time period has a year part child expression. </summary>
        /// <value>true for part present, false for not present</value>
        public bool HasYear
        {
            get { return _hasYear; }
        }

        /// <summary>Indicator whether the time period has a month part child expression. </summary>
        /// <value>true for part present, false for not present</value>
        public bool HasMonth
        {
            get { return _hasMonth; }
        }

        /// <summary>Indicator whether the time period has a week part child expression. </summary>
        /// <value>true for part present, false for not present</value>
        public bool HasWeek
        {
            get { return _hasWeek; }
        }

        /// <summary>Indicator whether the time period has a variable in any of the child expressions. </summary>
        /// <value>true for variable present, false for not present</value>
        public bool HasVariable
        {
            get { return _hasVariable; }
        }

        public override ExprNode Validate(ExprValidationContext validationContext)
        {
            _evaluators = ExprNodeUtility.GetEvaluators(ChildNodes);
            foreach (ExprNode childNode in ChildNodes)
            {
                Validate(childNode);
            }

            var list = new ArrayDeque<TimePeriodAdder>();
            if (_hasYear)
            {
                list.Add(new TimePeriodAdderYear());
            }
            if (_hasMonth)
            {
                list.Add(new TimePeriodAdderMonth());
            }
            if (_hasWeek)
            {
                list.Add(new TimePeriodAdderWeek());
            }
            if (_hasDay)
            {
                list.Add(new TimePeriodAdderDay());
            }
            if (_hasHour)
            {
                list.Add(new TimePeriodAdderHour());
            }
            if (_hasMinute)
            {
                list.Add(new TimePeriodAdderMinute());
            }
            if (_hasSecond)
            {
                list.Add(new TimePeriodAdderSecond());
            }
            if (_hasMillisecond)
            {
                list.Add(new TimePeriodAdderMSec());
            }
            if (_hasMicrosecond)
            {
                list.Add(new TimePeriodAdderUSec());
            }
            _adders = list.ToArray();

            return null;
        }

        private void Validate(ExprNode expression)
        {
            if (expression == null)
            {
                return;
            }
            var returnType = expression.ExprEvaluator.ReturnType;
            if (!returnType.IsNumeric())
            {
                throw new ExprValidationException("Time period expression requires a numeric parameter type");
            }
            if ((_hasMonth || _hasYear) && returnType.IsNotInt32())
            {
                throw new ExprValidationException("Time period expressions with month or year component require integer values, received a " + returnType.GetCleanName() + " value");
            }
            if (expression is ExprVariableNode)
            {
                _hasVariable = true;
            }
        }

        public double EvaluateAsSeconds(EventBean[] eventsPerStream, bool newData, ExprEvaluatorContext context)
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QExprTimePeriod(this); }
            double seconds = 0;
            for (int i = 0; i < _adders.Length; i++)
            {
                var result = Eval(_evaluators[i], eventsPerStream, newData, context);
                if (result == null)
                {
                    if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AExprTimePeriod(null); }
                    throw new EPException("Failed to evaluate time period, received a null value for '" + this.ToExpressionStringMinPrecedenceSafe() + "'");
                }
                seconds += _adders[i].Compute(result.Value);
            }
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AExprTimePeriod(seconds); }
            return seconds;
        }

        private double? Eval(ExprEvaluator expr, EventBean[] events, bool isNewData, ExprEvaluatorContext exprEvaluatorContext)
        {
            var value = expr.Evaluate(new EvaluateParams(events, isNewData, exprEvaluatorContext));
            if (value == null)
            {
                return null;
            }
            return (value).AsDouble();
        }

        public TimePeriod EvaluateGetTimePeriod(EvaluateParams evaluateParams)
        {
            int exprCtr = 0;

            int? year = null;
            if (_hasYear)
            {
                year = GetInt(_evaluators[exprCtr++].Evaluate(evaluateParams));
            }

            int? month = null;
            if (_hasMonth)
            {
                month = GetInt(_evaluators[exprCtr++].Evaluate(evaluateParams));
            }

            int? week = null;
            if (_hasWeek)
            {
                week = GetInt(_evaluators[exprCtr++].Evaluate(evaluateParams));
            }

            int? day = null;
            if (_hasDay)
            {
                day = GetInt(_evaluators[exprCtr++].Evaluate(evaluateParams));
            }

            int? hours = null;
            if (_hasHour)
            {
                hours = GetInt(_evaluators[exprCtr++].Evaluate(evaluateParams));
            }

            int? minutes = null;
            if (_hasMinute)
            {
                minutes = GetInt(_evaluators[exprCtr++].Evaluate(evaluateParams));
            }

            int? seconds = null;
            if (_hasSecond)
            {
                seconds = GetInt(_evaluators[exprCtr++].Evaluate(evaluateParams));
            }

            int? milliseconds = null;
            if (_hasMillisecond)
            {
                milliseconds = GetInt(_evaluators[exprCtr++].Evaluate(evaluateParams));
            }

            int? microseconds = null;
            if (_hasMicrosecond)
            {
                microseconds = GetInt(_evaluators[exprCtr].Evaluate(evaluateParams));
            }

            return new TimePeriod(year, month, week, day, hours, minutes, seconds, milliseconds, microseconds);
        }

        private int? GetInt(Object evaluated)
        {
            if (evaluated == null)
            {
                return null;
            }
            return (evaluated).AsInt();
        }

        public interface TimePeriodAdder
        {
            double Compute(double value);
            void Add(DateTimeEx dateTime, int value);
            bool IsMicroseconds { get; }
        }

        public class TimePeriodAdderYear : TimePeriodAdder
        {
            private const double MULTIPLIER = 365 * 24 * 60 * 60;

            public double Compute(double value)
            {
                return value * MULTIPLIER;
            }

            public void Add(DateTimeEx dateTime, int value)
            {
                dateTime.AddYears(value, DateTimeMathStyle.Java);
            }

            public bool IsMicroseconds
            {
                get { return false; }
            }
        }

        public class TimePeriodAdderMonth : TimePeriodAdder
        {
            private const double MULTIPLIER = 30 * 24 * 60 * 60;

            public double Compute(double value)
            {
                return value * MULTIPLIER;
            }

            public void Add(DateTimeEx dateTime, int value)
            {
                dateTime.AddMonths(value, DateTimeMathStyle.Java);
            }

            public bool IsMicroseconds
            {
                get { return false; }
            }
        }

        public class TimePeriodAdderWeek : TimePeriodAdder
        {
            private const double MULTIPLIER = 7 * 24 * 60 * 60;

            public double Compute(double value)
            {
                return value * MULTIPLIER;
            }

            public void Add(DateTimeEx dateTime, int value)
            {
                dateTime.AddDays(7 * value, DateTimeMathStyle.Java);
            }

            public bool IsMicroseconds
            {
                get { return false; }
            }
        }

        public class TimePeriodAdderDay : TimePeriodAdder
        {
            private const double MULTIPLIER = 24 * 60 * 60;

            public double Compute(double value)
            {
                return value * MULTIPLIER;
            }

            public void Add(DateTimeEx dateTime, int value)
            {
                dateTime.AddDays(value, DateTimeMathStyle.Java);
            }

            public bool IsMicroseconds
            {
                get { return false; }
            }
        }

        public class TimePeriodAdderHour : TimePeriodAdder
        {
            private const double MULTIPLIER = 60 * 60;

            public double Compute(double value)
            {
                return value * MULTIPLIER;
            }

            public void Add(DateTimeEx dateTime, int value)
            {
                dateTime.AddHours(value);
            }

            public bool IsMicroseconds
            {
                get { return false; }
            }
        }

        public class TimePeriodAdderMinute : TimePeriodAdder
        {
            private const double MULTIPLIER = 60;

            public double Compute(double value)
            {
                return value * MULTIPLIER;
            }

            public void Add(DateTimeEx dateTime, int value)
            {
                dateTime = dateTime.AddMinutes(value);
            }

            public bool IsMicroseconds
            {
                get { return false; }
            }
        }

        public class TimePeriodAdderSecond : TimePeriodAdder
        {
            public double Compute(double value)
            {
                return value;
            }

            public void Add(DateTimeEx dateTime, int value)
            {
                dateTime.AddSeconds(value);
            }

            public bool IsMicroseconds
            {
                get { return false; }
            }
        }

        public class TimePeriodAdderMSec : TimePeriodAdder
        {
            public double Compute(double value)
            {
                return value / 1000d;
            }

            public void Add(DateTimeEx dateTime, int value)
            {
                dateTime.AddMilliseconds(value);
            }

            public bool IsMicroseconds
            {
                get { return false; }
            }
        }

        public class TimePeriodAdderUSec : TimePeriodAdder
        {
            public double Compute(double value)
            {
                return value / 1000000d;
            }

            public void Add(DateTimeEx dateTime, int value)
            {
                // no action : DateTimeEx does not add microseconds
            }

            public bool IsMicroseconds
            {
                get { return true; }
            }
        }

        public Type ReturnType
        {
            get { return typeof(double?); }
        }

        public override bool IsConstantResult
        {
            get { return ChildNodes.All(child => child.IsConstantResult); }
        }

        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
            var exprCtr = 0;
            var delimiter = "";
            var childNodes = ChildNodes;
            if (_hasYear)
            {
                childNodes[exprCtr++].ToEPL(writer, Precedence);
                writer.Write(" years");
                delimiter = " ";
            }
            if (_hasMonth)
            {
                writer.Write(delimiter);
                childNodes[exprCtr++].ToEPL(writer, Precedence);
                writer.Write(" months");
                delimiter = " ";
            }
            if (_hasWeek)
            {
                writer.Write(delimiter);
                childNodes[exprCtr++].ToEPL(writer, Precedence);
                writer.Write(" weeks");
                delimiter = " ";
            }
            if (_hasDay)
            {
                writer.Write(delimiter);
                childNodes[exprCtr++].ToEPL(writer, Precedence);
                writer.Write(" days");
                delimiter = " ";
            }
            if (_hasHour)
            {
                writer.Write(delimiter);
                childNodes[exprCtr++].ToEPL(writer, Precedence);
                writer.Write(" hours");
                delimiter = " ";
            }
            if (_hasMinute)
            {
                writer.Write(delimiter);
                childNodes[exprCtr++].ToEPL(writer, Precedence);
                writer.Write(" minutes");
                delimiter = " ";
            }
            if (_hasSecond)
            {
                writer.Write(delimiter);
                childNodes[exprCtr++].ToEPL(writer, Precedence);
                writer.Write(" seconds");
                delimiter = " ";
            }
            if (_hasMillisecond)
            {
                writer.Write(delimiter);
                childNodes[exprCtr++].ToEPL(writer, Precedence);
                writer.Write(" milliseconds");
            }
            if (_hasMicrosecond)
            {
                writer.Write(delimiter);
                childNodes[exprCtr].ToEPL(writer, Precedence);
                writer.Write(" microseconds");
            }
        }

        public override ExprPrecedenceEnum Precedence
        {
            get { return ExprPrecedenceEnum.UNARY; }
        }

        public override bool EqualsNode(ExprNode node, bool ignoreStreamPrefix)
        {
            if (!(node is ExprTimePeriodImpl))
            {
                return false;
            }

            var other = (ExprTimePeriodImpl)node;

            if (_hasYear != other._hasYear)
            {
                return false;
            }
            if (_hasMonth != other._hasMonth)
            {
                return false;
            }
            if (_hasWeek != other._hasWeek)
            {
                return false;
            }
            if (_hasDay != other._hasDay)
            {
                return false;
            }
            if (_hasHour != other._hasHour)
            {
                return false;
            }
            if (_hasMinute != other._hasMinute)
            {
                return false;
            }
            if (_hasSecond != other._hasSecond)
            {
                return false;
            }
            if (_hasMillisecond != other._hasMillisecond)
            {
                return false;
            }
            return (_hasMicrosecond == other._hasMicrosecond);
        }
    }
}
