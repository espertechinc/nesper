///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.core.context.util;
using com.espertech.esper.epl.core;

namespace com.espertech.esper.epl.view
{
    /// <summary>Factory for output processing views. </summary>
    public interface OutputProcessViewFactory
    {
        OutputProcessViewBase MakeView(ResultSetProcessor resultSetProcessor, AgentInstanceContext agentInstanceContext);
    }
}
