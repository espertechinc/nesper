///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.epl.join.table;

namespace com.espertech.esper.core.context.factory
{
    public interface StatementAgentInstancePostLoadIndexVisitor
    {
        void Visit(EventTable[][] repositories);
        void Visit(IList<EventTable> tables);
        void Visit(EventTable index);
    }
}