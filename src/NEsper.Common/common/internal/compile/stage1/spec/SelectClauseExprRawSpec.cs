///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.util;

namespace com.espertech.esper.common.@internal.compile.stage1.spec
{
    /// <summary>
    ///     Represents a single item in a SELECT-clause, potentially unnamed
    ///     as no "as" tag may have been supplied in the syntax.
    ///     <para />
    ///     Compare to <seealso cref="SelectClauseExprCompiledSpec" /> which carries a determined name.
    /// </summary>
    [Serializable]
    public class SelectClauseExprRawSpec : SelectClauseElementRaw
    {
        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="selectExpression">the expression node to evaluate for matching events</param>
        /// <param name="optionalAsName">the name of the item, null if not name supplied</param>
        /// <param name="isEvents">whether event selected</param>
        public SelectClauseExprRawSpec(
            ExprNode selectExpression,
            string optionalAsName,
            bool isEvents)
        {
            SelectExpression = selectExpression;
            OptionalAsName = optionalAsName == null ? null : StringValue.RemoveTicks(optionalAsName);
            IsEvents = isEvents;
        }

        /// <summary>
        ///     Returns the expression node representing the item in the select clause.
        /// </summary>
        /// <returns>expression node for item</returns>
        public ExprNode SelectExpression { get; }

        /// <summary>
        ///     Returns the name of the item in the select clause.
        /// </summary>
        /// <returns>name of item</returns>
        public string OptionalAsName { get; }

        public bool IsEvents { get; }
    }
} // end of namespace