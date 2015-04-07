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
    /// Sum of the (distinct) values returned by an expression.
    /// </summary>
    [Serializable]
    public class SumProjectionExpression : ExpressionBase
    {
        /// <summary>
        /// Ctor.
        /// </summary>
        public SumProjectionExpression()
        {
        }
    
        /// <summary>
        /// Ctor - for use to create an expression tree, without inner expression
        /// </summary>
        /// <param name="isDistinct">true if distinct</param>
        public SumProjectionExpression(bool isDistinct)
        {
            this.IsDistinct = isDistinct;
        }
    
        /// <summary>
        /// Ctor - adds the expression to project.
        /// </summary>
        /// <param name="expression">returning values to project</param>
        /// <param name="isDistinct">true if distinct</param>
        public SumProjectionExpression(Expression expression, bool isDistinct)
        {
            this.IsDistinct = isDistinct;
            this.Children.Add(expression);
        }

        public override ExpressionPrecedenceEnum Precedence
        {
            get { return ExpressionPrecedenceEnum.UNARY; }
        }

        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
            ExpressionBase.RenderAggregation(writer, "sum", IsDistinct, this.Children);
        }

        /// <summary>
        /// Returns true if the projection considers distinct values only.
        /// </summary>
        /// <value>true if distinct</value>
        public bool IsDistinct { get; set; }
    }
}
