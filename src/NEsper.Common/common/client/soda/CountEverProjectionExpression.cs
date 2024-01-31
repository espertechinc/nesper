///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
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
    /// Represents the "countever" aggregation function.
    /// </summary>
    public class CountEverProjectionExpression : ExpressionBase
    {
        private bool distinct;

        /// <summary>
        /// Ctor.
        /// </summary>
        public CountEverProjectionExpression()
        {
        }

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="isDistinct">true for distinct</param>
        public CountEverProjectionExpression(bool isDistinct)
        {
            distinct = isDistinct;
        }

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="expression">to aggregate</param>
        /// <param name="isDistinct">true for distinct</param>
        public CountEverProjectionExpression(
            Expression expression,
            bool isDistinct)
        {
            distinct = isDistinct;
            Children.Add(expression);
        }

        public override ExpressionPrecedenceEnum Precedence => ExpressionPrecedenceEnum.UNARY;

        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
            RenderAggregation(writer, "countever", distinct, Children);
        }

        /// <summary>
        /// Returns true for distinct.
        /// </summary>
        /// <returns>boolean indicating distinct or not</returns>
        public bool IsDistinct => distinct;

        /// <summary>
        /// Returns true for distinct.
        /// </summary>
        /// <returns>boolean indicating distinct or not</returns>
        public bool Distinct {
            get => distinct;
            set => distinct = value;
        }

        /// <summary>
        /// Set to true for distinct.
        /// </summary>
        /// <param name="distinct">indicating distinct or not</param>
        public CountEverProjectionExpression SetDistinct(bool distinct)
        {
            this.distinct = distinct;
            return this;
        }
    }
} // end of namespace