///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.epl.@join.@base;

namespace com.espertech.esper.core.context.factory
{
    public class StatementAgentInstancePostLoadIndexVisiting : StatementAgentInstancePostLoad
    {
        private readonly JoinSetComposer _joinSetComposer;

        public StatementAgentInstancePostLoadIndexVisiting(JoinSetComposer joinSetComposer)
        {
            _joinSetComposer = joinSetComposer;
        }

        public void ExecutePostLoad()
        {
        }

        public void AcceptIndexVisitor(StatementAgentInstancePostLoadIndexVisitor visitor)
        {
            _joinSetComposer.VisitIndexes(visitor);
        }
    }
}
