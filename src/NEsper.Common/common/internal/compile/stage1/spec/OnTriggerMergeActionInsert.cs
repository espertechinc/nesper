///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.epl.expression.core;

namespace com.espertech.esper.common.@internal.compile.stage1.spec
{
    /// <summary>
    ///     Specification for the merge statement insert-part.
    /// </summary>
    public class OnTriggerMergeActionInsert : OnTriggerMergeAction
    {
        public OnTriggerMergeActionInsert(
            ExprNode optionalWhereClause,
            string optionalStreamName,
            IList<string> columns,
            IList<SelectClauseElementRaw> selectClause,
            ExprNode eventPrecedence) : base(optionalWhereClause)
        {
            OptionalStreamName = optionalStreamName;
            Columns = columns;
            SelectClause = selectClause;
            EventPrecedence = eventPrecedence;
        }

        public string OptionalStreamName { get; }

        public IList<string> Columns { get; }

        public IList<SelectClauseElementRaw> SelectClause { get; }

        [field: NonSerialized]
        public IList<SelectClauseElementCompiled> SelectClauseCompiled { set; get; }

        public ExprNode EventPrecedence { get; }
    }
} // end of namespace