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
    /// Logical AND for use in pattern expressions.
    /// </summary>
    [Serializable]
    public class PatternAndExpr : PatternExprBase
    {
        /// <summary>
        /// Ctor - for use to create a pattern expression tree, without pattern child expression.
        /// </summary>
        public PatternAndExpr()
        {
        }

        /// <summary>Ctor. </summary>
        /// <param name="first">a first pattern expression in the AND relationship</param>
        /// <param name="second">a second pattern expression in the AND relationship</param>
        /// <param name="patternExprs">further optional pattern expressions in the AND relationship</param>
        public PatternAndExpr(
            PatternExpr first,
            PatternExpr second,
            params PatternExpr[] patternExprs)
        {
            AddChild(first);
            AddChild(second);
            for (var i = 0; i < patternExprs.Length; i++) {
                AddChild(patternExprs[i]);
            }
        }

        /// <summary>Adds a pattern expression to the AND relationship between patterns. </summary>
        /// <param name="expr">to add</param>
        /// <returns>pattern expression</returns>
        public PatternAndExpr Add(PatternExpr expr)
        {
            Children.Add(expr);
            return this;
        }

        public override PatternExprPrecedenceEnum Precedence => PatternExprPrecedenceEnum.AND;

        public override void ToPrecedenceFreeEPL(
            TextWriter writer,
            EPStatementFormatter formatter)
        {
            var delimiter = "";
            foreach (var child in Children) {
                writer.Write(delimiter);
                child.ToEPL(writer, Precedence, formatter);
                delimiter = " and ";
            }
        }
    }
}