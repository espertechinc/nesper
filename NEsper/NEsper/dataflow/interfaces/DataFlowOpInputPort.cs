///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.dataflow.util;

namespace com.espertech.esper.dataflow.interfaces
{
    public class DataFlowOpInputPort
    {
        public DataFlowOpInputPort(GraphTypeDesc typeDesc, ICollection<String> streamNames, String optionalAlias, bool hasPunctuationSignal)
        {
            TypeDesc = typeDesc;
            StreamNames = streamNames;
            OptionalAlias = optionalAlias;
            HasPunctuationSignal = hasPunctuationSignal;
        }

        public GraphTypeDesc TypeDesc { get; private set; }

        public ICollection<string> StreamNames { get; private set; }

        public string OptionalAlias { get; private set; }

        public bool HasPunctuationSignal { get; private set; }
    }
}
