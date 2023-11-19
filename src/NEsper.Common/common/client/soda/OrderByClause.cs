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
    /// An order-by clause consists of expressions and flags indicating if ascending or descending.
    /// </summary>
    public class OrderByClause
    {
        private IList<OrderByElement> orderByExpressions;

        /// <summary>Create an empty order-by clause.</summary>
        /// <returns>clause</returns>
        public static OrderByClause Create()
        {
            return new OrderByClause();
        }

        /// <summary>Create an order-by clause.</summary>
        /// <param name="properties">is the property names to order by</param>
        /// <returns>clause</returns>
        public static OrderByClause Create(params string[] properties)
        {
            return new OrderByClause(properties);
        }

        /// <summary>Create an order-by clause.</summary>
        /// <param name="expressions">is the expressios returning values to order by</param>
        /// <returns>clause</returns>
        public static OrderByClause Create(params Expression[] expressions)
        {
            return new OrderByClause(expressions);
        }

        /// <summary>Adds a property and flag.</summary>
        /// <param name="property">is the name of the property to add</param>
        /// <param name="isDescending">true for descending, false for ascending sort</param>
        /// <returns>clause</returns>
        public OrderByClause Add(
            string property,
            bool isDescending)
        {
            orderByExpressions.Add(new OrderByElement(Expressions.GetPropExpr(property), isDescending));
            return this;
        }

        /// <summary>Adds an expression and flag.</summary>
        /// <param name="expression">returns values to order by</param>
        /// <param name="isDescending">true for descending, false for ascending sort</param>
        /// <returns>clause</returns>
        public OrderByClause Add(
            Expression expression,
            bool isDescending)
        {
            orderByExpressions.Add(new OrderByElement(expression, isDescending));
            return this;
        }

        /// <summary>Ctor.</summary>
        public OrderByClause()
        {
            orderByExpressions = new List<OrderByElement>();
        }

        /// <summary>Ctor.</summary>
        /// <param name="properties">property names</param>
        public OrderByClause(params string[] properties)
            : this()
        {
            for (var i = 0; i < properties.Length; i++) {
                orderByExpressions.Add(new OrderByElement(Expressions.GetPropExpr(properties[i]), false));
            }
        }

        /// <summary>Ctor.</summary>
        /// <param name="expressions">is the expressions</param>
        public OrderByClause(params Expression[] expressions)
            : this()
        {
            for (var i = 0; i < expressions.Length; i++) {
                orderByExpressions.Add(new OrderByElement(expressions[i], false));
            }
        }

        /// <summary>Gets or set a list of expressions and flags to order by.</summary>
        /// <returns>order-by elements</returns>
        public IList<OrderByElement> OrderByExpressions {
            get => orderByExpressions;
            set => orderByExpressions = value;
        }

        /// <summary>Renders the clause in textual representation.</summary>
        /// <param name="writer">to output to</param>
        public void ToEPL(TextWriter writer)
        {
            var delimiter = "";
            foreach (var element in orderByExpressions) {
                writer.Write(delimiter);
                element.ToEPL(writer);
                delimiter = ", ";
            }
        }
    }
} // End of namespace