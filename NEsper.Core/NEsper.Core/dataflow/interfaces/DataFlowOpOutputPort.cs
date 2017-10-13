///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.dataflow.util;

namespace com.espertech.esper.dataflow.interfaces
{
    public class DataFlowOpOutputPort
    {
        public DataFlowOpOutputPort(String streamName, GraphTypeDesc optionalDeclaredType)
        {
            StreamName = streamName;
            OptionalDeclaredType = optionalDeclaredType;
        }

        public string StreamName { get; private set; }

        public GraphTypeDesc OptionalDeclaredType { get; private set; }
    }
}