///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;

using com.espertech.esper.compat;

namespace com.espertech.esper.client.soda
{
    /// <summary>
    /// An output limit clause defines how to limit output of statements and consists of a 
    /// selector specifiying which events to select to output, a frequency and a unit.
    /// </summary>
    [Serializable]
    public class OutputLimitClause
    {
        /// <summary>Ctor. </summary>
        public OutputLimitClause()
        {
        }

        /// <summary>Ctor. </summary>
        /// <param name="selector">selector</param>
        /// <param name="unit">unit</param>
        public OutputLimitClause(OutputLimitSelector selector, OutputLimitUnit unit)
        {
            Selector = selector;
            Unit = unit;
        }

        /// <summary>Creates an output limit clause. </summary>
        /// <param name="timePeriodExpression">a frequency to output at</param>
        /// <returns>clause</returns>
        public static OutputLimitClause Create(TimePeriodExpression timePeriodExpression)
        {
            return new OutputLimitClause(OutputLimitSelector.DEFAULT, timePeriodExpression);
        }

        /// <summary>Create with after-only time period. </summary>
        /// <param name="afterTimePeriodExpression">time period</param>
        /// <returns>clause</returns>
        public static OutputLimitClause CreateAfter(TimePeriodExpression afterTimePeriodExpression)
        {
            return new OutputLimitClause(
                OutputLimitSelector.DEFAULT, OutputLimitUnit.AFTER, afterTimePeriodExpression, null);
        }

        /// <summary>Create with after-only and number of events. </summary>
        /// <param name="afterNumEvents">num events</param>
        /// <returns>clause</returns>
        public static OutputLimitClause CreateAfter(int afterNumEvents)
        {
            return new OutputLimitClause(OutputLimitSelector.DEFAULT, OutputLimitUnit.AFTER, null, afterNumEvents);
        }

        /// <summary>Creates an output limit clause. </summary>
        /// <param name="selector">is the events to select</param>
        /// <param name="timePeriodExpression">a frequency to output at</param>
        /// <returns>clause</returns>
        public static OutputLimitClause Create(OutputLimitSelector selector, TimePeriodExpression timePeriodExpression)
        {
            return new OutputLimitClause(selector, timePeriodExpression);
        }

        /// <summary>Creates an output limit clause. </summary>
        /// <param name="selector">is the events to select</param>
        /// <param name="frequency">a frequency to output at</param>
        /// <returns>clause</returns>
        public static OutputLimitClause Create(OutputLimitSelector selector, double frequency)
        {
            return new OutputLimitClause(selector, frequency);
        }

        /// <summary>Creates an output limit clause. </summary>
        /// <param name="selector">is the events to select</param>
        /// <param name="frequencyVariable">is the variable providing the output limit frequency</param>
        /// <returns>clause</returns>
        public static OutputLimitClause Create(OutputLimitSelector selector, String frequencyVariable)
        {
            return new OutputLimitClause(selector, frequencyVariable);
        }

        /// <summary>Creates an output limit clause. </summary>
        /// <param name="frequency">a frequency to output at</param>
        /// <returns>clause</returns>
        public static OutputLimitClause Create(double frequency)
        {
            return new OutputLimitClause(OutputLimitSelector.DEFAULT, frequency);
        }

        /// <summary>Creates an output limit clause. </summary>
        /// <param name="frequencyVariable">is the variable name providing output rate frequency values</param>
        /// <returns>clause</returns>
        public static OutputLimitClause Create(String frequencyVariable)
        {
            return new OutputLimitClause(OutputLimitSelector.DEFAULT, frequencyVariable);
        }

        /// <summary>Creates an output limit clause with a when-expression and optional then-assignment expressions to be added. </summary>
        /// <param name="whenExpression">the expression that returns true to trigger output</param>
        /// <returns>clause</returns>
        public static OutputLimitClause Create(Expression whenExpression)
        {
            return new OutputLimitClause(OutputLimitSelector.DEFAULT, whenExpression, new List<Assignment>());
        }

        /// <summary>Creates an output limit clause with a crontab 'at' schedule parameters, see <seealso cref="com.espertech.esper.type.FrequencyParameter" /> and related. </summary>
        /// <param name="scheduleParameters">the crontab schedule parameters</param>
        /// <returns>clause</returns>
        public static OutputLimitClause CreateSchedule(Expression[] scheduleParameters)
        {
            return new OutputLimitClause(OutputLimitSelector.DEFAULT, scheduleParameters);
        }

        /// <summary>Ctor. </summary>
        /// <param name="selector">is the events to select</param>
        /// <param name="frequency">a frequency to output at</param>
        public OutputLimitClause(OutputLimitSelector selector, Double frequency)
        {
            Selector = selector;
            Frequency = frequency;
            Unit = OutputLimitUnit.EVENTS;
        }

        /// <summary>Ctor. </summary>
        /// <param name="selector">is the events to select</param>
        /// <param name="timePeriodExpression">the unit for the frequency</param>
        public OutputLimitClause(OutputLimitSelector selector, TimePeriodExpression timePeriodExpression)
        {
            Selector = selector;
            TimePeriodExpression = timePeriodExpression;
            Unit = OutputLimitUnit.TIME_PERIOD;
        }

        /// <summary>Ctor. </summary>
        /// <param name="afterTimePeriodExpression">timer period for after.</param>
        public OutputLimitClause(TimePeriodExpression afterTimePeriodExpression)
        {
            Unit = OutputLimitUnit.AFTER;
            AfterTimePeriodExpression = afterTimePeriodExpression;
        }

        /// <summary>Ctor. </summary>
        /// <param name="selector">is the events to select</param>
        /// <param name="frequencyVariable">is the variable name providing output rate frequency values</param>
        public OutputLimitClause(OutputLimitSelector selector, String frequencyVariable)
        {
            Selector = selector;
            FrequencyVariable = frequencyVariable;
            Unit = OutputLimitUnit.EVENTS;
        }

        /// <summary>Ctor. </summary>
        /// <param name="selector">is the events to select</param>
        /// <param name="frequency">a frequency to output at</param>
        /// <param name="unit">the unit for the frequency</param>
        /// <param name="frequencyVariable">is the variable name providing output rate frequency values</param>
        public OutputLimitClause(
            OutputLimitSelector selector,
            Double? frequency,
            String frequencyVariable,
            OutputLimitUnit unit)
        {
            Selector = selector;
            Frequency = frequency;
            FrequencyVariable = frequencyVariable;
            Unit = unit;
        }

        /// <summary>Ctor. </summary>
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
            Selector = selector;
            Unit = unit;
            AfterTimePeriodExpression = afterTimePeriod;
            AfterNumberOfEvents = afterNumberOfEvents;
        }

        /// <summary>Ctor. </summary>
        /// <param name="selector">is the events to select</param>
        /// <param name="crontabAtParameters">the crontab schedule parameters</param>
        public OutputLimitClause(OutputLimitSelector selector, Expression[] crontabAtParameters)
        {
            Selector = selector;
            CrontabAtParameters = crontabAtParameters;
            Unit = OutputLimitUnit.CRONTAB_EXPRESSION;
        }

        /// <summary>Ctor. </summary>
        /// <param name="selector">is the events to select</param>
        /// <param name="whenExpression">the bool expression to evaluate to control output</param>
        /// <param name="thenAssignments">the variable assignments, optional or an empty list</param>
        public OutputLimitClause(
            OutputLimitSelector selector,
            Expression whenExpression,
            IList<Assignment> thenAssignments)
        {
            Selector = selector;
            WhenExpression = whenExpression;
            ThenAssignments = thenAssignments;
            Unit = OutputLimitUnit.WHEN_EXPRESSION;
        }

        /// <summary>Returns the selector indicating the events to output. </summary>
        /// <value>selector</value>
        public OutputLimitSelector Selector { get; set; }

        /// <summary>Returns output frequency. </summary>
        /// <value>frequency of output</value>
        public double? Frequency { get; private set; }

        /// <summary>Returns the unit the frequency is in. </summary>
        /// <value>unit for the frequency.</value>
        public OutputLimitUnit Unit { get; set; }

        /// <summary>Returns the variable name of the variable providing output rate frequency values, or null if the frequency is a fixed value. </summary>
        /// <value>variable name or null if no variable is used</value>
        public string FrequencyVariable { get; set; }

        /// <summary>Returns the expression that controls output for use with the when-keyword. </summary>
        /// <value>expression should be bool result</value>
        public Expression WhenExpression { get; private set; }

        /// <summary>Returns the time period, or null if none provided. </summary>
        /// <value>time period</value>
        public Expression TimePeriodExpression { get; private set; }

        /// <summary>Returns the list of optional then-keyword variable assignments, if any </summary>
        /// <value>list of variable assignments or null if none</value>
        public IList<Assignment> ThenAssignments { get; private set; }

        /// <summary>Adds a then-keyword variable assigment for use with the when-keyword. </summary>
        /// <param name="assignmentExpression">expression to calculate new value</param>
        /// <returns>clause</returns>
        public OutputLimitClause AddThenAssignment(Expression assignmentExpression)
        {
            ThenAssignments.Add(new Assignment(assignmentExpression));
            return this;
        }

        /// <summary>Returns the crontab parameters, or null if not using crontab-like schedule. </summary>
        /// <value>parameters</value>
        public Expression[] CrontabAtParameters { get; private set; }

        /// <summary>Returns true for output upon termination of a context partition </summary>
        /// <value>indicator</value>
        public bool IsAndAfterTerminate { get; set; }

        /// <summary>Renders the clause in textual representation. </summary>
        /// <param name="writer">to output to</param>
        public void ToEPL(TextWriter writer)
        {
            if (AfterTimePeriodExpression != null)
            {
                writer.Write("after ");
                AfterTimePeriodExpression.ToEPL(writer, ExpressionPrecedenceEnum.MINIMUM);
                writer.Write(" ");
            }
            else if ((AfterNumberOfEvents != null) && (AfterNumberOfEvents != 0))
            {
                writer.Write("after ");
                writer.Write(Convert.ToString(AfterNumberOfEvents));
                writer.Write(" events ");
            }

            if (Selector != OutputLimitSelector.DEFAULT)
            {
                writer.Write(Selector.GetText());
                writer.Write(" ");
            }
            if (Unit == OutputLimitUnit.WHEN_EXPRESSION)
            {
                writer.Write("when ");
                WhenExpression.ToEPL(writer, ExpressionPrecedenceEnum.MINIMUM);

                if ((ThenAssignments != null) && (ThenAssignments.Count > 0))
                {
                    WriteThenAssignments(writer, ThenAssignments);
                }
            }
            else if (Unit == OutputLimitUnit.CRONTAB_EXPRESSION)
            {
                writer.Write("at (");
                String delimiter = "";
                for (int i = 0; i < CrontabAtParameters.Length; i++)
                {
                    writer.Write(delimiter);
                    CrontabAtParameters[i].ToEPL(writer, ExpressionPrecedenceEnum.MINIMUM);
                    delimiter = ", ";
                }
                writer.Write(")");
            }
            else if (Unit == OutputLimitUnit.TIME_PERIOD && TimePeriodExpression != null)
            {
                writer.Write("every ");
                TimePeriodExpression.ToEPL(writer, ExpressionPrecedenceEnum.MINIMUM);
            }
            else if (Unit == OutputLimitUnit.AFTER)
            {
                // no action required
            }
            else if (Unit == OutputLimitUnit.CONTEXT_PARTITION_TERM)
            {
                writer.Write("when terminated");
                OutputAndAfter(writer);
            }
            else
            {
                writer.Write("every ");
                if (FrequencyVariable == null)
                {
                    writer.Write(Convert.ToString(Frequency.AsInt()));
                }
                else
                {
                    writer.Write(FrequencyVariable);
                }
                writer.Write(" events");
            }

            if (IsAndAfterTerminate)
            {
                writer.Write(" and when terminated");
                OutputAndAfter(writer);
            }
        }

        /// <summary>Returns the after-keyword time period. </summary>
        /// <value>after-keyword time period</value>
        public Expression AfterTimePeriodExpression { get; set; }

        /// <summary>Sets the after-keyword time period. </summary>
        /// <param name="afterTimePeriodExpression">after-keyword time period</param>
        public OutputLimitClause SetAfterTimePeriodExpression(Expression afterTimePeriodExpression)
        {
            AfterTimePeriodExpression = afterTimePeriodExpression;
            return this;
        }

        /// <summary>Returns the after-keyword number of events, or null if undefined. </summary>
        /// <value>num events for after-keyword</value>
        public int? AfterNumberOfEvents { get; set; }

        /// <summary>Sets the after-keyword number of events, or null if undefined. </summary>
        /// <param name="afterNumberOfEvents">set num events for after-keyword</param>
        public OutputLimitClause SetAfterNumberOfEvents(int? afterNumberOfEvents)
        {
            AfterNumberOfEvents = afterNumberOfEvents;
            return this;
        }

        /// <summary>Returns the optional expression evaluated when a context partition terminates before triggering output. </summary>
        /// <value>expression</value>
        public Expression AndAfterTerminateAndExpr { get; set; }

        /// <summary>Returns the set-assignments to execute when a context partition terminates. </summary>
        /// <value>set-assignments</value>
        public IList<Assignment> AndAfterTerminateThenAssignments { get; set; }

        private void WriteThenAssignments(TextWriter writer, IList<Assignment> thenAssignments)
        {
            writer.Write(" then ");
            UpdateClause.RenderEPLAssignments(writer, thenAssignments);
        }

        private void OutputAndAfter(TextWriter writer)
        {
            if (AndAfterTerminateAndExpr != null)
            {
                writer.Write(" and ");
                AndAfterTerminateAndExpr.ToEPL(writer, ExpressionPrecedenceEnum.MINIMUM);
            }
            if (AndAfterTerminateThenAssignments != null && AndAfterTerminateThenAssignments.Count > 0)
            {
                WriteThenAssignments(writer, AndAfterTerminateThenAssignments);
            }
        }
    }
}
