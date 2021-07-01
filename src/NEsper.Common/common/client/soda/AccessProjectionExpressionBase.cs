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
    ///     Represents the base expression for "first", "last" and "window" aggregation functions.
    /// </summary>
    [Serializable]
    public abstract class AccessProjectionExpressionBase : ExpressionBase
    {
        /// <summary>
        ///     Ctor.
        /// </summary>
        public AccessProjectionExpressionBase()
        {
        }

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="expression">to aggregate</param>
        public AccessProjectionExpressionBase(Expression expression)
        {
            Children.Add(expression);
        }

        /// <summary>
        ///     Returns the function name of the aggregation function.
        /// </summary>
        /// <returns>function name</returns>
        public abstract string AggregationFunctionName { get; }

        public override ExpressionPrecedenceEnum Precedence => ExpressionPrecedenceEnum.UNARY;

        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
            writer.Write(AggregationFunctionName);
            writer.Write('(');
            var delimiter = "";
            var children = Children;
            if (children.Count > 0)
            {
                writer.Write(delimiter);
                children[0].ToEPL(writer, ExpressionPrecedenceEnum.MINIMUM);
                for (var i = 1; i < children.Count; i++)
                {
                    writer.Write(",");
                    children[i].ToEPL(writer, ExpressionPrecedenceEnum.MINIMUM);
                }
            }

            writer.Write(")");
        }
    }
} // end of namespace