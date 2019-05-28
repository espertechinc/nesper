///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.client.soda
{
    /// <summary>
    /// Type-of expression return the type name, as a string value, of the events in the stream if passing a stream name or
    /// the fragment event type if passing a property name that results in a fragment event otherwise
    /// the class simple name of the expression result or null if the expression returns a null value.
    /// </summary>
    public class TypeOfExpression : ExpressionBase
    {
        /// <summary>
        /// Ctor.
        /// </summary>
        public TypeOfExpression()
        {
        }

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="expression">for which to return the result type or null if the result is null</param>
        public TypeOfExpression(Expression expression)
        {
            this.Children.Add(expression);
        }

        public override ExpressionPrecedenceEnum Precedence {
            get => ExpressionPrecedenceEnum.UNARY;
        }

        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
            writer.Write("typeof(");
            this.Children[0].ToEPL(writer, ExpressionPrecedenceEnum.MINIMUM);
            writer.Write(")");
        }
    }
} // end of namespace