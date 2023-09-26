///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.IO;

using com.espertech.esper.common.client;

namespace com.espertech.esper.runtime.@internal.dataflow.op.logsink
{
    public interface ConsoleOpRenderer
    {
        void Render(EventBean eventBean, TextWriter writer);
    }
} // end of namespace