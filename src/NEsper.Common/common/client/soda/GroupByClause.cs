///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;

using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.client.soda
{
    /// <summary>
    /// The group-by clause consists of a list of expressions that provide the grouped-by values.
    /// </summary>
    public class GroupByClause
    {
        private readonly IList<GroupByClauseExpression> _groupByExpressions;

        /// <summary>Creates an empty group-by clause, to add to via add methods.</summary>
        /// <returns>group-by clause</returns>
        public static GroupByClause Create()
        {
            return new GroupByClause();
        }

        /// <summary>Creates a group-by clause from property names.</summary>
        /// <param name="properties">a list of one or more property names</param>
        /// <returns>group-by clause consisting of the properties</returns>
        public static GroupByClause Create(params string[] properties)
        {
            return new GroupByClause(properties);
        }

        /// <summary>Creates a group-by clause from expressions.</summary>
        /// <param name="expressions">a list of one or more expressions</param>
        /// <returns>group-by clause consisting of the expressions</returns>
        public static GroupByClause Create(params Expression[] expressions)
        {
            return new GroupByClause(expressions);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GroupByClause"/> class.
        /// </summary>
        /// <param name="groupByExpressions">The group by expressions.</param>
        public GroupByClause(IList<GroupByClauseExpression> groupByExpressions)
        {
            _groupByExpressions = groupByExpressions;
        }

        /// <summary>
        /// Ctor - for use to create an expression tree, without child expression.
        /// <para/>
        /// Use add methods to add child expressions to acts upon.
        /// </summary>
        public GroupByClause()
        {
            _groupByExpressions = new List<GroupByClauseExpression>();
        }

        /// <summary>Ctor.</summary>
        /// <param name="properties">is a list of property names</param>
        public GroupByClause(params string[] properties)
            : this()
        {
            foreach (var property in properties) {
                _groupByExpressions.Add(new GroupByClauseExpressionSingle(Expressions.Property(property)));
            }
        }

        /// <summary>Ctor.</summary>
        /// <param name="expressions">list of expressions</param>
        public GroupByClause(params Expression[] expressions)
            : this()
        {
            foreach (var expression in expressions) {
                _groupByExpressions.Add(new GroupByClauseExpressionSingle(expression));
            }
        }

        /// <summary>Gets or sets the expressions providing the grouped-by values.</summary>
        /// <returns>expressions</returns>
        public IList<GroupByClauseExpression> GroupByExpressions {
            get => _groupByExpressions;
            set {
                _groupByExpressions.Clear();
                _groupByExpressions.AddAll(value);
            }
        }

        /// <summary>Renders the clause in textual representation.</summary>
        /// <param name="writer">to output to</param>
        public void ToEPL(TextWriter writer)
        {
            var delimiter = "";
            foreach (var child in _groupByExpressions) {
                writer.Write(delimiter);
                child.ToEPL(writer);
                delimiter = ", ";
            }
        }
    }
} // End of namespace