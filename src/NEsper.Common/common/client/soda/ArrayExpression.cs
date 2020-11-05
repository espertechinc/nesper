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
    /// Array expression forms array results, similar to the syntax of "{element 1, element 2, ... element n}".
    /// </summary>
    [Serializable]
    public class ArrayExpression : ExpressionBase
    {
        /// <summary>
        /// Ctor.
        /// </summary>
        public ArrayExpression()
        {
        }

        /// <summary>
        /// Add a property to the expression.
        /// </summary>
        /// <param name="property">to add</param>
        /// <returns>expression</returns>
        public ArrayExpression Add(string property)
        {
            Children.Add(new PropertyValueExpression(property));
            return this;
        }

        /// <summary>
        /// Add a constant to the expression.
        /// </summary>
        /// <param name="object">constant to add</param>
        /// <returns>expression</returns>
        public ArrayExpression Add(object @object)
        {
            Children.Add(new ConstantExpression(@object));
            return this;
        }

        /// <summary>
        /// Add an expression representing an array element to the expression.
        /// </summary>
        /// <param name="expression">to add</param>
        /// <returns>expression</returns>
        public ArrayExpression Add(Expression expression)
        {
            Children.Add(expression);
            return this;
        }

        public override ExpressionPrecedenceEnum Precedence
        {
            get => ExpressionPrecedenceEnum.UNARY;
        }

        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
            writer.Write("{");
            bool isFirst = true;
            foreach (Expression child in Children)
            {
                if (!isFirst)
                {
                    writer.Write(",");
                }

                child.ToEPL(writer, ExpressionPrecedenceEnum.MINIMUM);
                isFirst = false;
            }

            writer.Write("}");
        }
    }
} // end of namespace