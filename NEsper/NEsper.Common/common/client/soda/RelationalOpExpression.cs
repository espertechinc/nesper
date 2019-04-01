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
    ///     Comparison using one of the relational operators (=, !=, &lt;, &lt;=, &gt;, &gt;=, is, is not).
    /// </summary>
    [Serializable]
    public class RelationalOpExpression : ExpressionBase
    {
        private string @operator;

        /// <summary>
        ///     Ctor.
        /// </summary>
        public RelationalOpExpression()
        {
        }

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="operator">is the relational operator.</param>
        public RelationalOpExpression(string @operator)
        {
            this.@operator = @operator.Trim();
        }

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="left">provides a value to compare against</param>
        /// <param name="operator">is the operator to use</param>
        /// <param name="right">provides a value to compare against</param>
        public RelationalOpExpression(Expression left, string @operator, Expression right)
        {
            this.@operator = @operator.Trim();
            AddChild(left);

            if (right != null) {
                AddChild(right);
            }
            else {
                AddChild(new ConstantExpression(null));
            }
        }

        /// <summary>
        ///     Returns the operator to use.
        /// </summary>
        /// <returns>operator.</returns>
        public string Operator {
            get => @operator;
            set => @operator = value;
        }

        public override ExpressionPrecedenceEnum Precedence {
            get {
                if (@operator.Equals("=")) {
                    return ExpressionPrecedenceEnum.EQUALS;
                }

                return ExpressionPrecedenceEnum.RELATIONAL_BETWEEN_IN;
            }
        }

        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
            Children[0].ToEPL(writer, Precedence);
            if (@operator.ToLowerInvariant().Trim().Equals("is") ||
                @operator.ToLowerInvariant().Trim().Equals("is not")) {
                writer.Write(' ');
                writer.Write(@operator);
                writer.Write(' ');
            }
            else {
                writer.Write(@operator);
            }

            Children[1].ToEPL(writer, Precedence);
        }
    }
} // end of namespace