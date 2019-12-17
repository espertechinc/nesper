///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.@internal.epl.dataflow.interfaces
{
    public class DataFlowOpForgeCodegenEnv
    {
        public DataFlowOpForgeCodegenEnv(
            string @namespace,
            string classPostfix)
        {
            Namespace = @namespace;
            ClassPostfix = classPostfix;
        }

        public string Namespace { get; }

        public string ClassPostfix { get; }
    }
} // end of namespace