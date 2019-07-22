///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.epl.expression.core;

namespace com.espertech.esper.common.@internal.compile.stage2
{
    /// <summary>
    ///     Represents a single item in a SELECT-clause, with a name assigned either by the
    ///     engine or by the user specifying an "as" tag name.
    /// </summary>
    [Serializable]
    public class SelectClauseExprCompiledSpec : SelectClauseElementCompiled
    {
        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="selectExpression">the expression node to evaluate for matching events</param>
        /// <param name="assignedName">cannot be null as a name is always assigned orsystem-determined</param>
        /// <param name="providedName">Name of the provided.</param>
        /// <param name="isEvents">if set to <c>true</c> [is events].</param>
        public SelectClauseExprCompiledSpec(
            ExprNode selectExpression,
            string assignedName,
            string providedName,
            bool isEvents)
        {
            SelectExpression = selectExpression;
            AssignedName = assignedName;
            ProvidedName = providedName;
            IsEvents = isEvents;
        }

        /// <summary>Returns the expression node representing the item in the select clause. </summary>
        /// <value>expression node for item</value>
        public ExprNode SelectExpression { get; set; }

        /// <summary>Returns the name of the item in the select clause. </summary>
        /// <value>name of item</value>
        public string AssignedName { get; set; }

        public string ProvidedName { get; }

        public bool IsEvents { get; set; }
    }
}