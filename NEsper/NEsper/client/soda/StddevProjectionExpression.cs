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
    /// Standard deviation of the (distinct) values returned by an expression.
    /// </summary>
    [Serializable]
    public class StddevProjectionExpression : ExpressionBase
    {
        /// <summary>
        /// Ctor.
        /// </summary>
        public StddevProjectionExpression()
        {
        }
    
        /// <summary>
        /// Ctor - for use to create an expression tree, without inner expression
        /// </summary>
        /// <param name="isDistinct">true if distinct</param>
        public StddevProjectionExpression(bool isDistinct)
        {
            IsDistinct = isDistinct;
        }
    
        /// <summary>
        /// Ctor - adds the expression to project.
        /// </summary>
        /// <param name="expression">returning values to project</param>
        /// <param name="isDistinct">true if distinct</param>
        public StddevProjectionExpression(Expression expression, bool isDistinct)
        {
            IsDistinct = isDistinct;
            Children.Add(expression);
        }

        public override ExpressionPrecedenceEnum Precedence
        {
            get { return ExpressionPrecedenceEnum.UNARY; }
        }

        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
            RenderAggregation(writer, "stddev", IsDistinct, Children);
        }

        /// <summary>
        /// Returns true if the projection considers distinct values only.
        /// </summary>
        /// <value>true if distinct</value>
        public bool IsDistinct { get; set; }
    }
}
