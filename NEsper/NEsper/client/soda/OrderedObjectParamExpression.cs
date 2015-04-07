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
    /// For use in view parameter lists, this is a wrapper expression that adds an ascending or descending sort indicator to its single child expression.
    /// </summary>
    [Serializable]
    public class OrderedObjectParamExpression : ExpressionBase
    {
        /// <summary>
        /// Ctor.
        /// </summary>
        public OrderedObjectParamExpression()
        {
        }

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="descending">to indicate a descending sort, or false for ascending</param>
        public OrderedObjectParamExpression(bool descending)
        {
            IsDescending = descending;
        }

        /// <summary>
        /// Returns true for descending, false for ascending.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is descending; otherwise, <c>false</c>.
        /// </value>
        /// <returns>indicator for descending sort</returns>
        public bool IsDescending { get; set; }

        /// <summary>
        /// Return Precedence.
        /// </summary>
        /// <value>Precedence</value>
        public override ExpressionPrecedenceEnum Precedence
        {
            get { return ExpressionPrecedenceEnum.UNARY; }
        }

        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
            Children[0].ToEPL(writer, ExpressionPrecedenceEnum.MINIMUM);
            if (IsDescending)
            {
                writer.Write(" desc");
            }
            else
            {
                writer.Write(" asc");
            }
        }
    }
}
