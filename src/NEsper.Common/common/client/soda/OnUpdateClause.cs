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
    /// A clause to Update a named window based on a triggering event arriving and correlated
    /// to the named window events to be updated.
    /// </summary>
    [Serializable]
    public class OnUpdateClause : OnClause
    {
        /// <summary>
        /// Ctor.
        /// </summary>
        public OnUpdateClause()
        {
            Assignments = new List<Assignment>();
        }

        /// <summary>
        /// Creates an on-Update clause.
        /// </summary>
        /// <param name="windowName">is the named window name</param>
        /// <param name="optionalAsName">is the optional as-provided name</param>
        /// <returns>on-Update clause without assignments</returns>
        public static OnUpdateClause Create(
            string windowName,
            string optionalAsName)
        {
            return new OnUpdateClause(windowName, optionalAsName);
        }

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="windowName">is the named window name</param>
        /// <param name="optionalAsName">is the as-provided name of the named window</param>
        public OnUpdateClause(
            string windowName,
            string optionalAsName)
        {
            WindowName = windowName;
            OptionalAsName = optionalAsName;
            Assignments = new List<Assignment>();
        }

        /// <summary>
        /// Renders the clause in textual representation.
        /// </summary>
        /// <param name="writer">to output to</param>
        public void ToEPL(TextWriter writer)
        {
            writer.Write(WindowName);
            if (OptionalAsName != null)
            {
                writer.Write(" as ");
                writer.Write(OptionalAsName);
            }

            writer.Write(" ");
            UpdateClause.RenderEPLAssignments(writer, Assignments);
        }

        /// <summary>
        /// Returns the name of the named window to Update.
        /// </summary>
        /// <value>named window name</value>
        public string WindowName { get; set; }

        /// <summary>Returns the as-provided name for the named window. </summary>
        /// <value>name or null</value>
        public string OptionalAsName { get; set; }

        /// <summary>Adds a variable to set to the clause. </summary>
        /// <param name="expression">expression providing the new variable value</param>
        /// <returns>clause</returns>
        public OnUpdateClause AddAssignment(Expression expression)
        {
            Assignments.Add(new Assignment(expression));
            return this;
        }

        /// <summary>Returns the list of variable assignments. </summary>
        /// <value>pair of variable name and expression</value>
        public IList<Assignment> Assignments { get; set; }
    }
}