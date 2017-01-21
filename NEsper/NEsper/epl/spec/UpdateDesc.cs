///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.epl.expression.core;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.spec
{
    /// <summary>Specification for the Update statement. </summary>
    [Serializable]
    public class UpdateDesc : MetaDefItem
    {
        /// <summary>Ctor. </summary>
        /// <param name="optionalStreamName">a stream name if provided for the Update</param>
        /// <param name="assignments">the individual assignments made</param>
        /// <param name="optionalWhereClause">the where-clause expression if provided</param>
        public UpdateDesc(String optionalStreamName, IList<OnTriggerSetAssignment> assignments, ExprNode optionalWhereClause)
        {
            OptionalStreamName = optionalStreamName;
            Assignments = assignments;
            OptionalWhereClause = optionalWhereClause;
        }

        /// <summary>Returns a list of all assignment </summary>
        /// <value>list of assignments</value>
        public IList<OnTriggerSetAssignment> Assignments { get; private set; }

        /// <summary>Returns the stream name if defined. </summary>
        /// <value>stream name</value>
        public string OptionalStreamName { get; private set; }

        /// <summary>Returns the where-clause if defined. </summary>
        /// <value>where clause</value>
        public ExprNode OptionalWhereClause { get; set; }
    }
}
