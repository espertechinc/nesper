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
    /// Represents the "lastever" aggregation function.
    /// </summary>
    [Serializable]
    public class LastEverProjectionExpression : ExpressionBase
    {
        private bool distinct;

        /// <summary>
        /// Ctor.
        /// </summary>
        public LastEverProjectionExpression()
        {
        }

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="isDistinct">true for distinct</param>
        public LastEverProjectionExpression(bool isDistinct)
        {
            this.distinct = isDistinct;
        }

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="expression">to aggregate</param>
        /// <param name="isDistinct">true for distinct</param>
        public LastEverProjectionExpression(
            Expression expression,
            bool isDistinct)
        {
            this.distinct = isDistinct;
            this.Children.Add(expression);
        }

        public override ExpressionPrecedenceEnum Precedence
        {
            get => ExpressionPrecedenceEnum.UNARY;
        }

        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
            ExpressionBase.RenderAggregation(writer, "lastever", distinct, this.Children);
        }

        /// <summary>
        /// Returns true for distinct.
        /// </summary>
        /// <returns>boolean indicating distinct or not</returns>
        public bool IsDistinct
        {
            get => distinct;
        }

        /// <summary>
        /// Returns true for distinct.
        /// </summary>
        /// <returns>boolean indicating distinct or not</returns>
        public bool Distinct
        {
            get => distinct;
        }

        /// <summary>
        /// Set to true for distinct.
        /// </summary>
        /// <param name="distinct">indicating distinct or not</param>
        public void SetDistinct(bool distinct)
        {
            this.distinct = distinct;
        }
    }
} // end of namespace