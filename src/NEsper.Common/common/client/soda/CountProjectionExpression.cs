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
    /// Count of the (distinct) values returned by an expression, equivalent to "count(distinct property)"
    /// </summary>
    [Serializable]
    public class CountProjectionExpression : ExpressionBase
    {
        private bool distinct;

        /// <summary>
        /// Ctor.
        /// </summary>
        public CountProjectionExpression()
        {
        }

        /// <summary>
        /// Ctor - for use to create an expression tree, without inner expression
        /// </summary>
        /// <param name="isDistinct">true if distinct</param>
        public CountProjectionExpression(bool isDistinct)
        {
            distinct = isDistinct;
        }

        /// <summary>
        /// Ctor - adds the expression to project.
        /// </summary>
        /// <param name="expression">returning values to project</param>
        /// <param name="isDistinct">true if distinct</param>
        public CountProjectionExpression(
            Expression expression,
            bool isDistinct)
        {
            distinct = isDistinct;
            Children.Add(expression);
        }

        public override ExpressionPrecedenceEnum Precedence => ExpressionPrecedenceEnum.UNARY;

        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
            RenderAggregation(writer, "count", distinct, Children);
        }

        /// <summary>
        /// Returns true if the projection considers distinct values only.
        /// </summary>
        /// <returns>true if distinct</returns>
        public bool IsDistinct => distinct;

        /// <summary>
        /// Returns true if the projection considers distinct values only.
        /// </summary>
        /// <returns>true if distinct</returns>
        public bool Distinct {
            get => distinct;
            set => distinct = value;
        }

        /// <summary>
        /// Set the distinct flag indicating the projection considers distinct values only.
        /// </summary>
        /// <param name="distinct">true for distinct, false for not distinct</param>
        public CountProjectionExpression SetDistinct(bool distinct)
        {
            this.distinct = distinct;
            return this;
        }
    }
} // end of namespace