///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.@internal.epl.expression.core;

namespace com.espertech.esper.common.@internal.compile.stage1.spec
{
    /// <summary>
    ///     Specification for the update statement.
    /// </summary>
    public class UpdateDesc
    {
        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="optionalStreamName">a stream name if provided for the update</param>
        /// <param name="assignments">the individual assignments made</param>
        /// <param name="optionalWhereClause">the where-clause expression if provided</param>
        public UpdateDesc(
            string optionalStreamName,
            IList<OnTriggerSetAssignment> assignments,
            ExprNode optionalWhereClause)
        {
            OptionalStreamName = optionalStreamName;
            Assignments = assignments;
            OptionalWhereClause = optionalWhereClause;
        }

        /// <summary>
        ///     Returns a list of all assignment
        /// </summary>
        /// <returns>list of assignments</returns>
        public IList<OnTriggerSetAssignment> Assignments { get; }

        /// <summary>
        ///     Returns the stream name if defined.
        /// </summary>
        /// <returns>stream name</returns>
        public string OptionalStreamName { get; }

        /// <summary>
        ///     Returns the where-clause if defined.
        /// </summary>
        /// <returns>where clause</returns>
        public ExprNode OptionalWhereClause { get; set; }
    }
} // end of namespace