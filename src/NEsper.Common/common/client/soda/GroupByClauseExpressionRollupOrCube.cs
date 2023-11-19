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
    /// Represents a rollup or cube in a group-by clause.
    /// </summary>
    public class GroupByClauseExpressionRollupOrCube : GroupByClauseExpression
    {
        private bool _cube;
        private IList<GroupByClauseExpression> _expressions;

        /// <summary>Ctor. </summary>
        /// <param name="cube">true for cube, false for rollup</param>
        /// <param name="expressions">group-by expressions as part of rollup or cube</param>
        public GroupByClauseExpressionRollupOrCube(
            bool cube,
            IList<GroupByClauseExpression> expressions)
        {
            _cube = cube;
            _expressions = expressions;
        }

        /// <summary>Ctor. </summary>
        public GroupByClauseExpressionRollupOrCube()
        {
        }

        /// <summary>Returns the rollup or cube group-by expressions. </summary>
        /// <value>expressions</value>
        public IList<GroupByClauseExpression> Expressions {
            get => _expressions;
            set => _expressions = value;
        }

        /// <summary>Returns true for cube, false for rollup. </summary>
        /// <value>cube</value>
        public bool IsCube {
            get => _cube;
            set => _cube = value;
        }

        public void ToEPL(TextWriter writer)
        {
            if (_cube) {
                writer.Write("cube(");
            }
            else {
                writer.Write("rollup(");
            }

            var delimiter = "";
            foreach (var child in _expressions) {
                writer.Write(delimiter);
                child.ToEPL(writer);
                delimiter = ", ";
            }

            writer.Write(")");
        }
    }
}