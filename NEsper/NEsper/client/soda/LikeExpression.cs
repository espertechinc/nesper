///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;

namespace com.espertech.esper.client.soda
{
    /// <summary>
    /// SQL-Like expression for matching '%' and '_' wildcard strings following SQL
    /// standards.
    /// </summary>
    [Serializable]
    public class LikeExpression : ExpressionBase
    {
        /// <summary>
        /// Ctor - for use to create an expression tree, without child expression.
        /// <para/>
        /// Use add methods to add child expressions to acts upon.
        /// </summary>
        public LikeExpression()
        {
            IsNot = false;
        }

        /// <summary>
        /// Ctor - for use to create an expression tree, without child expression.
        /// <para/>
        /// Use add methods to add child expressions to acts upon.
        /// </summary>
        /// <param name="isNot">if the like-expression is negated</param>
        public LikeExpression(bool isNot)
        {
            IsNot = isNot;
        }

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="left">provides the value to match</param>
        /// <param name="right">provides the like-expression to match against</param>
        public LikeExpression(Expression left, Expression right)
            : this(left, right, null)
        {
        }

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="left">provides the value to match</param>
        /// <param name="right">provides the like-expression to match against</param>
        /// <param name="escape">is the expression providing the string escape character</param>
        public LikeExpression(Expression left, Expression right, Expression escape)
        {
            IList<Expression> children = Children;
            children.Add(left);
            children.Add(right);
            if (escape != null) {
                children.Add(escape);
            }
            IsNot = false;
        }

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="left">provides the value to match</param>
        /// <param name="right">provides the like-expression to match against</param>
        /// <param name="isNot">if the like-expression is negated</param>
        public LikeExpression(Expression left, Expression right, bool isNot)
            : this(left, right, null, isNot)
        {
        }

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="left">provides the value to match</param>
        /// <param name="right">provides the like-expression to match against</param>
        /// <param name="escape">is the expression providing the string escape character</param>
        /// <param name="isNot">if the like-expression is negated</param>
        public LikeExpression(Expression left, Expression right, Expression escape, bool isNot)
        {
            Children.Add(left);
            Children.Add(right);
            if (escape != null) {
                Children.Add(escape);
            }
            this.IsNot = isNot;
        }

        /// <summary>
        /// Returns true if this is a "not like", or false if just a like
        /// </summary>
        /// <returns>
        /// indicator whether negated or not
        /// </returns>
        public bool IsNot { get; set; }

        /// <summary>
        /// Gets the Precedence.
        /// </summary>
        /// <value>The Precedence.</value>
        public override ExpressionPrecedenceEnum Precedence
        {
            get { return ExpressionPrecedenceEnum.RELATIONAL_BETWEEN_IN; }
        }

        public override void  ToPrecedenceFreeEPL(TextWriter writer)
        {
            Children[0].ToEPL(writer, Precedence);
            if (IsNot) {
                writer.Write(" not");
            }
            writer.Write(" like ");
            Children[1].ToEPL(writer, Precedence);

            if (Children.Count > 2) {
                writer.Write(" escape ");
                Children[2].ToEPL(writer, Precedence);
            }
        }
    }
}
