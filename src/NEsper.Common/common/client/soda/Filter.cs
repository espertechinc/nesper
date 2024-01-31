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

namespace com.espertech.esper.common.client.soda
{
    /// <summary>
    /// Filter defines the event type to be filtered for, and an optional expression that returns true if
    /// the filter should consider the event, or false to reject the event.
    /// </summary>
    public class Filter
    {
        private string eventTypeName;
        private Expression filter;
        private IList<ContainedEventSelect> optionalPropertySelects;

        /// <summary>
        /// Ctor.
        /// </summary>
        public Filter()
        {
        }

        /// <summary>
        /// Creates a filter to the given named event type.
        /// </summary>
        /// <param name="eventTypeName">is the event type name to filter for</param>
        /// <returns>filter</returns>
        public static Filter Create(string eventTypeName)
        {
            return new Filter(eventTypeName);
        }

        /// <summary>
        /// Creates a filter to the given named event type and filter expression.
        /// </summary>
        /// <param name="eventTypeName">is the event type name to filter for</param>
        /// <param name="filter">is the expression filtering out events</param>
        /// <returns>filter is the filter expression</returns>
        public static Filter Create(
            string eventTypeName,
            Expression filter)
        {
            return new Filter(eventTypeName, filter);
        }

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="eventTypeName">is the event type name</param>
        public Filter(string eventTypeName)
        {
            this.eventTypeName = eventTypeName;
        }

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="eventTypeName">is the event type name</param>
        /// <param name="filter">is the filter expression</param>
        public Filter(
            string eventTypeName,
            Expression filter)
        {
            this.eventTypeName = eventTypeName;
            this.filter = filter;
        }

        /// <summary>
        /// Returns the name of the event type to filter for.
        /// </summary>
        /// <returns>event type name</returns>
        public string EventTypeName {
            get => eventTypeName;
            set => eventTypeName = value;
        }

        /// <summary>
        /// Returns the optional filter expression that tests the event, or null if no filter expression was defined.
        /// </summary>
        /// <returns>filter expression</returns>
        public Expression FilterExpression {
            get => filter;
            set => filter = value;
        }

        /// <summary>
        /// Returns contained-event spec.
        /// </summary>
        /// <returns>spec</returns>
        public IList<ContainedEventSelect> OptionalPropertySelects {
            get => optionalPropertySelects;
            set => optionalPropertySelects = value;
        }

        /// <summary>
        /// Returns a textual representation of the filter.
        /// </summary>
        /// <param name="writer">to output to</param>
        /// <param name="formatter">for newline-whitespace formatting</param>
        public void ToEPL(
            TextWriter writer,
            EPStatementFormatter formatter)
        {
            writer.Write(eventTypeName);
            if (filter != null) {
                writer.Write('(');
                filter.ToEPL(writer, ExpressionPrecedenceEnum.MINIMUM);
                writer.Write(')');
            }

            if (optionalPropertySelects != null) {
                ContainedEventSelect.ToEPL(writer, formatter, optionalPropertySelects);
            }
        }
    }
} // end of namespace