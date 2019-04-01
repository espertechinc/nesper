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
        public DataFlowOpForgeCodegenEnv(string packageName, string classPostfix)
        {
            PackageName = packageName;
            ClassPostfix = classPostfix;
        }

        public string PackageName { get; }

        public string ClassPostfix { get; }
    }
} // end of namespace