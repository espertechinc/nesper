///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.core.context.util;
using com.espertech.esper.util;

namespace com.espertech.esper.core.context.activator
{
    public class ViewableActivatorSubselectNone : ViewableActivator
    {
        public ViewableActivationResult Activate(AgentInstanceContext agentInstanceContext, bool isSubselect, bool isRecoveringResilient)
        {
            return new ViewableActivationResult(null, CollectionUtil.STOP_CALLBACK_NONE, null, null, null, false, false, null);
        }
    }
}
