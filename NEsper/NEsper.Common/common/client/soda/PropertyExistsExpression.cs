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
    /// Property-exists checks if a dynamic property exists.
    /// </summary>
    public class PropertyExistsExpression : ExpressionBase
    {
        /// <summary>
        /// Ctor - for use to create an expression tree, without child expression.
        /// </summary>
        public PropertyExistsExpression()
        {
        }

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="propertyName">is the name of the property to check existence</param>
        public PropertyExistsExpression(string propertyName)
        {
            this.Children.Add(Expressions.GetPropExpr(propertyName));
        }

        public override ExpressionPrecedenceEnum Precedence {
            get => ExpressionPrecedenceEnum.UNARY;
        }

        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
            writer.Write("exists(");
            this.Children[0].ToEPL(writer, ExpressionPrecedenceEnum.MINIMUM);
            writer.Write(")");
        }
    }
} // end of namespace