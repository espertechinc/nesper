///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using com.espertech.esper.common.@internal.epl.dataflow.util;

namespace com.espertech.esper.common.@internal.epl.dataflow.interfaces
{
    public class DataFlowOpInputPort
    {
        public DataFlowOpInputPort(
            GraphTypeDesc typeDesc,
            ICollection<string> streamNames,
            string optionalAlias,
            bool hasPunctuationSignal)
        {
            TypeDesc = typeDesc;
            StreamNames = streamNames;
            OptionalAlias = optionalAlias;
            HasPunctuationSignal = hasPunctuationSignal;
        }

        public GraphTypeDesc TypeDesc { get; }

        public ICollection<string> StreamNames { get; }

        public string OptionalAlias { get; }

        public bool HasPunctuationSignal { get; }
    }
}