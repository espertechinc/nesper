///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.IO;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.util;

namespace com.espertech.esper.runtime.@internal.dataflow.op.logsink
{
    public class ConsoleOpRendererSummary : ConsoleOpRenderer
    {
        public void Render(EventBean theEvent, StringWriter writer)
        {
            EventBeanSummarizer.Summarize(theEvent, writer);
        }
    }
} // end of namespace