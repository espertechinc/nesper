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
    /// For use in view parameter lists, this is a wrapper expression
    /// that adds an ascending or descending sort indicator to its single child expression.
    /// </summary>
    [Serializable]
    public class OrderedObjectParamExpression : ExpressionBase
    {
        private bool descending;

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
            this.descending = descending;
        }

        /// <summary>
        /// Returns true for descending, false for ascending.
        /// </summary>
        /// <returns>indicator for descending sort</returns>
        public bool IsDescending
        {
            get => descending;
        }

        /// <summary>
        /// Returns true for descending, false for ascending.
        /// </summary>
        /// <returns>indicator for descending sort</returns>
        public bool Descending {
            get => descending;
            set => @descending = value;
        }

        /// <summary>
        /// Return true for descending.
        /// </summary>
        /// <param name="descending">indicator</param>
        public OrderedObjectParamExpression SetDescending(bool descending)
        {
            this.descending = descending;
            return this;
        }

        /// <summary>
        /// Return precedence.
        /// </summary>
        /// <returns>precedence</returns>
        public override ExpressionPrecedenceEnum Precedence
        {
            get => ExpressionPrecedenceEnum.UNARY;
        }

        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
            this.Children[0].ToEPL(writer, ExpressionPrecedenceEnum.MINIMUM);
            if (descending)
            {
                writer.Write(" desc");
            }
            else
            {
                writer.Write(" asc");
            }
        }
    }
} // end of namespace