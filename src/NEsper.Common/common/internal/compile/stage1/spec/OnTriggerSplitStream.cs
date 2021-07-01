///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.epl.expression.core;

namespace com.espertech.esper.common.@internal.compile.stage1.spec
{
    /// <summary>Split-stream description.</summary>
    [Serializable]
    public class OnTriggerSplitStream
    {
        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="insertInto">the insert-into clause</param>
        /// <param name="selectClause">the select-clause</param>
        /// <param name="fromClause">the from-clause</param>
        /// <param name="whereClause">where-expression or null</param>
        public OnTriggerSplitStream(
            InsertIntoDesc insertInto,
            SelectClauseSpecRaw selectClause,
            OnTriggerSplitStreamFromClause fromClause,
            ExprNode whereClause)
        {
            InsertInto = insertInto;
            SelectClause = selectClause;
            FromClause = fromClause;
            WhereClause = whereClause;
        }

        /// <summary>
        /// Returns the insert-into clause.
        /// </summary>
        /// <value>insert-into</value>
        public InsertIntoDesc InsertInto { get; private set; }

        /// <summary>
        /// Returns the select clause.
        /// </summary>
        /// <value>select</value>
        public SelectClauseSpecRaw SelectClause { get; private set; }

        /// <summary>
        /// Returns the where clause or null if not defined
        /// </summary>
        /// <value>where clause</value>
        public ExprNode WhereClause { get; private set; }

        public OnTriggerSplitStreamFromClause FromClause { get; private set; }
    }
} // end of namespace