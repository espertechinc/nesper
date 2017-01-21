///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
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
    public class OutputProcessViewFactoryCallback : OutputProcessViewFactory
    {
        private readonly OutputProcessViewCallback _callback;

        public OutputProcessViewFactoryCallback(OutputProcessViewCallback callback)
        {
            _callback = callback;
        }

        #region OutputProcessViewFactory Members

        public OutputProcessViewBase MakeView(ResultSetProcessor resultSetProcessor,
                                              AgentInstanceContext agentInstanceContext)
        {
            return new OutputProcessViewBaseCallback(resultSetProcessor, _callback);
        }

        #endregion
    }
}