///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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
    /// Parameter expression for use in crontab expressions and representing a range.
    /// </summary>
    [Serializable]
    public class CrontabRangeExpression : ExpressionBase
    {
        /// <summary>Ctor. </summary>
        public CrontabRangeExpression()
        {
        }

        /// <summary>Ctor. </summary>
        /// <param name="lowerBounds">provides lower bounds</param>
        /// <param name="upperBounds">provides upper bounds</param>
        public CrontabRangeExpression(Expression lowerBounds,
                                      Expression upperBounds)
        {
            Children.Add(lowerBounds);
            Children.Add(upperBounds);
        }

        public override ExpressionPrecedenceEnum Precedence
        {
            get { return ExpressionPrecedenceEnum.UNARY; }
        }

        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
            Children[0].ToEPL(writer, ExpressionPrecedenceEnum.MINIMUM);
            writer.Write(":");
            Children[1].ToEPL(writer, ExpressionPrecedenceEnum.MINIMUM);
        }
    }
}