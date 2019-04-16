///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

namespace com.espertech.esper.common.client.soda
{
    /// <summary>
    /// Represents a single expression (non-combined, rollup/cube or grouping set) as part 
    /// of a group-by expression. 
    /// </summary>
    [Serializable]
    public class GroupByClauseExpressionSingle : GroupByClauseExpression
    {
        private Expression _expression;

        /// <summary>Ctor. </summary>
        /// <param name="expression">the expression</param>
        public GroupByClauseExpressionSingle(Expression expression)
        {
            _expression = expression;
        }

        /// <summary>Ctor. </summary>
        public GroupByClauseExpressionSingle()
        {
        }

        /// <summary>Returns the expression. </summary>
        /// <value>expressions</value>
        public Expression Expression {
            get { return _expression; }
            set { _expression = value; }
        }

        public void ToEPL(TextWriter writer)
        {
            _expression.ToEPL(writer, ExpressionPrecedenceEnum.MINIMUM);
        }
    }
}