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
    /// Negates the contained-within subexpression.
    /// <para />Has a single child expression to be negated.
    /// </summary>
    [Serializable]
    public class NotExpression : ExpressionBase
    {
        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="inner">is the expression to negate</param>
        public NotExpression(Expression inner)
        {
            AddChild(inner);
        }

        /// <summary>
        /// Ctor - for use to create an expression tree, without child expression.
        /// </summary>
        public NotExpression()
        {
        }

        public override ExpressionPrecedenceEnum Precedence => ExpressionPrecedenceEnum.NEGATED;

        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
            writer.Write("not ");
            Children[0].ToEPL(writer, Precedence);
        }
    }
} // end of namespace