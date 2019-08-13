///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
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
    /// Specification for the Update clause.
    /// </summary>
    [Serializable]
    public class UpdateClause
    {
        /// <summary>
        /// Ctor.
        /// </summary>
        public UpdateClause()
        {
        }

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="eventType">the name of the type to Update</param>
        /// <param name="expression">expression returning a value to write</param>
        /// <returns>Update clause</returns>
        public static UpdateClause Create(
            String eventType,
            Expression expression)
        {
            var clause = new UpdateClause(eventType, null);
            clause.AddAssignment(expression);
            return clause;
        }

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="eventType">the name of the type to Update</param>
        /// <param name="optionalAsClauseStreamName">as-clause for Update, if any</param>
        public UpdateClause(
            String eventType,
            String optionalAsClauseStreamName)
        {
            EventType = eventType;
            OptionalAsClauseStreamName = optionalAsClauseStreamName;
            Assignments = new List<Assignment>();
        }

        /// <summary>
        /// Adds a property to set to the clause.
        /// </summary>
        /// <param name="expression">expression providing the new property value</param>
        /// <returns>clause</returns>
        public UpdateClause AddAssignment(Expression expression)
        {
            Assignments.Add(new Assignment(expression));
            return this;
        }

        /// <summary>
        /// Returns the list of property assignments.
        /// </summary>
        /// <value>pair of property name and expression</value>
        public IList<Assignment> Assignments { get; set; }

        /// <summary>
        /// Returns the name of the event type to Update.
        /// </summary>
        /// <value>name of type</value>
        public string EventType { get; set; }

        /// <summary>
        /// Returns the where-clause if any.
        /// </summary>
        /// <value>where clause</value>
        public Expression OptionalWhereClause { get; set; }

        /// <summary>
        /// Returns the stream name.
        /// </summary>
        /// <value>stream name</value>
        public string OptionalAsClauseStreamName { get; set; }

        /// <summary>
        /// Renders the clause in EPL.
        /// </summary>
        /// <param name="writer">to output to</param>
        public void ToEPL(TextWriter writer)
        {
            writer.Write("update istream ");
            writer.Write(EventType);
            if (OptionalAsClauseStreamName != null)
            {
                writer.Write(" as ");
                writer.Write(OptionalAsClauseStreamName);
            }

            writer.Write(" ");
            RenderEPLAssignments(writer, Assignments);

            if (OptionalWhereClause != null)
            {
                writer.Write(" where ");
                OptionalWhereClause.ToEPL(writer, ExpressionPrecedenceEnum.MINIMUM);
            }
        }

        /// <summary>
        /// Write assignments.
        /// </summary>
        /// <param name="writer">to write to</param>
        /// <param name="assignments">to write</param>
        public static void RenderEPLAssignments(
            TextWriter writer,
            IList<Assignment> assignments)
        {
            writer.Write("set ");
            String delimiter = "";
            foreach (Assignment pair in assignments)
            {
                writer.Write(delimiter);
                pair.Value.ToEPL(writer, ExpressionPrecedenceEnum.MINIMUM);
                delimiter = ", ";
            }
        }
    }
}