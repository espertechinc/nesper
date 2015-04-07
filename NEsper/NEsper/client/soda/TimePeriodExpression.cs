///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

namespace com.espertech.esper.client.soda
{
    /// <summary>
    /// Represent an expression
    /// </summary>
    [Serializable]
    public class TimePeriodExpression : ExpressionBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TimePeriodExpression"/> class.
        /// </summary>
        public TimePeriodExpression()
        {
        }

        /// <summary>Ctor. </summary>
        /// <param name="hasYears">flag to indicate that a year-part expression exists</param>
        /// <param name="hasMonths">flag to indicate that a month-part expression exists</param>
        /// <param name="hasWeeks">flag to indicate that a week-part expression exists</param>
        /// <param name="hasDays">flag to indicate that a day-part expression exists</param>
        /// <param name="hasHours">flag to indicate that a hour-part expression exists</param>
        /// <param name="hasMinutes">flag to indicate that a minute-part expression exists</param>
        /// <param name="hasSeconds">flag to indicate that a seconds-part expression exists</param>
        /// <param name="hasMilliseconds">flag to indicate that a millisec-part expression exists</param>
        public TimePeriodExpression(bool hasYears, bool hasMonths, bool hasWeeks, bool hasDays, bool hasHours, bool hasMinutes, bool hasSeconds, bool hasMilliseconds)
            : this(hasDays, hasHours, hasMinutes, hasSeconds, hasMilliseconds)
        {
            HasYears = hasYears;
            HasMonths = hasMonths;
            HasWeeks = hasWeeks;
        }

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="hasDays">flag to indicate that a day-part expression exists</param>
        /// <param name="hasHours">flag to indicate that a hour-part expression exists</param>
        /// <param name="hasMinutes">flag to indicate that a minute-part expression exists</param>
        /// <param name="hasSeconds">flag to indicate that a seconds-part expression exists</param>
        /// <param name="hasMilliseconds">flag to indicate that a millisec-part expression exists</param>
        public TimePeriodExpression(bool hasDays, bool hasHours, bool hasMinutes, bool hasSeconds, bool hasMilliseconds)
        {
            HasDays = hasDays;
            HasHours = hasHours;
            HasMinutes = hasMinutes;
            HasSeconds = hasSeconds;
            HasMilliseconds = hasMilliseconds;
        }

        /// <summary>Ctor. </summary>
        /// <param name="yearsExpr">expression returning years value, or null if no such part</param>
        /// <param name="monthsExpr">expression returning months value, or null if no such part</param>
        /// <param name="weeksExpr">expression returning weeks value, or null if no such part</param>
        /// <param name="daysExpr">expression returning days value, or null if no such part</param>
        /// <param name="hoursExpr">expression returning hours value, or null if no such part</param>
        /// <param name="minutesExpr">expression returning minutes value, or null if no such part</param>
        /// <param name="secondsExpr">expression returning seconds value, or null if no such part</param>
        /// <param name="millisecondsExpr">expression returning millisec value, or null if no such part</param>
        public TimePeriodExpression(Expression yearsExpr, Expression monthsExpr, Expression weeksExpr, Expression daysExpr, Expression hoursExpr, Expression minutesExpr, Expression secondsExpr, Expression millisecondsExpr)
        {
            if (yearsExpr != null)
            {
                HasYears = true;
                AddChild(yearsExpr);
            }
            if (monthsExpr != null)
            {
                HasMonths = true;
                AddChild(monthsExpr);
            }
            if (weeksExpr != null)
            {
                HasWeeks = true;
                AddChild(weeksExpr);
            }
            AddExpr(daysExpr, hoursExpr, minutesExpr, secondsExpr, millisecondsExpr);
        }

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="daysExpr">expression returning days value, or null if no such part</param>
        /// <param name="hoursExpr">expression returning hours value, or null if no such part</param>
        /// <param name="minutesExpr">expression returning minutes value, or null if no such part</param>
        /// <param name="secondsExpr">expression returning seconds value, or null if no such part</param>
        /// <param name="millisecondsExpr">expression returning millisec value, or null if no such part</param>
        public TimePeriodExpression(Expression daysExpr, Expression hoursExpr, Expression minutesExpr,
                                    Expression secondsExpr, Expression millisecondsExpr)
        {
            AddExpr(daysExpr, hoursExpr, minutesExpr, secondsExpr, millisecondsExpr);
        }

        private void AddExpr(Expression daysExpr, Expression hoursExpr, Expression minutesExpr, Expression secondsExpr, Expression millisecondsExpr)
        {
        if (daysExpr != null) {
                HasDays = true;
                AddChild(daysExpr);
            }
            if (hoursExpr != null) {
                HasHours = true;
                AddChild(hoursExpr);
            }
            if (minutesExpr != null) {
                HasMinutes = true;
                AddChild(minutesExpr);
            }
            if (secondsExpr != null) {
                HasSeconds = true;
                AddChild(secondsExpr);
            }
            if (millisecondsExpr != null) {
                HasMilliseconds = true;
                AddChild(millisecondsExpr);
            }
        }

        /// <summary>
        /// Returns true if a subexpression exists that is a day-part.
        /// </summary>
        /// <returns>
        /// indicator for presence of part
        /// </returns>
        public bool HasDays { get; set; }

        /// <summary>
        /// Returns true if a subexpression exists that is a hour-part.
        /// </summary>
        /// <returns>
        /// indicator for presence of part
        /// </returns>
        public bool HasHours { get; set; }

        /// <summary>
        /// Returns true if a subexpression exists that is a minutes-part.
        /// </summary>
        /// <returns>
        /// indicator for presence of part
        /// </returns>
        public bool HasMinutes { get; set; }

        /// <summary>
        /// Returns true if a subexpression exists that is a seconds-part.
        /// </summary>
        /// <returns>
        /// indicator for presence of part
        /// </returns>
        public bool HasSeconds { get; set; }

        /// <summary>
        /// Returns true if a subexpression exists that is a milliseconds-part.
        /// </summary>
        /// <returns>
        /// indicator for presence of part
        /// </returns>
        public bool HasMilliseconds { get; set; }

        /// <summary>
        /// Returns true if a subexpression exists that is a year-part.
        /// </summary>
        public bool HasYears { get; set; }

        /// <summary>
        /// Returns true if a subexpression exists that is a months-part.
        /// </summary>
        public bool HasMonths { get; set; }

        /// <summary>
        /// Returns true if a subexpression exists that is a weeks-part.
        /// </summary>
        public bool HasWeeks { get; set; }

        public override ExpressionPrecedenceEnum Precedence
        {
            get { return ExpressionPrecedenceEnum.UNARY; }
        }

        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
            String delimiter = "";
            int countExpr = 0;

            if (HasYears)
            {
                Children[countExpr].ToEPL(writer, ExpressionPrecedenceEnum.MINIMUM);
                writer.Write(" years");
                delimiter = " ";
                countExpr++;
            }
            if (HasMonths)
            {
                writer.Write(delimiter);
                Children[countExpr].ToEPL(writer, ExpressionPrecedenceEnum.MINIMUM);
                writer.Write(" months");
                delimiter = " ";
                countExpr++;
            }
            if (HasWeeks)
            {
                writer.Write(delimiter);
                Children[countExpr].ToEPL(writer, ExpressionPrecedenceEnum.MINIMUM);
                writer.Write(" weeks");
                delimiter = " ";
                countExpr++;
            }
            if (HasDays) {
                writer.Write(delimiter);
                Children[countExpr].ToEPL(writer, ExpressionPrecedenceEnum.MINIMUM);
                writer.Write(" days");
                delimiter = " ";
                countExpr++;
            }
            if (HasHours) {
                writer.Write(delimiter);
                Children[countExpr].ToEPL(writer, ExpressionPrecedenceEnum.MINIMUM);
                writer.Write(" hours");
                delimiter = " ";
                countExpr++;
            }
            if (HasMinutes) {
                writer.Write(delimiter);
                Children[countExpr].ToEPL(writer, ExpressionPrecedenceEnum.MINIMUM);
                writer.Write(" minutes");
                delimiter = " ";
                countExpr++;
            }
            if (HasSeconds) {
                writer.Write(delimiter);
                Children[countExpr].ToEPL(writer, ExpressionPrecedenceEnum.MINIMUM);
                writer.Write(" seconds");
                delimiter = " ";
                countExpr++;
            }
            if (HasMilliseconds) {
                writer.Write(delimiter);
                Children[countExpr].ToEPL(writer, ExpressionPrecedenceEnum.MINIMUM);
                writer.Write(" milliseconds");
            }
        }
    }
}
