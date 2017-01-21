///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client.dataflow;

namespace com.espertech.esper.dataflow.interfaces
{
    public interface EPDataFlowEmitter
    {
        void Submit(Object @object);
        void SubmitSignal(EPDataFlowSignal signal);
        void SubmitPort(int portNumber, Object @object);
    }
}
