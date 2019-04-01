///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.IO;

namespace com.espertech.esper.common.client.soda
{
    /// <summary>
    /// A clause to assign new values to variables based on a triggering event arriving.
    /// </summary>
    public class OnSetClause : OnClause
    {
        /// <summary>
        /// Creates a new on-set clause for setting variables, and adds a variable to set.
        /// </summary>
        /// <param name="expression">is the assignment expression providing the new variable value</param>
        /// <returns>on-set clause</returns>
        public static OnSetClause Create(Expression expression)
        {
            var clause = new OnSetClause();
            clause.AddAssignment(expression);
            return clause;
        }

        /// <summary>
        /// Ctor.
        /// </summary>
        public OnSetClause()
        {
            Assignments = new List<Assignment>();
        }

        /// <summary>
        /// Adds a variable to set to the clause.
        /// </summary>
        /// <param name="expression">expression providing the new variable value</param>
        /// <returns>clause</returns>
        public OnSetClause AddAssignment(Expression expression)
        {
            Assignments.Add(new Assignment(expression));
            return this;
        }

        /// <summary>
        /// Returns the list of variable assignments.
        /// </summary>
        /// <value>pair of variable name and expression</value>
        public IList<Assignment> Assignments { get; set; }

        /// <summary>
        /// Renders the clause in EPL.
        /// </summary>
        /// <param name="writer">to output to</param>
        /// <param name="formatter">for NewLine-whitespace formatting</param>
        public void ToEPL(TextWriter writer, EPStatementFormatter formatter)
        {
            formatter.BeginOnSet(writer);
            UpdateClause.RenderEPLAssignments(writer, Assignments);
        }
    }
}
