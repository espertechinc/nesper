///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using XLR8.CGLib;

using com.espertech.esper.client.dataflow;
using com.espertech.esper.dataflow.interfaces;

namespace com.espertech.esper.dataflow.core
{
    public interface SubmitHandler : EPDataFlowEmitter {
        void SubmitInternal(Object @object);
        void HandleSignal(EPDataFlowSignal signal);
        FastMethod FastMethod { get; }
    }
}
