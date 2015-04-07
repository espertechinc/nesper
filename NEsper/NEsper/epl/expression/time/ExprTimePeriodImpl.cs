///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.epl.datetime.eval;
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
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    
        private readonly bool _hasYear;
        private readonly bool _hasMonth;
        private readonly bool _hasWeek;
        private readonly bool _hasDay;
        private readonly bool _hasHour;
        private readonly bool _hasMinute;
        private readonly bool _hasSecond;
        private readonly bool _hasMillisecond;
        private bool _hasVariable;
        [NonSerialized] private ExprEvaluator[] _evaluators;
        [NonSerialized] private TimePeriodAdder[] _adders;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="hasYear">if set to <c>true</c> [has year].</param>
        /// <param name="hasMonth">if set to <c>true</c> [has month].</param>
        /// <param name="hasWeek">if set to <c>true</c> [has week].</param>
        /// <param name="hasDay">true if the expression has that part, false if not</param>
        /// <param name="hasHour">true if the expression has that part, false if not</param>
        /// <param name="hasMinute">true if the expression has that part, false if not</param>
        /// <param name="hasSecond">true if the expression has that part, false if not</param>
        /// <param name="hasMillisecond">true if the expression has that part, false if not</param>
        public ExprTimePeriodImpl(bool hasYear, bool hasMonth, bool hasWeek, bool hasDay, bool hasHour, bool hasMinute, bool hasSecond, bool hasMillisecond)
        {
            _hasYear = hasYear;
            _hasMonth = hasMonth;
            _hasWeek = hasWeek;
            _hasDay = hasDay;
            _hasHour = hasHour;
            _hasMinute = hasMinute;
            _hasSecond = hasSecond;
            _hasMillisecond = hasMillisecond;
        }

        public ExprTimePeriodEvalDeltaConst ConstEvaluator(ExprEvaluatorContext context)
        {
            if (!_hasMonth && !_hasYear)
            {
                double seconds = EvaluateAsSeconds(null, true, context);
                long msec = (long) Math.Round(seconds * 1000d);
                return new ExprTimePeriodEvalDeltaConstMsec(msec);
            }
            else
            {
                var evaluateParams = new EvaluateParams(null, true, context);
                var values = new int[_adders.Length];
                for (int i = 0; i < values.Length; i++)
                {
                    values[i] = _evaluators[i].Evaluate(evaluateParams).AsInt();
                }
                return new ExprTimePeriodEvalDeltaConstDateTimeAdd(_adders, values);
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
                return new ExprTimePeriodEvalDeltaNonConstDateTimeAdd(this);
            }
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
            if (_hasYear) {
                list.Add(new TimePeriodAdderYear());
            }
            if (_hasMonth) {
                list.Add(new TimePeriodAdderMonth());
            }
            if (_hasWeek) {
                list.Add(new TimePeriodAdderWeek());
            }
            if (_hasDay) {
                list.Add(new TimePeriodAdderDay());
            }
            if (_hasHour) {
                list.Add(new TimePeriodAdderHour());
            }
            if (_hasMinute) {
                list.Add(new TimePeriodAdderMinute());
            }
            if (_hasSecond) {
                list.Add(new TimePeriodAdderSecond());
            }
            if (_hasMillisecond) {
                list.Add(new TimePeriodAdderMSec());
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
            if ((_hasMonth || _hasYear) && (returnType.GetBoxedType() != typeof(int?)))
            {
                throw new ExprValidationException("Time period expressions with month or year component require integer values, received a " + returnType.FullName + " value");
            }
            if (expression is ExprVariableNode)
            {
                _hasVariable = true;
            }
        }

        public double EvaluateAsSeconds(EventBean[] eventsPerStream, bool newData, ExprEvaluatorContext context)
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QExprTimePeriod(this);}
            double seconds = 0;
            for (int i = 0; i < _adders.Length; i++) {
                var result = Eval(_evaluators[i], eventsPerStream, newData, context);
                if (result == null)
                {
                    if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AExprTimePeriod(null);}
                    throw new EPException("Failed to evaluate time period, received a null value for '" + ExprNodeUtility.ToExpressionStringMinPrecedenceSafe(this) + "'");
                }
                seconds += _adders[i].Compute(result.Value);
            }
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AExprTimePeriod(seconds);}
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
            if (_hasYear){
                year = GetInt(_evaluators[exprCtr++].Evaluate(evaluateParams));
            }
    
            int? month = null;
            if (_hasMonth) {
                month = GetInt(_evaluators[exprCtr++].Evaluate(evaluateParams));
            }
    
            int? week = null;
            if (_hasWeek) {
                week = GetInt(_evaluators[exprCtr++].Evaluate(evaluateParams));
            }
    
            int? day = null;
            if (_hasDay) {
                day = GetInt(_evaluators[exprCtr++].Evaluate(evaluateParams));
            }
    
            int? hours = null;
            if (_hasHour) {
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
                milliseconds = GetInt(_evaluators[exprCtr].Evaluate(evaluateParams));
            }

            return new TimePeriod(year, month, week, day, hours, minutes, seconds, milliseconds);
        }
    
        private int? GetInt(Object evaluated) {
            if (evaluated == null) {
                return null;
            }
            return (evaluated).AsInt();
        }
    
        public interface TimePeriodAdder {
            double Compute(Double value);
            void Add(ref DateTime dateTime, int value);
        }
    
        public class TimePeriodAdderYear : TimePeriodAdder {
            private const double MULTIPLIER = 365*24*60*60;

            public double Compute(Double value) {
                return value*MULTIPLIER;
            }

            public void Add(ref DateTime dateTime, int value)
            {
                dateTime = dateTime.AddYears(value);
            }
        }
    
        public class TimePeriodAdderMonth : TimePeriodAdder {
            private const double MULTIPLIER = 30*24*60*60;

            public double Compute(Double value) {
                return value*MULTIPLIER;
            }

            public void Add(ref DateTime dateTime, int value)
            {
                dateTime = dateTime.AddMonths(value);
            }
        }
    
        public class TimePeriodAdderWeek : TimePeriodAdder {
            private const double MULTIPLIER = 7*24*60*60;

            public double Compute(Double value) {
                return value*MULTIPLIER;
            }

            public void Add(ref DateTime dateTime, int value)
            {
                dateTime = dateTime.AddDays(7*value);
            }
        }
    
        public class TimePeriodAdderDay : TimePeriodAdder {
            private const double MULTIPLIER = 24*60*60;

            public double Compute(Double value) {
                return value*MULTIPLIER;
            }

            public void Add(ref DateTime dateTime, int value)
            {
                dateTime = dateTime.AddDays(value);
            }
        }
    
        public class TimePeriodAdderHour : TimePeriodAdder {
            private const double MULTIPLIER = 60*60;

            public double Compute(Double value) {
                return value*MULTIPLIER;
            }

            public void Add(ref DateTime dateTime, int value)
            {
                dateTime = dateTime.AddHours(value);
            }
        }
    
        public class TimePeriodAdderMinute : TimePeriodAdder {
            private const double MULTIPLIER = 60;

            public double Compute(Double value) {
                return value*MULTIPLIER;
            }

            public void Add(ref DateTime dateTime, int value)
            {
                dateTime = dateTime.AddMinutes(value);
            }
        }
    
        public class TimePeriodAdderSecond : TimePeriodAdder {
            public double Compute(Double value) {
                return value;
            }

            public void Add(ref DateTime dateTime, int value)
            {
                dateTime = dateTime.AddSeconds(value);
            }
        }
    
        public class TimePeriodAdderMSec : TimePeriodAdder {
            public double Compute(Double value) {
                return value / 1000d;
            }

            public void Add(ref DateTime dateTime, int value)
            {
                dateTime = dateTime.AddMilliseconds(value);
            }
        }

        public Type ReturnType
        {
            get { return typeof (double?); }
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
                childNodes[exprCtr].ToEPL(writer, Precedence);
                writer.Write(" milliseconds");
            }
        }

        public override ExprPrecedenceEnum Precedence
        {
            get { return ExprPrecedenceEnum.UNARY; }
        }

        public override bool EqualsNode(ExprNode node)
        {
            if (!(node is ExprTimePeriodImpl))
            {
                return false;
            }

            var other = (ExprTimePeriodImpl) node;

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
            return (_hasMillisecond == other._hasMillisecond);
        }
    }
}
