///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.table.core;


namespace com.espertech.esper.common.@internal.epl.output.view
{
    /// <summary>
    /// An output strategy that handles routing (insert-into) and stream selection.
    /// </summary>
    public class OutputStrategyPostProcessFactory
    {
        private readonly Table _optionalTable;

        public OutputStrategyPostProcessFactory(
            bool isRoute,
            SelectClauseStreamSelectorEnum? insertIntoStreamSelector,
            SelectClauseStreamSelectorEnum selectStreamDirEnum,
            bool addToFront,
            Table optionalTable,
            ExprEvaluator eventPrecedence)
        {
            IsRoute = isRoute;
            InsertIntoStreamSelector = insertIntoStreamSelector;
            SelectStreamDirEnum = selectStreamDirEnum;
            IsAddToFront = addToFront;
            _optionalTable = optionalTable;
            EventPrecedence = eventPrecedence;
        }

        public OutputStrategyPostProcess Make(AgentInstanceContext agentInstanceContext)
        {
            TableInstance tableInstance = null;
            if (_optionalTable != null) {
                tableInstance = _optionalTable.GetTableInstance(agentInstanceContext.AgentInstanceId);
            }

            return new OutputStrategyPostProcess(this, agentInstanceContext, tableInstance);
        }

        public bool IsRoute { get; }

        public bool IsAddToFront { get; }

        public SelectClauseStreamSelectorEnum? InsertIntoStreamSelector { get; }

        public SelectClauseStreamSelectorEnum SelectStreamDirEnum { get; }

        public ExprEvaluator EventPrecedence { get; }
    }
} // end of namespace