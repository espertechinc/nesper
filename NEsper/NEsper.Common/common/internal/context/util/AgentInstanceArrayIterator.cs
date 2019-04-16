///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using com.espertech.esper.common.client;

namespace com.espertech.esper.common.@internal.context.util
{
    public class AgentInstanceArrayIterator
    {
        public static IEnumerator<EventBean> Create(AgentInstance[] instances)
        {
            foreach (AgentInstance agentInstance in instances) {
                foreach (EventBean eventBean in agentInstance.FinalView) {
                    yield return eventBean;
                }
            }
        }
    }
} // end of namespace