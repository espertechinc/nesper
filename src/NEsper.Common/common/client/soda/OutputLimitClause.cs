///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;

using com.espertech.esper.common.@internal.type;

namespace com.espertech.esper.common.client.soda
{
    /// <summary>
    ///     An output limit clause defines how to limit output of statements and consists of
    ///     a selector specifiying which events to select to output, a frequency and a unit.
    /// </summary>
    public class OutputLimitClause
    {
        protected internal int? afterNumberOfEvents;
        protected internal Expression afterTimePeriodExpression;
        protected internal bool andAfterTerminate;
        protected internal Expression andAfterTerminateAndExpr;
        protected internal IList<Assignment> andAfterTerminateThenAssignments;
        protected internal Expression[] crontabAtParameters;
        protected internal double? frequency;
        protected internal string frequencyVariable;
        protected internal OutputLimitSelector selector;
        protected internal IList<Assignment> thenAssignments;
        protected internal Expression timePeriodExpression;
        protected internal OutputLimitUnit unit;
        protected internal Expression whenExpression;

        /// <summary>
        ///     Ctor.
        /// </summary>
        public OutputLimitClause()
        {
        }

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="selector">selector</param>
        /// <param name="unit">unit</param>
        public OutputLimitClause(
            OutputLimitSelector selector,
            OutputLimitUnit unit)
        {
            this.selector = selector;
            this.unit = unit;
        }

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="selector">is the events to select</param>
        /// <param name="frequency">a frequency to output at</param>
        public OutputLimitClause(
            OutputLimitSelector selector,
            double? frequency)
        {
            this.selector = selector;
            this.frequency = frequency;
            unit = OutputLimitUnit.EVENTS;
        }

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="selector">is the events to select</param>
        /// <param name="timePeriodExpression">the unit for the frequency</param>
        public OutputLimitClause(
            OutputLimitSelector selector,
            TimePeriodExpression timePeriodExpression)
        {
            this.selector = selector;
            this.timePeriodExpression = timePeriodExpression;
            unit = OutputLimitUnit.TIME_PERIOD;
        }

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="afterTimePeriodExpression">timer period for after.</param>
        public OutputLimitClause(TimePeriodExpression afterTimePeriodExpression)
        {
            unit = OutputLimitUnit.AFTER;
            this.afterTimePeriodExpression = afterTimePeriodExpression;
        }

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="selector">is the events to select</param>
        /// <param name="frequencyVariable">is the variable name providing output rate frequency values</param>
        public OutputLimitClause(
            OutputLimitSelector selector,
            string frequencyVariable)
        {
            this.selector = selector;
            this.frequencyVariable = frequencyVariable;
            unit = OutputLimitUnit.EVENTS;
        }

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="selector">is the events to select</param>
        /// <param name="frequency">a frequency to output at</param>
        /// <param name="unit">the unit for the frequency</param>
        /// <param name="frequencyVariable">is the variable name providing output rate frequency values</param>
        public OutputLimitClause(
            OutputLimitSelector selector,
            double? frequency,
            string frequencyVariable,
            OutputLimitUnit unit)
        {
            this.selector = selector;
            this.frequency = frequency;
            this.frequencyVariable = frequencyVariable;
            this.unit = unit;
        }

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="selector">is the events to select</param>
        /// <param name="unit">the unit of selection</param>
        /// <param name="afterTimePeriod">after-keyword time period</param>
        /// <param name="afterNumberOfEvents">after-keyword number of events</param>
        public OutputLimitClause(
            OutputLimitSelector selector,
            OutputLimitUnit unit,
            TimePeriodExpression afterTimePeriod,
            int? afterNumberOfEvents)
        {
            this.selector = selector;
            this.unit = unit;
            afterTimePeriodExpression = afterTimePeriod;
            this.afterNumberOfEvents = afterNumberOfEvents;
        }

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="selector">is the events to select</param>
        /// <param name="crontabAtParameters">the crontab schedule parameters</param>
        public OutputLimitClause(
            OutputLimitSelector selector,
            Expression[] crontabAtParameters)
        {
            this.selector = selector;
            this.crontabAtParameters = crontabAtParameters;
            unit = OutputLimitUnit.CRONTAB_EXPRESSION;
        }

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="selector">is the events to select</param>
        /// <param name="whenExpression">the boolean expression to evaluate to control output</param>
        /// <param name="thenAssignments">the variable assignments, optional or an empty list</param>
        public OutputLimitClause(
            OutputLimitSelector selector,
            Expression whenExpression,
            IList<Assignment> thenAssignments)
        {
            this.selector = selector;
            this.whenExpression = whenExpression;
            this.thenAssignments = thenAssignments;
            unit = OutputLimitUnit.WHEN_EXPRESSION;
        }

        /// <summary>
        ///     Returns the selector indicating the events to output.
        /// </summary>
        /// <returns>selector</returns>
        public OutputLimitSelector Selector {
            get => selector;
            set => selector = value;
        }

        /// <summary>
        ///     Returns output frequency.
        /// </summary>
        /// <returns>frequency of output</returns>
        public double? Frequency {
            get => frequency;
            set => frequency = value;
        }

        /// <summary>
        ///     Returns the unit the frequency is in.
        /// </summary>
        /// <returns>unit for the frequency.</returns>
        public OutputLimitUnit Unit {
            get => unit;
            set => unit = value;
        }

        /// <summary>
        ///     Returns the variable name of the variable providing output rate frequency values, or null if the frequency is a
        ///     fixed value.
        /// </summary>
        /// <returns>variable name or null if no variable is used</returns>
        public string FrequencyVariable {
            get => frequencyVariable;
            set => frequencyVariable = value;
        }

        /// <summary>
        ///     Returns the expression that controls output for use with the when-keyword.
        /// </summary>
        /// <returns>expression should be boolean result</returns>
        public Expression WhenExpression {
            get => whenExpression;
            set => whenExpression = value;
        }

        /// <summary>
        ///     Returns the time period, or null if none provided.
        /// </summary>
        /// <returns>time period</returns>
        public Expression TimePeriodExpression {
            get => timePeriodExpression;
            set => timePeriodExpression = value;
        }

        /// <summary>
        ///     Returns the list of optional then-keyword variable assignments, if any
        /// </summary>
        /// <returns>list of variable assignments or null if none</returns>
        public IList<Assignment> ThenAssignments {
            get => thenAssignments;
            set => thenAssignments = value;
        }

        /// <summary>
        ///     Returns the crontab parameters, or null if not using crontab-like schedule.
        /// </summary>
        /// <returns>parameters</returns>
        public Expression[] CrontabAtParameters {
            get => crontabAtParameters;
            set => crontabAtParameters = value;
        }

        /// <summary>
        ///     Returns true for output upon termination of a context partition
        /// </summary>
        /// <returns>indicator</returns>
        public bool IsAndAfterTerminate {
            get => andAfterTerminate;
            set => andAfterTerminate = value;
        }

        /// <summary>
        ///     Returns the after-keyword time period.
        /// </summary>
        /// <returns>after-keyword time period</returns>
        public Expression AfterTimePeriodExpression {
            get => afterTimePeriodExpression;
            set => afterTimePeriodExpression = value;
        }

        /// <summary>
        ///     Returns the after-keyword number of events, or null if undefined.
        /// </summary>
        /// <returns>num events for after-keyword</returns>
        public int? AfterNumberOfEvents {
            get => afterNumberOfEvents;
            set => afterNumberOfEvents = value;
        }

        /// <summary>
        ///     Returns the optional expression evaluated when a context partition terminates before triggering output.
        /// </summary>
        /// <returns>expression</returns>
        public Expression AndAfterTerminateAndExpr {
            get => andAfterTerminateAndExpr;
            set => andAfterTerminateAndExpr = value;
        }

        /// <summary>
        ///     Returns the set-assignments to execute when a context partition terminates.
        /// </summary>
        /// <returns>set-assignments</returns>
        public IList<Assignment> AndAfterTerminateThenAssignments {
            get => andAfterTerminateThenAssignments;
            set => andAfterTerminateThenAssignments = value;
        }

        /// <summary>
        ///     Creates an output limit clause.
        /// </summary>
        /// <param name="timePeriodExpression">a frequency to output at</param>
        /// <returns>clause</returns>
        public static OutputLimitClause Create(TimePeriodExpression timePeriodExpression)
        {
            return new OutputLimitClause(OutputLimitSelector.DEFAULT, timePeriodExpression);
        }

        /// <summary>
        ///     Create with after-only time period.
        /// </summary>
        /// <param name="afterTimePeriodExpression">time period</param>
        /// <returns>clause</returns>
        public static OutputLimitClause CreateAfter(TimePeriodExpression afterTimePeriodExpression)
        {
            return new OutputLimitClause(
                OutputLimitSelector.DEFAULT,
                OutputLimitUnit.AFTER,
                afterTimePeriodExpression,
                null);
        }

        /// <summary>
        ///     Create with after-only and number of events.
        /// </summary>
        /// <param name="afterNumEvents">num events</param>
        /// <returns>clause</returns>
        public static OutputLimitClause CreateAfter(int afterNumEvents)
        {
            return new OutputLimitClause(OutputLimitSelector.DEFAULT, OutputLimitUnit.AFTER, null, afterNumEvents);
        }

        /// <summary>
        ///     Creates an output limit clause.
        /// </summary>
        /// <param name="selector">is the events to select</param>
        /// <param name="timePeriodExpression">a frequency to output at</param>
        /// <returns>clause</returns>
        public static OutputLimitClause Create(
            OutputLimitSelector selector,
            TimePeriodExpression timePeriodExpression)
        {
            return new OutputLimitClause(selector, timePeriodExpression);
        }

        /// <summary>
        ///     Creates an output limit clause.
        /// </summary>
        /// <param name="selector">is the events to select</param>
        /// <param name="frequency">a frequency to output at</param>
        /// <returns>clause</returns>
        public static OutputLimitClause Create(
            OutputLimitSelector selector,
            double frequency)
        {
            return new OutputLimitClause(selector, frequency);
        }

        /// <summary>
        ///     Creates an output limit clause.
        /// </summary>
        /// <param name="selector">is the events to select</param>
        /// <param name="frequencyVariable">is the variable providing the output limit frequency</param>
        /// <returns>clause</returns>
        public static OutputLimitClause Create(
            OutputLimitSelector selector,
            string frequencyVariable)
        {
            return new OutputLimitClause(selector, frequencyVariable);
        }

        /// <summary>
        ///     Creates an output limit clause.
        /// </summary>
        /// <param name="frequency">a frequency to output at</param>
        /// <returns>clause</returns>
        public static OutputLimitClause Create(double frequency)
        {
            return new OutputLimitClause(OutputLimitSelector.DEFAULT, frequency);
        }

        /// <summary>
        ///     Creates an output limit clause.
        /// </summary>
        /// <param name="frequencyVariable">is the variable name providing output rate frequency values</param>
        /// <returns>clause</returns>
        public static OutputLimitClause Create(string frequencyVariable)
        {
            return new OutputLimitClause(OutputLimitSelector.DEFAULT, frequencyVariable);
        }

        /// <summary>
        ///     Creates an output limit clause with a when-expression and optional then-assignment expressions to be added.
        /// </summary>
        /// <param name="whenExpression">the expression that returns true to trigger output</param>
        /// <returns>clause</returns>
        public static OutputLimitClause Create(Expression whenExpression)
        {
            return new OutputLimitClause(OutputLimitSelector.DEFAULT, whenExpression, new List<Assignment>());
        }

        /// <summary>
        /// Creates an output limit clause with a crontab 'at' schedule parameters,
        /// see <seealso cref="FrequencyParameter" /> and related.
        /// </summary>
        /// <param name="scheduleParameters">the crontab schedule parameters</param>
        /// <returns>clause</returns>
        public static OutputLimitClause CreateSchedule(Expression[] scheduleParameters)
        {
            return new OutputLimitClause(OutputLimitSelector.DEFAULT, scheduleParameters);
        }

        /// <summary>
        ///     Renders the clause in textual representation.
        /// </summary>
        /// <param name="writer">to output to</param>
        public void ToEPL(TextWriter writer)
        {
            if (afterTimePeriodExpression != null) {
                writer.Write("after ");
                afterTimePeriodExpression.ToEPL(writer, ExpressionPrecedenceEnum.MINIMUM);
                writer.Write(" ");
            }
            else if (afterNumberOfEvents != null && afterNumberOfEvents != 0) {
                writer.Write("after ");
                writer.Write(afterNumberOfEvents.Value);
                writer.Write(" events ");
            }

            if (selector != OutputLimitSelector.DEFAULT) {
                writer.Write(selector.GetText());
                writer.Write(" ");
            }

            if (unit == OutputLimitUnit.WHEN_EXPRESSION) {
                writer.Write("when ");
                whenExpression.ToEPL(writer, ExpressionPrecedenceEnum.MINIMUM);

                if (thenAssignments != null && thenAssignments.Count > 0) {
                    WriteThenAssignments(writer, thenAssignments);
                }
            }
            else if (unit == OutputLimitUnit.CRONTAB_EXPRESSION) {
                writer.Write("at (");
                var delimiter = "";
                for (var i = 0; i < crontabAtParameters.Length; i++) {
                    writer.Write(delimiter);
                    crontabAtParameters[i].ToEPL(writer, ExpressionPrecedenceEnum.MINIMUM);
                    delimiter = ", ";
                }

                writer.Write(")");
            }
            else if (unit == OutputLimitUnit.TIME_PERIOD && timePeriodExpression != null) {
                writer.Write("every ");
                timePeriodExpression.ToEPL(writer, ExpressionPrecedenceEnum.MINIMUM);
            }
            else if (unit == OutputLimitUnit.AFTER) {
                // no action required
            }
            else if (unit == OutputLimitUnit.CONTEXT_PARTITION_TERM) {
                writer.Write("when terminated");
                OutputAndAfter(writer);
            }
            else {
                writer.Write("every ");
                if (frequencyVariable == null) {
                    writer.Write(frequency.Value);
                }
                else {
                    writer.Write(frequencyVariable);
                }

                writer.Write(" events");
            }

            if (andAfterTerminate) {
                writer.Write(" and when terminated");
                OutputAndAfter(writer);
            }
        }

        /// <summary>
        ///     Sets the selector indicating the events to output.
        /// </summary>
        /// <param name="selector">to set</param>
        public OutputLimitClause WithSelector(OutputLimitSelector selector)
        {
            this.selector = selector;
            return this;
        }

        /// <summary>
        ///     Sets the unit the frequency is in.
        /// </summary>
        /// <param name="unit">is the unit for the frequency</param>
        public OutputLimitClause WithUnit(OutputLimitUnit unit)
        {
            this.unit = unit;
            return this;
        }

        /// <summary>
        ///     Sets the variable name of the variable providing output rate frequency values, or null if the frequency is a fixed
        ///     value.
        /// </summary>
        /// <param name="frequencyVariable">variable name or null if no variable is used</param>
        public OutputLimitClause WithFrequencyVariable(string frequencyVariable)
        {
            this.frequencyVariable = frequencyVariable;
            return this;
        }

        /// <summary>
        ///     Adds a then-keyword variable assigment for use with the when-keyword.
        /// </summary>
        /// <param name="assignmentExpression">expression to calculate new value</param>
        /// <returns>clause</returns>
        public OutputLimitClause WithAddThenAssignment(Expression assignmentExpression)
        {
            thenAssignments.Add(new Assignment(assignmentExpression));
            return this;
        }

        /// <summary>
        ///     Set true for output upon termination of a context partition
        /// </summary>
        /// <param name="andAfterTerminate">indicator</param>
        public OutputLimitClause WithAndAfterTerminate(bool andAfterTerminate)
        {
            this.andAfterTerminate = andAfterTerminate;
            return this;
        }

        /// <summary>
        ///     Sets the after-keyword time period.
        /// </summary>
        /// <param name="afterTimePeriodExpression">after-keyword time period</param>
        /// <returns>clause</returns>
        public OutputLimitClause WithAfterTimePeriodExpression(TimePeriodExpression afterTimePeriodExpression)
        {
            this.afterTimePeriodExpression = afterTimePeriodExpression;
            return this;
        }

        /// <summary>
        ///     Set frequency.
        /// </summary>
        /// <param name="frequency">to set</param>
        public OutputLimitClause WithFrequency(double? frequency)
        {
            this.frequency = frequency;
            return this;
        }

        /// <summary>
        ///     Set when.
        /// </summary>
        /// <param name="whenExpression">to set</param>
        public OutputLimitClause WithWhenExpression(Expression whenExpression)
        {
            this.whenExpression = whenExpression;
            return this;
        }

        /// <summary>
        ///     Set then.
        /// </summary>
        /// <param name="thenAssignments">to set</param>
        public OutputLimitClause WithThenAssignments(IList<Assignment> thenAssignments)
        {
            this.thenAssignments = thenAssignments;
            return this;
        }

        /// <summary>
        ///     Crontab.
        /// </summary>
        /// <param name="crontabAtParameters">to set</param>
        public OutputLimitClause WithCrontabAtParameters(Expression[] crontabAtParameters)
        {
            this.crontabAtParameters = crontabAtParameters;
            return this;
        }

        /// <summary>
        ///     Crontab
        /// </summary>
        /// <param name="timePeriodExpression">to set</param>
        public OutputLimitClause WithTimePeriodExpression(Expression timePeriodExpression)
        {
            this.timePeriodExpression = timePeriodExpression;
            return this;
        }

        /// <summary>
        ///     Sets the after-keyword number of events, or null if undefined.
        /// </summary>
        /// <param name="afterNumberOfEvents">set num events for after-keyword</param>
        /// <returns>clause</returns>
        public OutputLimitClause WithAfterNumberOfEvents(int? afterNumberOfEvents)
        {
            this.afterNumberOfEvents = afterNumberOfEvents;
            return this;
        }

        /// <summary>
        ///     Sets an optional expression evaluated when a context partition terminates before triggering output.
        /// </summary>
        /// <param name="andAfterTerminateAndExpr">expression</param>
        public OutputLimitClause WithAndAfterTerminateAndExpr(Expression andAfterTerminateAndExpr)
        {
            this.andAfterTerminateAndExpr = andAfterTerminateAndExpr;
            return this;
        }

        /// <summary>
        ///     Sets the set-assignments to execute when a context partition terminates.
        /// </summary>
        /// <param name="andAfterTerminateThenAssignments">set-assignments</param>
        public OutputLimitClause WithAndAfterTerminateThenAssignments(
            IList<Assignment> andAfterTerminateThenAssignments)
        {
            this.andAfterTerminateThenAssignments = andAfterTerminateThenAssignments;
            return this;
        }

        private void WriteThenAssignments(
            TextWriter writer,
            IList<Assignment> thenAssignments)
        {
            writer.Write(" then ");
            UpdateClause.RenderEPLAssignments(writer, thenAssignments);
        }

        private void OutputAndAfter(TextWriter writer)
        {
            if (andAfterTerminateAndExpr != null) {
                writer.Write(" and ");
                andAfterTerminateAndExpr.ToEPL(writer, ExpressionPrecedenceEnum.MINIMUM);
            }

            if (andAfterTerminateThenAssignments != null && andAfterTerminateThenAssignments.Count > 0) {
                WriteThenAssignments(writer, andAfterTerminateThenAssignments);
            }
        }
    }
} // end of namespace