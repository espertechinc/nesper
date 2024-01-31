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
    /// Pair of expressions with "Equals" operator between.
    /// </summary>
    public class PropertyValueExpressionPair
    {
        /// <summary>Ctor. </summary>
        public PropertyValueExpressionPair()
        {
        }

        /// <summary>Ctor. </summary>
        /// <param name="left">expression</param>
        /// <param name="right">expression</param>
        public PropertyValueExpressionPair(
            PropertyValueExpression left,
            PropertyValueExpression right)
        {
            Left = left;
            Right = right;
        }

        /// <summary>Returns left expr. </summary>
        /// <returns>left</returns>
        public Expression Left { get; set; }

        /// <summary>Returns right side. </summary>
        /// <returns>right side</returns>
        public Expression Right { get; set; }
    }
}