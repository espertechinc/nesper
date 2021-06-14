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
    /// <summary>Specification for the merge statement insert/Update/delete-part. </summary>
    [Serializable]
    public abstract class OnTriggerMergeAction
    {
        public ExprNode OptionalWhereClause { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="OnTriggerMergeAction"/> class.
        /// </summary>
        /// <param name="optionalWhereClause">The optional where clause.</param>
        protected OnTriggerMergeAction(ExprNode optionalWhereClause)
        {
            OptionalWhereClause = optionalWhereClause;
        }
    }
}