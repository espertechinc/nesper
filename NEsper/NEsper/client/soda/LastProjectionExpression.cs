///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.client.soda
{
    /// <summary>
    /// Represents the "last" aggregation function.
    /// </summary>
    [Serializable]
    public class LastProjectionExpression 
        : AccessProjectionExpressionBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LastProjectionExpression"/> class.
        /// </summary>
        public LastProjectionExpression()
        {
        }

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="expression">to aggregate</param>
        public LastProjectionExpression(Expression expression)
        {
            Children.Add(expression);
        }

        /// <summary>
        /// Returns the function name of the aggregation function.
        /// </summary>
        /// <value>function name</value>
        public override string AggregationFunctionName
        {
            get { return "last"; }
        }
    }
}
