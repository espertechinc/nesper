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
    ///     SQL-Like expression for matching '%' and '_' wildcard strings following SQL standards.
    /// </summary>
    [Serializable]
    public class LikeExpression : ExpressionBase
    {
        /// <summary>
        ///     Ctor - for use to create an expression tree, without child expression.
        ///     <para />
        ///     Use add methods to add child expressions to acts upon.
        /// </summary>
        public LikeExpression()
        {
            IsNot = false;
        }

        /// <summary>
        ///     Ctor - for use to create an expression tree, without child expression.
        ///     <para />
        ///     Use add methods to add child expressions to acts upon.
        /// </summary>
        /// <param name="isNot">if the like-expression is negated</param>
        public LikeExpression(bool isNot)
        {
            IsNot = isNot;
        }

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="left">provides the value to match</param>
        /// <param name="right">provides the like-expression to match against</param>
        public LikeExpression(
            Expression left,
            Expression right)
            : this(left, right, null)
        {
        }

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="left">provides the value to match</param>
        /// <param name="right">provides the like-expression to match against</param>
        /// <param name="escape">is the expression providing the string escape character</param>
        public LikeExpression(
            Expression left,
            Expression right,
            Expression escape)
        {
            Children.Add(left);
            Children.Add(right);
            if (escape != null)
            {
                Children.Add(escape);
            }

            IsNot = false;
        }

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="left">provides the value to match</param>
        /// <param name="right">provides the like-expression to match against</param>
        /// <param name="isNot">if the like-expression is negated</param>
        public LikeExpression(
            Expression left,
            Expression right,
            bool isNot)
            : this(left, right, null, isNot)
        {
        }

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="left">provides the value to match</param>
        /// <param name="right">provides the like-expression to match against</param>
        /// <param name="escape">is the expression providing the string escape character</param>
        /// <param name="isNot">if the like-expression is negated</param>
        public LikeExpression(
            Expression left,
            Expression right,
            Expression escape,
            bool isNot)
        {
            Children.Add(left);
            Children.Add(right);
            if (escape != null)
            {
                Children.Add(escape);
            }

            IsNot = isNot;
        }

        public override ExpressionPrecedenceEnum Precedence => ExpressionPrecedenceEnum.RELATIONAL_BETWEEN_IN;

        /// <summary>
        ///     Returns true if this is a "not like", or false if just a like
        /// </summary>
        /// <returns>indicator whether negated or not</returns>
        public bool IsNot { get; set; }

        /// <summary>
        ///     Returns true if this is a "not like", or false if just a like
        /// </summary>
        /// <returns>indicator whether negated or not</returns>
        public bool Not
        {
            get => IsNot;
            set => IsNot = value;
        }

        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
            Children[0].ToEPL(writer, Precedence);
            if (IsNot)
            {
                writer.Write(" not");
            }

            writer.Write(" like ");
            Children[1].ToEPL(writer, ExpressionPrecedenceEnum.MINIMUM);

            if (Children.Count > 2)
            {
                writer.Write(" escape ");
                Children[2].ToEPL(writer, ExpressionPrecedenceEnum.MINIMUM);
            }
        }
    }
} // end of namespace