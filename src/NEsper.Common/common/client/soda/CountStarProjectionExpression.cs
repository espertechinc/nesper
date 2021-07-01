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
    /// Count of (distinct) rows, equivalent to "count(*)"
    /// </summary>
    [Serializable]
    public class CountStarProjectionExpression : ExpressionBase
    {
        /// <summary>
        /// Ctor - for use to create an expression tree, without inner expression.
        /// </summary>
        public CountStarProjectionExpression()
        {
        }

        public override ExpressionPrecedenceEnum Precedence
        {
            get => ExpressionPrecedenceEnum.UNARY;
        }

        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
            RenderAggregation(writer, "count", false, Children);
        }
    }
} // end of namespace