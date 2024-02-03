///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.common.client.soda
{
    /// <summary>
    /// Represents the "window" aggregation function.
    /// </summary>
    public class WindowProjectionExpression : AccessProjectionExpressionBase
    {
        /// <summary>Ctor. </summary>
        public WindowProjectionExpression()
        {
        }

        /// <summary>Ctor. </summary>
        /// <param name="expression">to aggregate</param>
        public WindowProjectionExpression(Expression expression)
            : base(expression)
        {
        }

        public override string AggregationFunctionName => "window";
    }
}