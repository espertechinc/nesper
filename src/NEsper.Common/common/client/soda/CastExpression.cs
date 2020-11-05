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
    ///     Cast expression casts the return value of an expression to a specified type.
    /// </summary>
    [Serializable]
    public class CastExpression : ExpressionBase
    {
        /// <summary>
        ///     Ctor.
        /// </summary>
        public CastExpression()
        {
        }

        /// <summary>
        ///     Ctor - for use to create an expression tree, without child expression.
        /// </summary>
        /// <param name="typeName">is the type to cast to: a fully-qualified class name or primitive type name or "string"</param>
        public CastExpression(string typeName)
        {
            TypeName = typeName;
        }

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="expressionToCheck">provides values to cast</param>
        /// <param name="typeName">is the type to cast to: a fully-qualified class names or primitive type names or "string"</param>
        public CastExpression(
            Expression expressionToCheck,
            string typeName)
        {
            Children.Add(expressionToCheck);
            TypeName = typeName;
        }

        public override ExpressionPrecedenceEnum Precedence => ExpressionPrecedenceEnum.UNARY;

        /// <summary>
        ///     Returns the name of the type to cast to.
        /// </summary>
        /// <returns>type name</returns>
        public string TypeName { get; set; }

        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
            writer.Write("cast(");
            Children[0].ToEPL(writer, ExpressionPrecedenceEnum.MINIMUM);
            writer.Write(",");
            writer.Write(TypeName);
            for (var i = 1; i < Children.Count; i++)
            {
                writer.Write(",");
                Children[i].ToEPL(writer, ExpressionPrecedenceEnum.MINIMUM);
            }

            writer.Write(")");
        }

        /// <summary>
        ///     Sets the name of the type to cast to.
        /// </summary>
        /// <param name="typeName">is the name of type to cast to</param>
        public CastExpression SetTypeName(string typeName)
        {
            TypeName = typeName;
            return this;
        }
    }
} // end of namespace