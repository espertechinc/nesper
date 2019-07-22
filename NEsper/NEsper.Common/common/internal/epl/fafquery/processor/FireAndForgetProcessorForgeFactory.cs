///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.epl.namedwindow.path;

namespace com.espertech.esper.common.@internal.epl.fafquery.processor
{
    public class FireAndForgetProcessorForgeFactory
    {
        public static FireAndForgetProcessorForge ValidateResolveProcessor(StreamSpecCompiled streamSpec)
        {
            if (streamSpec is NamedWindowConsumerStreamSpec) {
                NamedWindowMetaData nwdetail = ((NamedWindowConsumerStreamSpec) streamSpec).NamedWindow;
                return new FireAndForgetProcessorNamedWindowForge(nwdetail);
            }

            TableQueryStreamSpec tableSpec = (TableQueryStreamSpec) streamSpec;
            return new FireAndForgetProcessorTableForge(tableSpec.Table);
        }
    }
} // end of namespace