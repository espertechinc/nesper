///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.epl.dataflow.util;

namespace com.espertech.esper.common.@internal.epl.dataflow.interfaces
{
    public class DataFlowOpForgeInitializeResult
    {
        public DataFlowOpForgeInitializeResult()
        {
        }

        public DataFlowOpForgeInitializeResult(GraphTypeDesc[] typeDescriptors)
        {
            TypeDescriptors = typeDescriptors;
        }

        public StmtForgeMethodResult AdditionalForgeables { get; set; }

        public GraphTypeDesc[] TypeDescriptors { get; set; }
    }
} // end of namespace