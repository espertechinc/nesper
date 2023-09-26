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
    /// Frequency expression for use in crontab expressions.
    /// </summary>
    [Serializable]
    public class CrontabFrequencyExpression : ExpressionBase
    {
        /// <summary>
        /// Ctor.
        /// </summary>
        public CrontabFrequencyExpression()
        {
        }

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="numericParameter">the frequency value</param>
        public CrontabFrequencyExpression(Expression numericParameter)
        {
            Children.Add(numericParameter);
        }

        public override ExpressionPrecedenceEnum Precedence => ExpressionPrecedenceEnum.UNARY;

        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
            writer.Write("*/");
            Children[0].ToEPL(writer, ExpressionPrecedenceEnum.MINIMUM);
        }
    }
} // end of namespace