///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.client.soda
{
    /// <summary>
    /// Median projection (aggregation) in the distinct and regular form.
    /// </summary>
    public class MedianProjectionExpression : ExpressionBase
    {
        private bool distinct;

        /// <summary>
        /// Ctor.
        /// </summary>
        public MedianProjectionExpression()
        {
        }

        /// <summary>
        /// Ctor - for use to create an expression tree, without inner expression
        /// </summary>
        /// <param name="isDistinct">true if distinct</param>
        public MedianProjectionExpression(bool isDistinct)
        {
            this.distinct = isDistinct;
        }

        /// <summary>
        /// Ctor - adds the expression to project.
        /// </summary>
        /// <param name="expression">returning values to project</param>
        /// <param name="isDistinct">true if distinct</param>
        public MedianProjectionExpression(
            Expression expression,
            bool isDistinct)
        {
            this.distinct = isDistinct;
            this.Children.Add(expression);
        }

        public override ExpressionPrecedenceEnum Precedence {
            get => ExpressionPrecedenceEnum.UNARY;
        }

        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
            ExpressionBase.RenderAggregation(writer, "median", distinct, this.Children);
        }

        /// <summary>
        /// Returns true if the projection considers distinct values only.
        /// </summary>
        /// <returns>true if distinct</returns>
        public bool IsDistinct {
            get => distinct;
        }

        /// <summary>
        /// Returns true if the projection considers distinct values only.
        /// </summary>
        /// <returns>true if distinct</returns>
        public bool Distinct {
            get => distinct;
        }

        /// <summary>
        /// Set the distinct flag indicating the projection considers distinct values only.
        /// </summary>
        /// <param name="distinct">true for distinct, false for not distinct</param>
        public void SetDistinct(bool distinct)
        {
            this.distinct = distinct;
        }
    }
} // end of namespace