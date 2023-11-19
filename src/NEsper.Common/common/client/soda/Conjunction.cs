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
    ///     Conjunction represents a logical AND allowing multiple sub-expressions to be connected by AND.
    /// </summary>
    public class Conjunction : Junction
    {
        /// <summary>
        ///     Ctor - for use to create an expression tree, without child expression.
        ///     <para />
        ///     Use add methods to add child expressions to acts upon.
        /// </summary>
        public Conjunction()
        {
        }

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="first">provides value to AND</param>
        /// <param name="second">provides value to AND</param>
        /// <param name="expressions">is more expressions to put in the AND-relationship.</param>
        public Conjunction(
            Expression first,
            Expression second,
            params Expression[] expressions)
        {
            AddChild(first);
            AddChild(second);
            for (var i = 0; i < expressions.Length; i++) {
                AddChild(expressions[i]);
            }
        }

        public override ExpressionPrecedenceEnum Precedence => ExpressionPrecedenceEnum.AND;

        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
            var delimiter = "";
            foreach (var child in Children) {
                writer.Write(delimiter);
                child.ToEPL(writer, Precedence);
                delimiter = " and ";
            }
        }
    }
} // end of namespace