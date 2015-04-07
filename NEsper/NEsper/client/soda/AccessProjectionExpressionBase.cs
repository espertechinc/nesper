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
    /// Represents the base expression for "first", "last" and "window" aggregation functions.
    /// </summary>
    [Serializable]
    public abstract class AccessProjectionExpressionBase : ExpressionBase
    {
        /// <summary>
        /// Ctor.
        /// </summary>
        protected AccessProjectionExpressionBase()
        {
        }

        /// <summary>
        /// Returns the function name of the aggregation function.
        /// </summary>
        /// <value>function name</value>
        public abstract string AggregationFunctionName { get; }

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="expression">to aggregate</param>
        protected AccessProjectionExpressionBase(Expression expression)
        {
            this.Children.Add(expression);
        }

        /// <summary>
        /// Gets the precedence.
        /// </summary>
        /// <value>
        /// The Precedence.
        /// </value>
        public override ExpressionPrecedenceEnum Precedence
        {
            get { return ExpressionPrecedenceEnum.UNARY; }
        }

        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
            writer.Write(AggregationFunctionName);
            writer.Write('(');
            var delimiter = "";
            var children = this.Children;
            if (children.Count > 0)
            {
                writer.Write(delimiter);
                children[0].ToEPL(writer, ExpressionPrecedenceEnum.MINIMUM);
                for(int i=1; i<children.Count; i++) {
                    writer.Write(",");
                    children[i].ToEPL(writer, ExpressionPrecedenceEnum.MINIMUM);
                }
            }
            writer.Write(")");
        }
    }
}
