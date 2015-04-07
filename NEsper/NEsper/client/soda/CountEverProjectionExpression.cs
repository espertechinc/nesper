///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.client.soda
{
    /// <summary>
    /// Represents the "countever" aggregation function.
    /// </summary>
    [Serializable]
    public class CountEverProjectionExpression : ExpressionBase
    {
        private bool _distinct;
    
        /// <summary>
        /// Ctor.
        /// </summary>
        public CountEverProjectionExpression() {
        }
    
        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="isDistinct">true for distinct</param>
        public CountEverProjectionExpression(bool isDistinct)
        {
            _distinct = isDistinct;
        }
    
        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="expression">to aggregate</param>
        /// <param name="isDistinct">true for distinct</param>
        public CountEverProjectionExpression(Expression expression, bool isDistinct)
        {
            _distinct = isDistinct;
            Children.Add(expression);
        }

        public override ExpressionPrecedenceEnum Precedence
        {
            get { return ExpressionPrecedenceEnum.UNARY; }
        }

        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
            RenderAggregation(writer, "countever", _distinct, Children);
        }

        /// <summary>
        /// Returns true for distinct.
        /// </summary>
        /// <value>boolean indicating distinct or not</value>
        public bool IsDistinct
        {
            get { return _distinct; }
            set { _distinct = value; }
        }
    }
}
