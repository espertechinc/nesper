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
    /// Previous function for obtaining property values of previous events.
    /// </summary>
    [Serializable]
    public class PreviousExpression : ExpressionBase
    {
        private PreviousExpressionType type = PreviousExpressionType.PREV;

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
            this.AddChild(expression);
            this.AddChild(new PropertyValueExpression(propertyName));
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
            this.AddChild(new ConstantExpression(index));
            this.AddChild(new PropertyValueExpression(propertyName));
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
            this.type = type;
            this.AddChild(expression);
        }

        public override ExpressionPrecedenceEnum Precedence
        {
            get => ExpressionPrecedenceEnum.UNARY;
        }

        /// <summary>
        /// Returns the type of the previous expression (tail, first, window, count)
        /// </summary>
        /// <returns>type</returns>
        public PreviousExpressionType Type
        {
            get => type;
            set => type = value;
        }

        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
            writer.Write(type.ToString().ToLowerInvariant());
            writer.Write("(");
            this.Children[0].ToEPL(writer, ExpressionPrecedenceEnum.MINIMUM);
            if (this.Children.Count > 1)
            {
                writer.Write(",");
                this.Children[1].ToEPL(writer, ExpressionPrecedenceEnum.MINIMUM);
            }

            writer.Write(')');
        }
    }
} // end of namespace