///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.context.util;

namespace com.espertech.esper.common.@internal.epl.output.view
{
    public interface OutputProcessViewAfterState
    {
        bool CheckUpdateAfterCondition(
            EventBean[] newEvents,
            StatementContext statementContext);

        bool CheckUpdateAfterCondition(
            ISet<MultiKeyArrayOfKeys<EventBean>> newEvents,
            StatementContext statementContext);

        bool CheckUpdateAfterCondition(
            UniformPair<EventBean[]> newOldEvents,
            StatementContext statementContext);

        void Destroy();
    }
} // end of namespace