///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Reflection;
using com.espertech.esper.common.client.dataflow.util;
using com.espertech.esper.common.@internal.epl.dataflow.interfaces;

namespace com.espertech.esper.common.@internal.epl.dataflow.realize
{
    public interface SubmitHandler : EPDataFlowEmitter
    {
        MethodInfo FastMethod { get; }
        void SubmitInternal(object @object);

        void HandleSignal(EPDataFlowSignal signal);
    }
} // end of namespace