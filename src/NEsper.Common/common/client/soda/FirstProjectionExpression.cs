///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.common.client.soda
{
    /// <summary>
    /// Represents the "first" aggregation function.
    /// </summary>
    [Serializable]
    public class FirstProjectionExpression : AccessProjectionExpressionBase
    {
        /// <summary>
        /// Ctor.
        /// </summary>
        public FirstProjectionExpression()
        {
        }

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="expression">to aggregate</param>
        public FirstProjectionExpression(Expression expression)
            : base(expression)
        {
        }

        public override string AggregationFunctionName => "first";
    }
} // end of namespace