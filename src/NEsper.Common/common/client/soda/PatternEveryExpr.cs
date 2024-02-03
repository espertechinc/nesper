///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

namespace com.espertech.esper.common.client.soda
{
    /// <summary>Pattern 'every' expression that controls the lifecycle of pattern sub-expressions. </summary>
    public class PatternEveryExpr : PatternExprBase
    {
        /// <summary>Ctor - for use to create a pattern expression tree, without pattern child expression. </summary>
        public PatternEveryExpr()
        {
        }

        /// <summary>Ctor. </summary>
        /// <param name="inner">is the pattern expression to control lifecycle on</param>
        public PatternEveryExpr(PatternExpr inner)
        {
            AddChild(inner);
        }

        public override PatternExprPrecedenceEnum Precedence => PatternExprPrecedenceEnum.EVERY_NOT;

        public override void ToPrecedenceFreeEPL(
            TextWriter writer,
            EPStatementFormatter formatter)
        {
            writer.Write("every ");
            var precedence = Precedence;
            if (Children[0] is PatternEveryExpr) {
                precedence = PatternExprPrecedenceEnum.MAXIMIM;
            }

            Children[0].ToEPL(writer, precedence, formatter);
        }
    }
}