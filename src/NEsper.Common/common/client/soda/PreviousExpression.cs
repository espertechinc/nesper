///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
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
    /// Previous function for obtaining property values of previous events.
    /// </summary>
    public class PreviousExpression : ExpressionBase
    {
        private PreviousExpressionType _type = PreviousExpressionType.PREV;

        /// <summary>
        /// Ctor.
        /// </summary>
        public PreviousExpression()
        {
        }

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="expression">provides the index to use</param>
        /// <param name="propertyName">is the name of the property to return the value for</param>
        public PreviousExpression(
            Expression expression,
            string propertyName)
        {
            AddChild(expression);
            AddChild(new PropertyValueExpression(propertyName));
        }

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="index">provides the index</param>
        /// <param name="propertyName">is the name of the property to return the value for</param>
        public PreviousExpression(
            int index,
            string propertyName)
        {
            AddChild(new ConstantExpression(index));
            AddChild(new PropertyValueExpression(propertyName));
        }

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="type">type of previous expression (tail, first, window, count)</param>
        /// <param name="expression">to evaluate</param>
        public PreviousExpression(
            PreviousExpressionType type,
            Expression expression)
        {
            _type = type;
            AddChild(expression);
        }

        public override ExpressionPrecedenceEnum Precedence => ExpressionPrecedenceEnum.UNARY;

        /// <summary>
        /// Returns the type of the previous expression (tail, first, window, count)
        /// </summary>
        /// <returns>type</returns>
        public PreviousExpressionType Type {
            get => _type;
            set => _type = value;
        }

        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
            writer.Write(_type.ToString().ToLowerInvariant());
            writer.Write("(");
            Children[0].ToEPL(writer, ExpressionPrecedenceEnum.MINIMUM);
            if (Children.Count > 1) {
                writer.Write(",");
                Children[1].ToEPL(writer, ExpressionPrecedenceEnum.MINIMUM);
            }

            writer.Write(')');
        }
    }
} // end of namespace