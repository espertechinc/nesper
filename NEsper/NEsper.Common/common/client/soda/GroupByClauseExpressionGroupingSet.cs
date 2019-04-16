///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
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
    /// Represents the "grouping sets" keywords.
    /// </summary>
    [Serializable]
    public class GroupByClauseExpressionGroupingSet : GroupByClauseExpression
    {
        private IList<GroupByClauseExpression> _expressions;

        /// <summary>Ctor. </summary>
        /// <param name="expressions">group-by expressions withing grouping set</param>
        public GroupByClauseExpressionGroupingSet(IList<GroupByClauseExpression> expressions)
        {
            _expressions = expressions;
        }

        /// <summary>Ctor. </summary>
        public GroupByClauseExpressionGroupingSet()
        {
        }

        /// <summary>Returns list of expressions in grouping set. </summary>
        /// <value>group-by expressions</value>
        public IList<GroupByClauseExpression> Expressions {
            get { return _expressions; }
            set { _expressions = value; }
        }

        public void ToEPL(TextWriter writer)
        {
            writer.Write("grouping sets(");
            String delimiter = "";
            foreach (GroupByClauseExpression child in _expressions) {
                writer.Write(delimiter);
                child.ToEPL(writer);
                delimiter = ", ";
            }

            writer.Write(")");
        }
    }
}