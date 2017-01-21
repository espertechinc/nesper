///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.parse;

namespace com.espertech.esper.epl.spec
{
    /// <summary>
    /// Represents a single item in a SELECT-clause, potentially unnamed as no "as" tag may 
    /// have been supplied in the syntax. 
    /// <para/>
    /// Compare to <seealso cref="SelectClauseExprCompiledSpec" /> which carries a determined name.
    /// </summary>
    [Serializable]
    public class SelectClauseExprRawSpec : SelectClauseElementRaw
    {
        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="selectExpression">the expression node to evaluate for matching events</param>
        /// <param name="optionalAsName">the name of the item, null if not name supplied</param>
        /// <param name="isEvents">if set to <c>true</c> [is events].</param>
        public SelectClauseExprRawSpec(ExprNode selectExpression, String optionalAsName, bool isEvents)
        {
            SelectExpression = selectExpression;
            OptionalAsName = optionalAsName == null ? null : ASTConstantHelper.RemoveTicks(optionalAsName);
            IsEvents = isEvents;
        }

        /// <summary>Returns the expression node representing the item in the select clause. </summary>
        /// <value>expression node for item</value>
        public ExprNode SelectExpression { get; private set; }

        /// <summary>Returns the name of the item in the select clause. </summary>
        /// <value>name of item</value>
        public string OptionalAsName { get; private set; }

        public bool IsEvents { get; private set; }
    }
}
