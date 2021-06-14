///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

using com.espertech.esper.compat;

namespace com.espertech.esper.common.client.soda
{
    /// <summary>
    ///     Case expression that act as a when-then-else.
    /// </summary>
    [Serializable]
    public class CaseWhenThenExpression : ExpressionBase
    {
        public override ExpressionPrecedenceEnum Precedence => ExpressionPrecedenceEnum.CASE;

        /// <summary>
        ///     Adds a when-then pair of expressions.
        /// </summary>
        /// <param name="when">providings conditions to evaluate</param>
        /// <param name="then">provides the result when a condition evaluates to true</param>
        /// <returns>expression</returns>
        public CaseWhenThenExpression Add(
            Expression when,
            Expression then)
        {
            var size = Children.Count;
            if (size % 2 == 0)
            {
                AddChild(when);
                AddChild(then);
            }
            else
            {
                // add next to last as the last node is the else clause
                Children.Insert(Children.Count - 1, when);
                Children.Insert(Children.Count - 1, then);
            }

            return this;
        }

        /// <summary>
        ///     Sets the expression to provide a value when no when-condition matches.
        /// </summary>
        /// <param name="elseExpr">expression providing default result</param>
        /// <returns>expression</returns>
        public CaseWhenThenExpression SetElse(Expression elseExpr)
        {
            var size = Children.Count;
            // remove last node representing the else
            if (size % 2 != 0)
            {
                Children.RemoveAt(size - 1);
            }

            AddChild(elseExpr);
            return this;
        }

        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
            writer.Write("case");
            var index = 0;
            while (index < Children.Count - 1)
            {
                writer.Write(" when ");
                Children[index].ToEPL(writer, ExpressionPrecedenceEnum.MINIMUM);
                index++;
                if (index == Children.Count)
                {
                    throw new IllegalStateException(
                        "Invalid case-when expression, count of when-to-then nodes not matching");
                }

                writer.Write(" then ");
                Children[index].ToEPL(writer, ExpressionPrecedenceEnum.MINIMUM);
                index++;
            }

            if (index < Children.Count)
            {
                writer.Write(" else ");
                Children[index].ToEPL(writer, ExpressionPrecedenceEnum.MINIMUM);
            }

            writer.Write(" end");
        }
    }
} // end of namespace