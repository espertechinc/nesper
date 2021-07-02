///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.resultset.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;


namespace com.espertech.esper.common.@internal.epl.output.core
{
    public class OutputProcessViewDirectSimpleFactory : OutputProcessViewFactory
    {
        public static readonly OutputProcessViewDirectSimpleFactory INSTANCE = new OutputProcessViewDirectSimpleFactory();

        private OutputProcessViewDirectSimpleFactory()
        {
        }

        public OutputProcessView MakeView(
            ResultSetProcessor resultSetProcessor,
            AgentInstanceContext agentInstanceContext)
        {
            return new OutputProcessViewDirectSimpleImpl(resultSetProcessor, agentInstanceContext);
        }
    }
} // end of namespace