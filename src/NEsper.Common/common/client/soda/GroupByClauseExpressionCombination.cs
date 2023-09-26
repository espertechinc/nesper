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
    /// A combination of expressions is for example "(a, b)", wherein the list of expressions
    /// provided together logically make up a grouping level.
    /// </summary>
    [Serializable]
    public class GroupByClauseExpressionCombination : GroupByClauseExpression
    {
        private IList<Expression> _expressions;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="expressions">combination of expressions</param>
        public GroupByClauseExpressionCombination(IList<Expression> expressions)
        {
            _expressions = expressions;
        }

        /// <summary>
        /// Ctor.
        /// </summary>
        public GroupByClauseExpressionCombination()
        {
        }

        /// <summary>
        /// Returns the combined expressions.
        /// </summary>
        /// <value>expressions</value>
        public IList<Expression> Expressions {
            get => _expressions;
            set => _expressions = value;
        }

        public void ToEPL(TextWriter writer)
        {
            writer.Write("(");
            var delimiter = "";
            foreach (var e in _expressions) {
                writer.Write(delimiter);
                e.ToEPL(writer, ExpressionPrecedenceEnum.MINIMUM);
                delimiter = ", ";
            }

            writer.Write(")");
        }
    }
}