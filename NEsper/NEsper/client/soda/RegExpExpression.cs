///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
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
    /// Regular expression evaluates a "regexp" regular expression.
    /// </summary>
    [Serializable]
    public class RegExpExpression : ExpressionBase
    {
        /// <summary>
        /// Ctor - for use to create an expression tree, without child expression.
        /// </summary>
        /// <param name="isNot">true for negated regex</param>
        public RegExpExpression(bool isNot)
        {
            IsNot = isNot;
        }

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="left">provides values to match against regexp string</param>
        /// <param name="right">provides the regexp string</param>
        /// <param name="isNot">true for negated regex</param>
        public RegExpExpression(Expression left, Expression right, bool isNot)
            : this(left, right, null, isNot)
        {
        }

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="left">provides values to match against regexp string</param>
        /// <param name="right">provides the regexp string</param>
        /// <param name="escape">provides the escape character</param>
        /// <param name="isNot">true for negated regex</param>
        public RegExpExpression(Expression left, Expression right, Expression escape, bool isNot)
        {
            Children.Add(left);
            Children.Add(right);
            if (escape != null) {
                Children.Add(escape);
            }
            
            IsNot = isNot;
        }

        /// <summary>
        /// Ctor - for use to create an expression tree, without child expression.
        /// </summary>
        public RegExpExpression()
        {
            IsNot = false;
        }

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="left">provides values to match against regexp string</param>
        /// <param name="right">provides the regexp string</param>
        public RegExpExpression(Expression left, Expression right)
            : this(left, right, null)
        {
        }

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="left">provides values to match against regexp string</param>
        /// <param name="right">provides the regexp string</param>
        /// <param name="escape">provides the escape character</param>
        public RegExpExpression(Expression left, Expression right, Expression escape)
        {
            Children.Add(left);
            Children.Add(right);
            if (escape != null) {
                Children.Add(escape);
            }
            IsNot = false;
        }

        /// <summary>
        /// Returns true if negated.
        /// </summary>
        /// <returns>
        /// indicator whether negated
        /// </returns>
        public bool IsNot { get; private set; }

        public override ExpressionPrecedenceEnum Precedence
        {
            get { return ExpressionPrecedenceEnum.RELATIONAL_BETWEEN_IN; }
        }

        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
            Children[0].ToEPL(writer, Precedence);
            if (IsNot)
            {
                writer.Write(" not");
            }
            writer.Write(" regexp ");
            Children[1].ToEPL(writer, ExpressionPrecedenceEnum.MINIMUM);

            if (Children.Count > 2)
            {
                writer.Write(" escape ");
                Children[2].ToEPL(writer, ExpressionPrecedenceEnum.MINIMUM);
            }
        }
    }
}
