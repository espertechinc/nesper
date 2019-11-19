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
    /// <summary>Logical OR for use in pattern expressions. </summary>
    [Serializable]
    public class PatternOrExpr : PatternExprBase
    {
        /// <summary>Ctor - for use to create a pattern expression tree, without pattern child expression. </summary>
        public PatternOrExpr()
        {
        }

        /// <summary>Ctor. </summary>
        /// <param name="first">a first pattern expression in the OR relationship</param>
        /// <param name="second">a second pattern expression in the OR relationship</param>
        /// <param name="patternExprs">further optional pattern expressions in the OR relationship</param>
        public PatternOrExpr(
            PatternExpr first,
            PatternExpr second,
            params PatternExpr[] patternExprs)
        {
            AddChild(first);
            AddChild(second);
            for (int i = 0; i < patternExprs.Length; i++)
            {
                AddChild(patternExprs[i]);
            }
        }

        /// <summary>Adds a pattern expression to the OR relationship between patterns. </summary>
        /// <param name="expr">to add</param>
        /// <returns>pattern expression</returns>
        public PatternOrExpr Add(PatternExpr expr)
        {
            Children.Add(expr);
            return this;
        }

        public override PatternExprPrecedenceEnum Precedence
        {
            get { return PatternExprPrecedenceEnum.OR; }
        }

        public override void ToPrecedenceFreeEPL(
            TextWriter writer,
            EPStatementFormatter formatter)
        {
            string delimiter = "";
            foreach (PatternExpr child in Children)
            {
                writer.Write(delimiter);
                child.ToEPL(writer, Precedence, formatter);
                delimiter = " or ";
            }
        }
    }
}