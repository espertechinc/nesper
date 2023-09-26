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
    /// <summary>Not-expression for negating a pattern sub-expression for use in pattern expressions. </summary>
    [Serializable]
    public class PatternNotExpr : PatternExprBase
    {
        /// <summary>Ctor - for use to create a pattern expression tree, without pattern child expression. </summary>
        public PatternNotExpr()
        {
        }

        /// <summary>Ctor. </summary>
        /// <param name="inner">is the pattern expression to negate</param>
        public PatternNotExpr(PatternExpr inner)
        {
            Children.Add(inner);
        }

        public override PatternExprPrecedenceEnum Precedence => PatternExprPrecedenceEnum.EVERY_NOT;

        public override void ToPrecedenceFreeEPL(
            TextWriter writer,
            EPStatementFormatter formatter)
        {
            writer.Write("not ");
            Children[0].ToEPL(writer, Precedence, formatter);
        }
    }
}