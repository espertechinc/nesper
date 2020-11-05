///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
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
    /// A single entry in an order-by clause consisting of an expression and order ascending or descending flag.
    /// </summary>
    [Serializable]
    public class OrderByElement
    {
        private Expression expression;
        private bool descending;

        /// <summary>
        /// Ctor.
        /// </summary>
        public OrderByElement()
        {
        }

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="expression">is the expression to order by</param>
        /// <param name="descending">true for descending sort, false for ascending sort</param>
        public OrderByElement(
            Expression expression,
            bool descending)
        {
            this.expression = expression;
            this.descending = descending;
        }

        /// <summary>
        /// Returns the order-by value expression.
        /// </summary>
        /// <returns>expression</returns>
        public Expression Expression
        {
            get => expression;
            set => expression = value;
        }

        /// <summary>
        /// Returns true for descending sorts for this column, false for ascending sort.
        /// </summary>
        /// <returns>true for descending sort</returns>
        public bool IsDescending
        {
            get => descending;
            set => descending = value;
        }

        /// <summary>
        /// Renders the clause in textual representation.
        /// </summary>
        /// <param name="writer">to output to</param>
        public void ToEPL(TextWriter writer)
        {
            expression.ToEPL(writer, ExpressionPrecedenceEnum.MINIMUM);
            if (descending)
            {
                writer.Write(" desc");
            }
        }
    }
} // end of namespace