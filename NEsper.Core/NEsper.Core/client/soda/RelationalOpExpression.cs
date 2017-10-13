///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

namespace com.espertech.esper.client.soda
{
    /// <summary>
    /// Comparison using one of the relational operators (=, !=, &lt;, &lt;=, &gt;, &gt;=).
    /// </summary>
    [Serializable]
    public class RelationalOpExpression : ExpressionBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RelationalOpExpression"/> class.
        /// </summary>
        public RelationalOpExpression()
        {
        }

        /// <summary>Ctor.</summary>
        /// <param name="operator">is the relational operator.</param>
        public RelationalOpExpression(String @operator)
        {
            this.Operator = @operator.Trim();
        }

        /// <summary>Ctor.</summary>
        /// <param name="left">provides a value to compare against</param>
        /// <param name="operator">is the operator to use</param>
        /// <param name="right">provides a value to compare against</param>
        public RelationalOpExpression(Expression left, String @operator, Expression right)
        {
            Operator = @operator.Trim();
            AddChild(left);

            if (right != null)
            {
                AddChild(right);
            }
            else
            {
                AddChild(new ConstantExpression(null));
            }
        }

        /// <summary>Gets or sets the operator to use.</summary>
        /// <returns>operator.</returns>
        public string Operator { get; set; }

        public override ExpressionPrecedenceEnum Precedence
        {
            get
            {
                if (Operator == "=")
                {
                    return ExpressionPrecedenceEnum.EQUALS;
                }
                return ExpressionPrecedenceEnum.RELATIONAL_BETWEEN_IN;
            }
        }

        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
            Children[0].ToEPL(writer, Precedence);
            if (Operator.ToLower().Trim().Equals("is") ||
                Operator.ToLower().Trim().Equals("is not"))
            {
                writer.Write(' ');
                writer.Write(Operator);
                writer.Write(' ');
            }
            else
            {
                writer.Write(Operator);
            }

            Children[1].ToEPL(writer, Precedence);
        }
    }
} // End of namespace
