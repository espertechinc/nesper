///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;

namespace com.espertech.esper.common.client.soda
{
    /// <summary>
    /// Followed-by for use in pattern expressions.
    /// </summary>
    [Serializable]
    public class PatternFollowedByExpr : PatternExprBase
    {
        /// <summary>
        /// Ctor - for use to create a pattern expression tree, without pattern child expression.
        /// </summary>
        public PatternFollowedByExpr()
        {
        }

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="optionalMaxPerSubexpression">if parameterized by a max-limits for each pattern sub-expressions</param>
        public PatternFollowedByExpr(IList<Expression> optionalMaxPerSubexpression)
        {
            OptionalMaxPerSubexpression = optionalMaxPerSubexpression;
        }

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="first">a first pattern expression in the followed-by relationship</param>
        /// <param name="second">a second pattern expression in the followed-by relationship</param>
        /// <param name="patternExprs">further optional pattern expressions in the followed-by relationship</param>
        public PatternFollowedByExpr(
            PatternExpr first,
            PatternExpr second,
            params PatternExpr[] patternExprs)
        {
            AddChild(first);
            AddChild(second);
            for (int i = 0; i < patternExprs.Length; i++)
            {
                AddChild(patternExprs[i]);
            }
        }

        /// <summary>
        /// Adds a pattern expression to the followed-by relationship between patterns.
        /// </summary>
        /// <param name="expr">to add</param>
        /// <returns>pattern expression</returns>
        public PatternFollowedByExpr Add(PatternExpr expr)
        {
            Children.Add(expr);
            return this;
        }

        public override PatternExprPrecedenceEnum Precedence
        {
            get { return PatternExprPrecedenceEnum.FOLLOWED_BY; }
        }

        /// <summary>Returns the instance limits, if any, for pattern-subexpressions. </summary>
        /// <value>list of max-limit or null</value>
        public IList<Expression> OptionalMaxPerSubexpression { get; set; }

        public override void ToPrecedenceFreeEPL(
            TextWriter writer,
            EPStatementFormatter formatter)
        {
            String delimiter = "";
            int childNum = 0;
            foreach (PatternExpr child in Children)
            {
                writer.Write(delimiter);
                child.ToEPL(writer, Precedence, formatter);

                delimiter = " -> ";
                if (OptionalMaxPerSubexpression != null && OptionalMaxPerSubexpression.Count > childNum)
                {
                    var maxExpr = OptionalMaxPerSubexpression[childNum];
                    if (maxExpr != null)
                    {
                        var inner = new StringWriter();
                        maxExpr.ToEPL(inner, ExpressionPrecedenceEnum.MINIMUM);
                        delimiter = " -[" + inner.ToString() + "]> ";
                    }
                }

                childNum++;
            }
        }
    }
}