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
    /// Grouping_id-function for use with rollup, cube or grouping sets.
    /// </summary>
    [Serializable]
    public class GroupingIdExpression : ExpressionBase
    {
        /// <summary>
        /// Ctor - for use to create an expression tree, without child expression.
        /// <para />Use add methods to add child expressions to acts upon.
        /// </summary>
        public GroupingIdExpression()
        {
        }

        public override ExpressionPrecedenceEnum Precedence => ExpressionPrecedenceEnum.UNARY;

        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
            ToPrecedenceFreeEPL("grouping_id", Children, writer);
        }
    }
} // end of namespace