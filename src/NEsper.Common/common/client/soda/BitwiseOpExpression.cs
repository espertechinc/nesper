///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

using com.espertech.esper.common.@internal.type;

namespace com.espertech.esper.common.client.soda
{
    /// <summary>
    ///     Bitwise (binary) operator for binary AND, binary OR and binary XOR.
    /// </summary>
    public class BitwiseOpExpression : ExpressionBase
    {
        /// <summary>
        ///     Ctor.
        /// </summary>
        public BitwiseOpExpression()
        {
        }

        /// <summary>
        ///     Ctor - for use to create an expression tree, without child expression.
        ///     <para />
        ///     Use add methods to add child expressions to acts upon.
        /// </summary>
        /// <param name="binaryOp">the binary operator</param>
        public BitwiseOpExpression(BitWiseOpEnum binaryOp)
        {
            BinaryOp = binaryOp;
        }

        public override ExpressionPrecedenceEnum Precedence => ExpressionPrecedenceEnum.BITWISE;

        /// <summary>
        ///     Returns the binary operator.
        /// </summary>
        /// <returns>operator</returns>
        public BitWiseOpEnum BinaryOp { get; set; }

        /// <summary>
        ///     Add a property to the expression.
        /// </summary>
        /// <param name="property">to add</param>
        /// <returns>expression</returns>
        public BitwiseOpExpression Add(string property)
        {
            Children.Add(new PropertyValueExpression(property));
            return this;
        }

        /// <summary>
        ///     Add a constant to the expression.
        /// </summary>
        /// <param name="object">constant to add</param>
        /// <returns>expression</returns>
        public BitwiseOpExpression Add(object @object)
        {
            Children.Add(new ConstantExpression(@object));
            return this;
        }

        /// <summary>
        ///     Add an expression to the expression.
        /// </summary>
        /// <param name="expression">to add</param>
        /// <returns>expression</returns>
        public BitwiseOpExpression Add(Expression expression)
        {
            Children.Add(expression);
            return this;
        }

        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
            var isFirst = true;
            foreach (var child in Children) {
                if (!isFirst) {
                    writer.Write(BinaryOp.GetExpressionText());
                }

                child.ToEPL(writer, Precedence);
                isFirst = false;
            }
        }
    }
} // end of namespace