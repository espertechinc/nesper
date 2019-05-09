///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
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
    /// <summary>Specification for the merge statement insert-part. </summary>
    [Serializable]
    public class OnTriggerMergeActionInsert : OnTriggerMergeAction
    {
        [NonSerialized] private IList<SelectClauseElementCompiled> selectClauseCompiled;

        public OnTriggerMergeActionInsert(
            ExprNode optionalWhereClause,
            String optionalStreamName,
            IList<String> columns,
            IList<SelectClauseElementRaw> selectClause)
            : base(optionalWhereClause)
        {
            OptionalStreamName = optionalStreamName;
            Columns = columns;
            SelectClause = selectClause;
        }

        public string OptionalStreamName { get; private set; }

        public IList<string> Columns { get; private set; }

        public IList<SelectClauseElementRaw> SelectClause { get; private set; }

        public IList<SelectClauseElementCompiled> SelectClauseCompiled {
            get { return selectClauseCompiled; }
            set { this.selectClauseCompiled = value; }
        }
    }
}