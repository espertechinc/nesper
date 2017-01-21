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
    /// Match-Until construct for use in pattern expressions.
    /// </summary>
    [Serializable]
    public class PatternMatchUntilExpr : PatternExprBase
    {
        /// <summary>Ctor - for use to create a pattern expression tree, without pattern child expression. </summary>
        public PatternMatchUntilExpr()
        {
        }

        /// <summary>Ctor - for use when adding required child nodes later. </summary>
        /// <param name="low">low number of matches, or null if no lower boundary</param>
        /// <param name="high">high number of matches, or null if no high boundary</param>
        /// <param name="single">if a single bound is provided, this carries the single bound (all others should be null)</param>
        public PatternMatchUntilExpr(Expression low, Expression high, Expression single)
        {
            Low = low;
            High = high;
            Single = single;
        }

        /// <summary>Ctor. </summary>
        /// <param name="single">the single bound expression</param>
        public PatternMatchUntilExpr(Expression single)
        {
            Single = single;
        }

        /// <summary>Ctor. </summary>
        /// <param name="low">low number of matches, or null if no lower boundary</param>
        /// <param name="high">high number of matches, or null if no high boundary</param>
        /// <param name="match">the pattern expression that is sought to match repeatedly</param>
        /// <param name="until">the pattern expression that ends matching (optional, can be null)</param>
        public PatternMatchUntilExpr(Expression low, Expression high, PatternExpr match, PatternExpr until)
        {
            Low = low;
            High = high;
            AddChild(match);
            AddChild(until);
        }

        /// <summary>Returns the optional low endpoint for the repeat, or null if none supplied. </summary>
        /// <value>low endpoint</value>
        public Expression Low { get; set; }

        /// <summary>Returns the optional high endpoint for the repeat, or null if none supplied. </summary>
        /// <value>high endpoint</value>
        public Expression High { get; set; }

        /// <summary>Returns the single-bounds expression. </summary>
        /// <value>single-bound expression</value>
        public Expression Single { get; set; }

        public override PatternExprPrecedenceEnum Precedence
        {
            get { return PatternExprPrecedenceEnum.MATCH_UNTIL; }
        }

        public override void ToPrecedenceFreeEPL(TextWriter writer, EPStatementFormatter formatter)
        {
            if (Single != null)
            {
                writer.Write("[");
                Single.ToEPL(writer, ExpressionPrecedenceEnum.MINIMUM);
                writer.Write("]");
            }
            else
            {
                if (Low != null || High != null)
                {
                    writer.Write("[");
                    if ((Low != null) && (High != null))
                    {
                        Low.ToEPL(writer, ExpressionPrecedenceEnum.MINIMUM);
                        writer.Write(":");
                        High.ToEPL(writer, ExpressionPrecedenceEnum.MINIMUM);
                    }
                    else if (Low != null)
                    {
                        Low.ToEPL(writer, ExpressionPrecedenceEnum.MINIMUM);
                        writer.Write(":");
                    }
                    else
                    {
                        writer.Write(":");
                        High.ToEPL(writer, ExpressionPrecedenceEnum.MINIMUM);
                    }
                    writer.Write("] ");
                }
            }

            PatternExprPrecedenceEnum precedence = Precedence;
            if (Children[0] is PatternMatchUntilExpr)
            {
                precedence = PatternExprPrecedenceEnum.MAXIMIM;
            }
            Children[0].ToEPL(writer, precedence, formatter);

            if (Children.Count > 1)
            {
                writer.Write(" until ");
                Children[1].ToEPL(writer, Precedence, formatter);
            }
        }
    }
}