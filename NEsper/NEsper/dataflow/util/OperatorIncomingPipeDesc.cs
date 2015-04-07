///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

namespace com.espertech.esper.dataflow.util
{
    public class OperatorIncomingPipeDesc {
        public OperatorIncomingPipeDesc(int incomingPortNum, IList<PortDesc> sources) {
            IncomingPortNum = incomingPortNum;
            Sources = sources;
        }

        public int IncomingPortNum { get; private set; }

        public IList<PortDesc> Sources { get; private set; }
    }
}
