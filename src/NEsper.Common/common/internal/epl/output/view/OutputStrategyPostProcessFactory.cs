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
        private readonly bool isRoute;
        private readonly SelectClauseStreamSelectorEnum insertIntoStreamSelector;
        private readonly SelectClauseStreamSelectorEnum selectStreamDirEnum;
        private readonly bool addToFront;
        private readonly Table optionalTable;
        private readonly ExprEvaluator eventPrecedence;

        public OutputStrategyPostProcessFactory(
            bool isRoute,
            SelectClauseStreamSelectorEnum insertIntoStreamSelector,
            SelectClauseStreamSelectorEnum selectStreamDirEnum,
            bool addToFront,
            Table optionalTable,
            ExprEvaluator eventPrecedence)
        {
            this.isRoute = isRoute;
            this.insertIntoStreamSelector = insertIntoStreamSelector;
            this.selectStreamDirEnum = selectStreamDirEnum;
            this.addToFront = addToFront;
            this.optionalTable = optionalTable;
            this.eventPrecedence = eventPrecedence;
        }

        public OutputStrategyPostProcess Make(AgentInstanceContext agentInstanceContext)
        {
            TableInstance tableInstance = null;
            if (optionalTable != null) {
                tableInstance = optionalTable.GetTableInstance(agentInstanceContext.AgentInstanceId);
            }

            return new OutputStrategyPostProcess(this, agentInstanceContext, tableInstance);
        }

        public bool IsRoute => isRoute;

        public bool IsAddToFront => addToFront;

        public SelectClauseStreamSelectorEnum InsertIntoStreamSelector => insertIntoStreamSelector;

        public SelectClauseStreamSelectorEnum SelectStreamDirEnum => selectStreamDirEnum;

        public ExprEvaluator EventPrecedence => eventPrecedence;
    }
} // end of namespace