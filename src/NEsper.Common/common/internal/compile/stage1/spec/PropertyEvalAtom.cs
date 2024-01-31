///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.epl.expression.core;

namespace com.espertech.esper.common.@internal.compile.stage1.spec
{
    /// <summary>
    /// Atom in a specification for property evaluation.
    /// </summary>
    public class PropertyEvalAtom
    {
        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="splitterExpression">The splitter expression.</param>
        /// <param name="optionalResultEventType">Type of the optional result event.</param>
        /// <param name="optionalAsName">column name assigned, if any</param>
        /// <param name="optionalSelectClause">select clause, if any</param>
        /// <param name="optionalWhereClause">where clause, if any</param>
        public PropertyEvalAtom(
            ExprNode splitterExpression,
            string optionalResultEventType,
            string optionalAsName,
            SelectClauseSpecRaw optionalSelectClause,
            ExprNode optionalWhereClause)
        {
            SplitterExpression = splitterExpression;
            OptionalResultEventType = optionalResultEventType;
            OptionalAsName = optionalAsName;
            OptionalSelectClause = optionalSelectClause;
            OptionalWhereClause = optionalWhereClause;
        }

        /// <summary>Returns the column name if assigned. </summary>
        /// <value>column name</value>
        public string OptionalAsName { get; private set; }

        /// <summary>Returns the select clause if specified. </summary>
        /// <value>select clause</value>
        public SelectClauseSpecRaw OptionalSelectClause { get; private set; }

        /// <summary>Returns the where clause, if specified. </summary>
        /// <value>filter expression</value>
        public ExprNode OptionalWhereClause { get; private set; }

        public ExprNode SplitterExpression { get; private set; }

        public string OptionalResultEventType { get; private set; }
    }
}