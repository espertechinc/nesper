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
    /// An expression for use in crontab provides all child expression as part of a parameter list.
    /// </summary>
    public class CrontabParameterSetExpression : ExpressionBase
    {
        /// <summary>
        /// Ctor.
        /// </summary>
        public CrontabParameterSetExpression()
        {
        }

        public override ExpressionPrecedenceEnum Precedence => ExpressionPrecedenceEnum.UNARY;

        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
            var delimiter = "";
            writer.Write("[");
            foreach (var expr in Children) {
                writer.Write(delimiter);
                expr.ToEPL(writer, ExpressionPrecedenceEnum.MINIMUM);
                delimiter = ",";
            }

            writer.Write("]");
        }
    }
} // end of namespace