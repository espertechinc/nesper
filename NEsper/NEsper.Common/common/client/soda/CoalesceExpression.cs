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
    /// Coalesce-function which returns the first non-null value in a list of values.
    /// </summary>
    [Serializable]
    public class CoalesceExpression : ExpressionBase
    {
        /// <summary>
        /// Ctor - for use to create an expression tree, without child expression.
        /// <para />Use add methods to add child expressions to acts upon.
        /// </summary>
        public CoalesceExpression()
        {
        }

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="propertyOne">the first property in the expression</param>
        /// <param name="propertyTwo">the second property in the expression</param>
        /// <param name="moreProperties">optional more properties in the expression</param>
        public CoalesceExpression(
            string propertyOne,
            string propertyTwo,
            params string[] moreProperties)
        {
            AddChild(new PropertyValueExpression(propertyOne));
            AddChild(new PropertyValueExpression(propertyTwo));
            for (int i = 0; i < moreProperties.Length; i++)
            {
                AddChild(new PropertyValueExpression(moreProperties[i]));
            }
        }

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="exprOne">provides the first value in the expression</param>
        /// <param name="exprTwo">provides the second value in the expression</param>
        /// <param name="moreExpressions">optional more expressions that are part of the function</param>
        public CoalesceExpression(
            Expression exprOne,
            Expression exprTwo,
            params Expression[] moreExpressions)
        {
            AddChild(exprOne);
            AddChild(exprTwo);
            for (int i = 0; i < moreExpressions.Length; i++)
            {
                AddChild(moreExpressions[i]);
            }
        }

        /// <summary>
        /// Add a constant to include in the computation.
        /// </summary>
        /// <param name="object">constant to add</param>
        /// <returns>expression</returns>
        public CoalesceExpression Add(object @object)
        {
            Children.Add(new ConstantExpression(@object));
            return this;
        }

        /// <summary>
        /// Add an expression to include in the computation.
        /// </summary>
        /// <param name="expression">to add</param>
        /// <returns>expression</returns>
        public CoalesceExpression Add(Expression expression)
        {
            Children.Add(expression);
            return this;
        }

        /// <summary>
        /// Add a property to include in the computation.
        /// </summary>
        /// <param name="propertyName">is the name of the property</param>
        /// <returns>expression</returns>
        public CoalesceExpression Add(string propertyName)
        {
            Children.Add(new PropertyValueExpression(propertyName));
            return this;
        }

        public override ExpressionPrecedenceEnum Precedence
        {
            get => ExpressionPrecedenceEnum.UNARY;
        }

        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
            ToPrecedenceFreeEPL("coalesce", Children, writer);
        }
    }
} // end of namespace