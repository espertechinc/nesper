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
    /// Regular expression evaluates a "regexp" regular expression.
    /// </summary>
    [Serializable]
    public class RegExpExpression : ExpressionBase
    {
        private bool not;

        /// <summary>
        /// Ctor - for use to create an expression tree, without child expression.
        /// </summary>
        /// <param name="isNot">true for negated regex</param>
        public RegExpExpression(bool isNot)
        {
            this.not = isNot;
        }

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="left">provides values to match against regexp string</param>
        /// <param name="right">provides the regexp string</param>
        /// <param name="isNot">true for negated regex</param>
        public RegExpExpression(
            Expression left,
            Expression right,
            bool isNot)
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
        public RegExpExpression(
            Expression left,
            Expression right,
            Expression escape,
            bool isNot)
        {
            this.Children.Add(left);
            this.Children.Add(right);
            if (escape != null) {
                this.Children.Add(escape);
            }

            this.not = isNot;
        }

        /// <summary>
        /// Ctor - for use to create an expression tree, without child expression.
        /// </summary>
        public RegExpExpression()
        {
            not = false;
        }

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="left">provides values to match against regexp string</param>
        /// <param name="right">provides the regexp string</param>
        public RegExpExpression(
            Expression left,
            Expression right)
            : this(left, right, null)
        {
        }

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="left">provides values to match against regexp string</param>
        /// <param name="right">provides the regexp string</param>
        /// <param name="escape">provides the escape character</param>
        public RegExpExpression(
            Expression left,
            Expression right,
            Expression escape)
        {
            this.Children.Add(left);
            this.Children.Add(right);
            if (escape != null) {
                this.Children.Add(escape);
            }

            not = false;
        }

        public override ExpressionPrecedenceEnum Precedence {
            get => ExpressionPrecedenceEnum.RELATIONAL_BETWEEN_IN;
        }

        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
            this.Children[0].ToEPL(writer, Precedence);
            if (not) {
                writer.Write(" not");
            }

            writer.Write(" regexp ");
            this.Children[1].ToEPL(writer, ExpressionPrecedenceEnum.MINIMUM);

            if (this.Children.Count > 2) {
                writer.Write(" escape ");
                this.Children[2].ToEPL(writer, ExpressionPrecedenceEnum.MINIMUM);
            }
        }

        /// <summary>
        /// Returns true if negated.
        /// </summary>
        /// <returns>indicator whether negated</returns>
        public bool IsNot {
            get => not;
        }
    }
} // end of namespace