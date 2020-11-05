///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
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
    public class OutputProcessViewAfterStateNone : OutputProcessViewAfterState
    {
        public static readonly OutputProcessViewAfterStateNone INSTANCE = new OutputProcessViewAfterStateNone();

        private OutputProcessViewAfterStateNone()
        {
        }

        public bool CheckUpdateAfterCondition(
            EventBean[] newEvents,
            StatementContext statementContext)
        {
            return true;
        }

        public bool CheckUpdateAfterCondition(
            ISet<MultiKeyArrayOfKeys<EventBean>> newEvents,
            StatementContext statementContext)
        {
            return true;
        }

        public bool CheckUpdateAfterCondition(
            UniformPair<EventBean[]> newOldEvents,
            StatementContext statementContext)
        {
            return true;
        }

        public void Destroy()
        {
            // no action required
        }
    }
} // end of namespace