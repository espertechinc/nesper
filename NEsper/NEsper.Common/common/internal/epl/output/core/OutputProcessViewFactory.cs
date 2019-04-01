///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.resultset.core;

namespace com.espertech.esper.common.@internal.epl.output.core
{
    /// <summary>
    /// Factory for output processing views.
    /// </summary>
    public interface OutputProcessViewFactory
    {
        OutputProcessView MakeView(ResultSetProcessor resultSetProcessor, AgentInstanceContext agentInstanceContext);
    }
} // end of namespace