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
    /// Expression representing the prior function.
    /// </summary>
    [Serializable]
    public class PriorExpression : ExpressionBase
    {
        /// <summary>
        /// Ctor - for use to create an expression tree, without child expression.
        /// </summary>
        public PriorExpression()
        {
        }

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="index">is the index of the prior event</param>
        /// <param name="propertyName">is the property to return</param>
        public PriorExpression(
            int index,
            string propertyName)
        {
            AddChild(new ConstantExpression(index));
            AddChild(new PropertyValueExpression(propertyName));
        }

        public override ExpressionPrecedenceEnum Precedence
        {
            get => ExpressionPrecedenceEnum.UNARY;
        }

        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
            writer.Write("prior(");
            Children[0].ToEPL(writer, ExpressionPrecedenceEnum.MINIMUM);
            writer.Write(",");
            Children[1].ToEPL(writer, ExpressionPrecedenceEnum.MINIMUM);
            writer.Write(')');
        }
    }
} // end of namespace