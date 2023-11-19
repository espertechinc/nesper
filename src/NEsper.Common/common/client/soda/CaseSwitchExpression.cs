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
    ///     Case-expression that acts as a switch testing a value against other values.
    ///     <para/>
    ///     The first child expression provides the value to switch on.
    ///     The following pairs of child expressions provide the "when expression then expression" results.
    ///     The last child expression provides the "else" result.
    /// </summary>
    public class CaseSwitchExpression : ExpressionBase
    {
        /// <summary>
        ///     Ctor - for use to create an expression tree, without inner expression
        /// </summary>
        public CaseSwitchExpression()
        {
        }

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name = "switchValue">is the expression providing the value to switch on</param>
        public CaseSwitchExpression(Expression switchValue)
        {
            // switch value expression is first
            AddChild(switchValue);
        }

        public override ExpressionPrecedenceEnum Precedence => ExpressionPrecedenceEnum.CASE;

        /// <summary>
        ///     Adds a pair of expressions representing a "when" and a "then" in the switch.
        /// </summary>
        /// <param name = "when">expression to match on</param>
        /// <param name = "then">expression to return a conditional result when the when-expression matches</param>
        /// <returns>expression</returns>
        public CaseSwitchExpression Add(
            Expression when,
            Expression then)
        {
            var size = Children.Count;
            if (size % 2 != 0) {
                AddChild(when);
                AddChild(then);
            }
            else {
                // add next to last as the last node is the else clause
                Children.Insert(Children.Count - 1, when);
                Children.Insert(Children.Count - 1, then);
            }

            return this;
        }

        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
            writer.Write("case ");
            Children[0].ToEPL(writer, ExpressionPrecedenceEnum.MINIMUM);
            var index = 1;
            while (index < Children.Count - 1) {
                writer.Write(" when ");
                Children[index].ToEPL(writer, ExpressionPrecedenceEnum.MINIMUM);
                index++;
                if (index == Children.Count) {
                    throw new IllegalStateException(
                        "Invalid case-when expression, count of when-to-then nodes not matching");
                }

                writer.Write(" then ");
                Children[index].ToEPL(writer, ExpressionPrecedenceEnum.MINIMUM);
                index++;
            }

            if (index < Children.Count) {
                writer.Write(" else ");
                Children[index].ToEPL(writer, ExpressionPrecedenceEnum.MINIMUM);
            }

            writer.Write(" end");
        }

        public CaseSwitchExpression SetElse(Expression value)
        {
            var size = Children.Count;
            // remove last node representing the else
            if (size % 2 == 0) {
                Children.RemoveAt(size - 1);
            }

            AddChild(value);
            return this;
        }
    }
} // end of namespace