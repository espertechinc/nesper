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
    /// Every-Distinct construct for use in pattern expressions.
    /// </summary>
    public class PatternEveryDistinctExpr : PatternExprBase
    {
        /// <summary>
        /// Ctor - for use to create a pattern expression tree, without unique-criterial expression.
        /// </summary>
        public PatternEveryDistinctExpr()
        {
        }

        /// <summary>
        /// Ctor - for use to create a pattern expression tree, without unique-criterial expression.
        /// </summary>
        /// <param name="expressions">distinct expressions</param>
        public PatternEveryDistinctExpr(IList<Expression> expressions)
        {
            Expressions = expressions;
        }

        /// <summary>Returns distinct expressions </summary>
        /// <value>expr</value>
        public IList<Expression> Expressions { get; set; }

        public override PatternExprPrecedenceEnum Precedence => PatternExprPrecedenceEnum.EVERY_NOT;

        public override void ToPrecedenceFreeEPL(
            TextWriter writer,
            EPStatementFormatter formatter)
        {
            writer.Write("every-distinct(");
            var delimiter = "";
            foreach (var expr in Expressions) {
                writer.Write(delimiter);
                expr.ToEPL(writer, ExpressionPrecedenceEnum.MINIMUM);
                delimiter = ",";
            }

            writer.Write(") ");

            Children[0].ToEPL(writer, Precedence, formatter);
        }
    }
}